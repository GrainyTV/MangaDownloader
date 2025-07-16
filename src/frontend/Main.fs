module Main

open Avalonia
open Avalonia.Controls
open Avalonia.Controls.ApplicationLifetimes
open System
open System.Diagnostics

type MyApp() =
    inherit Application()

    override self.OnFrameworkInitializationCompleted () : Unit =
        self.Styles.Add(Styles.MaterialLike())

        let window = DefaultWindow.DefaultWindow(960, 540, "Manga Downloader")
        let application = self.ApplicationLifetime :?> IClassicDesktopStyleApplicationLifetime
        
        application.MainWindow <- window.Layout
        application.MainWindow.Show()
        
        base.OnFrameworkInitializationCompleted()


type AppBuilder with
    member self.WithSoftwareRenderer(): AppBuilder =
        if OperatingSystem.IsWindows() then
            self.With(Win32PlatformOptions(RenderingMode = [Win32RenderingMode.Software]))

        elif OperatingSystem.IsMacOS() then
            self.With(AvaloniaNativePlatformOptions(RenderingMode = [AvaloniaNativeRenderingMode.Software]))

        elif OperatingSystem.IsLinux() then
            self.With(X11PlatformOptions(RenderingMode = [X11RenderingMode.Software]))

        else
            raise (UnreachableException("Unsupported OS detected"))

    member self.EnableDebugMessages(): AppBuilder =
    #if DEBUG
        Trace.Listeners.Add(new TextWriterTraceListener(Console.Out)) |> ignore
        Trace.AutoFlush <- true
        self.LogToTrace() |> ignore
    #endif
        self

[<EntryPoint;STAThread>]
let main (args: array<string>) : int =
    AppBuilder
        .Configure<MyApp>()
        .ConfigureFonts(Fonts.registerCustom)
        .EnableDebugMessages()
        //.WithSoftwareRenderer()
        .UsePlatformDetect()
        .StartWithClassicDesktopLifetime(args)
