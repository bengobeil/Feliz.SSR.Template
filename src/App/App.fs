namespace Template

open Elmish
open Shared

open Fable.Core.JsInterop

module App =
    let init () : State * Cmd<Msg> =
        let model : State = Browser.Dom.window?__INIT_STATE__
        model, Cmd.none

    let update (msg: Msg) (state: State) =
        match msg with
        | Increment -> { state with Count = state.Count + 1 }, Cmd.none
        | Decrement -> { state with Count = state.Count - 1 }, Cmd.none


