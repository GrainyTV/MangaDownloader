module Network

open CommonTypes
open LinkDotNet.StringBuilder
open System
open System.IO
open System.Net.Http
open System.Threading.Tasks
open System.Threading.RateLimiting

// ┏━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━┓
// ┃ User-Agent yoinked from latest Thorium Browser ┃
// ┗━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━┛
[<Literal>]
let private USER_AGENT = "Mozilla/5.0 (X11; Linux x86_64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/130.0.0.0 Safari/537.36"

// ┏━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━┓
// ┃ Ratelimit options populated from API docs                                  ┃
// ┃ https://api.mangadex.org/docs/2-limitations/#endpoint-specific-rate-limits ┃
// ┗━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━┛
let private ATHOME_RATELIMIT_OPTIONS = FixedWindowRateLimiterOptions (
    AutoReplenishment = true,
    PermitLimit = 40,
    QueueLimit = Int32.MaxValue,
    QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
    Window = TimeSpan.FromMinutes 1
)

type NetworkHelper() =
    let httpClient = new HttpClient()

    do
        httpClient.DefaultRequestHeaders.Add("User-Agent", USER_AGENT)

    member self.sendGetRequestAsync (url: string) : Async<Stream> =
        async {
            use request = new HttpRequestMessage(HttpMethod.Get, url)
            let! response = httpClient.SendAsync(request) |> Async.AwaitTask
            response.EnsureSuccessStatusCode() |> ignore

            let! content = response.Content.ReadAsStreamAsync() |> Async.AwaitTask
            return content
        }

    interface IDisposable with
        member self.Dispose (): Unit =
            httpClient.Dispose()

let getAthomeRateLimiter () = new FixedWindowRateLimiter(ATHOME_RATELIMIT_OPTIONS)

let startImageDownloadsAsync (network: NetworkHelper) (chapter: Chapter) : Async<Unit> =
    Seq.zip chapter.ImageInfo.Remote chapter.ImageInfo.Local
    |> Seq.map (fun (url, file) ->
        async {
            use! inputStream = network.sendGetRequestAsync (url)
            use outputStream = new FileStream(file, FileMode.Create)
            do! inputStream.CopyToAsync(outputStream) |> Async.AwaitTask
        })
    |> Async.Parallel
    |> Async.Ignore

let collectImageUrlsOfChapterAsync (network: NetworkHelper) (chapterId: string) : Async<array<string>> =
    async {
        let athomeUrl = joinPathsUnix [| Mangadex.IMAGE_ENDPOINT; chapterId |]
        let! response = network.sendGetRequestAsync (athomeUrl)

        let content = Utility.unpackJsonFromStream (Mangadex.MangadexImages.Parse) (response)
        let baseUrl = joinPathsUnix [| content.BaseUrl; "data"; content.Chapter.Hash |]
        return content.Chapter.Data |> Array.map (fun imageId -> joinPathsUnix [| baseUrl; imageId |])
    }

[<TailCall>]
let rec private collectChaptersFromFeedPaginatedAsync
    (network: NetworkHelper)
    (feedUrl: string)
    (i: int)
    (cachedTotal: Option<int>)
    (foundEntries: seq<Chapter>) : Async<Result<seq<Chapter>, exn>> =
    async {
        let feedUrlWithOffset = ValueStringBuilder.Concat(feedUrl, "&offset=", i * Mangadex.FEED_BATCHSIZE)

        try
            let! response = network.sendGetRequestAsync (feedUrlWithOffset)
            let content = Utility.unpackJsonFromStream (Mangadex.MangadexFeed.Parse) (response)
            let total = if cachedTotal.IsSome then cachedTotal else Option.Some(int content.Total)

            let newEntries =
                content.Data
                |> Seq.filter(fun json ->
                    // ┏━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━┓
                    // ┃ We do not care about non-integer chapters ┃
                    // ┗━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━┛
                    json.Attributes.Chapter.Contains('.') = false
                )
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
                return! collectChaptersFromFeedPaginatedAsync (network) (feedUrl) (i + 1) (total) (allEntries)

        with
        | exn -> return Error exn
    }

let collectChaptersFromFeedAsync (network: NetworkHelper) (feedUrl: string) =
    collectChaptersFromFeedPaginatedAsync (network) (feedUrl) (0) (Option.None) (Seq.empty)

