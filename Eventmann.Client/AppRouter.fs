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
        | [ "machinetype" ] -> MachineTypePage.Render()
        | [ "add-order" ] -> AddOrderPage.Render()
        | _ ->  Html.text "Not found"
      ]
    ]

  [<ReactComponent>]
  let Render() =
    let url, setUrl = React.useState(Router.currentUrl)

    React.router [
      router.onUrlChanged setUrl
      router.children (Content url)
    ]
