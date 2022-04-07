namespace Eventmann.Shared.Order

open System

type NewOrder = {
    SerialNumber : string
    Customer : string
    ModelName : string
    VacuumType : Guid
    DeliveryDate : DateTime
}

type OrderCommand =
| PlaceOrder of NewOrder