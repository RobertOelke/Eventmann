namespace Eventmann.Server

open System
open Fable.Remoting.Server
open Fable.Remoting.Giraffe
open Eventmann.Shared
open Eventmann.Shared.Order
open Eventmann.Shared.MachineType
open Eventmann.Shared.TechnicalData
open Kairos.Server

module Apis =

  let machineType (cmdHandler : ICommandHandler) (queryHandler : IQueryHandler) =

    let getAll () : Async<Eventmann.Shared.MachineType.MachineTypeOverview list> =
      async {
        match! queryHandler.TryHandle () with
        | QueryResult.Ok lst -> return lst
        | _ -> return []
      }

    let getDetails uid : Async<MachineType option> =
      async {
        match! queryHandler.TryHandle uid with
        | QueryResult.Ok ok -> return ok
        | _ -> return None
      }

    let update (uid : EventSource) (cmd : MachineTypeCommand) : Async<unit> =
      async {
        match! cmdHandler.Handle (uid, cmd) with
        | CommandResult.Ok -> ()
        | CommandResult.Error exn -> printfn "Error: %s" exn.Message
      }

    let create = update (Guid.NewGuid())

    let getLog (uid : EventSource) : Async<MachineTypeHistory list> =
      async {
        match! queryHandler.TryHandle uid with
        | QueryResult.Ok ok -> return ok
        | _ -> return []
      }

    let api : MachineTypeApi = {
      GetAll = getAll
      GetDetails = getDetails
      Update = update
      Create = fun args -> MachineTypeCommand.Create (args.MainType, args.SubType) |> create
      GetLog = getLog
    }

    Remoting.createApi ()
    |> Remoting.fromValue api
    |> Remoting.withRouteBuilder(fun t m -> sprintf "/machine-type/%s" m)
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
        match! queryHandler.TryHandle<OrderPhase, List<Aggregate<Order>>> phase with
        | QueryResult.Ok orders ->
          return
            orders
            |> List.map (fun o -> o.State)
          
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

  let technicalData (cmdHandler : ICommandHandler) (queryHandler : IQueryHandler) =
    
    let getAll () =
      async {
        match! queryHandler.TryHandle<unit, Aggregate<TechnicalData> list> () with
        | QueryResult.Ok lst ->
          return lst |> List.map (fun a -> (a.Source, a.State))
        | _ -> return []
      }

    let handle src cmd =
      async {
        match! cmdHandler.Handle<TechnicalDataCommand> (src, cmd) with
        | CommandResult.Ok -> ()
        | CommandResult.Error exn -> printfn "Error: %s" exn.Message
      }

    let api : TechnicalDataApi = {
      GetAll = getAll
      Create = fun args -> handle (Guid.NewGuid()) (TechnicalDataCommand.Create (args.Title, args.Editor))
      Delete = fun src -> handle src TechnicalDataCommand.Delete
    }

    Remoting.createApi ()
    |> Remoting.fromValue api
    |> Remoting.withRouteBuilder(fun t m -> sprintf "/technical-data/%s" m)
    |> Remoting.buildHttpHandler