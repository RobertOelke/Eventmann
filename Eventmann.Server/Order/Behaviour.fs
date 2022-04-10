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
              ScheduleInitialized {|
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

        | ChangeSketchStart (date, reason) ->
          if (state.State.SketchPeriod.End < date && state.State.SketchPeriod.Start <> date) then
            return [ SketchStartChanged {|Date = date; Reason = reason |} ]
          else
            return []

        | ChangeSketchEnd (date, reason) ->
          if (state.State.SketchPeriod.Start > date && state.State.SketchPeriod.End <> date) then
            return [ SketchEndChanged {|Date = date; Reason = reason |} ]
          else
            return []

        | FinishSketch ->
          match state.State.CurrentPhase with
          | Sketch -> return [ SketchFinished ]
          | _ -> return []
          
        | ChangeConstructionStart (date, reason) ->
          if (state.State.ConstructionPeriod.End < date && state.State.ConstructionPeriod.Start <> date) then
            return [ ConstructionStartChanged {|Date = date; Reason = reason |} ]
          else
            return []
            
        | ChangeConstructionEnd (date, reason) ->
          if (state.State.ConstructionPeriod.Start > date && state.State.ConstructionPeriod.End <> date) then
            return [ ConstructionEndChanged {|Date = date; Reason = reason |} ]
          else
            return []
            
        | FinishConstruction ->
          match state.State.CurrentPhase with
          | Construction -> return [ ConstructionFinished ]
          | _ -> return []
          
        | ChangeShippingStart (date, reason) ->
          if (state.State.ShippingPeriod.End < date && state.State.ShippingPeriod.Start <> date) then
            return [ ShippingStartChanged {|Date = date; Reason = reason |} ]
          else
            return []
            
        | ChangeShippingEnd (date, reason) ->
          if (state.State.ShippingPeriod.Start > date && state.State.ShippingPeriod.End <> date) then
            return [ ShippingEndChanged {|Date = date; Reason = reason |} ]
          else
            return []
            
        | FinishShipping ->
          match state.State.CurrentPhase with
          | Shipping -> return [ ShippingFinished ]
          | _ -> return []
    }