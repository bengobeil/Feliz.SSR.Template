namespace Template

module Main =

    open Fable.Core.JsInterop

    importAll "../../styles/main.scss"

    open Elmish
    open Elmish.React
    
    #if DEBUG
    open Elmish.Debug
    open Elmish.HMR
    #endif

    // App
    Program.mkProgram App.init App.update Shared.render
    #if DEBUG
    |> Program.withDebugger
    #endif
    |> Program.withReactSynchronous "feliz-app"
    |> Program.run