module DefaultWindow

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

let private initUserArea (backgroundImage: Image) : Panel =
    let wizard = DataProviderPanel()
    wizard.Show(false)

    let interactiveArea = Grid(ShowGridLines = true)
    
    interactiveArea.ColumnDefinitions.AddRange [
        ColumnDefinition(3, GridUnitType.Star)
        ColumnDefinition(2.25, GridUnitType.Star)
        ColumnDefinition(3, GridUnitType.Star)
    ]
    
    interactiveArea.RowDefinitions.AddRange [
        RowDefinition(1, GridUnitType.Star)
        RowDefinition(4, GridUnitType.Star)
        RowDefinition(1, GridUnitType.Star)
    ]
    
    interactiveArea.Children.Add(wizard.Layout)

    //   Wizard should be the middle cell
    //
    //     0   1   2
    //   #---#---#---#
    // 0 | x | x | x |
    //   #---#---#---#
    // 1 | x | O | x |
    //   #---#---#---#
    // 2 | x | x | x |
    //   #---#---#---#
    //

    Grid.SetColumn(wizard.Layout, 1)
    Grid.SetRow(wizard.Layout, 1)

    let createNewButton = FloatingActionButton (56, "New", fun _ -> wizard.Show(true))

    let control = Panel()
    
    control.Children.AddRange [
        backgroundImage
        interactiveArea
        createNewButton
    ]
    
    control

let private initAppBar (titleText: string) : Panel =
    let title = Text.from titleText
    title.FontSize <- 22
    title.FontWeight <- FontWeight.Bold
    title.HorizontalAlignment <- HorizontalAlignment.Left
    title.Padding <- Thickness(16, 0)

    let appBar = Panel()
    appBar.Background <- Color.from(0xff152238)
    appBar.Height <- 56
    appBar.Children.Add(title)
    appBar

type DefaultWindow(targetWidth: float, targetHeight: float, title: string) =
    let backgroundImage = Image(
        Stretch = Stretch.UniformToFill,
        Source = new Bitmap(AssetLoader.Open(Uri("resm:MangaDownloader.assets.img.starry-night-sky.jpg"))))

    let appBar = initAppBar (title)
    let userArea = initUserArea (backgroundImage)
    let globalContainer = DockPanel(LastChildFill = true)

    let baseControl = Window(
        Title = title,
        Width = targetWidth,
        Height = targetHeight,
        MinWidth = targetWidth,
        MinHeight = targetHeight,
        WindowStartupLocation = WindowStartupLocation.CenterScreen,
        Content = globalContainer)

    do
        globalContainer.Children.AddRange [ appBar; userArea ]
        DockPanel.SetDock(appBar, Dock.Top)

    member self.Layout = baseControl
