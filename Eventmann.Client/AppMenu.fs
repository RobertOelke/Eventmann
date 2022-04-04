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
        Bulma.navbarMenu [
          Bulma.navbarStart.div [
            let link (t : string) (url : string array) =
              Bulma.navbarItem.a [
                prop.text t
                prop.onClick (fun _ -> Router.Router.navigate url)
              ]

            link "Machine Types" [| "machinetype" |]
            link "Add Order" [| "add-order" |]
          ]
        ]
      ]
    ]
