[<AutoOpen>]
module AppScope

open Avalonia.Media.Imaging
open Avalonia.Platform
open NetworkHelper
open System
open System.Threading.RateLimiting

[<Sealed>]
[<AbstractClass>]
type AppScope =
    static member AVALONIA_RESOURCES = "avares://MangaDownloader/assets"

    // Ratelimit options populated from API docs
    // https://api.mangadex.org/docs/2-limitations/#endpoint-specific-rate-limits
    static member ATHOME_RATELIMIT_OPTIONS = FixedWindowRateLimiterOptions (
        AutoReplenishment = true,
        PermitLimit = 40,
        QueueLimit = Int32.MaxValue,
        QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
        Window = TimeSpan.FromMinutes 1
    )

    static member NETWORK = new NetworkHelper()

    static member BACKGROUND_DARKTHEME = new Bitmap(AssetLoader.Open(Uri(AppScope.AVALONIA_RESOURCES + "/img/wallhaven-jx1ex5_3840x2160.png")))

    static member BACKGROUND_LIGHTTHEME = new Bitmap(AssetLoader.Open(Uri(AppScope.AVALONIA_RESOURCES + "/img/wallhaven-wepl6x_3840x2160.png")))

    static member DYNAMIC_BINDINGS = ResizeArray<IDisposable>()

    static member val WORKING_DIRECTORY = Environment.ProcessPath with get, set
