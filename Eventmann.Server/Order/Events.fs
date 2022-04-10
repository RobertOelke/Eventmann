namespace Eventmann.Server.Order

open System
open Kairos.Server

type OrderEvent =
| OrderPlaced of {| SerialNumber : string; Customer : string; ModelName : string; MachineType : EventSource; DeliveryDate : DateOnly |}
| ScheduleInitialized of {| Sketch : int; Construction : int; Shipping : int; |}

| UpdatedTechnicalData of {| Title : string; Value : string |}
| PlanningFinished

| SketchStartChanged of {| Date:DateOnly; Reason:string |}
| SketchEndChanged of {| Date:DateOnly; Reason:string |}
| SketchFinished

| ConstructionStartChanged of {| Date:DateOnly; Reason:string |}
| ConstructionEndChanged of {| Date:DateOnly; Reason:string |}
| ConstructionFinished

| ShippingStartChanged of {| Date:DateOnly; Reason:string |}
| ShippingEndChanged of {| Date:DateOnly; Reason:string |}
| ShippingFinished
