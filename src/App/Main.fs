namespace Template

module Main =
    
    #if FABLE_COMPILER
    open Fable.Core.JsInterop

    importAll "../../styles/main.scss"
    
    open Browser.Dom
    open Feliz

//    let initState = App.init ()
//    ReactDOM.render(Shared.render' initState App.update (), document.getElementById "feliz-app")
    #endif
