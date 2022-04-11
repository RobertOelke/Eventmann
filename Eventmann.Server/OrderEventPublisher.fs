namespace Eventmann.Server

open System
open Eventmann.Shared.MachineType
open Eventmann.Server.MachineType
open Eventmann.Shared.Order
open Eventmann.Server.Order
open Eventmann.Shared.TechnicalData
open Eventmann.Server.TechnicalData
open Kairos.Server
open Kairos.Server.MsSql
open System.Net.Http
open System.Net.Http.Json

type Message = {
 Text : string
}

type OrderEventPublisher() =
    
  interface IEventConsumer<OrderEvent> with
    
    member this.Notify (event : EventData<OrderEvent>) =
      let msg =
        match event.Event with
        | OrderEvent.OrderPlaced args ->
          Some (sprintf "Order was placed. Customer: %s; SerialNumber: %s" args.Customer args.SerialNumber)

        | OrderEvent.ConstructionFinished ->
          Some (sprintf "Construction is finished. Shipping will start soon.")

        | _ ->
          None

      match msg with
      | None -> ()
      | Some msg ->
        let client = new HttpClient()
        client.PostAsync("http://localhost:6000/", JsonContent.Create({ Text = msg})).Wait()
