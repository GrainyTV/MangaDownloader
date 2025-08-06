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

let private Resources = Application.Current.Resources

[<Literal>]
let private THEME_COLOR = 0xff152238

[<Literal>]
let OFF_WHITE = 0xfff2f0ef

[<Literal>]
let LIGHT_BLUE = 0xff384b69

[<Literal>]
let LIGHT_BLUE2 = 0xe6384b69

module LightTheme =
    let Flax = Color.from 0xffe6d17c

module DarkTheme =
    let FederalBlue = Color.from 0xff201256
    let RussianViolet = Color.from 0xff3d2e58
    let Lavender = Color.from 0xffe4ebfe

module Static =
    let private textBlock = Style(fun elem -> elem.OfType<TextBlock>())
    textBlock.AddRange [|
        TextBlock.FontFamilyProperty, FontFamily Fonts.ARGENTUM_SANS
        TextBlock.ForegroundProperty, Color.from Colors.White
        TextBlock.FontSizeProperty, 20.0
        TextBlock.HorizontalAlignmentProperty, HorizontalAlignment.Center
        TextBlock.VerticalAlignmentProperty, VerticalAlignment.Center
        TextBlock.TextWrappingProperty, TextWrapping.Wrap
    |]

    let private button = Style(fun elem -> elem.OfType<Button>())
    button.AddRange [|
        Button.BorderThicknessProperty, Thickness(0)
        Button.BackgroundProperty, Color.from(THEME_COLOR)
        Button.HorizontalContentAlignmentProperty, HorizontalAlignment.Stretch
        Button.VerticalContentAlignmentProperty, VerticalAlignment.Stretch
    |]

    let private watermark = Style(fun elem -> Selectors.Name(elem, "watermark"))
    watermark.AddRange [|
        TextBlock.ForegroundProperty, Color.from Colors.Gray
        TextBlock.BackgroundProperty, Color.from Colors.Green
    |]

    let private textBox = Style(fun elem -> elem.OfType<TextBox>())
    textBox.AddRange [|
        // TODO ...
    |]

    let all = [|
        textBlock
        button
        watermark
    |]

module Dynamic =
    let private bindResource<'T when 'T :> AvaloniaObject>
        (target: 'T)
        (property: AvaloniaProperty)
        (resourceKey: string) : Unit = AppScope.DYNAMIC_BINDINGS.Add(target.Bind(property, Resources.GetResourceObservable resourceKey))

    let apply (control: Control) : Unit =
        match control with
        | :? Grid as grid when grid.Name = "appBar" -> bindResource (grid) (Grid.BackgroundProperty) ("mainThemeColor")
        | :? Image as img -> bindResource (img) (Image.SourceProperty) ("backgroundImage")
        | _ -> ()

let lightThemeResources() : ResourceDictionary =
    let resources = ResourceDictionary()
    resources.Add("mainThemeColor", LightTheme.Flax)
    resources.Add("backgroundImage", AppScope.BACKGROUND_LIGHTTHEME)
    resources

let darkThemeResources() : ResourceDictionary =
    let resources = ResourceDictionary()
    resources.Add("mainThemeColor", DarkTheme.FederalBlue)
    resources.Add("backgroundImage", AppScope.BACKGROUND_DARKTHEME)
    resources

let private isLightTheme(): Boolean = Application.Current.ActualThemeVariant = ThemeVariant.Light

let changeTheme(): Unit =
    if isLightTheme() then
        Application.Current.RequestedThemeVariant <- ThemeVariant.Dark

    else
        Application.Current.RequestedThemeVariant <- ThemeVariant.Light

type ExtendedSimpleTheme() as self =
    inherit SimpleTheme()

    do
        Static.all |> Seq.iter (fun style -> self.Add style)

(* LIGHT THEME PALETTE
<palette>
  <color name="Light blue" hex="99BBC3" r="153" g="187" b="195" />
  <color name="Chamoisee" hex="A18762" r="161" g="135" b="98" />
  <color name="Air Force blue" hex="5C8798" r="92" g="135" b="152" />
  <color name="Flax" hex="E6D17C" r="230" g="209" b="124" />
  <color name="Reseda green" hex="626843" r="98" g="104" b="67" />
</palette>
*)

(* DARK THEME PALETTE
<palette>
  <color name="Rich black" hex="090316" r="9" g="3" b="22" />
  <color name="Federal blue" hex="201256" r="32" g="18" b="86" />
  <color name="Bright pink (Crayola)" hex="F86D7F" r="248" g="109" b="127" /> Good for feedback messages
  <color name="Violet (web color)" hex="FB93FF" r="251" g="147" b="255" />
  <color name="Lavender (web)" hex="E4EBFE" r="228" g="235" b="254" /> Good for box shadows
</palette>

Main: Federal blue
Feedback: Crayola
Shadows: Lavender
Text:
Container: Russian violet #3d2e58

*)
