namespace Template

open Shared

open Fable.Core.JsInterop

module App =
    let init () : State =
        let model : State = Browser.Dom.window?__INIT_STATE__
        model

    let update (state: State) = function
        | Increment -> { state with Count = state.Count + 1 }
        | Decrement -> { state with Count = state.Count - 1 }


