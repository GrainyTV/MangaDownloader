module FloatingActionButton

open Avalonia
open Avalonia.Controls
open Avalonia.Layout
open Avalonia.Media
open ObjectInitHelper
open System.Diagnostics

type ShadowedButton(w: double, h: double, corner: double, label: string) as SB =
    inherit Border()

    let innerButton = ShadowedButton.initInner corner label

    do
        SB.Width <- w
        SB.Height <- h
        SB.Background <- Color.from (Colors.Transparent)
        SB.CornerRadius <- CornerRadius(corner)
        SB.BoxShadow <- BoxShadows.Parse("4 4 8 0 Red")
        SB.Child <- innerButton

    member SB.InnerButton = innerButton

    static member private initInner (corner: double) (label: string) : Button =
        let button = Button()
        button.CornerRadius <- CornerRadius(corner)
        button.Content <- Text.from (label) 
        button

type FloatingActionButton(w: double, label: string, action) as FAB =
    inherit ShadowedButton(w, w, w / 2.0, label)

    do
        FAB.HorizontalAlignment <- HorizontalAlignment.Right
        FAB.VerticalAlignment <- VerticalAlignment.Bottom
        FAB.Margin <- Thickness(24)
        FAB.InnerButton.Click.Add(action)
