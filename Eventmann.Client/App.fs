namespace Eventmann.Client

open Feliz

module App =

  [<ReactComponent>]
  let Render() =
    React.fragment [
      AppMenu.Render()
      AppRouter.Render()
      AppFooter.Render()
    ]
