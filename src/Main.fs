namespace MangaDownloader

open Avalonia
open Avalonia.Controls
open Avalonia.Styling
open System

module EntryPoint =
    [<EntryPoint>]
    [<STAThread>]
    let main (args: array<string>): int =
        let appMain (app: Application) (_: array<String>): Unit =
            app.Styles.Add(Styles.ExtendedSimpleTheme())
            app.Resources.ThemeDictionaries.Add(ThemeVariant.Light, Styles.lightThemeResources())
            app.Resources.ThemeDictionaries.Add(ThemeVariant.Dark, Styles.darkThemeResources())
            app.RunWithMainWindow<MainWindow>()

        AppBuilder
            .Configure()
            .ConfigureFonts(Fonts.registerCustom)
            .EnableDebugMessagesIfNeeded()
            .UsePlatformDetect()
            .UseWayland()
            .Start(appMain, args)

        0
