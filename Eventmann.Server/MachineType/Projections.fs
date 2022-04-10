namespace Eventmann.Server.MachineType

open System
open Eventmann.Shared.MachineType
open Kairos.Server

[<RequireQualifiedAccess>]
module MachineType =
  let projection : Projection<MachineType, MachineTypeEvent> =
    let zero = {
      Category = ""
      Name = ""
      Descriptions = []
      Colour = "black"
      Sketch = 0
      Construction = 0
      Shipping = 0
      IsDeleted = false
    }

    let update state = function
      | Created args -> { zero with Category = args.Category; Name = args.Name }
      | DescriptionAdded args -> { state with Descriptions = state.Descriptions @ [ args.Description ] }
      | DescriptionRemoved args -> { state with Descriptions = state.Descriptions |> List.filter ((<>) args.Description) }
      | ColourChanged args -> { state with Colour = args.Colour }
      | SketchExtended args -> { state with Sketch = state.Sketch + args.Days }
      | SketchShortened args -> { state with Sketch = state.Sketch - args.Days }
      | ConstructionExtended args -> { state with Construction = state.Construction + args.Days }
      | ConstructionShortened args -> { state with Construction = state.Construction - args.Days }
      | ShippingExtended args -> { state with Shipping = state.Shipping + args.Days }
      | ShippingShortened args -> { state with Shipping = state.Shipping - args.Days }
      | Deleted -> { state with IsDeleted = true }

    { Zero = zero ; Update = update }