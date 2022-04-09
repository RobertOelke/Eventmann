namespace Eventmann.Server.Order

open System
open Eventmann.Shared.Order
open Kairos.Server

module InMemoryOrders =
  
  type private InMemoryOrderMsg =
  | Start
  | NewEvent of EventData<OrderEvent>
  | GetForPhase of OrderPhase * AsyncReplyChannel<Map<EventSource, Order>>

  let readModel
    (store : IEventStore<OrderEvent>)
    (projection : Projection<Order, OrderEvent>)
    (bus : IEventBus) =
    
    let handleEvent (store : Map<EventSource, Order>) (event : EventData<OrderEvent>) =
      let projection =
        match store.TryGetValue event.Source with
        | true, state -> { projection with Zero = state }
        | false, _ -> projection
      
      let newState = [ event ] |> Projection.project projection

      store |> Map.add event.Source newState.Value.State

    let update models =
      function
      | Start ->
        async {
          let! allEvents = store.Get ()

          let newModels = allEvents |> List.fold handleEvent models
          return newModels
        }
      | NewEvent event ->
        async {
          return handleEvent models event
        }
      | GetForPhase (phase, reply) ->
        async {
          models
          |> Map.filter (fun _ value -> true)
          |> reply.Reply

          return models
        }

    let agent = StatefullAgent<_,_>.Start(Map.empty, update)
    agent.Post Start
    bus.OnEvent().Subscribe(NewEvent >> agent.Post) |> ignore
  
    (fun phase -> agent.PostAndAsyncReply(fun r -> GetForPhase (phase, r)))
      