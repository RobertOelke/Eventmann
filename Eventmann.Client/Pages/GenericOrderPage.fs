namespace Eventmann.Client.Page

open System
open Elmish
open Feliz
open Feliz.UseElmish
open Feliz.Bulma
open Eventmann.Client
open Eventmann.Shared
open Eventmann.Shared.TechnicalData
open Eventmann.Shared.MachineType
open Eventmann.Shared.Order

module GenericOrderPage =

  type State = {
    IsLoading : bool
    TechicalColumns : TechnicalData list
    Orders : (Guid * Order) list
    SelectedOrder : (Guid * Order) option
  }

  let empty = {
    IsLoading = false
    TechicalColumns = []
    Orders = []
    SelectedOrder = None
  }

  type Msg =
  | LoadData
  | DataLoaded of TechnicalData list * (Guid * Order) list
  | SelectOrder of (Guid * Order) option
  | FinishPhase of Guid

  let init () = empty, Cmd.ofMsg LoadData

  let update phase msg state =
    match msg with
    | LoadData ->
      let load () =
        async {
          let! orders = Apis.order.GetOrders phase
          let! columns = Apis.technicalData.GetAll ()

          let columns = columns |> List.map (fun (k, x) -> x) |> List.filter (fun x -> not x.IsDeleted)

          return (columns, orders)
        }

      { state with IsLoading = true }, Cmd.OfAsync.perform load () DataLoaded

    | DataLoaded (td, orders) ->
      { state with IsLoading = false; TechicalColumns = td; Orders = orders }, Cmd.none

    | SelectOrder selected ->
      { state with SelectedOrder = selected }, Cmd.none

    | FinishPhase src ->
      let load () =
        async {
          let cmd =
            match phase with
            | Sketch -> FinishSketch
            | Construction -> FinishConstruction
            | _ -> FinishShipping

          do! Apis.order.Update src cmd
          let! orders = Apis.order.GetOrders phase
          let! columns = Apis.technicalData.GetAll ()

          let columns = columns |> List.map (fun (k, x) -> x) |> List.filter (fun x -> not x.IsDeleted)

          return (columns, orders)
        }

      { state with IsLoading = true }, Cmd.OfAsync.perform load () DataLoaded

  [<ReactComponent>]
  let Render phase =
    let state, dispatch = React.useElmish(init, update phase)

    React.fragment [
      Bulma.table [
        table.isFullWidth
        prop.children [
          Html.thead [
            Html.tr [
              Html.th "Serial number"
              Html.th "Customer"
              Html.th "Model"
              Html.th "Scheduled"
              for col in state.TechicalColumns do
                Html.th col.Title
                
              Html.th ""
            ]
          ]
          Html.tableBody [
            for (src, order) in state.Orders do
              Html.tr [

                prop.children [
                  Html.td [
                    prop.text order.SerialNumber
                  ]
                  Html.td order.Customer
                  Html.td order.ModelName
                  Html.td [
                    let printDate (start : DateOnly) (endDt : DateOnly) =
                      sprintf "%i.%i.%i - %i.%i.%i" start.Day start.Month start.Year endDt.Day endDt.Month endDt.Year

                    prop.onClick(fun _ ->
                      (src, order) |> Some |> SelectOrder |> dispatch
                    )
                    prop.text (
                      match phase with
                      | Sketch -> printDate order.SketchPeriod.Start order.SketchPeriod.End
                      | Construction -> printDate order.ConstructionPeriod.Start order.ConstructionPeriod.End
                      | _ -> printDate order.ShippingPeriod.Start order.ShippingPeriod.End
                    )
                  ]
                  
                  for col in state.TechicalColumns do
                    Html.td (order.AdditionalData |> Map.tryFind col.Title |> Option.defaultValue "")
                
                  Html.td [
                    Bulma.button.button [
                      color.isPrimary
                      prop.textf "Finish %A" phase
                      prop.onClick (fun _ -> src |> FinishPhase |> dispatch)
                    ]
                  ]
                ]
              ]
          ]
        ]
      ]
    ]