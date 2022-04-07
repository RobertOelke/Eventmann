namespace Eventmann.Client.Page

open System
open Feliz
open Feliz.Bulma
open Eventmann.Client
open Eventmann.Shared
open Eventmann.Shared.Order
open Eventmann.Shared.VacuumType

module AddOrderPage =
  
  type State = {
    SerialNumber : string
    Customer : string
    ModelName : string
    VacuumType : Guid
    DeliveryDate : DateTime option
  }

  let empty = {
    SerialNumber = ""
    Customer = ""
    ModelName = ""
    VacuumType = Guid.Empty
    DeliveryDate = None
  }

  let loadMachineTypes setter () =
    async {
      let! types = Apis.vacuumType.GetAll()
      setter types
    } |> Async.StartImmediate

  [<ReactComponent>]
  let Render() =
    let state, setState = React.useState(empty)
    let machineTypes, setMachineTypes = React.useState([])

    React.useEffectOnce(loadMachineTypes setMachineTypes)

    let inline update map (value : 'value) =
      state |> map value |> setState
   
    React.fragment [
      Bulma.field.div [
        Bulma.label "Serial number"
        Bulma.input.text [
          prop.value state.SerialNumber
          prop.onTextChange (update (fun t s -> { s  with SerialNumber = t }))
        ]
      ]
      Bulma.field.div [
        Bulma.label "Customer"
        Bulma.input.text [
          prop.value state.Customer
          prop.onTextChange (update (fun t s -> { s  with Customer = t }))
        ]
      ]
      Bulma.field.div [
        Bulma.label "Model name"
        Bulma.input.text [
          prop.value state.ModelName
          prop.onTextChange (update (fun t s -> { s  with ModelName = t }))
        ]
      ]
      Bulma.field.div [
        Bulma.label "Vaccum type"
        Bulma.control.div [
          prop.children [
            Bulma.select [
              select.isFullWidth
              prop.value state.VacuumType
              prop.onChange (update (fun (value : string) s -> { s  with VacuumType = Guid value }))
              prop.children [
                yield!
                  machineTypes
                  |> List.map (fun m ->
                    Html.option [
                      prop.value m.Id
                      prop.textf "%s (%s)" m.Name m.Category
                    ]
                  )
                yield
                  Html.option [
                    prop.value Guid.Empty
                    prop.text "<empty>"
                  ]
              ]
            ]
          ]
        ]
      ]
      Bulma.field.div [
        Bulma.label "Delivery date"
        Bulma.input.date [
          match state.DeliveryDate with
          | Some dt -> prop.value dt
          | None -> ()
          prop.onChange (update (fun dt s -> { s  with DeliveryDate = Some dt }))
        ]
      ]
      Bulma.field.div [
        Bulma.buttons [
          Bulma.button.button [
            prop.text "Create"
            prop.disabled (
              state.DeliveryDate.IsNone
              || state.ModelName = ""
              || state.SerialNumber = ""
              || state.Customer = ""
              || state.VacuumType = Guid.Empty
            )
            prop.onClick (fun _ ->
              async {
                let newOrder : NewOrder = {
                  SerialNumber = state.SerialNumber
                  Customer = state.Customer
                  ModelName = state.ModelName
                  VacuumType = state.VacuumType
                  DeliveryDate = state.DeliveryDate.Value
                }
                do! Apis.order.PlaceOrder newOrder
                setState empty

              } |> Async.StartImmediate
            )
          ]
        ]
      ]
    ]

