namespace Eventmann.Client.Page

open System
open Elmish
open Feliz
open Feliz.UseElmish
open Feliz.Bulma
open Eventmann.Client
open Eventmann.Shared
open Eventmann.Shared.TechnicalData
open Eventmann.Shared.Order

module NewOrderDetails =

  [<ReactComponent>]
  let Render
    (columns : TechnicalData list)
    (saveAdditionalData : (Guid * Map<string, string>) -> unit)
    (close : unit -> unit)
    (src : Guid, order : Order) =
    let state, setState = React.useState(order.AdditionalData)

    let getValue key = state |> Map.tryFind key |> Option.defaultValue ""
    let setValue key value = state |> Map.add key value |> setState
  
    React.fragment [
      Bulma.label "Technical Data"
      Bulma.table [
        table.isFullWidth
        prop.children [
          Html.thead [
            Html.tr [
              Html.th "Title"
              Html.th "Value"
            ]
          ]
          Html.tableBody [
            for col in columns do
              Html.tr [
                Html.td col.Title
                Html.td [
                  match col.Editor with
                  | EditorType.Text ->
                    Bulma.input.text [
                      input.isSmall
                      prop.value (getValue col.Title)
                      prop.onTextChange (setValue col.Title)
                    ]
                  | EditorType.Number ->
                    Bulma.input.number [
                      input.isSmall
                      prop.value (getValue col.Title)
                      prop.onTextChange (setValue col.Title)
                    ]
                ]
              ]
          ]
        ]
      ]
      Bulma.buttons [

        Bulma.button.button [
          prop.onClick (fun _ -> saveAdditionalData (src, state))
          prop.text "Save"
        ]
        Bulma.button.button [
          prop.onClick (fun _ -> close())
          prop.text "Close"
        ]
      ]
    ]

module NewOrdersPage =
  
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
  | SaveTechnicalData of (Guid * Map<string, string>)
  | OrderSaved of (Guid * Order)
  | FinishPlanning of Guid

  let init () = empty, Cmd.ofMsg LoadData

  let update msg state =
    match msg with
    | LoadData ->
      let load () =
        async {
          let! orders = Apis.order.GetOrders OrderPhase.Pending
          let! columns = Apis.technicalData.GetAll ()

          let columns = columns |> List.map (fun (k, x) -> x) |> List.filter (fun x -> not x.IsDeleted)

          return (columns, orders)
        }

      { state with IsLoading = true }, Cmd.OfAsync.perform load () DataLoaded

    | DataLoaded (td, orders) ->
      { state with IsLoading = false; TechicalColumns = td; Orders = orders }, Cmd.none

    | SelectOrder selected ->
      { state with SelectedOrder = selected }, Cmd.none

    | SaveTechnicalData (src, data) ->
      let httpCall () = 
        async {
          do! Apis.order.Update src (OrderCommand.UpdateTechicalData data)
          let! updated = Apis.order.GetBySrc src
          return (src, updated)
        }

      { state with IsLoading = true }, Cmd.OfAsync.perform httpCall () OrderSaved

    | OrderSaved (src, order) ->
      let newOrders =
        state.Orders
        |> List.map (fun (s, data) -> if s = src then (s, order) else (s, data))

      { state with IsLoading = false; Orders = newOrders; SelectedOrder = None }, Cmd.none

    | FinishPlanning src ->
      let load () =
        async {
          do! Apis.order.Update src OrderCommand.FinishPlanning
          let! orders = Apis.order.GetOrders OrderPhase.Pending
          let! columns = Apis.technicalData.GetAll ()

          let columns = columns |> List.map (fun (k, x) -> x) |> List.filter (fun x -> not x.IsDeleted)

          return (columns, orders)
        }

      { state with IsLoading = true }, Cmd.OfAsync.perform load () DataLoaded

  [<ReactComponent>]
  let Render() =
    let state, dispatch = React.useElmish(init, update)

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
            for (src, order) in state.Orders do
              Html.tr [

                prop.children [
                  Html.td [
                    prop.text order.SerialNumber
                    prop.onClick(fun _ ->
                      (src, order) |> Some |> SelectOrder |> dispatch
                    )
                  ]
                  Html.td order.Customer
                  Html.td order.ModelName
                  Html.td [
                    Bulma.button.button [
                      color.isPrimary
                      prop.text "Finish Planning"
                      prop.onClick (fun _ -> src |> FinishPlanning |> dispatch)
                    ]
                  ]
                ]
              ]
          ]
        ]
      ]
      
      match state.SelectedOrder with
      | Some selected ->
        Bulma.modal [
          modal.isActive
          prop.children [
            Bulma.modalBackground []
            Bulma.modalContent [
              Bulma.box [
                Bulma.section [
                  NewOrderDetails.Render
                    state.TechicalColumns
                    (SaveTechnicalData >> dispatch)
                    (fun () -> dispatch (SelectOrder None))
                    selected
                ]
              ]
            ]
          ]
        ]
      | None -> ()
    ]