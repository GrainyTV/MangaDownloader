module Styles

open Avalonia
open Avalonia.Controls
open Avalonia.Layout
open Avalonia.Media
open Avalonia.Styling
open Avalonia.Themes.Simple
open ObjectInitHelper
open Fonts

[<Literal>]
let private THEME_COLOR = 0xff152238

[<Literal>]
let OFF_WHITE = 0xfff2f0ef

[<Literal>]
let LIGHT_BLUE = 0xff384b69

[<Literal>]
let LIGHT_BLUE2 = 0xe6384b69

let private textStyle () : Style =
    let textStyle = Style(fun elem -> elem.OfType<TextBlock>())
    textStyle.Setters.Add(Setter(TextBlock.FontFamilyProperty, FontFamily(Fonts.ARGENTUM_SANS)))
    textStyle.Setters.Add(Setter(TextBlock.ForegroundProperty, Color.from(Colors.White)))
    textStyle.Setters.Add(Setter(TextBlock.FontSizeProperty, 20.0))
    textStyle.Setters.Add(Setter(TextBlock.HorizontalAlignmentProperty, HorizontalAlignment.Center))
    textStyle.Setters.Add(Setter(TextBlock.VerticalAlignmentProperty, VerticalAlignment.Center))
    textStyle

let private buttonStyle () : Style =
    let buttonStyle = Style(fun elem -> elem.OfType<Button>())
    buttonStyle.Setters.Add(Setter(Button.BorderThicknessProperty, Thickness(0)))
    buttonStyle.Setters.Add(Setter(Button.BackgroundProperty, Color.from(THEME_COLOR)))
    buttonStyle

type MaterialLike() as theme =
    inherit SimpleTheme()

    do
        theme.AddRange [
            textStyle()
            buttonStyle()
        ]
