// Learn more about F# at http://fsharp.org

open System.Reflection

[<RequireQualifiedAccess>]
module Seq =
    let exclude f =
        Seq.filter (f >> not)

type DiscoverableAssembly = {Name: string; Exclusions: string list; IncludeAliases: string list}
type IsomorphicAssemblyPair = {Client: DiscoverableAssembly; Server: DiscoverableAssembly}

type DiscoveredType =
    | Type of typeName: string
    | InnerModuleType of fullModuleName:string * type': DiscoveredType 
    
type DiscoveredModule<'a> =
    | RootType of typeName: 'a
    | Module of moduleName: 'a * types: DiscoveredModule<'a> list

    
module DiscoveredType =
    let separate items =
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
        
module DiscoveredModule =
    let rec toDiscoveredModule (types: DiscoveredType list) =
        types
        |> DiscoveredType.separate
        |> fun (rootTypes,x) -> (List.map RootType rootTypes, x)
        |> fun (x,moduleTypes) ->
            moduleTypes
            |> List.groupBy fst
            |> List.map (fun (moduleName, innerModuleTypes) ->
                innerModuleTypes
                |> List.map snd
                |> fun types -> Module (moduleName, toDiscoveredModule types))
            |> fun modules -> x @ modules
            
    let content = function
        | RootType s -> s
        | Module (m,_) -> m
        
    let rec map f = function
        | RootType s -> RootType (f s)
        | Module (m,x) -> Module (f m,x |> List.map (map f))

let (|Type|InnerModuleType|InnerInnerModuleType|Ignore|) (typeName: string) =
    let trimModule (str: string) = str.Split "Module" |> Array.head
    
    match typeName.Split "+" with
    | arr when Array.last arr = "" -> Ignore
    | [| moduleFullName; innerModuleName; typeName |] ->
        InnerInnerModuleType (trimModule moduleFullName, trimModule innerModuleName, typeName)
    | [| moduleFullName; typeName |] ->
        InnerModuleType (trimModule moduleFullName , typeName)
    | [| x |] when x.EndsWith "Module" -> Ignore
    | [| x |] -> Type x
    | _ ->
        printf "%s" typeName
        failwithf "Not expected"

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
    
let discoverModules types =
    types
    |> Seq.choose (function
        | Ignore -> None
        | Type typeName -> Some <| Type typeName
        | InnerModuleType (moduleName, typeName) -> Some <| InnerModuleType (moduleName, Type(sprintf "%s.%s" moduleName typeName))
        | InnerInnerModuleType (qualifiedName, moduleName, typeName) ->
            Some <| InnerModuleType (qualifiedName, InnerModuleType (sprintf "%s.%s" qualifiedName moduleName, Type(sprintf "%s.%s.%s" qualifiedName moduleName typeName))))
    |> List.ofSeq
    |> DiscoveredModule.toDiscoveredModule
    
let discoverModulesFromAssembly = findValidTypes >> discoverModules

module ViewEngine =
    let toClientName (str: string) =
        str.Replace(".ViewEngine","")
        
type Presence<'a> =
    | InBoth of client: 'a * server: 'a
    | ClientOnly of 'a
    | ServerOnly of 'a

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
            
    let clientModules =
        clientDiscoverableAssembly
        |> discoverModulesFromAssembly
    
    let serverModules =
        serverDiscoverableAssembly
        |> discoverModulesFromAssembly
    
    zipClientServerTypes clientModules serverModules
        
[<EntryPoint>]
let main argv =
    let felizBulma = 
        { Name = "Feliz.Bulma"
          Exclusions = [
              "Feliz.Bulma.ElementBuilders"
              "Feliz.Bulma.PropertyBuilders"
              "Feliz.Bulma.ClassLiterals"
              "Feliz.Bulma.Operators"
              "Feliz.Bulma.ElementLiterals"
          ]
          IncludeAliases = [] }
    
    let felizBulmaViewEngine =
        { Name = "Feliz.Bulma.ViewEngine"
          Exclusions = [
              "Feliz.Bulma.ViewEngine.ElementBuilders"
              "Feliz.Bulma.ViewEngine.PropertyBuilders"
              "Feliz.Bulma.ViewEngine.ClassLiterals"
              "Feliz.Bulma.ViewEngine.Operators"
              "Feliz.Bulma.ViewEngine.ElementLiterals"
          ]
          IncludeAliases = []}
        
    let feliz = {
        Name = "Feliz"
        Exclusions = [
            
        ]
        IncludeAliases = [
            "Feliz.ReactElement"
        ]
    }
    
    let felizViewEngine = {
        Name = "Feliz.ViewEngine"
        Exclusions = [
            
        ]
        IncludeAliases = []
    }
    
    let felizPair =
        { Client = feliz
          Server = felizViewEngine }
    
    let felizBulmaPair =
        { Client = felizBulma
          Server = felizBulmaViewEngine }
    
        
    discoverModulesForPair felizPair
    |> List.iter (printfn "%A")
    0
