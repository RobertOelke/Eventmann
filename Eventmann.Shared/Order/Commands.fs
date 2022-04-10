namespace Eventmann.Shared.Order

open System


type NewOrder = {
  SerialNumber : string
  Customer : string
  ModelName : string
  MachineType : Guid
  DeliveryDate : DateOnly
}

type OrderCommand =
| PlaceOrder of NewOrder
// | FinishPlanning
// | StartSketch
// | FinishSketch
// 
// | StartConstruction
// | FinishConstruction
// 
// | StartShipping
// | FinishShipping