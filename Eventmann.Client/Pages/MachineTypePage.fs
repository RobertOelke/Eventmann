namespace Eventmann.Client.Page

open System
open Feliz
open Feliz.UseDeferred
open Feliz.Bulma
open Eventmann.Shared.MachineType

module MachineTypePage =

  let loadMachineTypes =
    async {
      do! Async.Sleep(200)

      return [
        {
          Id = System.Guid.NewGuid()
          MainType = "T"
          SubType = "T-A"
          Examples = [ "ABC"; "DEF" ]
          Colour = "black"
        }
        {
          Id = System.Guid.NewGuid()
          MainType = "T"
          SubType = "T-B"
          Examples = []
          Colour = "red"
        }
      ]
    }

  [<ReactComponent>]
  let Render() =
    let x = React.useDeferred(loadMachineTypes, [||])

    match x with
    | Deferred.Resolved lst ->
      Bulma.table [
        table.isFullWidth
        prop.children [
          Html.thead [
            Html.tr [
              Html.th "Actions"
              Html.th "Name"
              Html.th "CategoryName"
              Html.th "Examples"
            ]
          ]
          Html.tfoot [
            Html.tr [
              Html.td [
                Bulma.buttons [
                  Bulma.button.button [
                    prop.children [
                      Bulma.icon [
                        Html.i [ prop.className "fas fa-square-plus" ]
                      ]
                    ]
                  ]
                ]
              ]
              Html.td ""
              Html.td ""
              Html.td ""
            ]
          ]
          Html.tableBody [
            for machineType in lst do
              Html.tr [
                Html.td [
                  Bulma.buttons [
                    Bulma.button.button [
                      button.isSmall
                      prop.children [
                        Bulma.icon [
                          Html.i [ prop.className "fas fa-pen" ]
                        ]
                      ]
                    ]
                    Bulma.button.button [
                      button.isSmall
                      prop.children [
                        Bulma.icon [
                          Html.i [ prop.className "fas fa-trash" ]
                        ]
                      ]
                    ]
                  ]
                ]
                Html.td machineType.MainType
                Html.td [
                  prop.style [ style.color machineType.Colour]
                  prop.text machineType.SubType
                ]
                Html.td (String.Join(", ", machineType.Examples))
              ]
          ]
        ]
      ]

    | _ ->
      PageLoader.pageLoader [
        pageLoader.isSuccess
        pageLoader.isActive
        prop.children [
          PageLoader.title "I am loading some awesomeness"
        ]
      ]