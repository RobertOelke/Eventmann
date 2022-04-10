namespace Eventmann.Server.TechnicalData

open Eventmann.Shared.TechnicalData

type TechnicalDataEvent =
| Created of {| Title : string; Editor : EditorType |}
| Deleted