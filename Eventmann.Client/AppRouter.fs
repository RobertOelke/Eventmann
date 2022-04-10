namespace Eventmann.Client

open Feliz
open Feliz.Bulma
open Feliz.Router
open Eventmann.Client.Page

module AppRouter =
  
  [<ReactComponent>]
  let Content (url) =
    Bulma.section [
      prop.style [
        style.flexGrow 1
      ]

      prop.children [
        match url with
        | [] -> Html.text "Home"
        | [ "machine-type" ] -> MachineTypePage.Render()
        | [ "add-order" ] -> AddOrderPage.Render()
        | [ "new-orders" ] -> NewOrdersPage.Render()
        | [ "sketch" ] -> SketchPage.Render()
        | [ "construction" ] -> ConstructionPage.Render()
        | [ "shipping" ] -> ShippingPage.Render()
        | [ "completed-orders" ] -> CompletedOrdersPage.Render()
        | [ "technical-data" ] -> TechnicalDataPage.Render()
        | _ ->  Html.text "Not found"
      ]
    ]

  [<ReactComponent>]
  let Render url setUrl =
    React.router [
      router.onUrlChanged setUrl
      router.children (Content url)
    ]
