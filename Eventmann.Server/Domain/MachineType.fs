namespace Eventmann.Server.Domain

open System
open Eventmann.Shared.MachineType

type CmdResult<'event> =
| Accepted of 'event list
| Rejected of string
| Failed of exn

type MachineTypeEvent =
| Created of {| MainType : string; SubType : string |}
| ExampleAdded of string
| ExampleRemoved of string
| ColourChanged of {| Colour : string |}
| SketchDurationChanged of int
| ConstructionDurationChanged of int
| MontageDurationChanged of int
| ShippingDurationChanged of int
| Deleted

module MachineType =

  let create (cmd : MachineTypeCommand) =
    match cmd with
    | Create args ->
      Accepted [ Created {| MainType = args.MainType; SubType = args.SubType |} ]

    | otherwise ->
      Rejected "Machine type does not exists"

  let update (state : MachineType) (cmd : MachineTypeCommand) =
    match cmd with
    | Create _ ->
      Rejected "Machine type already exists"

    | AddExample example ->
      if state.Examples |> List.contains example || String.IsNullOrWhiteSpace example then
        Rejected "Dublicate or empty example"
      else
        Accepted [ ExampleAdded example ]
    | RemoveExample example ->
      if state.Examples |> List.contains example then
        Accepted [ ExampleRemoved example ]
      else
        Rejected "Example not found"

    | ChangeColour args ->
      if state.Colour <> Some args.Colour then
        Accepted [ ColourChanged {| Colour = args.Colour |}  ]
      else
        Rejected "Colour not changed"

    | ChangeSketchDuration duration -> Accepted [ SketchDurationChanged duration ]
    | ChangeConstructionDuration duration -> Accepted [ ConstructionDurationChanged duration ]
    | ChangeMontageDuration duration -> Accepted [ MontageDurationChanged duration ]
    | ChangeShippingDuration duration -> Accepted [ ShippingDurationChanged duration ]

    | Delete ->
      Accepted [ Deleted ]