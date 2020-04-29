namespace Template

[<RequireQualifiedAccess>]
module App =
    open Feliz.Shared
    
    let [<Literal>] name = "feliz-app"
    
    type Dependencies = Counter.Aggregate
    
    let render (counterAggregate: Dependencies) _ _ =
        Bulma.hero [
            color.isPrimary
            prop.children [
                Bulma.heroHead [
                    prop.children [
                        Bulma.navbar[
                        ]
                    ]
                ]
                Bulma.heroBody [
                    Counter.createCounter counterAggregate ()
                ]
            ]
        ]
        
            
        
        
