namespace Eventmann.Client.Page

open System
open Elmish
open Feliz
open Feliz.UseElmish
open Feliz.Bulma
open Eventmann.Client
open Eventmann.Shared
open Eventmann.Shared.MachineType

module ConstructionPage =

  [<ReactComponent>]
  let Render() =
    Html.text "ToDo"

