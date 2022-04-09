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
    let store = new SqlEventStore<VacuumTypeEvent>(connectionString)
      
    let overView : QueryHander<unit, VacuumTypeOverview list> =
      QueryHander (fun eventbus () ->
        VacuumTypeOverview.readModel connectionString eventbus ()
      )

    let details : QueryHander<EventSource, VacuumType option> =
      QueryHander (fun eventBus uid ->
        let store = store :> IEventStore<VacuumTypeEvent>
        
        async {
          let! eventStream = store.GetStream uid
        
          return
            eventStream
            |> Projection.project VacuumType.projection
            |> Option.map(fun a -> a.State)
            |> Option.filter (fun a -> not a.IsDeleted)
        }
      )

    let history : QueryHander<EventSource, VacuumTypeHistory list> =
      QueryHander (fun eventBus uid ->
        let store = store :> IEventStore<VacuumTypeEvent>

        async {
          let! events = store.GetStream uid

          return
            events
            |> List.map (fun e -> {
              Date = e.RecordedAtUtc
              Action = sprintf "%A" e.Event
            })
            |> List.sortBy (fun h -> h.Date)
        }
      )

    let commandHandler : CommandHandler<VacuumTypeCommand> =
      CommandHandler (fun src cmd ->
        async {
          let store = store :> IEventStore<VacuumTypeEvent>

          let! newEvents = VacuumTypeBehaviour.handler store VacuumType.projection src cmd
          do! store.Append { StreamSource = src; ExpectedVersion = None; Events = newEvents }

          return CommandResult.Ok
        }
        |> Async.CatchCommandResult
      )

  module private Order =
    let store = new SqlEventStore<OrderEvent>(connectionString)

    let getForPhase : QueryHander<OrderPhase, Map<EventSource, Order>> =
      QueryHander (fun eventBus phase ->
        InMemoryOrders.readModel store Order.projection eventBus phase
      )

    let commandHandler =
      CommandHandler (fun src cmd ->
        async {
          let store = store :> IEventStore<OrderEvent>

          let! newEvents = OrderBehaviour.handler store Order.projection src cmd
          do! store.Append { StreamSource = src; ExpectedVersion = None; Events = newEvents }

          return CommandResult.Ok
        }
      )

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
    |> EventSourced.addQueryHandler Order.getForPhase
    |> EventSourced.addCommandHandler Order.commandHandler
    // Build
    |> EventSourced.build
