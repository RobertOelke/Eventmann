namespace Eventmann.Server.Order

open System
open Eventmann.Shared.Order
open Eventmann.Shared.MachineType
open Kairos.Server

module OrderBehaviour =
  let handler
    (machineTypeById : EventSource -> Async<MachineType option>)
    (store : IEventStore<OrderEvent>)
    (projection : Projection<Order, OrderEvent>)
    (src : EventSource)
    (cmd : OrderCommand) =
    async {
      let! events = store.GetStream src

      match events |> Projection.project projection with
      | None ->
        match cmd with
        | PlaceOrder order ->
          match! machineTypeById order.MachineType with
          | None -> return []
          | Some mt ->
            return [
              OrderPlaced {|
                SerialNumber = order.SerialNumber
                Customer = order.Customer
                ModelName = order.ModelName
                MachineType = order.MachineType
                DeliveryDate = order.DeliveryDate
              |}
              PeriodsInitialized {|
                Construction = mt.Construction
                Sketch = mt.Sketch
                Shipping = mt.Shipping
              |}
            ]

      | Some state ->
        match cmd with
        | PlaceOrder order -> return []
    }