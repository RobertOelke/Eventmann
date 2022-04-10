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
        | _ -> return []

      | Some state ->
        match cmd with
        | PlaceOrder order -> return []
        | UpdateTechicalData data -> 
          return [
            for (title, value) in data |> Map.toSeq do
              let currentValue = state.State.AdditionalData |> Map.tryFind title |> Option.defaultValue ""
              if (currentValue <> value) then
                UpdatedTechnicalData {| Title = title; Value = value|}
          ]
        | FinishPlanning ->
          match state.State.CurrentPhase with
          | Pending -> return [ PlanningFinished ]
          | _ -> return []
    }