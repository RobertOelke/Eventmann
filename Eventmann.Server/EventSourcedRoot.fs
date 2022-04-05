namespace Eventmann.Server

open System
open Fable.Remoting.Server
open Fable.Remoting.Giraffe
open Eventmann.Shared.MachineType
open Eventmann.Shared.Order
open Eventmann.Server.MachineType
open Eventmann.Server.Order
open Kairos.Server

module EventSourcedRoot =
  module private MachineType =
    let store = new InMemoryAggregateStore<MachineType, MachineTypeEvent>(MachineType.zero, MachineType.project)
      
    let overView = MachineTypeOverview.machineTypeOverview

    let details _ uid =
      let store = store :> IAggregateStore<MachineType, MachineTypeEvent>
      async {
        let! x = store.GetAggregate uid
        return
          x
          |> Option.map(fun a -> {
            Id = a.Source
            MainType = a.State.MainType
            SubType = a.State.SubType
            Examples = a.State.Examples
            Colour = a.State.Colour
            Sketch = a.State.Sketch
            Construction = a.State.Construction
            Montage = a.State.Montage
            Shipping = a.State.Shipping
          })
      }

    let history _ uid : Async<History list> =
      async {
        let store = store :> IAggregateStore<MachineType, MachineTypeEvent>
        let! events = store.GetStream uid

        return
          events
          |> List.map (fun e -> {
            Date = e.RecordedAtUtc
            Action = sprintf "%A" e.Event
          })
          |> List.sortBy (fun h -> h.Date)
      }

    let commandHandler (src : EventSource) (cmd : MachineTypeCommand) =
      async {
        let store = store :> IAggregateStore<MachineType, MachineTypeEvent>
        let! aggregate = store.GetAggregate src
        match MachineTypeCommand.commandHandler (aggregate |> Option.map (fun a -> a.State)) cmd with
        | Accepted events ->
          do! store.Append { StreamSource = src; ExpectedVersion = None; Events = events }
          return CommandResult.Ok
        | Rejected _ ->
          return CommandResult.Rejected
        | Failed exn ->
          return CommandResult.Error exn
      }

  module private Order =
    let store = new InMemoryAggregateStore<Order, OrderEvent>(Order.zero, Order.project)

    let commandHandler (src : EventSource) (cmd : OrderCommand) =
      async {
        let store = store :> IAggregateStore<Order, OrderEvent>
        let! aggregate = store.GetAggregate src
        match OrderCommand.commandHandler (aggregate |> Option.map (fun a -> a.State)) cmd with
        | Accepted events ->
          do! store.Append { StreamSource = src; ExpectedVersion = None; Events = events }
          return CommandResult.Ok
        | Rejected _ ->
          return CommandResult.Rejected
        | Failed exn ->
          return CommandResult.Error exn
      }

  let cmd, query =
    EventSourced.create()
    // MachineType
    |> EventSourced.addProducer MachineType.store
    |> EventSourced.addQueryHandler MachineType.overView
    |> EventSourced.addQueryHandler MachineType.details
    |> EventSourced.addQueryHandler MachineType.history
    |> EventSourced.addCommandHandler MachineType.commandHandler
    // Order
    |> EventSourced.addProducer Order.store
    |> EventSourced.addCommandHandler Order.commandHandler
    // Build
    |> EventSourced.build
