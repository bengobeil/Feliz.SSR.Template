﻿namespace Template

open Feliz

module Shared =
    type State = { Count: int }

    type Msg =
        | Increment
        | Decrement
        
    let render (state: State) (dispatch: Msg -> unit) =
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