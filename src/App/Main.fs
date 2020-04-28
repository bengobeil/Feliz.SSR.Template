namespace Template

#if FABLE_COMPILER
module Main =
    open Fable.Core.JsInterop
    
    importAll "../../styles/main.scss"
    
    open Feliz

//    let initState = App.init ()
//    ReactDOM.render(Shared.render' initState App.update (), document.getElementById "feliz-app")
    
    open Elmish
    open Elmish.React
    #if DEBUG
    open Elmish.HMR
    open Elmish.Debug
    #endif
    
    // App
    Program.mkProgram App.init App.update App.render
    #if DEBUG
    |> Program.withDebugger
    |> Program.withReactSynchronous App.name
    #else
    |> Program.withReactHydrate App.name
    #endif
    |> Program.run
#endif
