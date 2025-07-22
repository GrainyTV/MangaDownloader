module DataProviderPanel

open Avalonia
open Avalonia.Controls
open ObjectInitHelper
open System
open Widgets

type DataProviderPanel() =
    let baseControl = Grid()
    let baseControlCornered = Border(
        IsVisible = false,
        CornerRadius = CornerRadius(16),
        Background = Color.from (Styles.LIGHT_BLUE2),
        BorderThickness = Thickness(2),
        BorderBrush = Color.from(Styles.OFF_WHITE),
        Child = baseControl)
    
    let stackPanel = StackLayout ()

    let proceedButtonText = DynamicTB(stackPanel.ProceedButtonText)
    let proceedButton = Button(Content = proceedButtonText, IsEnabled = false)
    let proceedButtonContainer = PercentageContainer (proceedButton, "33.334%", "50%")

    let userHelpText = DynamicTB()

    do
        baseControl.ColumnDefinitions.Add(ColumnDefinition(1, GridUnitType.Star))
        baseControl.RowDefinitions.AddRange [|
            RowDefinition(1, GridUnitType.Star)
            RowDefinition(3, GridUnitType.Star)
            RowDefinition(1, GridUnitType.Star)
            RowDefinition(1, GridUnitType.Star)
        |]

        baseControl.Children.AddRange [|
            stackPanel.Active
            userHelpText
            proceedButtonContainer
        |]

        Grid.SetRow(stackPanel.Active, 1)
        Grid.SetRow(userHelpText, 2)
        Grid.SetRow(proceedButtonContainer, 3)

        proceedButton.Click.Add(fun _ -> stackPanel.Proceed())
        stackPanel.StepChanged.Add(fun _ -> proceedButtonText.Text <- stackPanel.ProceedButtonText)
        stackPanel.DisallowProceed.Add(fun _ -> proceedButton.IsEnabled <- false)

        stackPanel.FailedInput.Add(fun msg ->
            if not (userHelpText.Text.Equals msg) then
                userHelpText.Text <- msg
        )

        stackPanel.AllowProceed.Add(fun _ ->
            proceedButton.IsEnabled <- true
            userHelpText.Text <- String.Empty
        )

    member self.Layout = baseControlCornered

    member self.Show(shouldShow: bool) : Unit =
        if shouldShow then
            baseControlCornered.IsVisible <- true

        else
            baseControlCornered.IsVisible <- false
