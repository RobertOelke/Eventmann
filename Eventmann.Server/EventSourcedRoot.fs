namespace Eventmann.Server

open System
open Eventmann.Shared.MachineType
open Eventmann.Server.MachineType
open Eventmann.Shared.Order
open Eventmann.Server.Order
open Eventmann.Shared.TechnicalData
open Eventmann.Server.TechnicalData
open Kairos.Server
open Kairos.Server.MsSql

module EventSourcedRoot =
  let [<Literal>] connectionString = "Server=.\SQLExpress;Database=Eventmann;Trusted_Connection=Yes;"

  module private MachineType =
    let store = new SqlEventStore<MachineTypeEvent>(connectionString)
      
    let overviewReadModel = MachineTypeOverviewReadModel.create connectionString

    let getAll : QueryHander<unit, MachineTypeOverview list> =
      QueryHander overviewReadModel.GetAll

    let details : QueryHander<EventSource, MachineType option> =
      QueryHander (fun uid ->
        let store = store :> IEventStore<MachineTypeEvent>
        
        async {
          let! eventStream = store.GetStream uid
        
          return
            eventStream
            |> Projection.project MachineType.projection
            |> Option.map(fun a -> a.State)
            |> Option.filter (fun a -> not a.IsDeleted)
        }
      )

    let history : QueryHander<EventSource, MachineTypeHistory list> =
      QueryHander (fun uid ->
        let store = store :> IEventStore<MachineTypeEvent>

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

    let commandHandler : CommandHandler<MachineTypeCommand> =
      CommandHandler (fun src cmd ->
        async {
          let store = store :> IEventStore<MachineTypeEvent>

          let! newEvents = MachineTypeBehaviour.handler store MachineType.projection src cmd
          do! store.Append { StreamSource = src; ExpectedVersion = None; Events = newEvents }

          return CommandResult.Ok
        }
        |> Async.CatchCommandResult
      )

  module private Order =
    let store = new SqlEventStore<OrderEvent>(connectionString)

    let inMemoryOrderReadModel = InMemoryOrdersReadModel.create store Order.projection

    let getForPhase : QueryHander<OrderPhase, Aggregate<Order> list> =
      QueryHander (inMemoryOrderReadModel.GetForPhase)

    let commandHandler (getMachineType : QueryHander<EventSource, MachineType option>) =
      CommandHandler (fun src cmd ->
        async {
          let store = store :> IEventStore<OrderEvent>
          let (QueryHander getMachineTypeById) = getMachineType

          let! newEvents = OrderBehaviour.handler getMachineTypeById store Order.projection src cmd
          do! store.Append { StreamSource = src; ExpectedVersion = None; Events = newEvents }

          return CommandResult.Ok
        }
      )

  module private TechnicalData =
    let store = new SqlEventStore<TechnicalDataEvent>(connectionString)

    let getAll : QueryHander<unit, Aggregate<TechnicalData> list> =
      QueryHander (fun () ->
        async {
          let store = store :> IEventStore<TechnicalDataEvent>
          let! events = store.Get ()
          return
            events
            |> List.groupBy (fun e -> e.Source)
            |> List.map (fun (k, lst) -> lst |> Projection.project TechnicalData.projection)
            |> List.choose id
        }
      )

    let commandHandler : CommandHandler<TechnicalDataCommand> =
      CommandHandler (fun src cmd ->
        async {
          let store = store :> IEventStore<TechnicalDataEvent>

          let! newEvents = TechnicalDataBehaviour.handler store TechnicalData.projection src cmd
          do! store.Append { StreamSource = src; ExpectedVersion = None; Events = newEvents }

          return CommandResult.Ok
        }
        |> Async.CatchCommandResult
      )

  let cmd, query =
    let eventBus = new EventBus()

    EventSourced.create eventBus
    // MachineType
    |> EventSourced.addQueryHandler MachineType.getAll
    |> EventSourced.addQueryHandler MachineType.details
    |> EventSourced.addQueryHandler MachineType.history
    |> EventSourced.addCommandHandler MachineType.commandHandler
    |> EventSourced.addProducer MachineType.store
    |> EventSourced.addConsumer MachineType.overviewReadModel
    // Orders
    |> EventSourced.addQueryHandler Order.getForPhase
    |> EventSourced.addCommandHandler (Order.commandHandler MachineType.details)
    |> EventSourced.addProducer Order.store
    |> EventSourced.addConsumer Order.inMemoryOrderReadModel
    // TechnicalData
    |> EventSourced.addQueryHandler TechnicalData.getAll
    |> EventSourced.addCommandHandler TechnicalData.commandHandler
    |> EventSourced.addProducer TechnicalData.store
    // Build
    |> EventSourced.build
