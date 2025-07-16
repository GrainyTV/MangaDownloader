module Program

open CommonTypes
open System
open System.IO
open Network

type UserRequest = {
    Title: string
    Url: string
    FirstChapter: int
    FinalChapter: int
}

let EMPTY_REQUEST = {
    Title = String.Empty
    Url = String.Empty
    FirstChapter = 0
    FinalChapter = 0
}

let separateMissingAndFoundChapters (needed: seq<int>) (have: seq<Chapter>) : seq<Chapter> * seq<int> =
    let neededSet = needed |> Set.ofSeq
    let haveSet = have |> Seq.map (fun ch -> ch.Number) |> Set.ofSeq

    let found = have |> Seq.filter (fun ch -> neededSet.Contains ch.Number)
    let missing = Set.difference neededSet haveSet

    found, missing

let createChaptersFrom (request: UserRequest) : Async<Unit> =
    async {
        use network = new NetworkHelper ()

        let chapterRange = { request.FirstChapter .. request.FinalChapter }
        let feedUrl = Utility.assembleFeedUrl (request.Url)
        let! availableChapters = collectChaptersFromFeedAsync (network) (feedUrl)

        if availableChapters.IsError() then
            let exn = availableChapters.UnwrapError()
            printfn "Could not collect chapters from feed"
            printfn $"==> {exn.Message}"

        else
            let foundChapters, missingChapters = separateMissingAndFoundChapters (chapterRange) (availableChapters.Unwrap())

            if Seq.isEmpty foundChapters then
                printfn $"No available chapters found for: {request.Url}"
                printfn "==> Check for any typos and ensure the requested chapters do exist"

            else
                use rateLimiter = getAthomeRateLimiter ()

                return! foundChapters
                    |> Seq.map (fun chapter -> async {
                        try
                            let tempDir = Utility.joinPathsUnix [| request.Title; string chapter.Number |]
                            Directory.CreateDirectory (tempDir) |> ignore
                            use! lease = rateLimiter.AcquireAsync().AsTask() |> Async.AwaitTask

                            // ┏━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━┓
                            // ┃ Process will wait here after exhausting the quota ┃
                            // ┗━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━┛

                            let! imageUrls = collectImageUrlsOfChapterAsync (network) (chapter.MangadexId)

                            let finalizedChapter = {
                                chapter with
                                    Id = $"{request.Title} Chapter {chapter.Number}"
                                    ImageInfo = {
                                        Remote = imageUrls
                                        Local = Array.init imageUrls.Length (fun i -> joinPathsUnix [| request.Title; string chapter.Number; i.ToString "D5" |])
                                    }
                            }

                            do! startImageDownloadsAsync (network) (finalizedChapter)
                            Pdf.generateNew (finalizedChapter) (request.Title)
                            Directory.Delete(tempDir, true)

                        with
                        | exn ->
                            printfn $"Generation of chapter {chapter.Number} failed"
                            printfn $"==> {exn.Message}"
                    })
                    |> fun computations -> Async.Parallel(computations, Environment.ProcessorCount)
                    |> Async.Ignore
    }
