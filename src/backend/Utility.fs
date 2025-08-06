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

let extractIdFromUrl (url: String) : String = Uri(url).Segments[2].TrimEnd('/')

let assembleFeedUrl (url: string) : string =
    let titleId = extractIdFromUrl (url)
    String.Format(Mangadex.FEED_ENDPOINT, titleId, Mangadex.FEED_BATCHSIZE)

let assembleChapterUrl (url: String) : String =
    let chapterId = extractIdFromUrl (url)
    String.Format(Mangadex.CHAPTER_ENDPOINT, chapterId)

let unpackJsonFromStream (parseFunction: string -> 'T) (inputStream: Stream) : 'T =
    use reader = new StreamReader(inputStream)
    parseFunction (reader.ReadToEnd())

let quotedStringList(entries: seq<String>): String =
    entries
    |> Seq.map (fun entry -> $"\"{entry}\"")
    |> String.concat " "

