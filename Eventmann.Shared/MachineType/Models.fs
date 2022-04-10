namespace Eventmann.Shared.MachineType

open System

type MachineType = {
  Category : string
  Name : string
  Colour : string
  Descriptions : string list
  Sketch : int
  Construction : int
  Shipping : int
  IsDeleted : bool
}

[<CLIMutable>]
type MachineTypeOverview = {
  Id : Guid
  Category : string
  Name : string
  Colour : string
  Descriptions : string
}

type MachineTypeHistory = {
  Date : DateTime
  Action : string
}