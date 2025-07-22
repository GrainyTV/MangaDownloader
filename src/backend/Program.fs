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

let createChaptersFrom
    (request: UserRequest)
    (progressBar: IBackendApplicable<double>)
    (finalVerdict: IBackendApplicable<RequestResult>)
    (endProcess: Unit -> Unit) : Async<Unit> =
    async {
        progressBar.apply(0)
        use network = new NetworkHelper ()

        let chapterRange = { request.FirstChapter .. request.FinalChapter }
        let feedUrl = Utility.assembleFeedUrl (request.Url)
        let! availableChapters = collectChaptersFromFeedAsync (network) (feedUrl)

        if availableChapters.IsError() then
            let exn = availableChapters.UnwrapError()
            printfn "Could not collect chapters from feed"
            printfn $"==> {exn.Message}"
            finalVerdict.apply (TotalFailure)

        else
            progressBar.apply(10)
            let foundChapters, missingChapters = separateMissingAndFoundChapters (chapterRange) (availableChapters.Unwrap())

            if Seq.isEmpty foundChapters then
                printfn $"No available chapters found for: {request.Url}"
                printfn "==> Check for any typos and ensure the requested chapters do exist"
                finalVerdict.apply (TotalFailure)

            else
                let steps = 4
                let progressChunk = 90.0 / double (Seq.length foundChapters) / double steps
                use rateLimiter = getAthomeRateLimiter ()

                let processChapter (chapter: Chapter) : Async<Unit> =
                    async {
                        try
                            let tempDir = Utility.joinPathsUnix [| request.Title; string chapter.Number |]
                            Directory.CreateDirectory (tempDir) |> ignore
                            use! lease = rateLimiter.AcquireAsync().AsTask() |> Async.AwaitTask

                            // ┏━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━┓
                            // ┃ Process will wait here after exhausting the quota ┃
                            // ┗━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━┛

                            // Step #1

                            let! imageUrls = collectImageUrlsOfChapterAsync (network) (chapter.MangadexId)
                            progressBar.apply(progressChunk)

                            let finalizedChapter = {
                                chapter with
                                    Id = $"{request.Title} Chapter {chapter.Number}"
                                    ImageInfo = {
                                        Remote = imageUrls
                                        Local = Array.init imageUrls.Length (fun i -> joinPathsUnix [| request.Title; string chapter.Number; i.ToString "D5" |])
                                    }
                            }

                            // Step #2

                            do! startImageDownloadsAsync (network) (finalizedChapter)
                            progressBar.apply(progressChunk)

                            // Step #3

                            Pdf.generateNew (finalizedChapter) (request.Title)
                            progressBar.apply(progressChunk)

                            // Step #4

                            Directory.Delete(tempDir, true)
                            progressBar.apply (progressChunk)

                        with
                        | exn ->
                            printfn $"Generation of chapter {chapter.Number} failed"
                            printfn $"==> {exn.Message}"
                    }

                do! foundChapters
                    |> Seq.map (processChapter)
                    |> fun computations -> Async.Parallel(computations, Environment.ProcessorCount)
                    |> Async.Ignore

                finalVerdict.apply(PerfectCompletion)

        endProcess()
    }
