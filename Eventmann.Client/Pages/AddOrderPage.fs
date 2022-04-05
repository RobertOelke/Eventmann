namespace Eventmann.Client.Page

open System
open Feliz
open Feliz.Bulma
open Eventmann.Client
open Eventmann.Shared
open Eventmann.Shared.Order
open Eventmann.Shared.MachineType

module AddOrderPage =
  
  type State = {
    SerialNumber : string
    Customer : string
    MachineName : string
    MachineType : Guid
    DeliveryDate : DateTime option
  }

  let empty = {
    SerialNumber = ""
    Customer = ""
    MachineName = ""
    MachineType = Guid.Empty
    DeliveryDate = None
  }

  let loadMachineTypes setter () =
    async {
      let! types = Apis.machineType.GetAll()
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
        Bulma.label "Machine name"
        Bulma.input.text [
          prop.value state.MachineName
          prop.onTextChange (update (fun t s -> { s  with MachineName = t }))
        ]
      ]
      Bulma.field.div [
        Bulma.label "Machine type"
        Bulma.control.div [
          prop.children [
            Bulma.select [
              select.isFullWidth
              prop.value state.MachineType
              prop.onChange (update (fun (value : string) s -> { s  with MachineType = Guid value }))
              prop.children [
                yield!
                  machineTypes
                  |> List.sortByDescending(fun m -> state.MachineName.StartsWith(m.MainType))
                  |> List.map (fun m ->
                    Html.option [
                      prop.value m.Id
                      prop.text m.MainType
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
              || state.MachineName = ""
              || state.SerialNumber = ""
              || state.Customer = ""
              || state.MachineType = Guid.Empty
            )
            prop.onClick (fun _ ->
              async {
                let newOrder : NewOrder = {
                  SerialNumber = state.SerialNumber
                  Customer = state.Customer
                  MachineName = state.MachineName
                  MachineType = state.MachineType
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

