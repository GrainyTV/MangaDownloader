module CommonTypes

open System

type ImageInfo = {
    Remote: array<string>
    Local: array<string>
}

type Chapter = {
    Id: string
    MangadexId: string
    Number: int
    ScanlationGroups: seq<string>
    ImageInfo: ImageInfo
}

let EMPTY_CHAPTER = {
    Id = String.Empty
    MangadexId = String.Empty
    Number = 0
    ScanlationGroups = Seq.empty
    ImageInfo = {
        Remote = Array.empty
        Local = Array.empty
    }
}
