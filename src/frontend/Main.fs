module Main

open Avalonia
open System

[<EntryPoint>]
[<STAThread>]
let main (args: array<string>) : int =
    AppBuilder
        .Configure()
        .ConfigureFonts(Fonts.registerCustom)
        .EnableDebugMessagesIfNeeded()
        .UsePlatformDetect()
        .Start(Config.runApp, args)

    0
