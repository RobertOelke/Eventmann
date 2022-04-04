namespace Eventmann.Shared.MachineType

open System

type MachineTypeOverview = {
  Id : Guid
  MainType : string
  SubType : string
  Examples : string list
  Colour : string
}

type MachineTypeDetail = {
  Id : Guid
  MainType : string
  SubType : string
  Examples : string list
  Colour : string option
  Sketch : int option
  Construction : int option
  Montage : int option
  Shipping : int option
}

type History = {
  Date : DateTime
  Action : string
}