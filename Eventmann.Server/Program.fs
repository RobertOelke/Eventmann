open System
open Microsoft.AspNetCore.Builder
open Microsoft.Extensions.Hosting
open Microsoft.Extensions.Logging

open Giraffe
open Kairos.Server
open Eventmann.Server
open Eventmann.Server.MachineType

[<EntryPoint>]
let main args =

  let webApp =
    choose [
      subRoute "/api" (choose [
        Apis.machineType EventSourcedRoot.cmd EventSourcedRoot.query
        Apis.order EventSourcedRoot.cmd EventSourcedRoot.query
      ])
    ]

  let builder = WebApplication.CreateBuilder(args)
  builder.Services
    .AddGiraffe()
    |> ignore
    
  builder.Logging
    .ClearProviders()
    .AddDebug()
    .AddConsole()
    |> ignore

  let app = builder.Build()
  app.UseGiraffe(webApp)
  app.UseHttpLogging() |> ignore

  app.MapGet("/", Func<string>(fun () -> "Hello World!")) |> ignore

  app.Run()

  0 // Exit code

