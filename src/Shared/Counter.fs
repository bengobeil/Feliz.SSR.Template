namespace Template

[<RequireQualifiedAccess>]
module Counter =
    open Feliz.Shared
    open Feliz.ElmishTypes
    
    type State = { Count: int }

    type Msg =
        | Increment
        | Decrement
    
    let private render (state:State) (dispatch:Msg -> unit) =
        Bulma.container [
            button.button [
                color.isBlack
                prop.onClick (fun _ -> dispatch Increment)
                prop.text "Increment"
            ]
    
            button.button [
                prop.onClick (fun _ -> dispatch Decrement)
                prop.text "Decrement"
            ]
    
            Html.h1 state.Count
        ]
        
    type Factory = ComponentFactory<Msg, unit, State>
    type Aggregate = ComponentAggregate<Msg, unit, State>
    
    let createCounter: Factory =
        React.elmishComponent' "Counter" render
            