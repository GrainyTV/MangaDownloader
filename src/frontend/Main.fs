module Main

open Avalonia
open Avalonia.Controls
open Avalonia.Controls.ApplicationLifetimes
open System
open System.Diagnostics

type MyApp() =
    inherit Application()

    override self.OnFrameworkInitializationCompleted () : Unit =
        match self.ApplicationLifetime with
        | :? IClassicDesktopStyleApplicationLifetime as desktop ->
            self.Styles.Add(Styles.MaterialLike())

            let window = DefaultWindow.DefaultWindow(960, 540, "Manga Downloader")
            desktop.MainWindow <- window.Layout
            desktop.MainWindow.Show()

        | _ -> raise (UnreachableException("Application should have been started as ClassicDesktopStyle"))

        base.OnFrameworkInitializationCompleted()

[<EntryPoint;STAThread>]
let main (args: array<string>) : int =
    Trace.Listeners.Add(new TextWriterTraceListener(Console.Out)) |> ignore
    Trace.AutoFlush <- true

    AppBuilder
        .Configure<MyApp>()
        .ConfigureFonts(Fonts.registerCustom)
        .With(X11PlatformOptions(RenderingMode = [ X11RenderingMode.Software ]))
        .UsePlatformDetect()
        .LogToTrace()
        .StartWithClassicDesktopLifetime(args)
