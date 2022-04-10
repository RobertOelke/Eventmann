namespace Eventmann.Client.Page

open System
open Elmish
open Feliz
open Feliz.UseElmish
open Feliz.Bulma
open Eventmann.Client
open Eventmann.Shared
open Eventmann.Shared.TechnicalData

module TechnicalDataPage =

  type State = {
    IsLoading : bool
    NewTitle : string
    NewEditor : EditorType
    Columns : (Guid * TechnicalData) list
  }

  let empty = {
    IsLoading = false
    NewTitle = ""
    NewEditor = EditorType.Text
    Columns = []
  }

  type Msg =
  | LoadColumns
  | ColumnsLoaded of (Guid * TechnicalData) list
  | NewTitleChanged of string
  | NewEditorChanged of EditorType
  | Create
  | Delete of Guid

  let update msg state =
    match msg with
    | LoadColumns ->
      let cmd = Cmd.OfAsync.perform Apis.technicalData.GetAll () ColumnsLoaded
      { state with IsLoading = true }, cmd

    | ColumnsLoaded cols  ->
      { state with IsLoading = false; Columns = cols }, Cmd.none

    | NewTitleChanged t ->
      { state with NewTitle = t }, Cmd.none

    | NewEditorChanged e ->
      { state with NewEditor = e }, Cmd.none

    | Create ->
      let httpCalls () =
        async {
          do! Apis.technicalData.Create {| Editor = state.NewEditor; Title = state.NewTitle |}

          return! (Apis.technicalData.GetAll ())
        }

      let cmd = Cmd.OfAsync.perform httpCalls () ColumnsLoaded

      { state with IsLoading = true; NewTitle = ""; NewEditor = EditorType.Text }, cmd

    | Delete uid ->
      let httpCalls () =
        async {
          do! Apis.technicalData.Delete uid

          return! (Apis.technicalData.GetAll ())
        }

      let cmd = Cmd.OfAsync.perform httpCalls () ColumnsLoaded

      { state with IsLoading = true }, cmd


  let init () = empty, Cmd.ofMsg LoadColumns

  [<ReactComponent>]
  let Render() =
    let state, dispatch = React.useElmish(init, update)

    React.fragment [
      Bulma.field.div [
        Bulma.label "Title"
        Bulma.input.text [
          prop.disabled state.IsLoading
          prop.value state.NewTitle
          prop.onTextChange (NewTitleChanged >> dispatch)
        ]
      ]
      Bulma.field.div [
        Bulma.label "Editor"
          
        Bulma.field.div [
          Checkradio.radio [
            prop.disabled state.IsLoading
            prop.id "editor-text"
            prop.name "Text"
            prop.isChecked (EditorType.Text = state.NewEditor)
            prop.onClick (fun _ -> EditorType.Text |> NewEditorChanged |> dispatch)
          ]
          Html.label [
            prop.htmlFor "editor-text"
            prop.text "Text"
          ]
          Checkradio.radio [
            prop.disabled state.IsLoading
            prop.id "editor-number"
            prop.name "radio"
            prop.isChecked (EditorType.Number = state.NewEditor)
            prop.onClick (fun _ -> EditorType.Number |> NewEditorChanged |> dispatch)
          ]
          Html.label [
            prop.htmlFor "editor-number"
            prop.text "Number"
          ]
        ]
      ]
      Bulma.field.div [
        Bulma.buttons [
          Bulma.button.button [
            prop.disabled (state.NewTitle = "" || state.IsLoading)
            prop.text "Create"
            prop.onClick (fun _ -> dispatch Create)
          ]
        ]
      ]

      Bulma.table [
        table.isFullWidth
        prop.children [
          Html.thead [
            Html.tr [
              Html.th ""
              Html.th "Title"
              Html.th "Editor"
            ]
          ]
          Html.tableBody [
            for (uid, column) in state.Columns |> List.filter(fun (_, td) -> not td.IsDeleted) do
              Html.tr [
                Html.td [
                  Bulma.buttons [
                    Bulma.button.button [
                      button.isSmall
                      prop.onClick (fun _ -> uid |> Delete |> dispatch)
                      prop.children [
                        Bulma.icon [ Html.i [ prop.className "fas fa-trash" ] ]
                      ]
                    ]
                  ]
                ]
                Html.td column.Title
                Html.td (column.Editor.ToString())
              ]
          ]
        ]
      ]
    ]