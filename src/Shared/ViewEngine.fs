module Template.ViewEngine

open Elmish

#if FABLE_COMPILER
open Feliz
open Feliz.ElmishComponents

type Html = Feliz.Html
type prop = Feliz.prop
type ReactElement = Feliz.ReactElement

#else
open Feliz.ViewEngine

type Html = Feliz.ViewEngine.Html
type prop = Feliz.ViewEngine.prop
type ReactElement = Feliz.ViewEngine.ReactElement

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

type Init<'msg, 'props,'state> = 'props -> 'state * Cmd<'msg>
type Update<'msg,'state> = 'msg -> 'state -> 'state * Cmd<'msg>
type Dispatch<'msg> = 'msg -> unit
type Render<'state, 'msg> = 'state -> Dispatch<'msg> -> ReactElement

type ElmishComponentAggregate<'msg,'props,'state> = {Init: Init<'msg,'props,'state>; Update: Update<'msg,'state>; }
type ElmishComponentFactory<'msg,'props,'state> = ElmishComponentAggregate<'msg,'props,'state> -> 'props -> ReactElement

type RootRender<'dep,'state,'msg> = 'dep -> 'state -> Dispatch<'msg> -> ReactElement

let elmishComponent name (render:Render<'state,'msg>) {Init = init; Update = update;} (props:'props) =
    React.elmishComponent(name, init props, update, render)