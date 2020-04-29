// Learn more about F# at http://fsharp.org


open Feliz.Isomorphic.Generator


        
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
