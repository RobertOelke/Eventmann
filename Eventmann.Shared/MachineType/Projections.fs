namespace Eventmann.Shared.MachineType

open System

type MachineTypeOverview = {
  Id : Guid
  MainType : string
  SubType : string
  Examples : string list
  Colour : string
}