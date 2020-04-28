// Learn more about F# at http://fsharp.org

open System.IO
open Feliz.ViewEngine
open Template

[<EntryPoint>]
let main _ =
    Server.initialState()
    |> Server.makeInitialHtml
    |> Render.htmlDocument
    |> fun content -> File.WriteAllText (sprintf "%s/../Server/index.html" __SOURCE_DIRECTORY__,content)
    0
    