namespace Eventmann.Shared.MachineType

type MachineType = {
  MainType : string
  SubType : string
  Examples : string list
  Colour : string option
  Sketch : int option
  Construction : int option
  Montage : int option
  Shipping : int option
}