namespace Eventmann.Server.Order

open System
open Kairos.Server

type OrderEvent =
| OrderPlaced of {| SerialNumber : string; Customer : string; ModelName : string; MachineType : EventSource; DeliveryDate : DateOnly |}
| PeriodsInitialized of {| Sketch : int; Construction : int; Shipping : int; |}
// | PlanningFinished
// | SketchStarted
// | SketchFinishedOnTime
// | SketchFinishedDelayed of {| Days : int |}
// | ConstructionStarted
// | ConstructionFinishedOnTime
// | ConstructionFinishedDelayed of {| Days : int |}