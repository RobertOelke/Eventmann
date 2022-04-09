namespace Eventmann.Server.VacuumType

open System
open Eventmann.Shared.VacuumType
open Kairos.Server
open Dapper
open System.Data.SqlClient

module VacuumTypeOverview =

  type private VacuumTypeOverviewMsg =
  | Start
  | NewEvent of EventData<VacuumTypeEvent>
  | Query of AsyncReplyChannel<VacuumTypeOverview list>

  let readModel (connectionString : string) (bus : IEventBus) =

    let handleEvent (event : EventData<VacuumTypeEvent>) =
      match event.Event with
      | Created args ->
        task {
          use connection = new SqlConnection(connectionString)

          let! _ =
            connection.ExecuteAsync(
              "INSERT INTO [VacuumTypeOverview] (Id, Category, Name, Descriptions, Colour) VALUES (@Id, @Category, @Name, @Descriptions, @Colour)",
              {| Id = event.Source; Category = args.Category; Name = args.Name; Descriptions = ""; Colour = "black" |})

          return ()
        } |> Some

      | DescriptionAdded args ->
        task {
          use connection = new SqlConnection(connectionString)

          let! example =
            connection.QueryFirstAsync<string>(
              "SELECT Descriptions FROM [VacuumTypeOverview] WHERE Id = @Id",
              {| Id = event.Source |})

          let! _ =
            connection.ExecuteAsync(
              "UPDATE [VacuumTypeOverview] SET Descriptions = @Descriptions WHERE Id = @Id",
              {| Id = event.Source; Descriptions = if String.IsNullOrWhiteSpace(example) then args.Description else $"{example}, {args.Description}" |})

          return ()
        } |> Some
        
      | DescriptionRemoved args ->
        task {
          use connection = new SqlConnection(connectionString)
          let! example =
            connection.QueryFirstAsync<string>(
              "SELECT Examples FROM [VacuumTypeOverview] WHERE Id = @Id",
              {| Id = event.Source |})

          let examples =
            example.Split(", ")
            |> Seq.filter((<>) args.Description)

          let! _ =
            connection.ExecuteAsync(
              "UPDATE [VacuumTypeOverview] SET Examples = @Examples WHERE Id = @Id",
              {| Id = event.Source; Examples = String.Join(", ", examples) |})

          return ()
        } |> Some

      | ColourChanged args ->
        task {
          use connection = new SqlConnection(connectionString)

          let! _ = connection.ExecuteAsync(
            "UPDATE [VacuumTypeOverview] SET Colour = @Colour WHERE Id = @Id",
            {| Id = event.Source; Colour = args.Colour |})

          return ()
        } |> Some

      | Deleted ->
        task {
          use connection = new SqlConnection(connectionString)

          let! _ = connection.ExecuteAsync(
            "DELETE FROM [VacuumTypeOverview] WHERE Id = @Id",
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
            CREATE TABLE [dbo].[VacuumTypeOverview] (
	          [Id] [uniqueidentifier] PRIMARY KEY,
              [Category] [varchar](100) NOT NULL,
              [Name] [varchar](100) NOT NULL,
              [Descriptions] [varchar](500) NOT NULL,
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
          let queryAll = "SELECT * FROM [VacuumTypeOverview] ORDER BY Category, Name"

          let! overView = connection.QueryAsync<VacuumTypeOverview>(queryAll)
          do reply.Reply(overView |> List.ofSeq)
          return ()
        } |> Async.AwaitTask

    let agent = StatelessAgent<_>.Start(update)
    agent.Post Start

    bus.OnEvent().Subscribe(NewEvent >> agent.Post) |> ignore

    (fun () -> agent.PostAndAsyncReply(Query))
