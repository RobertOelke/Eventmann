namespace Eventmann.Shared.Order

open System

type TimePeriod = {
  Start : DateOnly
  End : DateOnly
}

module TimePeriod =
  let zero = { Start = DateOnly.MinValue; End = DateOnly.MinValue }

type OrderPhase =
| Pending
| Sketch
| Construction
| Shipping
| Completed

type Order = {
  SerialNumber : string
  Customer : string
  ModelName : string
  MachineType : Guid
  DeliveryDate : DateOnly
  SketchPeriod : TimePeriod
  ConstructionPeriod : TimePeriod
  ShippingPeriod : TimePeriod
  CurrentPhase : OrderPhase
  AdditionalData : Map<string, string>
  Remarks : string list
}

type OrderHistory = {
  Date : DateTime
  Action : string
}