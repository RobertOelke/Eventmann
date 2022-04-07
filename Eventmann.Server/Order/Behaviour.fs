namespace Eventmann.Server.Order

open System
open Eventmann.Shared.Order
open Kairos.Server

module OrderBehaviour =
  let handler
    (store : IEventStore<OrderEvent>)
    (projection : Projection<Order, OrderEvent>)
    (src : EventSource)
    (cmd : OrderCommand) =
    async {
      let! events = store.GetStream src

      return
        match events |> Projection.project projection with
        | None ->
          match cmd with
          | PlaceOrder order ->
            [
              OrderPlaced {|
                SerialNumber = order.SerialNumber
                Customer = order.Customer
                ModelName = order.ModelName
                VacuumType = order.VacuumType
                DeliveryDate = order.DeliveryDate
              |}
            ]
        | Some state ->
          match cmd with
          | PlaceOrder order -> []
    }