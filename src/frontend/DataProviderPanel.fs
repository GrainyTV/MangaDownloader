module DataProviderPanel

open Avalonia
open Avalonia.Controls
open Avalonia.Media
open ObjectInitHelper
open StackLayout
open Styles
open System
open Widgets

type DataProviderPanel() =
    let stackPanel = StackLayout ()

    let userHelpText = DynamicTB()
    let userHelpTextContainer = PercentageContainer (userHelpText, "75%", "75%")

    let proceedButtonText = DynamicTB(stackPanel.ProceedButtonText)
    let proceedButton = Button(Content = proceedButtonText, IsEnabled = false)
    let proceedButtonContainer = PercentageContainer (proceedButton, "33.334%", "50%")

    let rows = Grid()

    let panel = Border(
        IsVisible = false,
        CornerRadius = CornerRadius(16),
        Background = DarkTheme.RussianViolet, //Color.from (Styles.LIGHT_BLUE2),
        //BorderThickness = Thickness(1),
        //BorderBrush = DarkTheme.FederalBlue, //Color.from(Styles.OFF_WHITE),
        BoxShadow = BoxShadows(BoxShadow(OffsetX = 0, OffsetY = 0, Blur = 10, Spread = 2, Color = DarkTheme.Lavender.Color, IsInset = false)),
        Child = rows)

    do
        rows.RowDefinitions.AddRange [|
            RowDefinition(1, GridUnitType.Star)
            RowDefinition(3, GridUnitType.Star)
            RowDefinition(1, GridUnitType.Star)
            RowDefinition(1, GridUnitType.Star)
        |]

        rows.Children.AddRange [|
            stackPanel.Active
            userHelpTextContainer
            proceedButtonContainer
        |]

        Grid.SetRow(stackPanel.Active, 1)
        Grid.SetRow(userHelpTextContainer, 2)
        Grid.SetRow(proceedButtonContainer, 3)

        proceedButton.Click.Add(fun _ -> stackPanel.Proceed())
        stackPanel.StepChanged.Add(fun _ -> proceedButtonText.Text <- stackPanel.ProceedButtonText)
        stackPanel.DisallowProceed.Add(fun _ -> proceedButton.IsEnabled <- false)

        stackPanel.FailedInput.Add(fun msg ->
            if not (userHelpText.Text.Equals msg) then
                userHelpText.Text <- msg)

        stackPanel.AllowProceed.Add(fun _ ->
            proceedButton.IsEnabled <- true
            userHelpText.Text <- String.Empty)

        stackPanel.Close.Add(fun _ -> panel.IsVisible <- false)

    member self.Layout = panel

    member self.Show(shouldShow: bool) : Unit =
        if shouldShow then
            panel.IsVisible <- true

        else
            panel.IsVisible <- false
