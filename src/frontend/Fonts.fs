module Fonts

open System
open Avalonia.Media
open Avalonia.Media.Fonts

[<Literal>]
let private DESIGNATOR = "fonts:App"

[<Literal>]
let ARGENTUM_SANS = DESIGNATOR + "#" + "Argentum Sans"

let registerCustom (fontManager: FontManager) : Unit =
    fontManager.AddFontCollection(new EmbeddedFontCollection(Uri(DESIGNATOR), Uri("avares://MangaDownloader/assets/font")))
