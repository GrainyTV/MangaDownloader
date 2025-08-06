[<AutoOpen>]
module Extensions

open Avalonia
open Avalonia.Styling
open PdfSharp.Pdf
open System
open System.Collections.Generic
open System.Diagnostics
open System.IO

type String with
    member self.IsNotEmpty : Boolean = not (self.Length = 0)

type PdfDocument with
    [<TailCall>]
    member self.AddPages(howMany: int) : Unit =
        if howMany > 0 then
            self.AddPage() |> ignore
            self.AddPages(howMany - 1)

type AppBuilder with
    member self.EnableDebugMessagesIfNeeded(): AppBuilder =
    #if DEBUG
        Trace.Listeners.Add(new TextWriterTraceListener(Console.Error)) |> ignore
        Trace.AutoFlush <- true
        self.LogToTrace() |> ignore
    #endif
        self

type Style with
    member self.AddRange(entries: seq<AvaloniaProperty * Object>) : Unit =
        entries |> Seq.iter (fun (prop, value) -> self.Add (Setter(prop, value)))
