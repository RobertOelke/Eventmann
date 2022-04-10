namespace Eventmann.Server.Order

open System
open Eventmann.Shared.Order
open Kairos.Server

type InMemoryOrders =
  {
    Notify : EventData<OrderEvent> -> unit
    GetForPhase : OrderPhase -> Async<Aggregate<Order> list>
  }
  interface IEventConsumer<OrderEvent> with
    member this.Notify event = this.Notify event

module InMemoryOrdersReadModel =
  
  type private InMemoryOrderReadModelMsg =
  | Start
  | NewEvent of EventData<OrderEvent>
  | GetForPhase of OrderPhase * AsyncReplyChannel<Aggregate<Order> list>

  let create
    (store : IEventStore<OrderEvent>)
    (projection : Projection<Order, OrderEvent>)
    =
    
    let handleEvent (store : Aggregate<Order> list) (event : EventData<OrderEvent>) =
      let projection =
        match store |> List.tryFind (fun a -> a.Source = event.Source) with
        | Some state -> { projection with Zero = state.State }
        | None -> projection
      
      let newState = [ event ] |> Projection.project projection

      newState.Value :: (store |> List.filter (fun a -> a.Source <> event.Source))

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
          |> List.filter (fun value -> true)
          |> reply.Reply

          return models
        }

    let agent = StatefullAgent<_,_>.Start([], update)
    agent.Post Start

    {
      Notify = (NewEvent >> agent.Post)
      GetForPhase = fun phase -> agent.PostAndAsyncReply(fun r -> GetForPhase (phase, r))
    }
      