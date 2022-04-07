namespace Eventmann.Server.VacuumType

open System
open Eventmann.Shared.VacuumType
open Kairos.Server

module VacuumTypeBehaviour =
  let handler
    (store : IEventStore<VacuumTypeEvent>)
    (projection : Projection<VacuumType, VacuumTypeEvent>)
    (src : EventSource)
    (cmd : VacuumTypeCommand) =
    async {
      let! events = store.GetStream src

      return
        match events |> Projection.project projection with
        | None ->
          match cmd with
          | Create (category, name) ->
            Accepted [ Created {| Category = category; Name = name |} ]
          | _ ->
            Rejected "Vacuum type already exists"
        | Some state ->
          let state = state.State
          match cmd with
          | Create (category, name) ->
            Rejected "Machine type already exists"
        
          | AddDescription description ->
            if state.Descriptions |> List.contains description || String.IsNullOrWhiteSpace description then
              Rejected "Dublicate or empty example"
            else
              Accepted [ DescriptionAdded {| Description = description |} ]
        
          | RemoveDescription description ->
            if state.Descriptions |> List.contains description then
              Accepted [ DescriptionRemoved {| Description = description |} ]
            else
              Rejected "Example not found"
        
          | ChangeColour colour ->
            if state.Colour <> colour then
              Accepted [ ColourChanged {| Colour = colour |}  ]
            else
              Rejected "Colour not changed"
        
          | ChangeSketchDuration duration ->
            if duration < 0 then
              Rejected "Duration can not be negativs"
            else
              match (duration - state.Sketch) with
              | i when i > 0 -> Accepted [ SketchExtended {| Days = i |} ]
              | i when i < 0 -> Accepted [ SketchShortened {| Days = -i |} ]
              | _ -> Rejected "Sketch duration not changed"
        
          | ChangeConstructionDuration duration -> 
            if duration < 0 then
              Rejected "Duration can not be negativs"
            else
              match (duration - state.Construction) with
              | i when i > 0 -> Accepted [ ConstructionExtended {| Days = i |} ]
              | i when i < 0 -> Accepted [ ConstructionShortened {| Days = -i |} ]
              | _ -> Rejected "Construction duration not changed"
        
          | ChangeMontageDuration duration -> 
            if duration < 0 then
              Rejected "Duration can not be negativs"
            else
              match (duration - state.Montage) with
              | i when i > 0 -> Accepted [ MontageExtended {| Days = i |} ]
              | i when i < 0 -> Accepted [ MontageShortened {| Days = -i |} ]
              | _ -> Rejected "Montage duration not changed"
        
          | ChangeShippingDuration duration -> 
            if duration < 0 then
              Rejected "Duration can not be negativs"
            else
              match (duration - state.Shipping) with
              | i when i > 0 -> Accepted [ ShippingExtended {| Days = i |} ]
              | i when i < 0 -> Accepted [ ShippingShortened {| Days = -i |} ]
              | _ -> Rejected "Construction duration not changed"
        
          | Delete -> Accepted [ Deleted ]
    }