namespace Eventmann.Server.MachineType

open System
open Eventmann.Shared.MachineType
open Kairos.Server

type MachineTypeEvent =
| Created of {| MainType : string; SubType : string |}
| ExampleAdded of {| Example : string |}
| ExampleRemoved of {| Example : string |}
| ColourChanged of {| Colour : string |}
| SketchDurationChanged of {| Days : int |}
| ConstructionDurationChanged of {| Days : int |}
| MontageDurationChanged of {| Days : int |}
| ShippingDurationChanged of {| Days : int |}
| Deleted

type MachineType = {
  MainType : string
  SubType : string
  Examples : string list
  Colour : string option
  Sketch : int option
  Construction : int option
  Montage : int option
  Shipping : int option
  IsDeleted : bool
}

module MachineType =
  let zero = {
    MainType = ""
    SubType = ""
    Examples = []
    Colour = None
    Sketch = None
    Construction = None
    Montage = None
    Shipping = None
    IsDeleted = false
  }

  let project state = function
    | Created args -> { zero with MainType = args.MainType; SubType = args.SubType }
    | ExampleAdded args -> { state with Examples = state.Examples @ [ args.Example ] }
    | ExampleRemoved args -> { state with Examples = state.Examples |> List.filter ((<>) args.Example)}
    | ColourChanged args -> { state with Colour = Some args.Colour }
    | SketchDurationChanged args -> { state with Sketch = Some args.Days }
    | ConstructionDurationChanged args -> { state with Construction = Some args.Days }
    | MontageDurationChanged args -> { state with Montage = Some args.Days }
    | ShippingDurationChanged args -> { state with Shipping = Some args.Days }
    | Deleted -> { state with IsDeleted = true }

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
        if state.Colour <> Some colour then
          Accepted [ ColourChanged {| Colour = colour |}  ]
        else
          Rejected "Colour not changed"

      | ChangeSketchDuration duration -> Accepted [ SketchDurationChanged {| Days = duration |} ]
      | ChangeConstructionDuration duration -> Accepted [ ConstructionDurationChanged {| Days = duration |} ]
      | ChangeMontageDuration duration -> Accepted [ MontageDurationChanged {| Days = duration |} ]
      | ChangeShippingDuration duration -> Accepted [ ShippingDurationChanged {| Days = duration |} ]
      | Delete -> Accepted [ Deleted ]