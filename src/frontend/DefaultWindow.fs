module DefaultWindow

open Avalonia
open Avalonia.Controls
open Avalonia.Layout
open Avalonia.Media
open Avalonia.Media.Imaging
open ObjectInitHelper
open DataProviderPanel
open FloatingActionButton
open System

let private initGlobalArea (top: Panel) (bottom: Panel) : DockPanel =
    let globalArea = DockPanel()
    globalArea.LastChildFill <- true
    globalArea.Children.Add(top)
    globalArea.Children.Add(bottom)
    globalArea

let private initMainArea () : Panel =
    let backgroundImage = Image()
    backgroundImage.Source <- new Bitmap("assets/img/starry-night-sky.jpg")
    backgroundImage.Stretch <- Stretch.UniformToFill

    let interactiveArea = Grid()
    interactiveArea.ShowGridLines <- true
    
    interactiveArea.ColumnDefinitions.Add(ColumnDefinition(3, GridUnitType.Star))
    interactiveArea.ColumnDefinitions.Add(ColumnDefinition(2.25, GridUnitType.Star))
    interactiveArea.ColumnDefinitions.Add(ColumnDefinition(3, GridUnitType.Star))
    
    interactiveArea.RowDefinitions.Add(RowDefinition(1, GridUnitType.Star))
    interactiveArea.RowDefinitions.Add(RowDefinition(4, GridUnitType.Star))
    interactiveArea.RowDefinitions.Add(RowDefinition(1, GridUnitType.Star))

    let centerCell = DataProviderPanel()
    centerCell.Show(false)
    interactiveArea.Children.Add(centerCell.Layout)

    Grid.SetColumn(centerCell.Layout, 1)
    Grid.SetRow(centerCell.Layout, 1)

    let createNewButton = FloatingActionButton (56, "New", fun _ -> centerCell.Show(true))

    let mainArea = Panel()
    mainArea.Children.Add(backgroundImage)
    mainArea.Children.Add(interactiveArea)
    mainArea.Children.Add(createNewButton)
    mainArea

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

type DefaultWindow(targetWidth: float, targetHeight: float, titleText: string) as window =
    inherit Window()

    let appBar = initAppBar titleText
    let mainArea = initMainArea ()
    let globalArea = initGlobalArea appBar mainArea

    do
        DockPanel.SetDock(appBar, Dock.Top)

        window.Title <- titleText
        
        window.Width <- targetWidth
        window.MinWidth <- targetWidth
        
        window.Height <- targetHeight
        window.MinHeight <- targetHeight
        
        window.WindowStartupLocation <- WindowStartupLocation.CenterScreen
        window.Content <- globalArea
        
        window.Show()
