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

| UpdateTechicalData of Map<string, string>
| FinishPlanning

| ChangeSketchStart of Date:DateOnly * Reason:string
| ChangeSketchEnd of Date:DateOnly * Reason:string
| FinishSketch

| ChangeConstructionStart of Date:DateOnly * Reason:string
| ChangeConstructionEnd of Date:DateOnly * Reason:string
| FinishConstruction

| ChangeShippingStart of Date:DateOnly * Reason:string
| ChangeShippingEnd of Date:DateOnly * Reason:string
| FinishShipping