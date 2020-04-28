namespace Template

module Main =
    open Fable.Core.JsInterop
    
    #if FABLE_COMPILER
    importAll "../../styles/main.scss"
    
    open Browser.Dom
    open Feliz

//    let initState = App.init ()
//    ReactDOM.render(Shared.render' initState App.update (), document.getElementById "feliz-app")
    #endif
