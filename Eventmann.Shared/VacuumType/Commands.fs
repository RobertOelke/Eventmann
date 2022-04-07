namespace Eventmann.Shared.VacuumType

type VacuumTypeCommand =
| Create of Categorie : string * Name : string
| AddDescription of Description : string
| RemoveDescription of Description : string
| ChangeColour of Colour : string
| ChangeSketchDuration of int
| ChangeConstructionDuration of int
| ChangeMontageDuration of int
| ChangeShippingDuration of int
| Delete