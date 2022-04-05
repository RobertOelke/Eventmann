namespace Eventmann.Client

open Feliz
open Feliz.Router

module App =

  [<ReactComponent>]
  let Render() =
    let url, setUrl = React.useState(Router.currentUrl)

    React.fragment [
      AppMenu.Render url
      AppRouter.Render url setUrl
      AppFooter.Render()
    ]
