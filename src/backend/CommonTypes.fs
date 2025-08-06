module CommonTypes

open System

type UserRequest = {
    Title: string
    Url: string
    ChapterRange: Option<seq<Int32>>
    //FirstChapter: int
    //FinalChapter: int
}

let EMPTY_REQUEST = {
    Title = String.Empty
    Url = String.Empty
    ChapterRange = None
    //FirstChapter = 0
    //FinalChapter = 0
}

type ImageInfo = {
    Remote: array<string>
    Local: array<string>
}

type Chapter =
    { MangadexId: string
      Number: int
      ScanlationGroups: seq<string>
      ImageInfo: ImageInfo }

    override self.ToString (): String =
        $"""{{
    Chapter: {self.Number},
    MangadexId: "{self.MangadexId}"
}}
"""

let EMPTY_CHAPTER = {
    MangadexId = String.Empty
    Number = 0
    ScanlationGroups = Seq.empty
    ImageInfo = {
        Remote = Array.empty
        Local = Array.empty
    }
}

type IBackendApplicable<'T> =
    abstract member apply : 'T -> Unit

type RequestResult =
    | TotalFailure
    | PartialSuccess
    | PerfectCompletion

type ForeignUIElements =
// Type used to collect properties of UI elements
// that need to be updated from the backend
// probably from a different thread as well
    { ProgressBar: IBackendApplicable<Double>
      FinalVerdict: IBackendApplicable<RequestResult> }
