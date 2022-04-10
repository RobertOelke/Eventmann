namespace Eventmann.Shared.TechnicalData

type EditorType =
| Text
| Number

type TechnicalData = {
  Title : string
  Editor : EditorType
  IsDeleted : bool
}