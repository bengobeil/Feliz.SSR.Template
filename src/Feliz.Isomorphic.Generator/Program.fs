// Learn more about F# at http://fsharp.org

open System
open System.Reflection

[<RequireQualifiedAccess>]
module Seq =
    let exclude f =
        Seq.filter (f >> not)

type DiscoverableAssembly = {Name: string; Exclusions: string list }
type IsomorphicAssemblyPair = {Client: DiscoverableAssembly; Server: DiscoverableAssembly}

type DiscoveredType =
    | Type of typeName: string
    | InnerModuleType of fullModuleName:string * type': DiscoveredType 
    
type DiscoveredModule =
    | RootType of typeName: string
    | Module of moduleName: string * types: DiscoveredModule list
    

    
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

let (|Type|InnerModuleType|InnerInnerModuleType|Ignore|) (typeName: string) =
    let removePlus (str:string) = str.TrimStart '+'
    
    match typeName.Split "Module" with
    | [| _; _; "" |] ->
        Ignore
    | [| moduleFullName; innerModuleName; typeName |] ->
        InnerInnerModuleType (moduleFullName, removePlus innerModuleName, removePlus typeName)
    | [| _; "" |] ->
        Ignore
    | [| moduleFullName; typeName |] ->
        InnerModuleType (moduleFullName,removePlus typeName)
    | [| x |] -> Type x
    | _ ->
        printf "%s" typeName
        failwithf "Not expected"

let findValidTypes {Name = name; Exclusions = exclusions } = 
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
//    |> Seq.iter (printfn "%s")
    |> Seq.choose (function
        | Ignore -> None
        | Type typeName -> Some <| Type typeName
        | InnerModuleType (moduleName, typeName) -> Some <| InnerModuleType (moduleName, Type(sprintf "%s.%s" moduleName typeName))
        | InnerInnerModuleType (qualifiedName, moduleName, typeName) ->
            Some <| InnerModuleType (qualifiedName, InnerModuleType (sprintf "%s.%s" qualifiedName moduleName, Type(sprintf "%s.%s.%s" qualifiedName moduleName typeName))))
    |> List.ofSeq
    |> DiscoveredModule.toDiscoveredModule
    |> List.iter (printfn "%A")
    
[<EntryPoint>]
let main argv =
    let felizBulmaViewEngine =
        { Name = "Feliz.Bulma.ViewEngine"
          Exclusions = [
              "Feliz.Bulma.ViewEngine.ElementBuilders"
              "Feliz.Bulma.ViewEngine.PropertyBuilders"
              "Feliz.Bulma.ViewEngine.ClassLiterals"
              "Feliz.Bulma.ViewEngine.Operators"
              "Feliz.Bulma.ViewEngine.ElementLiterals"
          ]}
        
    findValidTypes felizBulmaViewEngine
    0
