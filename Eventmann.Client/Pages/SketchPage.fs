namespace Eventmann.Client.Page

open System
open Elmish
open Feliz
open Feliz.UseElmish
open Feliz.Bulma
open Eventmann.Client
open Eventmann.Shared
open Eventmann.Shared.TechnicalData
open Eventmann.Shared.MachineType
open Eventmann.Shared.Order

module SketchPage =

  [<ReactComponent>]
  let Render() =
    GenericOrderPage.Render OrderPhase.Sketch
