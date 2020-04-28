namespace Template

module App =
    type State = Counter.State
    type Msg = Counter.Msg
    
    let init = Counter.init
    let update = Counter.update
    
    let render (state: State) (dispatch: Msg -> unit) = App.render {Init = init; Update = update} state dispatch

