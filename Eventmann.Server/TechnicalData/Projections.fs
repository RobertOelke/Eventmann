namespace Eventmann.Server.TechnicalData

open System
open Eventmann.Shared.TechnicalData
open Kairos.Server

[<RequireQualifiedAccess>]
module TechnicalData =
  let projection : Projection<TechnicalData, TechnicalDataEvent> =
    let zero = {
      Title = ""
      Editor = EditorType.Text
      IsDeleted = false
    }

    let update state = function
      | Created args -> { zero with Title = args.Title; Editor = args.Editor }
      | Deleted -> { state with IsDeleted = true }

    { Zero = zero ; Update = update }