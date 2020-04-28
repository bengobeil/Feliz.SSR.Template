namespace Template

open Elmish
open Feliz.ElmishTypes


[<RequireQualifiedAccess>]
module Init =
    let addCmdNone state = (state,Cmd.none)
    let konst state = fun _ -> state

module ViewEngine =
    let private ignoreUpdate (_:'msg) (state: 'state): 'state * Cmd<'msg>  =
        state,Cmd.none
        
    type ServerElmishComponentAggregate<'msg,'props,'state> = {Init: Init<'msg,'props,'state>;}
    
    [<RequireQualifiedAccess>]
    module ServerElmishComponentAggregate =
        let fromInit init =
            {Init = init}
    
    [<RequireQualifiedAccess>]
    module ElmishComponentAggregate =
        let private fromServer ({Init = init}: ServerElmishComponentAggregate<_, 'props,'state>): ComponentAggregate<_,'props,'state> =
            {Init = init; Update = ignoreUpdate}
        
        let konstAggregate state =
            Init.addCmdNone state
            |> Init.konst
            |> ServerElmishComponentAggregate.fromInit
            |> fromServer
            
[<RequireQualifiedAccess>]
module RootRender =
    let getReactElement (applicationRender: RootRender<'dep,'state,_>) deps initialState =
        applicationRender deps initialState ignore

