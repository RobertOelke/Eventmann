namespace Eventmann.Server.MachineType

type MachineTypeEvent =
| Created of {| Category : string; Name : string |}
| DescriptionAdded of {| Description : string |}
| DescriptionRemoved of {| Description : string |}
| ColourChanged of {| Colour : string |}
| SketchExtended of {| Days : int |}
| SketchShortened of {| Days : int |}
| ConstructionExtended of {| Days : int |}
| ConstructionShortened of {| Days : int |}
| ShippingExtended of {| Days : int |}
| ShippingShortened of {| Days : int |}
| Deleted