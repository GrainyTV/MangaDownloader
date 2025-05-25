module Main

open Avalonia
open Avalonia.Controls
open System

let private appMain (app: Application) (_: array<string>) : Unit =
    app.Styles.Add(Styles.MaterialLike())
    app.RequestedThemeVariant <- Styling.ThemeVariant.Default
    app.Run(DefaultWindow.DefaultWindow(960, 540, "Manga Downloader"))

[<EntryPoint;STAThread>]
let main (args: array<string>) : int =
    AppBuilder
        .Configure()
        .ConfigureFonts(Fonts.registerCustom)
        .UsePlatformDetect()
        .Start(appMain, args)

    0
