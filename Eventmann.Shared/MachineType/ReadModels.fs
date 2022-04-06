namespace Eventmann.Shared.MachineType

open System

[<CLIMutable>]
type MachineTypeOverview = {
  Id : Guid
  MainType : string
  SubType : string
  Examples : string
  Colour : string
}

type MachineTypeDetail = {
  Id : Guid
  MainType : string
  SubType : string
  Examples : string list
  Colour : string
  Sketch : int
  Construction : int
  Montage : int
  Shipping : int
}

type History = {
  Date : DateTime
  Action : string
}