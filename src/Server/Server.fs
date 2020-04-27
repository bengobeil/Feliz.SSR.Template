﻿namespace Template

open Feliz

module Say =
    let rand = System.Random()
    
    let initialState () :Shared.State =
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
                Html.script [
                    prop.src "https://cdn.polyfill.io/v2/polyfill.js?features=es6"
                ]
            ]
            
        let body =
            Html.body [
                Html.div [
                    prop.id "feliz-app"
                    prop.children [
                        Shared.render model ignore
                    ]
                ]
                Html.script [
                    sprintf "var __INIT_STATE__ = %s" (Thoth.Json.Encode.Auto.toString(0,model))
                    |> prop.text
                ]
            ]
            
        Html.html [
            head
            body
        ]
