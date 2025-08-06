module Network

open CommonTypes
open LinkDotNet.StringBuilder
open System
open System.Collections.Concurrent
open System.IO
open System.Net.Http
open System.Threading.Tasks
// open System.Threading.RateLimiting

let startImageDownloadsAsync (*(network: NetworkHelper)*) (chapter: Chapter) : Async<Unit> =
    Seq.zip chapter.ImageInfo.Remote chapter.ImageInfo.Local
    |> Seq.map (fun (url, file) ->
        async {
            use! inputStream = AppScope.NETWORK.sendGetRequestAsync (url)
            use outputStream = new FileStream(file, FileMode.Create)
            do! inputStream.CopyToAsync(outputStream) |> Async.AwaitTask
        })
    |> Async.Parallel
    |> Async.Ignore

let collectImageUrlsOfChapterAsync (*(network: NetworkHelper)*) (chapterId: string) : Async<array<string>> =
    async {
        let athomeUrl = String.Format(Mangadex.IMAGE_ENDPOINT, chapterId)
        let! response = AppScope.NETWORK.sendGetRequestAsync (athomeUrl)

        let content = unpackJsonFromStream (Mangadex.MangadexImages.Parse) (response)
        let baseUrl = joinPathsUnix [| content.BaseUrl; "data"; content.Chapter.Hash |]
        return content.Chapter.Data |> Array.map (fun imageId -> joinPathsUnix [| baseUrl; imageId |])
    }

[<TailCall>]
let rec private collectChaptersFromFeedPaginatedAsync
    (*(network: NetworkHelper)*)
    (feedUrl: string)
    (i: int)
    (cachedTotal: Option<int>)
    (foundEntries: seq<Chapter>) : Async<Result<seq<Chapter>, exn>> =
    async {
        let feedUrlWithOffset = ValueStringBuilder.Concat(feedUrl, "&offset=", i * Mangadex.FEED_BATCHSIZE)

        try
            let! response = AppScope.NETWORK.sendGetRequestAsync (feedUrlWithOffset)
            let content = Utility.unpackJsonFromStream (Mangadex.MangadexFeed.Parse) (response)
            let total = if cachedTotal.IsSome then cachedTotal else Option.Some(int content.Total)

            let newEntries =
                content.Data
                |> Seq.filter(fun json -> json.Attributes.Chapter.Contains('.') = false)
                |> Seq.map(fun json ->
                    { EMPTY_CHAPTER with
                        MangadexId = json.Id
                        Number = int json.Attributes.Chapter
                        ScanlationGroups =
                            json.Relationships
                            |> Seq.filter (fun rel -> rel.Type.Equals "scanlation_group")
                            |> Seq.map (fun rel -> rel.Id)
                    })

            let allEntries = Seq.append foundEntries newEntries

            if (i + 1) * Mangadex.FEED_BATCHSIZE >= total.Value then
                return Ok allEntries

            else
                return! collectChaptersFromFeedPaginatedAsync (*(network)*) (feedUrl) (i + 1) (total) (allEntries)

        with
        | exn -> return Error (exn.GetBaseException())
    }

let collectChaptersFromFeedAsync (*(network: NetworkHelper)*) (feedUrl: string) =
    collectChaptersFromFeedPaginatedAsync (*(network)*) (feedUrl) (0) (Option.None) (Seq.empty)

let collectInfoOnChapterAsync (*(network: NetworkHelper)*) (chapterUrl: String): Async<Result<Chapter, exn>> =
    async {
        try
            let! response = AppScope.NETWORK.sendGetRequestAsync (chapterUrl)
            let content = unpackJsonFromStream (Mangadex.MangadexChapter.Parse) (response)

            return Ok
                { EMPTY_CHAPTER with
                    MangadexId = content.Data.Id
                    Number = int content.Data.Attributes.Chapter
                    ScanlationGroups =
                        content.Data.Relationships
                        |> Array.filter (fun rel -> rel.Type.Equals "scanlation_group")
                        |> Array.map (fun rel -> rel.Id)
                }

        with
        | exn -> return Error (exn.GetBaseException())
    }

let collectScanlatorGroups (*(network: NetworkHelper)*) (ids: seq<String>): Async<array<String>> =
    async {
        try
            let cache = ConcurrentDictionary<String, String>()

            let findScanlatorById (id: String): Async<String> =
                async {
                    if not (cache.ContainsKey (id)) then
                        let! response = AppScope.NETWORK.sendGetRequestAsync (String.Format(Mangadex.SCANLATOR_ENDPOINT, id))
                        let content = unpackJsonFromStream (Mangadex.MangadexScanlator.Parse) (response)
                        let scanlator = content.Data.Attributes.Name

                        cache[id] <- scanlator
                        return scanlator

                    else
                        return cache[id]
                }

            return! ids
            |> Seq.map (findScanlatorById)
            |> fun computations -> Async.Parallel(computations, Environment.ProcessorCount)

        with
        | exn -> printfn $"{exn.Message}"; return Array.empty
    }
