namespace Template

open Elmish

open Fable.Core.JsInterop

module Counter =
    type State = Counter.State
    type Msg = Counter.Msg
    
    let init () : State * Cmd<Msg> =
        let model : State = Browser.Dom.window?__INIT_STATE__
        model, Cmd.none

    let update (msg:Msg) (state: State) =
        match msg with
        | Counter.Increment -> { state with Count = state.Count + 1 },Cmd.none
        | Counter.Decrement -> { state with Count = state.Count - 1 },Cmd.none


