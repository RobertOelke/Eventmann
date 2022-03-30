namespace Eventmann.Client

open Feliz
open Feliz.Bulma

module AppMenu =

  [<ReactComponent>]
  let Render() =
    Bulma.navbar [
      color.isPrimary
      prop.children [
        Bulma.navbarBrand.div [
          Bulma.navbarItem.a [
            Html.img [ prop.src "https://bulma.io/images/bulma-logo-white.png"; prop.height 28; prop.width 112; ]
          ]
        ]
      ]
    ]
