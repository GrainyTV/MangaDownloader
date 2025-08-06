module Config

open Avalonia
open Avalonia.Controls
open Avalonia.Styling
open System

let runApp (app: Application) (_: array<String>) : Unit =
    app.Styles.Add(Styles.ExtendedSimpleTheme())
    app.Resources.ThemeDictionaries.Add(ThemeVariant.Light, Styles.lightThemeResources())
    app.Resources.ThemeDictionaries.Add(ThemeVariant.Dark, Styles.darkThemeResources())
    app.Run(ApplicationWindow.Window("Manga Downloader", 960, 540))
