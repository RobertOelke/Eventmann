namespace Eventmann.Server.MachineType

open System
open Eventmann.Shared.MachineType
open Kairos.Server

type MachineTypeEvent =
| Created of {| MainType : string; SubType : string |}
| ExampleAdded of {| Example : string |}
| ExampleRemoved of {| Example : string |}
| ColourChanged of {| Colour : string |}
| SketchExtended of {| Days : int |}
| SketchShortened of {| Days : int |}
| ConstructionExtended of {| Days : int |}
| ConstructionShortened of {| Days : int |}
| MontageExtended of {| Days : int |}
| MontageShortened of {| Days : int |}
| ShippingExtended of {| Days : int |}
| ShippingShortened of {| Days : int |}
| Deleted

type MachineType = {
  MainType : string
  SubType : string
  Examples : string list
  Colour : string
  Sketch : int
  Construction : int
  Montage : int
  Shipping : int
  IsDeleted : bool
}

module MachineType =
  let defaultProjection : Projection<MachineType, MachineTypeEvent> =
    let zero = {
      MainType = ""
      SubType = ""
      Examples = []
      Colour = "black"
      Sketch = 0
      Construction = 0
      Montage = 0
      Shipping = 0
      IsDeleted = false
    }

    let update state = function
      | Created args -> { zero with MainType = args.MainType; SubType = args.SubType }
      | ExampleAdded args -> { state with Examples = state.Examples @ [ args.Example ] }
      | ExampleRemoved args -> { state with Examples = state.Examples |> List.filter ((<>) args.Example)}
      | ColourChanged args -> { state with Colour = args.Colour }
      | SketchExtended args -> { state with Sketch = state.Sketch + args.Days }
      | SketchShortened args -> { state with Sketch = state.Sketch - args.Days }
      | ConstructionExtended args -> { state with Construction = state.Construction + args.Days }
      | ConstructionShortened args -> { state with Construction = state.Construction - args.Days }
      | MontageExtended args -> { state with Montage = state.Montage + args.Days } 
      | MontageShortened args -> { state with Montage = state.Montage - args.Days }
      | ShippingExtended args -> { state with Shipping = state.Shipping + args.Days }
      | ShippingShortened args -> { state with Shipping = state.Shipping - args.Days }
      | Deleted -> { state with IsDeleted = true }

    { Zero = zero ; Update = update }

module MachineTypeCommand =
  let commandHandler (state : MachineType option) (cmd : MachineTypeCommand) =
    match state with
    | None ->
      match cmd with
      | Create (mainType, subType) ->
        Accepted [ Created {| MainType = mainType; SubType = subType |} ]
      | _ ->
        Rejected "Machine type does not exists"

    | Some state ->
      match cmd with
      | Create (mainType, subType) ->
        Rejected "Machine type already exists"

      | AddExample example ->
        if state.Examples |> List.contains example || String.IsNullOrWhiteSpace example then
          Rejected "Dublicate or empty example"
        else
          Accepted [ ExampleAdded {| Example = example |} ]

      | RemoveExample example ->
        if state.Examples |> List.contains example then
          Accepted [ ExampleRemoved {| Example = example |} ]
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