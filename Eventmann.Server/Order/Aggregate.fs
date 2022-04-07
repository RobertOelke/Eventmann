namespace Eventmann.Server.Order

open System
open Eventmann.Shared.Order
open Kairos.Server

type OrderEvent =
| OrderPlaced of {| SerialNumber : string; Customer : string; MachineName : string; MachineType : EventSource; DeliveryDate : DateTime |}

type Order = {
  SerialNumber : string
  Customer : string
  MachineName : string
  MachineType : EventSource
  DeliveryDate : DateTime
}

module Order =
  let defaultProjection : Projection<Order, OrderEvent> =
    let zero = {
      SerialNumber = ""
      Customer = ""
      MachineName = ""
      MachineType = Guid.Empty
      DeliveryDate = DateTime.MinValue
    }

    let update state = function
      | OrderPlaced args -> {
        zero with
          SerialNumber = args.SerialNumber
          Customer = args.Customer
          MachineName = args.MachineName
          MachineType = args.MachineType
          DeliveryDate = args.DeliveryDate
        }

    { Zero = zero; Update = update }

module OrderCommand =
  let commandHandler (state : Order option) (cmd : OrderCommand) =
    match state with
    | None ->
      match cmd with
      | PlaceOrder order ->
        [
          OrderPlaced {|
            SerialNumber = order.SerialNumber
            Customer = order.Customer
            MachineName = order.ModelName
            MachineType = order.VacuumType
            DeliveryDate = order.DeliveryDate
          |}
        ]
      // | _ -> []

    | Some state ->
      match cmd with
      | PlaceOrder _ -> []