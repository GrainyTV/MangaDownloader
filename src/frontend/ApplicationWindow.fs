module ApplicationWindow

open Avalonia
open Avalonia.Controls
open Avalonia.Layout
open Avalonia.Media
open Avalonia.Media.Imaging
open Avalonia.Platform
open ObjectInitHelper
open DataProviderPanel
open FloatingActionButton
open System

let private initUserArea () : Panel =
    let backgroundImage = Image(
        Stretch = Stretch.UniformToFill,
        Source = new Bitmap(AssetLoader.Open(Uri("avares://MangaDownloader/assets/img/starry-night-sky.jpg")))
    )

    let interactiveArea = Grid(ShowGridLines = false)

    // ┏━━━━━━━━━━━━━━━━━━━━━━━━━━━┓
    // ┃ Area should be a 3x3 grid ┃
    // ┗━━━━━━━━━━━━━━━━━━━━━━━━━━━┛
    
    interactiveArea.ColumnDefinitions.AddRange [|
        ColumnDefinition(3, GridUnitType.Star)
        ColumnDefinition(2.25, GridUnitType.Star)
        ColumnDefinition(3, GridUnitType.Star)
    |]
    
    interactiveArea.RowDefinitions.AddRange [|
        RowDefinition(1, GridUnitType.Star)
        RowDefinition(4, GridUnitType.Star)
        RowDefinition(1, GridUnitType.Star)
    |]
    
    let wizard = DataProviderPanel()
    interactiveArea.Children.Add(wizard.Layout)

    // ┏━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━┓
    // ┃ Wizard should be the middle cell (1, 1) ┃
    // ┗━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━┛

    Grid.SetColumn(wizard.Layout, 1)
    Grid.SetRow(wizard.Layout, 1)

    let createNewButton = FloatingActionButton (56, "New", fun _ -> wizard.Show(true))

    let overlay = Panel()
    
    overlay.Children.AddRange [|
        backgroundImage
        interactiveArea
        createNewButton
    |]
    
    overlay

let private initAppBar (title: string) : UserControl =
    let text = TextBlock(
        Text = title,
        FontSize = 22,
        FontWeight = FontWeight.Bold,
        HorizontalAlignment = HorizontalAlignment.Left,
        Padding = Thickness(16, 0)
    )

    UserControl(
        Background = Color.from(0xff152238),
        Height = 56,
        Content = text
    )

type Window(title: String, targetWidth: Int32, targetHeight: Int32) as self =
    inherit Controls.Window(
        Title = title,
        Width = targetWidth,
        MinWidth = targetWidth,
        Height = targetHeight,
        MinHeight = targetHeight,
        WindowStartupLocation = WindowStartupLocation.CenterScreen
    )

    let appBar = initAppBar (title)
    let userArea = initUserArea ()
    let windowContent = DockPanel(LastChildFill = true)

    do
        windowContent.Children.AddRange [| appBar; userArea |]
        DockPanel.SetDock(appBar, Dock.Top)

        self.Content <- windowContent
