namespace Eventmann.Client

open Eventmann.Shared
open Fable.Remoting.Client

module Apis =
  let machineType =
    Remoting.createApi()
    |> Remoting.withRouteBuilder (fun t m -> sprintf "/api/machine-type/%s" m)
    |> Remoting.buildProxy<MachineTypeApi>

  let order =
    Remoting.createApi()
    |> Remoting.withRouteBuilder (fun t m -> sprintf "/api/order/%s" m)
    |> Remoting.buildProxy<OrderApi>
    
  let technicalData =
    Remoting.createApi()
    |> Remoting.withRouteBuilder (fun t m -> sprintf "/api/technical-data/%s" m)
    |> Remoting.buildProxy<TechnicalDataApi>
