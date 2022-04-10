namespace Eventmann.Server.Order

open System
open Eventmann.Shared.Order
open Kairos.Server

[<RequireQualifiedAccess>]
module Order =
  let projection : Projection<Order, OrderEvent> =
    let zero : Order = {
      SerialNumber = ""
      Customer = ""
      ModelName = ""
      MachineType = Guid.Empty
      DeliveryDate = DateOnly.MinValue

      CurrentPhase = OrderPhase.Pending
      SketchPeriod = TimePeriod.zero
      ConstructionPeriod = TimePeriod.zero
      ShippingPeriod = TimePeriod.zero
      
      AdditionalData = Map.empty
      Remarks = []
    }

    let fillPhases
      (sketch: int)
      (construction : int)
      (shipping : int)
      (order : Order) =
      let shipping = { Start = order.DeliveryDate.AddDays(-shipping); End = order.DeliveryDate }
      let construction = { Start = shipping.Start.AddDays(-construction - 1); End = shipping.Start.AddDays(-1) }
      let sketch = { Start = construction.Start.AddDays(-sketch - 1); End = construction.Start.AddDays(-1) }

      {
        order with
          ShippingPeriod = shipping
          ConstructionPeriod = construction
          SketchPeriod = sketch
      }

    let update state = function
      | OrderPlaced args -> {
        zero with
          SerialNumber = args.SerialNumber
          Customer = args.Customer
          ModelName = args.ModelName
          MachineType = args.MachineType
          DeliveryDate = args.DeliveryDate
        }

      | PeriodsInitialized args ->
        state
        |> fillPhases args.Sketch args.Construction args.Shipping

    { Zero = zero; Update = update }