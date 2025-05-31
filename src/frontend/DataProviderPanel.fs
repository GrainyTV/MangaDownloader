module DataProviderPanel

open Avalonia
open Avalonia.Controls
open Avalonia.Controls.Primitives
open Avalonia.Layout
open Avalonia.Media
open ObjectInitHelper
open System

type OpMode =
    | SingleChapter
    | MultiChapters

type WizardStep =
    | Title
    | Mode
    | Url
    | HowMany
    | Process

type UserAction = {
    Header: string
    Action: Control
    Description: string
}

type OperationModeSelector () =
    let baseControl = UniformGrid(Rows = 1, Columns = 2)
    let groupName = "Operation Mode Selector"

    let singleChapterSelectText = Text.from("Single\nChapter")
    let multiChapterSelectText = Text.from("Multiple\nChapters")

    let singleChapterSelect = RadioButton(Content = singleChapterSelectText, GroupName = groupName, IsChecked = true, Background = Color.from (Colors.AliceBlue))
    let multiChapterSelect = RadioButton(Content = multiChapterSelectText, GroupName = groupName, Background = Color.from (Colors.Red))

    let mutable selectedMode = OpMode.SingleChapter

    let checkButtonState (button: RadioButton) (selected: OpMode) : Unit =
        if button.IsChecked.HasValue && button.IsChecked.Value = true then
            selectedMode <- selected

    do
        singleChapterSelectText.Margin <- Thickness(0,10)
        singleChapterSelectText.HorizontalAlignment <- HorizontalAlignment.Left
        multiChapterSelectText.HorizontalAlignment <- HorizontalAlignment.Left

        singleChapterSelect.IsCheckedChanged.Add(fun _ -> checkButtonState (singleChapterSelect) (OpMode.SingleChapter))
        multiChapterSelect.IsCheckedChanged.Add(fun _ -> checkButtonState (multiChapterSelect) (OpMode.MultiChapters))
        
        baseControl.Children.AddRange [ singleChapterSelect; multiChapterSelect ]
        
    member self.Layout = baseControl
    member self.SelectedMode = selectedMode


type StackLayout() =
    let baseControl = UniformGrid(Rows = 3, Columns = 1)
    let titleTextInput = TextBox()
    let opModeSelector = OperationModeSelector()
    let urlTextInput = TextBox()

    let configSteps = Map.ofArray [|
        WizardStep.Title, { Header = "Title"; Action = titleTextInput; Description = "Enter a title for your manga." }
        WizardStep.Mode, { Header = "Mode"; Action = opModeSelector.Layout; Description = "Select the desired operation mode." }
        WizardStep.Url, { Header = "Url"; Action = urlTextInput; Description = "Paste the URL of the desired manga." }
        WizardStep.HowMany, { Header = "How Many"; Action = Button(); Description = "Provide an inclusive range of chapters." }
        WizardStep.Process, { Header = "In Progress"; Action = Button(); Description = "Your files are being generated." }
    |]

    let mutable selectedConfigStep = WizardStep.Title
    let request = Program.UserRequest()

    do
        baseControl.Children.AddRange [
            Text.from (configSteps[selectedConfigStep].Header)
            configSteps[selectedConfigStep].Action
            Text.from (configSteps[selectedConfigStep].Description)
        ]

    member self.Layout = baseControl

    member private self.RefreshLayout() : Unit =
        baseControl.Children[0] <- Text.from (configSteps[selectedConfigStep].Header)
        baseControl.Children[1] <- Viewbox(Child = configSteps[selectedConfigStep].Action)
        baseControl.Children[2] <- Viewbox(Child = Text.from (configSteps[selectedConfigStep].Description))

    member self.Proceed() : Unit =
        match selectedConfigStep with
        | Title ->
            request.Title <- titleTextInput.Text
            selectedConfigStep <- Mode
        
        | Mode -> 
            match opModeSelector.SelectedMode with
            | SingleChapter -> selectedConfigStep <- Url
            | MultiChapters -> selectedConfigStep <- HowMany
        
        | Url ->
            request.Url <- urlTextInput.Text
            selectedConfigStep <- Process
        
        | HowMany ->
            request.FirstChapter <- 1
            request.FinalChapter <- 5
            selectedConfigStep <- Url
        
        | Process ->
            Program.createChaptersFrom (request)
            selectedConfigStep <- Title

        self.RefreshLayout()

type DataProviderPanel() =
    let baseControl = Grid()
    let actionArea = StackLayout ()
    let proceedButton = Button(Content = Text.from ("Next"))

    do
        baseControl.ColumnDefinitions.Add(ColumnDefinition(1, GridUnitType.Star))
        baseControl.RowDefinitions.AddRange [
            RowDefinition(1, GridUnitType.Star)
            RowDefinition(3, GridUnitType.Star)
            RowDefinition(1, GridUnitType.Star)
            RowDefinition(1, GridUnitType.Star)
        ]

        baseControl.Children.Add(actionArea.Layout)
        Grid.SetRow(actionArea.Layout, 1)

        baseControl.Children.Add(proceedButton)
        Grid.SetRow(proceedButton, 3)

        proceedButton.Click.Add(fun _ -> actionArea.Proceed())

    member self.Layout = baseControl

    member self.Show(yesOrNo: bool) : Unit =
        match yesOrNo with
        | true -> baseControl.IsVisible <- true
        | false -> baseControl.IsVisible <- false
