namespace Eventmann.Shared.Order

open System

type Order = {
  SerialNumber : string
  Customer : string
  ModelName : string
  VacuumType : Guid
  DeliveryDate : DateTime
}