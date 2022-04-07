namespace Eventmann.Server.Order

open System
open Eventmann.Shared.Order
open Kairos.Server

[<RequireQualifiedAccess>]
module Order =
  let projection : Projection<Order, OrderEvent> =
    let zero : Order = {
      SerialNumber = ""
      Customer = ""
      ModelName = ""
      VacuumType = Guid.Empty
      DeliveryDate = DateTime.MinValue
    }

    let update state = function
      | OrderPlaced args -> {
        zero with
          SerialNumber = args.SerialNumber
          Customer = args.Customer
          ModelName = args.ModelName
          VacuumType = args.VacuumType
          DeliveryDate = args.DeliveryDate
        }

    { Zero = zero; Update = update }