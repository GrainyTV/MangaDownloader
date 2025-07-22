module Config

open Avalonia
open Avalonia.Controls
open System

let runApp (app: Application) (_: array<String>) : Unit =
    app.Styles.Add(Styles.ExtendedSimpleTheme())
    app.Run(ApplicationWindow.Window("Manga Downloader", 960, 540))
