namespace Eventmann.Client.Page

open System
open Elmish
open Feliz
open Feliz.UseElmish
open Feliz.Bulma
open Eventmann.Client
open Eventmann.Shared
open Eventmann.Shared.Order

module ConstructionPage =

  [<ReactComponent>]
  let Render() =
    GenericOrderPage.Render OrderPhase.Construction

