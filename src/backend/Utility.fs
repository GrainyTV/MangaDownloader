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

let assembleFeedUrl (url: string) : string =
    let mangadexId = Uri(url).Segments[2].TrimEnd('/')
    String.Format(Mangadex.FEED_ENDPOINT, mangadexId, Mangadex.FEED_BATCHSIZE)

let unpackJsonFromStream (parseFunction: string -> 'T) (inputStream: Stream) : 'T =
    use reader = new StreamReader(inputStream)
    parseFunction (reader.ReadToEnd())
