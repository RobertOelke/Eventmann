namespace Eventmann.Shared.TechnicalData

type TechnicalDataCommand =
| Create of Title:string * Editor:EditorType
| Delete