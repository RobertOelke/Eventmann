namespace Eventmann.Client.Page

open System
open Elmish
open Feliz
open Feliz.UseElmish
open Feliz.Bulma
open Eventmann.Client
open Eventmann.Shared
open Eventmann.Shared.MachineType

module MachineTypeCreate =

  [<ReactComponent>]
  let Render save cancel =
    let main, setMain = React.useState("")
    let sub, setSub = React.useState("")
    Bulma.box [
      Bulma.section [
        Bulma.field.div [
          Bulma.label "Main type"
          Bulma.input.text [
            prop.value main
            prop.onTextChange setMain
          ]
        ]
        Bulma.field.div [
          Bulma.label "Sub type"
          Bulma.input.text [
            prop.value sub
            prop.onTextChange setSub
          ]
        ]
        Bulma.field.div [
          Bulma.buttons [
            Bulma.button.button [
              if main = "" || sub = "" then
                prop.disabled true

              prop.text "Create"
              prop.onClick (fun _ -> save (main, sub))
            ]
            Bulma.button.button [
              prop.text "Cancel"
              prop.onClick (fun _ -> cancel ())
            ]
          ]
        ]
      ]
    ]

module MachineTypeDetails =

  type StateDif = {
    Original : MachineTypeDetail
    Current : MachineTypeDetail
  }

  type State = {
    IsLoading : bool
    Uid : Guid
    State : StateDif option
    NewExample : string
  }

  type Msg =
  | Load
  | Loaded of MachineTypeDetail option
  | NewExampleChanged of string
  | ChangeColour of string
  | ChangeSketch of int
  | ChangeConstruction of int
  | ChangeMontage of int
  | ChangeShipping of int
  | UpdateDuration
  | Update of MachineTypeCommand

  let init uid () = 
    let empty = {
      IsLoading = false
      Uid = uid
      State = None
      NewExample = ""
    }
    empty, Cmd.ofMsg Load

  let update msg state =
    let updateCurrent update =
      match state.State with
      | None -> state
      | Some s -> { state with State = Some { s with Current = (update s.Current)  } }

    match msg with
    | Load ->
      let loadData = Cmd.OfAsync.perform Apis.machineType.GetDetails state.Uid Loaded
      { state with IsLoading = true }, loadData
    | Loaded (Some data) ->
      { state with IsLoading = false; State = Some { Original = data; Current = data } }, Cmd.none
    | Loaded None ->
      { state with IsLoading = false; State = None }, Cmd.none
    | NewExampleChanged str ->
      { state with NewExample = str }, Cmd.none
    | ChangeColour colour ->
      updateCurrent (fun s -> { s with Colour = colour }), Cmd.none

    | ChangeSketch sketch ->
      updateCurrent (fun s -> { s with Sketch = sketch }), Cmd.none
    | ChangeConstruction construction ->
      updateCurrent (fun s -> { s with Construction = construction }), Cmd.none
    | ChangeMontage montage ->
      updateCurrent (fun s -> { s with Montage = montage }), Cmd.none
    | ChangeShipping shipping ->
      updateCurrent (fun s -> { s with Shipping = shipping }), Cmd.none

    | UpdateDuration ->
      let update () =
        async {
          match state.State with
          | Some { Original = o; Current = c } ->
            if (o.Sketch <> c.Sketch) then
              do! Apis.machineType.Update state.Uid (ChangeSketchDuration c.Sketch)

            if (o.Construction <> c.Construction) then
              do! Apis.machineType.Update state.Uid (ChangeConstructionDuration c.Construction)

            if (o.Montage <> c.Montage) then
              do! Apis.machineType.Update state.Uid (ChangeMontageDuration c.Montage)

            if (o.Shipping <> c.Shipping) then
              do! Apis.machineType.Update state.Uid (ChangeShippingDuration c.Shipping)

          | None -> ()

          return! Apis.machineType.GetDetails state.Uid
        }
      
      { state with IsLoading = true }, Cmd.OfAsync.perform update () Loaded

    | Update cmd ->
      let update () =
        async {
          do! Apis.machineType.Update state.Uid cmd
          return! Apis.machineType.GetDetails state.Uid
        }
      
      { state with IsLoading = true; NewExample = "" }, Cmd.OfAsync.perform update () Loaded

  [<ReactComponent>]
  let Render (uid : Guid) (close : unit -> unit) =
    let state, dispatch = React.useElmish(init uid, update)

    Bulma.box [
      Bulma.section [
        match (state.IsLoading, state.State) with
        | true, None -> Html.text "Please wait ..."
        | false, None -> Html.text "Not found"
        | loading, Some { Original = original; Current = current } ->
          Bulma.field.div [
            Bulma.label (sprintf "%s - %s" original.MainType original.SubType)

            Html.br []

            Bulma.label "Examples"
            Bulma.field.div [
              field.hasAddons
              prop.children [
                Bulma.control.div [
                  control.isExpanded
                  prop.children [
                    Bulma.input.text [
                      prop.disabled loading
                      prop.value state.NewExample
                      prop.onTextChange (NewExampleChanged >> dispatch)
                    ]
                  ]
                ]
                Bulma.control.div [
                  Bulma.button.button [
                    prop.disabled loading
                    prop.text "Add"
                    prop.onClick (fun _ -> dispatch (state.NewExample |> AddExample |> Update))
                  ]
                ]
              ]
            ]
            React.fragment (
              original.Examples
              |> List.map (fun example ->
                Bulma.field.div [
                  field.hasAddons
                  prop.children [
                    Bulma.control.div [
                      control.isExpanded
                      prop.text example
                    ]
                    Bulma.control.div [
                      Bulma.button.button [
                        prop.disabled loading
                        prop.text "Delete"
                        prop.onClick (fun _ -> dispatch (example |> RemoveExample |> Update))
                      ]
                    ]
                  ]
                ]
              )
            )

            Html.br []

            Bulma.label "Colour"
            Bulma.field.div [
              field.hasAddons
              prop.children [
                Bulma.control.div [
                  control.isExpanded
                  prop.children [
                    Bulma.input.text [
                      prop.disabled loading
                      prop.value current.Colour
                      prop.onTextChange (ChangeColour >> dispatch)
                    ]
                  ]
                ]
                Bulma.control.div [
                  Bulma.button.button [
                    prop.style [
                      style.color "white"
                      style.backgroundColor current.Colour
                    ]
                    prop.disabled (loading || current.Colour = original.Colour || current.Colour = "")
                    prop.text "Change"
                    prop.onClick (fun _ -> current.Colour |> MachineTypeCommand.ChangeColour |> Update |> dispatch)
                  ]
                ]
              ]
            ]
            
            Html.br []
            
            Bulma.label "Duration"

            let duration (label : string) (value : int) msg =
              React.fragment [
                Html.p label
                Bulma.field.div [
                  Bulma.input.number [
                    input.isSmall
                    prop.disabled loading
                    prop.value value
                    prop.onTextChange (int >> msg >> dispatch)
                  ]
                ]
              ]

            duration "Sketch" current.Sketch ChangeSketch
            duration "Construction" current.Construction ChangeConstruction
            duration "Montage" current.Montage ChangeMontage
            duration "Shipping" current.Shipping ChangeShipping
            
            Bulma.field.div [
              Bulma.buttons [
                Bulma.button.button [
                  prop.text "Update duration"
                  prop.onClick (fun _ -> dispatch UpdateDuration)
                ]
              ]
            ]

            Html.br []

            Bulma.field.div [
              Bulma.buttons [
                Bulma.button.button [
                  prop.text "Close"
                  prop.onClick (fun _ -> close ())
                ]
              ]
            ]
          ]
      ]
    ]

module MachineTypeHistory =
  
  let loadLog uid setter () =
    async {
      let! logs = Apis.machineType.GetLog uid
      setter logs
      return ()
    } |> Async.StartImmediate

  [<ReactComponent>]
  let Render (uid : Guid) (close : unit -> unit) =
    let log, setLog = React.useState([])

    React.useEffectOnce(loadLog uid setLog)

    Bulma.box [
      Bulma.section (log |> List.map (fun l -> Html.div [ Html.textf "%s- %s:  %s" (l.Date.ToShortDateString()) (l.Date.ToShortTimeString()) l.Action ]))
      Bulma.field.div [
        Bulma.buttons [
          Bulma.button.button [
            prop.text "Close"
            prop.onClick (fun _ -> close ())
          ]
        ]
      ]
    ]

module MachineTypePage =

  type Popup =
  | Empty
  | Create
  | History of Guid
  | Edit of Guid

  type State = {
    IsLoading : bool
    Data : MachineTypeOverview list
    Popup : Popup
  }

  let empty = {
    IsLoading = true
    Data = []
    Popup = Empty
  }

  type Msg =
  | Load
  | Loaded of MachineTypeOverview list
  | ShowCreate
  | FinishCreate of {| Main : string; Sub : string |}
  | ClosePopup
  | Delete of Guid
  | StartEdit of Guid
  | ShowHistory of Guid

  let update msg state = 
    match msg with
    | Load ->
      { state with IsLoading = true }, Cmd.OfAsync.perform Apis.machineType.GetAll () Loaded

    | Loaded lst ->
      { state with IsLoading = false; Data = lst }, Cmd.none

    | ShowCreate ->
      { state with Popup = Create }, Cmd.none
      
    | FinishCreate args ->
      let async () =
        async {
          do! Apis.machineType.Create {| MainType = args.Main; SubType = args.Sub |}
          return! Apis.machineType.GetAll()
        }

      { state with IsLoading = true; Popup = Empty }, Cmd.OfAsync.perform async () Loaded

    | Delete uid ->
      let async () =
        async {
          do! Apis.machineType.Update uid MachineTypeCommand.Delete
          return! Apis.machineType.GetAll()
        }

      { state with IsLoading = true; Popup = Empty }, Cmd.OfAsync.perform async () Loaded

    | ClosePopup ->
      { state with Popup = Empty }, Cmd.OfAsync.perform Apis.machineType.GetAll () Loaded

    | StartEdit uid ->
      { state with Popup = Edit uid }, Cmd.none

    | ShowHistory uid ->
      { state with Popup = History uid }, Cmd.none

  [<ReactComponent>]
  let Render() =
    let state, dispatch = React.useElmish((fun () -> (empty, Cmd.ofMsg Load)), update)

    if state.IsLoading then
      PageLoader.pageLoader [
        pageLoader.isSuccess
        pageLoader.isActive
        prop.children [
          PageLoader.title "Loading machine types"
        ]
      ]
    else
      React.fragment [
        Bulma.modal [
          match state.Popup with
          | Empty -> ()
          | _ -> modal.isActive

          prop.children [
            Bulma.modalBackground []
            Bulma.modalContent [
              match state.Popup with
              | Empty -> React.fragment []
              | Create -> 
                MachineTypeCreate.Render
                  (fun (m, s) -> {| Main = m; Sub = s |} |> FinishCreate |> dispatch)  
                  (fun () -> ClosePopup |> dispatch)
              | Edit uid ->
                MachineTypeDetails.Render
                  uid
                  (fun () -> ClosePopup |> dispatch)
              | History uid ->
                MachineTypeHistory.Render
                  uid
                  (fun () -> ClosePopup |> dispatch)
            ]
          ]
        ]

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
                      prop.onClick (fun _ -> dispatch ShowCreate)
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
              for machineType in state.Data do
                Html.tr [
                  Html.td [
                    Bulma.buttons [
                      Bulma.button.button [
                        button.isSmall
                        prop.onClick (fun _ -> machineType.Id |> StartEdit |> dispatch)
                        prop.children [
                          Bulma.icon [ Html.i [ prop.className "fas fa-pen" ] ]
                        ]
                      ]
                      Bulma.button.button [
                        button.isSmall
                        prop.onClick (fun _ -> machineType.Id |> Delete |> dispatch)
                        prop.children [
                          Bulma.icon [ Html.i [ prop.className "fas fa-trash" ] ]
                        ]
                      ]
                      Bulma.button.button [
                        button.isSmall
                        prop.onClick (fun _ -> machineType.Id |> ShowHistory |> dispatch)
                        prop.children [
                          Bulma.icon [ Html.i [ prop.className "fas fa-clock-rotate-left" ] ]
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
      ]
      