module NetworkHelper

open System
open System.IO
open System.Net
open System.Net.Http

type NetworkHelper() =
    // User-Agent yoinked from the latest Thorium Browser
    [<Literal>]
    let userAgent = "Mozilla/5.0 (X11; Linux x86_64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/130.0.0.0 Safari/537.36"

    [<Literal>]
    let referer = "https://mangadex.org/"

    let socketsHandler = new SocketsHttpHandler(PooledConnectionLifetime = TimeSpan.FromMinutes 1)
    let httpClient = new HttpClient(socketsHandler, Timeout = TimeSpan.FromSeconds 5, DefaultRequestVersion = HttpVersion.Version30)

    do
        httpClient.DefaultRequestHeaders.Add("User-Agent", userAgent)
        httpClient.DefaultRequestHeaders.Add("Referer", referer)

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
            socketsHandler.Dispose()
            httpClient.Dispose()
