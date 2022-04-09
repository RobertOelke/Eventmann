namespace Eventmann.Server

open System
open Fable.Remoting.Server
open Fable.Remoting.Giraffe
open Eventmann.Shared
open Eventmann.Shared.VacuumType
open Eventmann.Shared.Order
open Eventmann.Server.VacuumType
open Eventmann.Server.Order
open Kairos.Server

module Apis =

  let machineType (cmdHandler : ICommandHandler) (queryHandler : IQueryHandler) =

    let getAll () : Async<VacuumTypeOverview list> =
      async {
        match! queryHandler.TryHandle () with
        | QueryResult.Ok lst -> return lst
        | _ -> return []
      }

    let getDetails uid : Async<VacuumType option> =
      async {
        match! queryHandler.TryHandle uid with
        | QueryResult.Ok ok -> return ok
        | _ -> return None
      }

    let update (uid : EventSource) (cmd : VacuumTypeCommand) : Async<unit> =
      async {
        match! cmdHandler.Handle (uid, cmd) with
        | CommandResult.Ok -> ()
        | CommandResult.Error exn -> printfn "Error: %s" exn.Message
      }

    let create = update (Guid.NewGuid())

    let getLog (uid : EventSource) : Async<VacuumTypeHistory list> =
      async {
        match! queryHandler.TryHandle uid with
        | QueryResult.Ok ok -> return ok
        | _ -> return []
      }

    let api : VacuumTypeApi = {
      GetAll = getAll
      GetDetails = getDetails
      Update = update
      Create = fun args -> VacuumTypeCommand.Create (args.MainType, args.SubType) |> create
      GetLog = getLog
    }

    Remoting.createApi ()
    |> Remoting.fromValue api
    |> Remoting.withRouteBuilder(fun t m -> sprintf "/vacuum-type/%s" m)
    |> Remoting.buildHttpHandler

  let order (cmdHandler : ICommandHandler) (queryHandler : IQueryHandler) =
  
    let update (uid : EventSource) (cmd : OrderCommand) =
      async {
        match! cmdHandler.Handle (uid, cmd) with
        | CommandResult.Ok -> ()
        | CommandResult.Error exn -> printfn "Error: %s" exn.Message
      }

    let getOrders (phase : OrderPhase) : Async<Order list> =
      async {
        match! queryHandler.TryHandle<OrderPhase, Map<EventSource, Order>> phase with
        | QueryResult.Ok orders ->
          return
            orders
            |> Map.values
            |> List.ofSeq
          
        | _ -> return []
      }

    let create = update (Guid.NewGuid())
  
    let api : OrderApi = {
      PlaceOrder = OrderCommand.PlaceOrder >> create
      GetOrders = getOrders
    }

    Remoting.createApi ()
    |> Remoting.fromValue api
    |> Remoting.withRouteBuilder(fun t m -> sprintf "/order/%s" m)
    |> Remoting.buildHttpHandler