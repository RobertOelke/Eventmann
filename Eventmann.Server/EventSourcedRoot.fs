namespace Eventmann.Server

open System
open Fable.Remoting.Server
open Fable.Remoting.Giraffe
open Eventmann.Shared.VacuumType
open Eventmann.Shared.Order
open Eventmann.Server.VacuumType
open Eventmann.Server.Order
open Kairos.Server
open Kairos.Server.MsSql

module EventSourcedRoot =
  let [<Literal>] connectionString = "Server=.\SQLExpress;Database=Eventmann;Trusted_Connection=Yes;"

  module private MachineType =
    let store = new SqlAggregateStore<VacuumType, VacuumTypeEvent>(connectionString, VacuumType.projection)
      
    let overView = VacuumTypeOverview.vacuumTypeOverview connectionString

    let details _ uid =
      let store = store :> IAggregateStore<VacuumType, VacuumTypeEvent>
      async {
        let! aggregate = store.GetAggregate uid
        return
          aggregate
          |> Option.filter (fun a -> not a.State.IsDeleted)
          |> Option.map(fun a -> a.State)
      }

    let history _ uid : Async<VacuumTypeHistory list> =
      async {
        let store = store :> IAggregateStore<VacuumType, VacuumTypeEvent>
        let! events = store.GetStream uid

        return
          events
          |> List.map (fun e -> {
            Date = e.RecordedAtUtc
            Action = sprintf "%A" e.Event
          })
          |> List.sortBy (fun h -> h.Date)
      }

    let commandHandler (src : EventSource) (cmd : VacuumTypeCommand) =
      async {
        let store = store :> IEventStore<VacuumTypeEvent>

        let! newEvents = VacuumTypeBehaviour.handler store VacuumType.projection src cmd
        do! store.Append { StreamSource = src; ExpectedVersion = None; Events = newEvents }

        return CommandResult.Ok
      }
      |> Async.CatchCommandResult

  module private Order =
    let store = new SqlAggregateStore<Order, OrderEvent>(connectionString,Order.defaultProjection)

    let commandHandler (src : EventSource) (cmd : OrderCommand) =
      async {
        let store = store :> IAggregateStore<Order, OrderEvent>
        let! aggregate = store.GetAggregate src
        let newEvents = OrderCommand.commandHandler (aggregate |> Option.map (fun a -> a.State)) cmd
        do! store.Append { StreamSource = src; ExpectedVersion = None; Events = newEvents }
        return CommandResult.Ok
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
