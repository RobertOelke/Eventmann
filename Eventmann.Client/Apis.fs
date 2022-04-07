namespace Eventmann.Client

open Eventmann.Shared
open Fable.Remoting.Client

module Apis =
  let vacuumType =
    Remoting.createApi()
    |> Remoting.withRouteBuilder (fun t m -> sprintf "/api/vacuum-type/%s" m)
    |> Remoting.buildProxy<VacuumTypeApi>

  let order =
    Remoting.createApi()
    |> Remoting.withRouteBuilder (fun t m -> sprintf "/api/order/%s" m)
    |> Remoting.buildProxy<OrderApi>

