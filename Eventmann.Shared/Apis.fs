namespace Eventmann.Shared

open System
open Eventmann.Shared.MachineType
open Eventmann.Shared.Order

type MachineTypeApi = {
  GetAll : unit -> Async<MachineTypeOverview list>
  GetDetails : Guid -> Async<MachineTypeDetail option>
  Update : Guid -> MachineTypeCommand -> Async<unit>
  Create : {| MainType : string; SubType : string |} -> Async<unit>
  GetLog : Guid -> Async<History list>
}

type OrderApi = {
  PlaceOrder : NewOrder -> Async<unit>
}