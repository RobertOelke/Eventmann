namespace Eventmann.Client.Page

open System
open Elmish
open Feliz
open Feliz.UseElmish
open Feliz.Bulma
open Eventmann.Client
open Eventmann.Shared
open Eventmann.Shared.MachineType
open Eventmann.Shared.Order

module CompletedOrdersPage =

  let load setter () =
    async {
      let! res = Apis.order.GetOrders OrderPhase.Completed
      setter res
    }
    |> Async.StartImmediate

  [<ReactComponent>]
  let Render() =
    let history, setHistory = React.useState(None)
    let orders, setOrders = React.useState([])

    React.useEffectOnce(load setOrders)
    
    React.fragment [
      Bulma.table [
        table.isFullWidth
        prop.children [
          Html.thead [
            Html.tr [
              Html.th "Serial number"
              Html.th "Customer"
              Html.th "Model"
              Html.th ""
            ]
          ]
          Html.tableBody [
            for (src, order) in orders do
              Html.tr [

                prop.children [
                  Html.td [
                    prop.text order.SerialNumber
                  ]
                  Html.td order.Customer
                  Html.td order.ModelName
                
                  Html.td [
                    Bulma.button.button [
                      color.isPrimary
                      prop.text "Show History"
                      prop.onClick (fun _ -> 
                        async {
                          let! log = Apis.order.GetLog src
                          setHistory (Some log)
                        } |> Async.StartImmediate 
                      )
                    ]
                  ]
                ]
              ]
          ]
        ]
      ]

      match history with
      | None -> ()
      | Some log ->
        Bulma.modal [
          modal.isActive
          prop.children [
            Bulma.modalBackground []
            Bulma.modalContent [
              prop.style [ style.width (length.vw 90)]
              prop.children [
                Bulma.box [
                  Bulma.section (log |> List.map (fun l -> Html.div [ Html.textf "%s- %s:  %s" (l.Date.ToShortDateString()) (l.Date.ToShortTimeString()) l.Action ]))
                  Bulma.field.div [
                    Bulma.buttons [
                      Bulma.button.button [
                        prop.text "Close"
                        prop.onClick (fun _ -> setHistory None)
                      ]
                    ]
                  ]
                ]
              ]
            ]
          ]
        ]
    ]