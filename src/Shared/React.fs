namespace Feliz.Shared

#if FABLE_COMPILER
open Feliz
open Feliz.ElmishComponents
#else
open Elmish
open Feliz.ViewEngine

type React =
    static member useReducer<'msg,'state> (x:'msg -> unit , y: 'state) =
        (y,x)
    
    static member elmishComponent (name:string, init: 'state * Cmd<'msg>, update: 'msg -> 'state -> 'state * Cmd<'msg>, render: 'state -> ('msg -> unit) -> ReactElement) =
        render (fst init) ignore
        |> function
            | Element (_,propsList) -> Element (name,propsList)
            | VoidElement (_,propsList) -> VoidElement (name,propsList)
            | TextElement content -> TextElement content
    
#endif
open Feliz.ElmishTypes

module React = 
    let elmishComponent' name (render:Render<'state,'msg>) {Init = init; Update = update;} (props:'props) =
        React.elmishComponent(name, init props, update, render)
