namespace Eventmann.Shared.VacuumType

open System

type VacuumType = {
  Category : string
  Name : string
  Colour : string
  Descriptions : string list
  Sketch : int
  Construction : int
  Montage : int
  Shipping : int
  IsDeleted : bool
}

[<CLIMutable>]
type VacuumTypeOverview = {
  Id : Guid
  Category : string
  Name : string
  Colour : string
  Descriptions : string
}

type VacuumTypeHistory = {
  Date : DateTime
  Action : string
}