namespace Eventmann.Server.TechnicalData

open System
open Eventmann.Shared.TechnicalData
open Kairos.Server

module TechnicalDataBehaviour =
  let handler
    (store : IEventStore<TechnicalDataEvent>)
    (projection : Projection<TechnicalData, TechnicalDataEvent>)
    (src : EventSource)
    (cmd : TechnicalDataCommand) =
    async {
      let! events = store.GetStream src

      return
        match events |> Projection.project projection with
        | None ->
          match cmd with
          | Create (title, editor) ->
            [ Created {| Editor = editor; Title = title |} ]
          | Delete ->
            []

        | Some aggregate ->
          match cmd with
          | Create _ ->
            []
          | Delete ->
            if aggregate.State.IsDeleted
            then []
            else [ Deleted ]
    }