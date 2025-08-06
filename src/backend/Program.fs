module Program

open CommonTypes
open System
open System.IO
open System.Threading.RateLimiting
open Network

let private separateMissingAndFoundChapters (needed: seq<Int32>) (have: seq<Int32>) : Set<Int32> * Set<Int32> =
    let neededSet = needed |> Set.ofSeq
    let haveSet = have |> Set.ofSeq

    let found = Set.intersect haveSet neededSet
    let missing = Set.difference neededSet haveSet

    assert (neededSet.Count = found.Count + missing.Count)

    found, missing

let private processChapter
    (chapter: Chapter)
    (request: UserRequest)
    (ui: ForeignUIElements)
    (rateLimiter: FixedWindowRateLimiter)
    (progressChunk: Double) : Async<Option<exn * Chapter>> =
    //(network: NetworkHelper)
    // (workDir: String): Async<Option<exn * Chapter>> =
    async {
        let tempImageDir = joinPathsUnix [| AppScope.WORKING_DIRECTORY; string chapter.Number |]
        Directory.CreateDirectory (tempImageDir) |> ignore

        try
            try
                // ┏━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━┓
                // ┃ Process will wait here after exhausting the quota ┃
                // ┗━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━┛
                use! lease = rateLimiter.AcquireAsync().AsTask() |> Async.AwaitTask

                // ┏━━━━━━━━━┓
                // ┃ Step #2 ┃
                // ┗━━━━━━━━━┛
                let! imageUrls = collectImageUrlsOfChapterAsync (chapter.MangadexId)
                ui.ProgressBar.apply (progressChunk)

                let finalizedChapter = {
                    chapter with
                        ImageInfo = {
                            Remote = imageUrls
                            Local = Array.init imageUrls.Length (fun i -> joinPathsUnix [| tempImageDir; i.ToString "D5" |])
                        }
                }

                // ┏━━━━━━━━━┓
                // ┃ Step #3 ┃
                // ┗━━━━━━━━━┛
                do! startImageDownloadsAsync (finalizedChapter)
                ui.ProgressBar.apply (progressChunk)

                // ┏━━━━━━━━━┓
                // ┃ Step #4 ┃
                // ┗━━━━━━━━━┛
                Pdf.generateNew (finalizedChapter) (request.Title)
                ui.ProgressBar.apply (progressChunk)

                return None

            with
            | exn -> return Some (exn.GetBaseException(), chapter)

        finally
            // ┏━━━━━━━━━┓
            // ┃ Step #5 ┃
            // ┗━━━━━━━━━┛
            Directory.Delete(tempImageDir, true)
            ui.ProgressBar.apply (progressChunk)
    }

let private beginChapterGeneration
    (chapters: seq<Chapter>)
    (missingChapters: Set<Int32>)
    (request: UserRequest)
    (ui: ForeignUIElements): Async<Unit> =
    (*(network: NetworkHelper)*)
    //(workDir: String): Async<Unit> =
    async {
        use rateLimiter = new FixedWindowRateLimiter (AppScope.ATHOME_RATELIMIT_OPTIONS)

        let steps = 4
        let chapterCount = Seq.length chapters
        let progressChunk = 90.0 / double chapterCount / double steps

        let! results =
            chapters
            |> Seq.map (fun chapter -> processChapter chapter request ui rateLimiter progressChunk)
            |> fun computations -> Async.Parallel(computations, Environment.ProcessorCount)

        let failedChapters =
            results
            |> Array.filter (fun err -> err.IsSome)
            |> Array.map (fun err -> err.Value)

        failedChapters |> Seq.iter (fun (exn, ch) ->
            Errors.log (Errors.FailedChapterGen ({
                Issue = "Failed to generate chapter"
                Suggestion = "Check for any errors in the input, then open an issue if you think it is an application error"
                Exception = exn.Message }, ch)))

        missingChapters |> Seq.iter (fun num ->
            Errors.log (Errors.MissingChapter ({
                Issue = "Could not find chapter"
                Suggestion = "Ensure that the chapter is available on the site" }, num)))

        match (failedChapters.Length, missingChapters.Count) with
        | (0, 0) -> ui.FinalVerdict.apply (PerfectCompletion)
        | (failedCount, _) when failedCount = chapterCount -> ui.FinalVerdict.apply (TotalFailure)
        | _ -> ui.FinalVerdict.apply (PartialSuccess)
    }

let private createMultipleChapters
    (request: UserRequest)
    (ui: ForeignUIElements) : Async<Unit> =
    async {
        // use network = new NetworkHelper ()
        //let baseDir = Directory.CreateDirectory (request.Title)

        let feedUrl = Utility.assembleFeedUrl (request.Url)
        let chaptersUserWants = request.ChapterRange.Value

        // ┏━━━━━━━━━┓
        // ┃ Step #0 ┃
        // ┗━━━━━━━━━┛
        let! feedResult = collectChaptersFromFeedAsync (feedUrl)
        ui.ProgressBar.apply (5)

        match feedResult with
        | Ok chaptersAvailable ->
            // ┏━━━━━━━━━┓
            // ┃ Step #1 ┃
            // ┗━━━━━━━━━┛
            let foundChapters, missingChapters = separateMissingAndFoundChapters (chaptersUserWants) (chaptersAvailable |> Seq.map _.Number)
            ui.ProgressBar.apply (5)

            if not (foundChapters.IsEmpty) then
                let chapters = chaptersAvailable |> Seq.filter (fun ch -> foundChapters.Contains ch.Number)
                do! beginChapterGeneration (chapters) (missingChapters) (request) (ui) //(baseDir.Name)

            else
                Errors.log (Errors.NoAvailableChapters {
                    Issue = "No available chapters found for input"
                    Suggestion = "Check for any typos and ensure the requested chapters do exist" })

                ui.FinalVerdict.apply (TotalFailure)

        | Error exn ->
            Errors.log (Errors.CouldNotAccessFeed {
                Issue = "Could not access chapter feed"
                Suggestion = "Ensure you have internet connectivity and the site is online"
                Exception = exn.Message })

            ui.FinalVerdict.apply (TotalFailure)
    }

let private createSingleChapter
    (request: UserRequest)
    (ui: ForeignUIElements) : Async<Unit> =
    async {
        // use network = new NetworkHelper ()
        // let baseDir = Directory.CreateDirectory (request.Title)
        // AppScope.WORKING_DIRECTORY <- baseDir.Name


        let chapterUrl = assembleChapterUrl (request.Url)
        let! infoResult = collectInfoOnChapterAsync (*(network)*) (chapterUrl)

        match infoResult with
        | Ok chapter ->
            // let! scanlators = collectScanlatorGroups (*(network)*) (chapter.ScanlationGroups)
            // let finalChapter = { chapter with ScanlationGroups = scanlators }
            do! beginChapterGeneration [| chapter |] (Set.empty) (request) (ui) (*(network)*) //(baseDir.Name)

        | Error exn ->
            Errors.log (Errors.CouldNotAccessFeed {
                Issue = "Could not access chapter feed"
                Suggestion = "Ensure you have internet connectivity and the site is online"
                Exception = exn.Message })

            ui.FinalVerdict.apply (TotalFailure)
    }

let createChaptersFrom
    (request: UserRequest)
    (ui: ForeignUIElements)
    (endProcess: Unit -> Unit) : Async<Unit> =
    async {
        try
            let workingDir = Directory.CreateDirectory (request.Title)
            AppScope.WORKING_DIRECTORY <- workingDir.Name

            match request.ChapterRange with
            | Some _ -> do! createMultipleChapters (request) (ui)
            | None -> do! createSingleChapter (request) (ui)

        with
        | exn ->
            Errors.log (Errors.FailedToCreateDir {
                Issue = "Failed to create directory"
                Suggestion = "Ensure you have write permission for the designated directory"
                Exception = exn.GetBaseException().Message })

        endProcess()
    }
