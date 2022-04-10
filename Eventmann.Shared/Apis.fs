namespace Eventmann.Shared

open System
open Eventmann.Shared.MachineType
open Eventmann.Shared.Order
open Eventmann.Shared.TechnicalData

type MachineTypeApi = {
  GetAll : unit -> Async<MachineTypeOverview list>
  GetDetails : Guid -> Async<MachineType option>
  Update : Guid -> MachineTypeCommand -> Async<unit>
  Create : {| MainType : string; SubType : string |} -> Async<unit>
  GetLog : Guid -> Async<MachineTypeHistory list>
}

type OrderApi = {
  PlaceOrder : NewOrder -> Async<unit>
  GetOrders : OrderPhase -> Async<Order list>
}

type TechnicalDataApi = {
  GetAll : unit -> Async<(Guid * TechnicalData) list>
  Create : {| Title : string; Editor : EditorType |} -> Async<unit>
  Delete : Guid -> Async<unit>
}