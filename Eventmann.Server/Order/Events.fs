namespace Eventmann.Server.Order

open System
open Kairos.Server

type OrderEvent =
| OrderPlaced of {| SerialNumber : string; Customer : string; ModelName : string; VacuumType : EventSource; DeliveryDate : DateTime |}