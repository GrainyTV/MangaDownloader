module Styles

open Avalonia
open Avalonia.Controls
open Avalonia.Layout
open Avalonia.Media
open Avalonia.Styling
open Avalonia.Themes.Simple
open ObjectInitHelper
open Fonts
open System
open Widgets

//open Avalonia.Controls.Templates
//open Avalonia.Controls.Presenters
//open Avalonia.Controls.Primitives

[<Literal>]
let private THEME_COLOR = 0xff152238

[<Literal>]
let OFF_WHITE = 0xfff2f0ef

[<Literal>]
let LIGHT_BLUE = 0xff384b69

[<Literal>]
let LIGHT_BLUE2 = 0xe6384b69

let private textBlockStyle () : Style =
    let textStyle = Style(fun elem -> Selectors.Or(elem.OfType<TextBlock>(), elem.OfType<DynamicTB>()))
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

let private textBoxStyle () : Style =
    let textBoxStyle = Style(fun elem -> elem.OfType<TextBox>())
    //textBoxStyle.Setters.Add(Setter(TextBox.Fore, ))
    textBoxStyle

let private waterMarkStyle () : Style =
    let waterMarkStyle = Style(fun elem -> Selectors.Name(elem, "watermark"))
    waterMarkStyle.Setters.Add(Setter(TextBlock.ForegroundProperty, Color.from Colors.Gray))
    waterMarkStyle

type ExtendedSimpleTheme() as self =
    inherit SimpleTheme()

    do
        self.AddRange [|
            textBlockStyle()
            buttonStyle()
            textBoxStyle()
            waterMarkStyle()
        |]
