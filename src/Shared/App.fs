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
                    Bulma.navbar [
                        prop.children [
                            Bulma.container [
                                Bulma.notification [
                                    prop.children [
                                        Bulma.label "Hi there"
                                    ]
                                ]
                            ]
                        ]
                    ]
                ]
                Bulma.heroBody [
                    Counter.createCounter counterAggregate ()
                ]
            ]
        ]
        
            
        
        
