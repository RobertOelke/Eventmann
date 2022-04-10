namespace Eventmann.Shared.MachineType

type MachineTypeCommand =
| Create of Categorie : string * Name : string
| AddDescription of Description : string
| RemoveDescription of Description : string
| ChangeColour of Colour : string
| ChangeSketchDuration of int
| ChangeConstructionDuration of int
| ChangeShippingDuration of int
| Delete