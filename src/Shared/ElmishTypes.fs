namespace Feliz.ElmishTypes

open Elmish
open Feliz.Shared


type Init<'msg, 'props,'state> = 'props -> 'state * Cmd<'msg>
type Update<'msg,'state> = 'msg -> 'state -> 'state * Cmd<'msg>
type Dispatch<'msg> = 'msg -> unit
type Render<'state, 'msg> = 'state -> Dispatch<'msg> -> ReactElement

type ComponentAggregate<'msg,'props,'state> = {Init: Init<'msg,'props,'state>; Update: Update<'msg,'state>; }
type ComponentFactory<'msg,'props,'state> = ComponentAggregate<'msg,'props,'state> -> 'props -> ReactElement

type RootRender<'dep,'state,'msg> = 'dep -> 'state -> Dispatch<'msg> -> ReactElement

