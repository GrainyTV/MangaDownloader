module DataProviderPanel

open Avalonia
open Avalonia.Controls
open Avalonia.Controls.Primitives
open Avalonia.Layout
open Avalonia.Media
open ObjectInitHelper
open System
open System.Diagnostics

open Widgets

type WizardStep =
    | Title of TextInputField
    | Mode of OperationModeSelector
    | Url of TextInputField
    | Howmany of ChapterRangeInputField
    | Process

type StackLayout() =
    let titleDefine = TextInputField("Title", "Enter a title for your manga")
    let opModeSelector = OperationModeSelector("Mode", "Select the desired operation mode")
    let urlDefine = TextInputField("Url", "Paste the URL of the desired manga")
    let chapterRangeDefine = ChapterRangeInputField("How Many", "Provide an inclusive range of chapters")
    let backendProcess = BackendProgressBar("In Progress", "Your files are being generated")

    let mutable request = Program.EMPTY_REQUEST
    let mutable selectedWizardStep = Title titleDefine
    let content = WizardContent(titleDefine)

    member self.Active = content

    member self.Proceed() : Unit =
        match selectedWizardStep with
        | Title t ->
            selectedWizardStep <- Mode opModeSelector
            content.updateWith(opModeSelector)
            request <- { request with Title = t.Typed }

        | Mode m ->
            match m.Selected with
            | SingleChapter -> ()
                //selectedWizardStep <- Url urlDefine
                //content.updateWith(urlDefine)

            | MultiChapters ->
                //chapterRangeDefine.apply(0)
                selectedWizardStep <- Howmany chapterRangeDefine
                content.updateWith(chapterRangeDefine)

        | Url u ->
            selectedWizardStep <- Process
            content.updateWith(backendProcess)
            request <- { request with Url = u.Typed }
            Program.createChaptersFrom (request) |> Async.Start

        | Howmany h ->
            selectedWizardStep <- Url urlDefine
            content.updateWith(urlDefine)
            request <- { request with FirstChapter = h.LowerBound; FinalChapter = h.UpperBound }

        | Process -> ()

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
    
    let proceedButtonText = DynamicText(Text.from "Next", 0.175)
    let proceedButton = Button(Content = proceedButtonText)
    let proceedButtonContainer = PercentageContainer (proceedButton, "33.334%", "50%")

    do
        baseControl.ColumnDefinitions.Add(ColumnDefinition(1, GridUnitType.Star))
        baseControl.RowDefinitions.AddRange [|
            RowDefinition(1, GridUnitType.Star)
            RowDefinition(3, GridUnitType.Star)
            RowDefinition(1, GridUnitType.Star)
            RowDefinition(1, GridUnitType.Star)
        |]

        baseControl.Children.Add(stackPanel.Active)
        Grid.SetRow(stackPanel.Active, 1)

        baseControl.Children.Add(proceedButtonContainer)
        Grid.SetRow(proceedButtonContainer, 3)

        proceedButton.Click.Add(fun _ -> stackPanel.Proceed())

    member self.Layout = baseControlCornered

    member self.Show(yesOrNo: bool) : Unit =
        match yesOrNo with
        | true -> baseControlCornered.IsVisible <- true
        | false -> baseControlCornered.IsVisible <- false
