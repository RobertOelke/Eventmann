namespace Eventmann.Server.VacuumType

type VacuumTypeEvent =
| Created of {| Category : string; Name : string |}
| DescriptionAdded of {| Description : string |}
| DescriptionRemoved of {| Description : string |}
| ColourChanged of {| Colour : string |}
| SketchExtended of {| Days : int |}
| SketchShortened of {| Days : int |}
| ConstructionExtended of {| Days : int |}
| ConstructionShortened of {| Days : int |}
| MontageExtended of {| Days : int |}
| MontageShortened of {| Days : int |}
| ShippingExtended of {| Days : int |}
| ShippingShortened of {| Days : int |}
| Deleted