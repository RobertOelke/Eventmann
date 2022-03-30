namespace Eventmann.Client

open Feliz
open Feliz.Bulma
open Feliz.Router

module AppRouter =
  
  [<ReactComponent>]
  let Content (url) =
    Bulma.container [
      container.isWidescreen
      
      prop.style [
        style.marginTop 10
        style.marginBottom 10
      ]

      prop.children [
        match url with
        | [] -> Html.text "Home"
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
