namespace Eventmann.Server.MachineType

open System
open Eventmann.Shared.MachineType
open Kairos.Server

module MachineTypeOverview =
  type private MachineTypeOverviewMsg =
  | NewEvent of EventData<MachineTypeEvent>
  | Query of AsyncReplyChannel<MachineTypeOverview list>

  let machineTypeOverview (bus : IEventBus) =

    let handleEvent (models : MachineTypeOverview list) (event : EventData<MachineTypeEvent>) =
      match event.Event with
      | Created args -> 
        let newModel = {
          Id = event.Source
          MainType = args.MainType
          SubType = args.SubType
          Examples = []
          Colour = "black"
        }
        newModel :: models 

      | ExampleAdded args ->
        models
        |> List.map (fun m -> if m.Id = event.Source then { m with Examples = args.Example :: m.Examples } else m)
        
      | ExampleRemoved args ->
        models
        |> List.map (fun m -> if m.Id = event.Source then { m with Examples = m.Examples |> List.filter ((<>) args.Example) } else m)

      | ColourChanged args ->
        models
        |> List.map (fun m -> if m.Id = event.Source then { m with Colour = args.Colour } else m)

      | Deleted ->
        models
        |> List.filter (fun x -> x.Id <> event.Source)

      | SketchDurationChanged _
      | ConstructionDurationChanged _
      | MontageDurationChanged _
      | ShippingDurationChanged _ -> models

    let update models = function
      | NewEvent event ->
        async {
          return handleEvent models event
        }
      | Query reply ->
        async {
          do reply.Reply(models)
          return models
        }
      
    let agent = StatefullAgent<_,_>.Start([], update)

    bus.OnEvent().Subscribe(NewEvent >> agent.Post) |> ignore

    (fun () -> agent.PostAndAsyncReply(Query))
