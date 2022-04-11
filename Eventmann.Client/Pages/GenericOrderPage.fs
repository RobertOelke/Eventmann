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

module GenericOrderDelay =

  type State = {
    IsLoading : bool
    Start :  DateOnly
    End : DateOnly
    SavedStart :  DateOnly
    SavedEnd : DateOnly
    ReasonStart : string
    ReasonEnd : string
  }

  type Msg =
  | StartChanged of DateOnly
  | EndChanged of DateOnly
  | ReasonStartChanged of string
  | ReasonEndChanged of string
  | UpdateStart
  | UpdateEnd
  | StartUpdated of unit
  | EndUpdated of unit

  let init (period : TimePeriod) =
    {
      IsLoading = false
      SavedStart = period.Start
      SavedEnd = period.Start
      Start = period.Start
      End = period.End
      ReasonStart = ""
      ReasonEnd = ""
    }

  let update src (phase : OrderPhase) (msg : Msg) (state : State) =
    match msg with
    | StartChanged dt ->
      { state  with Start = dt }, Cmd.none
    | EndChanged dt ->
      { state with End = dt }, Cmd.none
    | ReasonStartChanged reason ->
      { state with ReasonStart = reason }, Cmd.none
    | ReasonEndChanged reason ->
      { state with ReasonEnd = reason }, Cmd.none
    | UpdateStart ->
      let httpCall () =
        async {
          let cmd =
            match phase with
            | Sketch -> ChangeSketchStart
            | Construction -> ChangeConstructionStart
            | _ -> ChangeShippingStart

          do! Apis.order.Update src (cmd (state.Start, state.ReasonStart))
        }
      { state with IsLoading = true }, Cmd.OfAsync.perform httpCall () StartUpdated
    | StartUpdated () ->
      { state with SavedStart = state.Start; IsLoading = false }, Cmd.none
    | UpdateEnd ->
      let httpCall () =
        async {
          let cmd =
            match phase with
            | Sketch -> ChangeSketchEnd
            | Construction -> ChangeConstructionEnd
            | _ -> ChangeShippingEnd

          do! Apis.order.Update src (cmd (state.End, state.ReasonEnd))
        }
      { state with IsLoading = true }, Cmd.OfAsync.perform httpCall () EndUpdated
    | EndUpdated () ->
      { state with SavedEnd = state.End; IsLoading = false }, Cmd.none

  [<ReactComponent>]
  let Render (phase : OrderPhase) (src : Guid) (order : Order) (close : unit -> unit) =
    let period =
      match phase with
      | Sketch -> order.SketchPeriod
      | Construction -> order.ConstructionPeriod
      | _ -> order.ShippingPeriod

    let state, dispatch = React.useElmish((fun () -> (init period), Cmd.none), update src phase)

    React.fragment [
      Bulma.label "Change Schedule"
      Bulma.field.div [
        Bulma.label "Start"
        Bulma.input.date [
          prop.value (state.Start.ToDateTime(TimeOnly.MinValue))
          prop.onChange (fun (dt : DateTime) ->
            new DateOnly(dt.Year, dt.Month, dt.Day)
            |> StartChanged
            |> dispatch
          )
        ]
        Bulma.input.text [
          prop.placeholder "Reason"
          prop.value state.ReasonStart
          prop.onChange (ReasonStartChanged >> dispatch)
        ]
      ]

      Bulma.field.div [
        Bulma.label "End"
        Bulma.input.date [
          prop.value (state.End.ToDateTime(TimeOnly.MinValue))
          prop.onChange (fun (dt : DateTime) ->
            new DateOnly(dt.Year, dt.Month, dt.Day)
            |> EndChanged
            |> dispatch
          )
        ]
        Bulma.input.text [
          prop.placeholder "Reason"
          prop.value state.ReasonEnd
          prop.onChange (ReasonEndChanged >> dispatch)
        ]
      ]

      Bulma.buttons [
        Bulma.button.button [
          prop.text "Update Start"
          prop.onClick (fun _ -> dispatch UpdateStart)
          prop.disabled ("" = state.ReasonStart || state.Start = state.SavedStart)
        ]
        Bulma.button.button [
          prop.text "Update End"
          prop.onClick (fun _ -> dispatch UpdateEnd)
          prop.disabled ("" = state.ReasonEnd || state.End = state.SavedEnd)
        ]
        Bulma.button.button [
          prop.text "Close"
          prop.onClick (fun _ -> close ())
        ]
      ]
    ]

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
      { state with IsLoading = false; TechicalColumns = td; Orders = orders; SelectedOrder = None }, Cmd.none

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
    
      match state.SelectedOrder with
      | Some (src, order) ->
        Bulma.modal [
          modal.isActive
          prop.children [
            Bulma.modalBackground []
            Bulma.modalContent [
              Bulma.box [
                Bulma.section [
                  GenericOrderDelay.Render
                    phase
                    src
                    order
                    (fun () -> LoadData |> dispatch)
                ]
              ]
            ]
          ]
        ]
      | None -> ()
    ]