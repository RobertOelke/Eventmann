namespace Eventmann.Shared

open System
open Eventmann.Shared.MachineType

type MachineTypeApi = {
  GetAll : unit -> Async<MachineTypeOverview list>
  GetDetails : Guid -> Async<MachineTypeDetail option>
  Update : Guid -> MachineTypeCommand -> Async<unit>
  Create : {| MainType : string; SubType : string |} -> Async<unit>
}