namespace Feliz.Shared

#if FABLE_COMPILER
open Feliz
open Feliz.Bulma
open Feliz.ElmishComponents

type Html = Feliz.Html
type prop = Feliz.prop
type ReactElement = Feliz.ReactElement
type Bulma = Feliz.Bulma.Bulma
type button = Feliz.Bulma.Bulma.button
type color = Feliz.Bulma.color

#else

type Html = Feliz.ViewEngine.Html
type prop = Feliz.ViewEngine.prop
type ReactElement = Feliz.ViewEngine.ReactElement
type Bulma = Feliz.Bulma.ViewEngine.Bulma
type button = Feliz.Bulma.ViewEngine.Bulma.button
type color = Feliz.Bulma.ViewEngine.color

#endif

