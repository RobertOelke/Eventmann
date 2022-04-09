namespace Eventmann.Client.Page

open System
open Elmish
open Feliz
open Feliz.UseElmish
open Feliz.Bulma
open Eventmann.Client
open Eventmann.Shared
open Eventmann.Shared.Order

module NewOrdersPage =
  
  let loadOrders setter () =
    async {
      let! orders = Apis.order.GetOrders OrderPhase.NewOrder
      orders |> Some |> setter
    } |> Async.StartImmediate

  [<ReactComponent>]
  let Render() =
    let selectedRow, setSelectedRow = React.useState(None)
    let orders, setOrders = React.useState(None)
    React.useEffectOnce(loadOrders setOrders)

    match orders with
    | None ->
      React.fragment []
    | Some lst ->
      Bulma.table [
        table.isFullWidth
        prop.children [
          Html.thead [
            Html.tr [
              Html.th "Serial number"
              Html.th "Customer"
              Html.th "Model"
            ]
          ]
          Html.tableBody [
            for order in lst do
              Html.tr [
                if (Some order = selectedRow) then
                  prop.className "is-selected"
                prop.onClick(fun _ -> order |> Some |> setSelectedRow)

                prop.children [
                  Html.td order.SerialNumber
                  Html.td order.Customer
                  Html.td order.ModelName
                ]
              ]
          ]
        ]
      ]