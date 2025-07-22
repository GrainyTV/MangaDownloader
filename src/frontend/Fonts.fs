module Fonts

open System
open Avalonia.Media
open Avalonia.Media.Fonts

[<Literal>]
let private KEY = "fonts:Local" + "#"

[<Literal>]
let private LOCATION = "avares://MangaDownloader/assets/font"

[<Literal>]
let ARGENTUM_SANS = KEY + "Argentum Sans"

[<Literal>]
let NOTO_COLOR_EMOJI = KEY + "Noto Color Emoji"

let registerCustom (fontManager: FontManager) : Unit =
    fontManager.AddFontCollection(new EmbeddedFontCollection(Uri(ARGENTUM_SANS), Uri(LOCATION)))
    fontManager.AddFontCollection(new EmbeddedFontCollection(Uri(NOTO_COLOR_EMOJI), Uri(LOCATION)))
