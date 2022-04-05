namespace Eventmann.Shared.Order

open System

type NewOrder = {
    SerialNumber : string
    Customer : string
    MachineName : string
    MachineType : Guid
    DeliveryDate : DateTime
}

type OrderCommand =
| PlaceOrder of NewOrder