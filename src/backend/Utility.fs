[<AutoOpen>]
module Utility

open LinkDotNet.StringBuilder
open System
open System.IO

[<Literal>]
let private UNIX_SEPARATOR = '/'

let joinPathsUnix (entries: seq<string>) : string =
    use builder = new ValueStringBuilder()
    builder.AppendJoin(UNIX_SEPARATOR, entries)
    builder.ToString ()

let createDirectoryIfNeeded (path: string) : Unit =
    if Directory.Exists(path) = false then
        Directory.CreateDirectory(path) |> ignore

//let addLeadingZerosIfNecessary (chapter: int) (total: int) : string =
//    chapter.ToString($"D{(string total).Length}")

let assembleIntermediateFeedUrl (url: string) (limit: int) : string =
    let mangaId = Uri(url).Segments[2].TrimEnd('/')
    String.Format(Mangadex.FEED_ENDPOINT, mangaId, limit)

let calculateLeadingZeros (from: int) : int = (string from).Length

let unpackJsonFromStream (parseFunction: string -> 'T) (inputStream: Stream) : 'T =
    use stream = inputStream
    let content = stream.AsString()
    parseFunction (content)
