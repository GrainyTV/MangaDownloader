module Network
//open Extension
open CommonTypes
open LinkDotNet.StringBuilder
open System
open System.IO
open System.Net.Http
open System.Threading.Tasks

let private httpClient = new HttpClient()

let private sendGetRequestAsync (url: string) : Async<Result<Stream, string>> =
    async {
        use request = new HttpRequestMessage(HttpMethod.Get, url)

        try
            let! response = httpClient.SendAsync(request) |> Async.AwaitTask
            response.EnsureSuccessStatusCode() |> ignore

            let! content = response.Content.ReadAsStreamAsync() |> Async.AwaitTask
            return Ok content

        with
            | ex -> return Error ex.Message
    }

let setUserAgent () : Unit =
    httpClient.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (X11; Linux x86_64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/128.0.0.0 Safari/537.36")

//let startImageDownloads (toDownload: ImageDescriptor) (outDir: string) : PdfDescriptor =
let startImageDownloads (chapter: ChapterEntry) : Unit = //PdfDescriptor =
    //let imageDir = joinPathsUnix ([ outDir; string toDownload.Number ])
    Directory.CreateDirectory(chapter.TemporaryImageDirectory) |> ignore

    let localFiles = Array.init (chapter.RemoteFiles.Length) (fun i -> joinPathsUnix ([ chapter.TemporaryImageDirectory; string i ]))
    chapter.LocalFiles <- localFiles

    //Seq.zip toDownload.Urls files
    Seq.zip (chapter.RemoteFiles) (chapter.LocalFiles)
    |> Seq.map (fun (url, file) ->
        async {
            let! response = sendGetRequestAsync (url)

            if response.IsOk() then
                use inputStream = response.Unwrap()
                use outputStream = new FileStream(file, FileMode.CreateNew)
                do! inputStream.CopyToAsync(outputStream) |> Async.AwaitTask

            else
                ()
        })
    |> Async.Parallel
    |> Async.Ignore
    |> Async.RunSynchronously

    //{ Number = toDownload.Number; Files = files }

let collectImageUrlsOfChapterAsync (chapterId: string) : Async<array<string>> =
    async {
        let url = joinPathsUnix ([ Mangadex.CHAPTER_ENDPOINT; chapterId ])
        let! response = sendGetRequestAsync (url)

        if response.IsOk() then
            let content = Utility.unpackJsonFromStream (Mangadex.MangadexImages.Parse) (response.Unwrap())
            let imageProviderUrl = joinPathsUnix ([ content.BaseUrl; "data"; content.Chapter.Hash ])
            return content.Chapter.Data |> Array.map (fun imageId -> joinPathsUnix ([ imageProviderUrl; imageId ]))

        else
            return Array.empty
    }

[<TailCall>]
let rec private collectChaptersFromFeedPaginated (i: int) (url: string) (limit: int) (cachedTotal: Option<int>) (entries: seq<ChapterEntry>) : seq<ChapterEntry> =
    let paginatedUrl = ValueStringBuilder.Concat(url, "&offset=", i * limit)
    let response = sendGetRequestAsync (paginatedUrl) |> Async.RunSynchronously
    
    if response.IsOk() then
        let content = Utility.unpackJsonFromStream (Mangadex.MangadexFeed.Parse) (response.Unwrap())
        let total = if cachedTotal.IsSome then cachedTotal else Option.Some(int content.Total)
        
        let foundEntries =
            content.Data
            |> Seq.filter(fun json -> not (json.Attributes.Chapter.Contains('.')))
            |> Seq.distinctBy(fun json -> int json.Attributes.Chapter)
            |> Seq.map(fun json -> int json.Attributes.Chapter, json.Id)
            |> Map.ofSeq

        let updatedEntries =
            entries
            |> Seq.map (fun entry ->
                if entry.Id = String.Empty then
                    match Map.tryFind (entry.Number) (foundEntries) with
                    | Some id -> entry.Id <- id
                    | _ -> ()

                entry)

        let foundAllEntries = updatedEntries |> Seq.forall(fun entry -> entry.Id <> String.Empty)
        let noMorePagination = (i + 1) * limit >= total.Value

        if foundAllEntries || noMorePagination then
            updatedEntries

        else
            collectChaptersFromFeedPaginated (i + 1) (url) (limit) (total) (updatedEntries)

    else
        Seq.empty

let collectChaptersFromFeed (url: string) (limit: int) (chapters: seq<ChapterEntry>) : seq<ChapterEntry> =
    collectChaptersFromFeedPaginated (0) (url) (limit) (Option.None) (chapters) 
