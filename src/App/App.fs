namespace Template

open Elmish
open Shared

module App =
    let init() = { Count = 0 }, Cmd.none

    let update (msg: Msg) (state: State) =
        match msg with
        | Increment -> { state with Count = state.Count + 1 }, Cmd.none
        | Decrement -> { state with Count = state.Count - 1 }, Cmd.none


