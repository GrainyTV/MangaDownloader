module Fonts

open System
open Avalonia.Media
open Avalonia.Media.Fonts

let private FONTS_LOCATION = AppScope.AVALONIA_RESOURCES + "/font"

[<Literal>]
let private KEY = "fonts:Local" + "#"

[<Literal>]
let ARGENTUM_SANS = KEY + "Argentum Sans"

[<Literal>]
let NOTO_COLOR_EMOJI = KEY + "Noto Color Emoji"

let registerCustom (fontManager: FontManager) : Unit =
    fontManager.AddFontCollection(new EmbeddedFontCollection(Uri(ARGENTUM_SANS), Uri(FONTS_LOCATION)))
    fontManager.AddFontCollection(new EmbeddedFontCollection(Uri(NOTO_COLOR_EMOJI), Uri(FONTS_LOCATION)))
