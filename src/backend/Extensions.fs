[<AutoOpen>]
module Extensions

open System.Diagnostics
open System.IO
open PdfSharp.Pdf

type Result<'T, 'Error> with
    member self.IsOk() : bool =
        Result.isOk(self)

    member self.IsError() : bool =
        not (self.IsOk())

    member self.Unwrap() : 'T =
        match self with
        | Ok value -> value
        | Error _ -> raise (UnreachableException("Tried to access value of result in Error state."))

type Stream with
    member self.AsString() : string =
        use reader = new StreamReader(self)
        reader.ReadToEnd()

type PdfDocument with
    [<TailCall>]
    member self.AddPages(howMany: int) : Unit =
        if howMany > 0 then
            self.AddPage() |> ignore
            self.AddPages(howMany - 1)
