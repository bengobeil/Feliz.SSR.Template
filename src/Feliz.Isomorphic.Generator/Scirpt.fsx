open Program

#load "StringBuilder.fs"
#load "Program.fs"
#load "String.fs"
#r "netstandard"

open Feliz.Isomorphic.Generator
open System.Reflection
open GobEx.Core

[<RequireQualifiedAccess>]
module Seq =
    let exclude f =
        Seq.filter (f >> not)
        
[<RequireQualifiedAccess>]
module Tuple =
    let reduceTuplesOfLists tuples =
        tuples
        |> List.reduce (fun (cAcc,sAcc) (cCurr,sCurr)-> (cAcc @ cCurr, sAcc @ sCurr))
        
    let map f = function (x,y) -> (f x,f y)
    let map2 f1 f2 = function (x,y) -> (f1 x,f2 y)

type DiscoverableAssembly = {Name: string; Exclusions: string list; IncludeAliases: string list}
type IsomorphicAssemblyPair = {Client: DiscoverableAssembly; Server: DiscoverableAssembly}
    
type Presence<'a> =
    | InBoth of client: 'a * server: 'a
    | ClientOnly of 'a
    | ServerOnly of 'a
    
[<RequireQualifiedAccess>]
module Presence =
    let unzip = function
        | InBoth (c,s) -> (Some c, Some s)
        | ServerOnly s -> (None, Some s)
        | ClientOnly c -> (Some c, None)
        
type DiscoveredType =
    | Type of typeName: string
    | InnerModuleType of fullModuleName:string * type': DiscoveredType 
    
type DiscoveredModule<'a> =
    | RootType of typeName: 'a
    | Module of moduleName: 'a * types: DiscoveredModule<'a> list
    
module Discovery =
    let private separateDiscoveredTypes items =
        let rec separate items (acc1,acc2) =
            match items with
            | x::xs ->
                match x with
                | Type typeName ->
                    (typeName :: acc1,acc2)
                | InnerModuleType (m,t) ->
                    (acc1,(m,t):: acc2)
                |> separate xs
            | [] -> (acc1,acc2)
        separate items ([],[])
        
    let private (|Type|InnerModuleType|InnerInnerModuleType|Ignore|) (typeName: string) =
        let trimModule (str: string) = String.split str "Module" |> Array.head
        
        match String.split typeName "+" with
        | arr when Array.last arr = "" -> Ignore
        | [| moduleFullName; innerModuleName; typeName |] ->
            InnerInnerModuleType (trimModule moduleFullName, trimModule innerModuleName, typeName)
        | [| moduleFullName; typeName |] ->
            InnerModuleType (trimModule moduleFullName , typeName)
        | [| x |] when x.EndsWith "Module" -> Ignore
        | [| x |] -> Type x
        | _ -> failwithf "Not expected"
        
    let rec private toDiscoveredModule (types: DiscoveredType list) =
        types
        |> separateDiscoveredTypes
        |> fun (rootTypes,x) -> (List.map RootType rootTypes, x)
        |> fun (x,moduleTypes) ->
            moduleTypes
            |> List.groupBy fst
            |> List.map (fun (moduleName, innerModuleTypes) ->
                innerModuleTypes
                |> List.map snd
                |> fun types -> Module (moduleName, toDiscoveredModule types))
            |> fun modules -> x @ modules
        
    let discoverModules types =
        types
        |> Seq.choose (function
            | Ignore -> None
            | Type typeName -> Some <| Type typeName
            | InnerModuleType (moduleName, typeName) -> Some <| InnerModuleType (moduleName, Type(sprintf "%s.%s" moduleName typeName))
            | InnerInnerModuleType (qualifiedName, moduleName, typeName) ->
                Some <| InnerModuleType (qualifiedName, InnerModuleType (sprintf "%s.%s" qualifiedName moduleName, Type(sprintf "%s.%s.%s" qualifiedName moduleName typeName))))
        |> List.ofSeq
        |> toDiscoveredModule
        
module DiscoveredModule =
    let content = function
        | RootType s -> s
        | Module (m,_) -> m
        
    let rec map f = function
        | RootType s -> RootType (f s)
        | Module (m,x) -> Module (f m,x |> List.map (map f))
        
    let mkModule subModules name =
        Module (name, subModules)
        
    let unzip (discoveredModules: DiscoveredModule<Presence<string>> list) =
        let rec unzipModule (discoveredModule: DiscoveredModule<Presence<string>>): DiscoveredModule<string> list * DiscoveredModule<string> list =
            match discoveredModule with
            | RootType presence ->
                Presence.unzip presence
                |> Tuple.map (Option.map RootType >> Option.toList)
                    
            | Module (presence, subModules) ->
                let (clientModules, serverModules) =
                    subModules
                    |> List.map unzipModule
                    |> Tuple.reduceTuplesOfLists
                    
                Presence.unzip presence
                |> Tuple.map2
                    (Option.map (mkModule clientModules) >> Option.toList)
                    (Option.map (mkModule serverModules) >> Option.toList)
                    
        discoveredModules
        |> List.map unzipModule
        |> Tuple.reduceTuplesOfLists



let findValidTypes {Name = name; Exclusions = exclusions; IncludeAliases = aliases } = 
    let assembly = Assembly.Load(name)
    
    let excludeAssemblySpecific (exclusions:string seq) seq =
        let shouldBeExcluded (assemblyName: string) =
            exclusions |> Seq.exists (fun exclusion -> assemblyName.Contains exclusion)
        Seq.exclude shouldBeExcluded seq
    
    assembly.GetTypes()
    |> Array.map (fun type' -> type'.FullName)
    |> Seq.exclude (fun typeName -> typeName.StartsWith "<StartupCode$")
    |> Seq.exclude (fun typeName -> typeName.Contains "@")
    |> excludeAssemblySpecific exclusions
    |> Seq.append aliases
    

    


module ViewEngine =
    let toClientName (str: string) =
        str.Replace(".ViewEngine","")

let discoverModulesForPair {Client = clientDiscoverableAssembly; Server = serverDiscoverableAssembly}: DiscoveredModule<Presence<string>> list =
    let toClientString =
        DiscoveredModule.content >> ViewEngine.toClientName
    
    let makeMapFromModules modules =
        modules
        |> List.map (fun module' -> (DiscoveredModule.content module', module'))
        |> Map.ofList
        
    let rec zipClientServerTypes clientModules serverModules:DiscoveredModule<Presence<string>> list =
        let clientTypeMap =
            makeMapFromModules clientModules
            
        let serverTypeMap =
            makeMapFromModules serverModules
        
        let findModuleInMap map moduleName =
            Map.find moduleName map
            
        let findClientModule name =
            findModuleInMap clientTypeMap name
            
        let findServerModule name =
            findModuleInMap serverTypeMap name
            
        let clientModuleExists name =
            Map.containsKey name clientTypeMap
            
        serverModules
        |> List.partition (fun item -> Map.containsKey (toClientString item) clientTypeMap)
        |> fun (typesInBoth,typesInServerOnly) ->
            let namesInBoth =
                typesInBoth
                |> List.map (fun type' -> (toClientString type', DiscoveredModule.content type'))
                
            let serverModulesInServerOnly =
                typesInServerOnly
                |> List.map DiscoveredModule.content
                |> List.map findServerModule
                |> List.map (DiscoveredModule.map ServerOnly)
                
            let clientModulesInClientOnly =
                namesInBoth
                |> List.map fst
                |> List.partition clientModuleExists
                |> snd
                |> List.map findClientModule
                |> List.map (DiscoveredModule.map ClientOnly)
            
            let modulesInBoth =
                namesInBoth
                |> List.map (fun (clientName,serverName) ->
                    (findClientModule clientName, findServerModule serverName)
                    |> function
                        | RootType cName, RootType sName ->
                            RootType <| InBoth (cName,sName)
                        | Module (cName, cSubModules),Module (sName, sSubModules) ->
                            Module (InBoth (cName, sName), zipClientServerTypes cSubModules sSubModules)
                        | Module (cName, subModules), RootType sName ->
                            Module (InBoth (cName, sName), subModules |> List.map (DiscoveredModule.map ClientOnly))
                        | RootType cName, Module (sName, subModules) ->
                            Module (InBoth (cName, sName), subModules |> List.map (DiscoveredModule.map ServerOnly)))
                
            (clientModulesInClientOnly)
            @ (serverModulesInServerOnly)
            @ (modulesInBoth)
            
    let discoverModulesFromAssembly =
        findValidTypes >> Discovery.discoverModules
            
    let clientModules =
        clientDiscoverableAssembly
        |> discoverModulesFromAssembly
    
    let serverModules =
        serverDiscoverableAssembly
        |> discoverModulesFromAssembly
    
    zipClientServerTypes clientModules serverModules
    
[<RequireQualifiedAccess>]
module TypeName =
    let significantName (typeName: string) =
        typeName.Split '.'
        |> Array.last
    
type TypeAliasDefinition = TypeAliasDefinition of string
module TypeAliasDefinition =
    let fromQualifiedTypeName typeName =
        stringBuilder {
            "type "
            TypeName.significantName typeName
            " = "
            typeName
        } |> TypeAliasDefinition


type ModuleDefinition = ModuleDefinition of string
module ModuleDefinition =
    let fromQualifiedTypeName typeName =
        stringBuilder {
            "module "
            TypeName.significantName typeName
            " ="
        } |> ModuleDefinition

type TypeDefinition =
    | AliasDefinition of TypeAliasDefinition
    | FullModule of ModuleDefinition * TypeDefinition list
    
module TypeDefinition =
    let [<Literal>] private tab = "    "
    
    let rec toFileString indentLevel typeDefinition =
        let indentation = Seq.init indentLevel (fun _ -> tab) |> Seq.reduce (++)
        
        match typeDefinition with
        | AliasDefinition (TypeAliasDefinition s) -> indentation ++ s
        | FullModule ((ModuleDefinition moduleDefinition), childTypeDefs) ->
            stringBuilder {
                seq {
                    indentation ++ moduleDefinition
                    yield! childTypeDefs
                           |> Seq.map (toFileString (indentLevel + 1))
                }
            }
    let rec getTypeDefinitionsFromModules (modules: DiscoveredModule<string> list): TypeDefinition list =
        let getTypeDefinitionFromModule (module': DiscoveredModule<string>): TypeDefinition =
            match module' with
            | RootType name -> AliasDefinition <| TypeAliasDefinition.fromQualifiedTypeName name
            | Module (name, subModules) ->
                FullModule (ModuleDefinition.fromQualifiedTypeName name, getTypeDefinitionsFromModules subModules)
        List.map getTypeDefinitionFromModule modules
        
    let getTypeDefinitionsFromZippedModules (modulePresences: DiscoveredModule<Presence<string>> list): TypeDefinition list * TypeDefinition list =
        DiscoveredModule.unzip modulePresences
        |> Tuple.map getTypeDefinitionsFromModules
        
let buildFile namespace' clientAssemblyName serverAssemblyName clientTypeDefinitions serverTypeDefinitions =
    stringBuilder {
        seq {
        "namespace " ++ namespace' 
        ""
        "#if FABLE_COMPILER"
        "open " ++ clientAssemblyName
        ""
        yield!
            clientTypeDefinitions
            |> Seq.map (TypeDefinition.toFileString 0)
        "#else"
        "open " ++ serverAssemblyName
        yield!
            serverTypeDefinitions
            |> Seq.map (TypeDefinition.toFileString 0)
        "#endif"
        }
    }
    
let writeToFile path content =
    System.IO.File.WriteAllText(path, content)
    

stringBuilder {
    ["sup"
     "dog"]
}