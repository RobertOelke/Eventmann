namespace Eventmann.Server.MachineType

open System
open Eventmann.Shared.MachineType
open Kairos.Server
open Dapper
open System.Data.SqlClient

module MachineTypeOverview =
  type private MachineTypeOverviewMsg =
  | Start
  | NewEvent of EventData<MachineTypeEvent>
  | Query of AsyncReplyChannel<MachineTypeOverview list>

  let machineTypeOverview (connectionString : string) (bus : IEventBus) =

    let handleEvent (event : EventData<MachineTypeEvent>) =
      match event.Event with
      | Created args ->
        task {
          use connection = new SqlConnection(connectionString)
          let insert = "INSERT INTO [MachineTypeOverview] (Id, MainType, SubType, Examples, Colour) VALUES (@Id, @MainType, @SubType, @Examples, @Colour)"
          let! _ = connection.ExecuteAsync(insert, {| Id = event.Source; MainType = args.MainType; SubType = args.SubType; Examples = ""; Colour = "black" |})
          return ()
        } |> Some

      | ExampleAdded args ->
        task {
          use connection = new SqlConnection(connectionString)
          let! example = connection.QueryFirstAsync<string>("SELECT Examples FROM [MachineTypeOverview] WHERE Id = @Id", {| Id = event.Source |})

          let! _ = connection.ExecuteAsync(
            "UPDATE [MachineTypeOverview] SET Examples = @Examples WHERE Id = @Id",
            {| Id = event.Source; Examples = if String.IsNullOrWhiteSpace(example) then args.Example else $"{example}, {args.Example}" |})
          return ()
        } |> Some
        
      | ExampleRemoved args ->
        task {
          use connection = new SqlConnection(connectionString)
          let! example = connection.QueryFirstAsync<string>("SELECT Examples FROM [MachineTypeOverview] WHERE Id = @Id", {| Id = event.Source |})

          let examples =
            example.Split(", ")
            |> Seq.filter((<>) args.Example)

          let! _ = connection.ExecuteAsync(
            "UPDATE [MachineTypeOverview] SET Examples = @Examples WHERE Id = @Id",
            {| Id = event.Source; Examples = String.Join(", ", examples) |})
          return ()
        } |> Some

      | ColourChanged args ->
        task {
          use connection = new SqlConnection(connectionString)
          let! _ = connection.ExecuteAsync(
            "UPDATE [MachineTypeOverview] SET Colour = @Colour WHERE Id = @Id",
            {| Id = event.Source; Colour = args.Colour |})
          return ()
        } |> Some

      | Deleted ->
        task {
          use connection = new SqlConnection(connectionString)
          let! _ = connection.ExecuteAsync(
            "DELETE FROM [MachineTypeOverview] WHERE Id = @Id",
            {| Id = event.Source |})
          return ()
        } |> Some

      | SketchExtended _
      | SketchShortened _
      | ConstructionExtended _
      | ConstructionShortened _
      | MontageExtended _
      | MontageShortened _
      | ShippingExtended _
      | ShippingShortened _ -> None

    let update =
      function
      | Start ->
        task {
          use connection = new SqlConnection(connectionString)
          let createTable = $"
            CREATE TABLE [dbo].[MachineTypeOverview] (
	          [Id] [uniqueidentifier] PRIMARY KEY,
              [MainType] [varchar](100) NOT NULL,
              [SubType] [varchar](100) NOT NULL,
              [Examples] [varchar](500) NOT NULL,
              [Colour] [varchar](100) NOT NULL
            )"

          let! _ = connection.ExecuteAsync(createTable)
          return ()
        } |> Async.AwaitTask

      | NewEvent event ->
        async {
          match handleEvent event with
          | Some t -> do! Async.AwaitTask t
          | None -> ()
        }

      | Query reply ->
        task {
          use connection = new SqlConnection(connectionString)
          let queryAll = "SELECT * FROM [MachineTypeOverview] ORDER BY MainType, SubType"

          let! overView = connection.QueryAsync<MachineTypeOverview>(queryAll)
          do reply.Reply(overView |> List.ofSeq)
          return ()
        } |> Async.AwaitTask

    let agent = StatelessAgent<_>.Start(update)
    agent.Post Start

    bus.OnEvent().Subscribe(NewEvent >> agent.Post) |> ignore

    (fun () -> agent.PostAndAsyncReply(Query))
