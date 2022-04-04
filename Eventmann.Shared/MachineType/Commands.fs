namespace Eventmann.Shared.MachineType

type MachineTypeCommand =
| Create of MainType : string * SubType : string
| AddExample of string
| RemoveExample of string
| ChangeColour of Colour : string
| ChangeSketchDuration of int
| ChangeConstructionDuration of int
| ChangeMontageDuration of int
| ChangeShippingDuration of int
| Delete