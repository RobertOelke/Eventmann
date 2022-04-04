namespace Eventmann.Client


open Feliz
open Feliz.Bulma


module AppFooter =

  [<ReactComponent>]
  let Render() =
    Bulma.container [
      prop.style [
        style.marginTop 10
        style.marginBottom 10
        style.flexGrow 0
      ]
      prop.children [
        Html.text "Eventmann - Version 0.1"
      ]
    ]