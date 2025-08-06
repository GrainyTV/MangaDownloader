module ApplicationWindow

open Avalonia
open Avalonia.Controls
open Avalonia.Controls.Primitives
open Avalonia.Input
open Avalonia.Layout
open Avalonia.Media
open Avalonia.Media.Imaging
open Avalonia.Platform
open ObjectInitHelper
open DataProviderPanel
open FloatingActionButton
open System
open Widgets

let private initUserArea () : Panel =
    let backgroundImage = Image(Stretch = Stretch.UniformToFill)
    Styles.Dynamic.apply (backgroundImage)

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

let private initAppBar (title: string) : Grid =
    let text = TextBlock(
        Text = title,
        FontSize = 22,
        FontWeight = FontWeight.Bold,
        HorizontalAlignment = HorizontalAlignment.Left,
        Padding = Thickness(16, 0)
    )

    let themeSwap = Button(Content = "Light Theme")
    themeSwap.Click.Add(fun _ -> Styles.changeTheme ())

    let enableMute = Button(Content = "Audio ON")

    let buttons = UniformGrid(Rows = 1, Columns = 2)
    buttons.Children.AddRange [|
        PercentageContainer(themeSwap, "50%", "50%")
        PercentageContainer(enableMute, "50%", "50%")
    |]

    let appBar = Grid(Name = "appBar", Height = 56)
    Styles.Dynamic.apply (appBar)

    appBar.ColumnDefinitions.Add(ColumnDefinition(1, GridUnitType.Star))
    appBar.ColumnDefinitions.Add(ColumnDefinition(GridLength(192)))
    appBar.Children.AddRange [| text; buttons |]

    Grid.SetColumn(text, 0)
    Grid.SetColumn(buttons, 1)

    appBar

type Window(title: String, targetWidth: Int32, targetHeight: Int32) as self =
    inherit Controls.Window(
        Title = title,
        Width = targetWidth,
        MinWidth = targetWidth,
        Height = targetHeight,
        MinHeight = targetHeight,
        WindowStartupLocation = WindowStartupLocation.CenterScreen)

    let appBar = initAppBar (title)
    let userArea = initUserArea ()
    let windowContent = DockPanel(LastChildFill = true)
    let mutable isWindowDragInEffect = false
    let mutable cursorPositionAtWindowDragStart = Point(0, 0)

    do
        appBar.PointerMoved.Add(fun args ->
            if isWindowDragInEffect then
                let currentCursorPosition = args.GetPosition self
                let cursorPositionDelta = currentCursorPosition - cursorPositionAtWindowDragStart
                self.Position <- self.PointToScreen cursorPositionDelta)

        appBar.PointerPressed.Add(fun args ->
            match args.Source with
            | :? Control as control when control.Name = "appBar" ->
                isWindowDragInEffect <- true
                cursorPositionAtWindowDragStart <- args.GetPosition self
            | _ -> ())

        appBar.PointerReleased.Add(fun _ -> isWindowDragInEffect <- false)

        appBar.DoubleTapped.Add(fun _ ->
            match self.WindowState with
            | WindowState.FullScreen -> self.WindowState <- WindowState.Normal
            | _ -> self.WindowState <- WindowState.FullScreen)

        windowContent.Children.AddRange [| appBar; userArea |]
        DockPanel.SetDock(appBar, Dock.Top)

        self.Content <- windowContent
