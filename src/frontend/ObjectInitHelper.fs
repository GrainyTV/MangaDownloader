module ObjectInitHelper

open Avalonia.Controls
open Avalonia.Media
open Avalonia

[<AbstractClass; Sealed>]
type Color private () =
    static member from(color: Avalonia.Media.Color) : SolidColorBrush = SolidColorBrush(color)
    static member from(color: int32) : SolidColorBrush = SolidColorBrush(uint32 color)

[<AbstractClass; Sealed>]
type Text private () =
    static member from(text: string) = TextBlock(Text = text)
