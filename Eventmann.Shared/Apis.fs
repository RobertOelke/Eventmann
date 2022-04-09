namespace Eventmann.Shared

open System
open Eventmann.Shared.VacuumType
open Eventmann.Shared.Order

type VacuumTypeApi = {
  GetAll : unit -> Async<VacuumTypeOverview list>
  GetDetails : Guid -> Async<VacuumType option>
  Update : Guid -> VacuumTypeCommand -> Async<unit>
  Create : {| MainType : string; SubType : string |} -> Async<unit>
  GetLog : Guid -> Async<VacuumTypeHistory list>
}

type OrderApi = {
  PlaceOrder : NewOrder -> Async<unit>
  GetOrders : OrderPhase -> Async<Order list>
}