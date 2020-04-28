namespace Template

[<RequireQualifiedAccess>]
module App =
    let [<Literal>] name = "feliz-app"
    
    type Dependencies = Counter.Aggregate
    
    let render (counterAggregate: Dependencies) _ _ =
        Counter.createCounter counterAggregate ()
