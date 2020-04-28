namespace Template

#if FABLE_COMPILER
open Feliz
#else
open Feliz.ViewEngine

type React =
    static member useReducer<'msg,'state> ((x:'msg -> unit) , (y: 'state)) =
        (y,x)
#endif

module Shared =
    type State = { Count: int }

    type Msg =
        | Increment
        | Decrement
        
    let render' initState update = React.functionComponent("test", fun () ->
        let (state, dispatch) = React.useReducer(update, initState)
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
    )