namespace Eventmann.Client

open Feliz
open Feliz.Bulma
open Feliz.Router

module AppMenu =

  [<ReactComponent>]
  let Render (currentUrl : string list) =
    Bulma.navbar [
      color.isPrimary
      prop.children [
        Bulma.navbarBrand.div [
          Bulma.navbarItem.a [
            Html.img [ prop.src "https://bulma.io/images/bulma-logo-white.png"; prop.height 28; prop.width 112; ]
          ]
        ]
        Bulma.navbarMenu [
          let link (t : string) (url : string array) =
            Bulma.navbarItem.a [
              if (List.ofArray url) = currentUrl then
                navbarItem.isActive

              prop.text t
              prop.onClick (fun _ -> Router.Router.navigate url)
            ]
          Bulma.navbarStart.div [
            link "Vacuum Types" [| "vacuum-type" |]
            link "Add Order" [| "add-order" |]
            link "New Orders" [| "new-orders" |]
            link "Sketch" [| "sketch" |]
            link "Construction" [| "construction" |]
            link "Montage" [| "montage" |]
            link "Shipping" [| "shipping" |]
            link "Completed Orders" [| "completed-orders" |]
          ]
        ]
      ]
    ]
