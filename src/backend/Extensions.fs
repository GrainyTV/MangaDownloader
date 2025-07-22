[<AutoOpen>]
module Extensions

open Avalonia
open PdfSharp.Pdf
open System
open System.Diagnostics
open System.IO

type String with
    member self.IsEmpty : Boolean = self.Length = 0

#nowarn "FS0025"
type Result<'T, 'TError> with
    member self.IsOk() : bool =
        Result.isOk(self)

    member self.IsError() : bool =
        not (self.IsOk())

    member self.Unwrap() : 'T =
        assert (self.IsOk ())
        let (Ok unwrapped) = self
        unwrapped

    member self.UnwrapError() : 'TError =
        assert (self.IsError ())
        let (Error error) = self
        error

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
