namespace Template


open Feliz.ViewEngine
open Template.ViewEngine
    
[<RequireQualifiedAccess>]   
module App =
   open Elmish
   
   let getDependencies initialState: App.Dependencies =
       ElmishComponentAggregate.konstAggregate initialState
       
   let toServer (initialState: Counter.State) =
        getDependencies initialState
        |> fun deps -> RootRender.getReactElement App.render deps initialState

module Server =
    
    let rand = System.Random()
    
    let initialState (): Counter.State =
        {Count = rand.Next(0,50)}
    
    let makeInitialHtml model =
        let head =
            Html.head [
                Html.title "Feliz App"
                Html.meta [
                    prop.custom ("http-equiv","Content-type")
                    prop.content "text/html; charset=utf-8"
                ]
                Html.meta [
                    prop.name "viewport"
                    prop.content "width=device-width, initial-scale=1"
                ]
                Html.link [
                    prop.rel "shortcut icon"
                    prop.type' "image/png"
                    prop.href "img/favicon-32x32.png"
                ]
                Html.link [
                    prop.rel "shortcut icon"
                    prop.type' "image/png"
                    prop.href "img/favicon-16x16.png"
                ]
                Html.link [
                    prop.rel "stylesheet"
                    prop.href "https://cdn.jsdelivr.net/npm/bulma@0.8.2/css/bulma.min.css"
                ]
                Html.script [
                    prop.src "https://cdn.polyfill.io/v2/polyfill.js?features=es6"
                ]
                Html.script [
                    prop.defer true
                    prop.src "https://use.fontawesome.com/releases/v5.3.1/js/all.js"
                ]
            ]
            
        let body =
            Html.body [
                Html.div [
                    prop.id App.name
                    prop.children [
                        App.toServer model
                    ]
                ]
                Html.script [
                    (Thoth.Json.Net.Encode.Auto.toString(0,model))
                    |> sprintf "var __INIT_STATE__ = %s" 
                    |> prop.dangerouslySetInnerHTML
                ]
            ]
            
        Html.html [
            head
            body
        ]