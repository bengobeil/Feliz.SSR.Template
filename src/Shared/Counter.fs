namespace Template

open Template.ViewEngine

[<RequireQualifiedAccess>]
module Counter =
    type State = { Count: int }

    type Msg =
        | Increment
        | Decrement
    
    let private render (state:State) (dispatch:Msg -> unit) =
        Html.div [
            Html.button [
                prop.onClick (fun _ -> dispatch Increment)
                prop.text "Increment"
            ]
    
            Html.button [
                prop.onClick (fun _ -> dispatch Decrement)
                prop.text "Decrement"
            ]
    
            Html.h1 state.Count
        ]
        
    type Factory = ElmishComponentFactory<Msg, unit, State>
    type Aggregate = ElmishComponentAggregate<Msg, unit, State>
    
    let createCounter: Factory =
        ViewEngine.elmishComponent "Counter" render
            