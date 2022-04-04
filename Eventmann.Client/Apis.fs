namespace Eventmann.Client

open Eventmann.Shared
open Fable.Remoting.Client

module Apis =
  let machineType =
    Remoting.createApi()
    |> Remoting.withRouteBuilder (fun t m -> sprintf "/api/machine-type/%s" m)
    |> Remoting.buildProxy<MachineTypeApi>

