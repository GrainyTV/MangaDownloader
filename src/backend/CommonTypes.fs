module CommonTypes

open System

type ChapterEntry(number: int, title: string, leadingZeroCount: int) =
    member self.Number: int = number
    member self.TemporaryImageDirectory: string = joinPathsUnix ([ title; string self.Number ])
    member self.OutFile: string = joinPathsUnix ([ title; String.Format("{0} Chapter {1}.pdf", title, self.Number.ToString($"D{leadingZeroCount}")) ])

    member val Id: string = String.Empty with get, set
    member val RemoteFiles: array<string> = Array.empty with get, set
    member val LocalFiles: array<string> = Array.empty with get, set
