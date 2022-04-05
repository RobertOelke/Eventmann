namespace Eventmann.Server

open System
open Fable.Remoting.Server
open Fable.Remoting.Giraffe
open Eventmann.Shared
open Eventmann.Shared.MachineType
open Eventmann.Shared.Order
open Eventmann.Server.MachineType
open Eventmann.Server.Order
open Kairos.Server

module Apis =

  let machineType (cmdHandler : ICommandHandler) (queryHandler : IQueryHandler) =

    let getAll () : Async<MachineTypeOverview list> =
      async {
        match! queryHandler.TryHandle () with
        | QueryResult.Ok lst -> return lst
        | _ -> return []
      }

    let create main sub =
      async {
        let! creationResult = cmdHandler.Handle (Guid.NewGuid(), MachineTypeCommand.Create (main, sub))
        match creationResult with
        | CommandResult.Ok -> ()
        | CommandResult.Rejected ->
          printfn "Rejected"
        | CommandResult.NoHandler t -> 
          printfn "NoHandler: %s" t.Name
        | CommandResult.Error exn ->
          printfn "Error: %s" exn.Message
      }

    let getDetails uid : Async<MachineTypeDetail option> =
      async {
        match! queryHandler.TryHandle uid with
        | QueryResult.Ok ok -> return ok
        | _ -> return None
      }

    let update (uid : EventSource) (cmd : MachineTypeCommand) : Async<unit> =
      async {
        let! creationResult = cmdHandler.Handle (uid, cmd)
        match creationResult with
        | CommandResult.Ok -> ()
        | CommandResult.Rejected ->
          printfn "Rejected"
        | CommandResult.NoHandler t -> 
          printfn "NoHandler: %s" t.Name
        | CommandResult.Error exn ->
          printfn "Error: %s" exn.Message
      }

    let getLog (uid : EventSource) : Async<History list> =
      async {
        match! queryHandler.TryHandle uid with
        | QueryResult.Ok ok -> return ok
        | _ -> return []
      }

    let api : MachineTypeApi = {
      GetAll = getAll
      GetDetails = getDetails
      Update = update
      Create = fun args -> create args.MainType args.SubType
      GetLog = getLog
    }

    Remoting.createApi ()
    |> Remoting.fromValue api
    |> Remoting.withRouteBuilder(fun t m -> sprintf "/machine-type/%s" m)
    |> Remoting.buildHttpHandler

  let order (cmdHandler : ICommandHandler) (queryHandler : IQueryHandler) =
  
    let create (cmd : OrderCommand) =
      async {
        let! creationResult = cmdHandler.Handle (Guid.NewGuid(), cmd)
        match creationResult with
        | CommandResult.Ok -> ()
        | CommandResult.Rejected ->
          printfn "Rejected"
        | CommandResult.NoHandler t -> 
          printfn "NoHandler: %s" t.Name
        | CommandResult.Error exn ->
          printfn "Error: %s" exn.Message
      }
  
    let api : OrderApi = {
      PlaceOrder = OrderCommand.PlaceOrder >> create
    }

    Remoting.createApi ()
    |> Remoting.fromValue api
    |> Remoting.withRouteBuilder(fun t m -> sprintf "/order/%s" m)
    |> Remoting.buildHttpHandler