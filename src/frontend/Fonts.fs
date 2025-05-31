module Fonts

open System
open Avalonia.Media
open Avalonia.Media.Fonts

[<Literal>]
let ARGENTUM_SANS = "fonts:Local#Argentum Sans"

let registerCustom (fontManager: FontManager) : Unit =
    fontManager.AddFontCollection(new EmbeddedFontCollection(Uri(ARGENTUM_SANS), Uri("resm:MangaDownloader.assets.font")))
