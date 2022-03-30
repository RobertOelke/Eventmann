namespace Eventmann.Client

open Feliz
open Browser.Dom
open Fable.Core.JsInterop

module Main =

    importSideEffects "./styles/global.scss"

    ReactDOM.render(
        App.Render(),
        document.getElementById "root"
    )