open System
open Microsoft.AspNetCore.Builder
open Microsoft.Extensions.Hosting

type Message = { Text : string }

[<EntryPoint>]
let main args =
  let lines = new System.Collections.Generic.List<string>()

  let builder = WebApplication.CreateBuilder(args)
  let app = builder.Build()

  app.MapPost("/", Action<Message>(fun message -> lines.Add(message.Text); printfn "New message: %s" message.Text)) |> ignore
  app.MapGet("/", Func<string>(fun () -> String.Join(Environment.NewLine, lines))) |> ignore

  app.Run()

  0 // Exit code

