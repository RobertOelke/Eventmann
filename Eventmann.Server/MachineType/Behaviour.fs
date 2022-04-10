namespace Eventmann.Server.MachineType

open System
open Eventmann.Shared.MachineType
open Kairos.Server

module MachineTypeBehaviour =
  let handler
    (store : IEventStore<MachineTypeEvent>)
    (projection : Projection<MachineType, MachineTypeEvent>)
    (src : EventSource)
    (cmd : MachineTypeCommand) =
    async {
      let! events = store.GetStream src

      return
        match events |> Projection.project projection with
        | None ->
          match cmd with
          | Create (category, name) ->
            [ Created {| Category = category; Name = name |} ]
          | _ -> []
        | Some state ->
          let state = state.State
          match cmd with
          | Create (category, name) ->
            []
        
          | AddDescription description ->
            if state.Descriptions |> List.contains description || String.IsNullOrWhiteSpace description
            then []
            else [ DescriptionAdded {| Description = description |} ]
        
          | RemoveDescription description ->
            if state.Descriptions |> List.contains description
            then [ DescriptionRemoved {| Description = description |} ]
            else []
        
          | ChangeColour colour ->
            if state.Colour <> colour
            then [ ColourChanged {| Colour = colour |}  ]
            else []
        
          | ChangeSketchDuration duration ->
            if duration < 0 then
              []
            else
              match (duration - state.Sketch) with
              | i when i > 0 -> [ SketchExtended {| Days = i |} ]
              | i when i < 0 -> [ SketchShortened {| Days = -i |} ]
              | _ -> []
        
          | ChangeConstructionDuration duration -> 
            if duration < 0
            then []
            else
              match (duration - state.Construction) with
              | i when i > 0 -> [ ConstructionExtended {| Days = i |} ]
              | i when i < 0 -> [ ConstructionShortened {| Days = -i |} ]
              | _ -> []
        
          | ChangeShippingDuration duration -> 
            if duration < 0
            then []
            else
              match (duration - state.Shipping) with
              | i when i > 0 -> [ ShippingExtended {| Days = i |} ]
              | i when i < 0 -> [ ShippingShortened {| Days = -i |} ]
              | _ -> []
        
          | Delete ->
            if state.IsDeleted
            then []
            else [ Deleted ]
    }