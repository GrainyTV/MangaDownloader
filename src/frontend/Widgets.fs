module Widgets

open Avalonia
open Avalonia.Controls
open Avalonia.Controls.Primitives
open Avalonia.Layout
open System
open System.Diagnostics

type DynamicText(text: TextBlock, dFactor: double) =
    inherit UserControl(Content = text)

    let MAX_FONTSIZE = 50

    let calculateNewFontSize (from: Size) : double =
        let diagonal = Math.Sqrt(from.Width * from.Width + from.Height * from.Height)
        let calculatedSize = diagonal * dFactor
        Math.Min(calculatedSize, MAX_FONTSIZE)

    override this.MeasureOverride(constrain: Size) =
        let newSize = calculateNewFontSize(constrain)

        if Math.Abs (text.FontSize - newSize) >= 0.1 then
            text.FontSize <- newSize
        
        base.MeasureOverride(constrain)

type PercentageContainer(child: Control, widthPercentage: string, heightPercentage: string) as self =
    inherit Grid()

    let TOTAL_PERCENTAGE = 100.0

    let parsePercentage (valueStr: string) : double =
        Debug.Assert(valueStr.EndsWith '%', "PercentageContainer parameters must end with a '%' character.")
        
        let mutable value = 0.0
        let parseSuccess = Double.TryParse(valueStr.TrimEnd '%', &value)

        if not parseSuccess then
            TOTAL_PERCENTAGE

        else
            Debug.Assert(value >= 0 && value <= 100, $"PercentageContainer values must be between 0%% and 100%%. Found: {value}.")
            value

    do        
        let width = parsePercentage (widthPercentage)
        let height = parsePercentage (heightPercentage)

        let horizontalMargin = (TOTAL_PERCENTAGE - width) / 2.0
        let verticalMargin = (TOTAL_PERCENTAGE - height) / 2.0

        self.ColumnDefinitions.AddRange [|
            ColumnDefinition(horizontalMargin, GridUnitType.Star)
            ColumnDefinition(width, GridUnitType.Star)
            ColumnDefinition(horizontalMargin, GridUnitType.Star)
        |]

        self.RowDefinitions.AddRange [|
            RowDefinition(verticalMargin, GridUnitType.Star)
            RowDefinition(height, GridUnitType.Star)
            RowDefinition(verticalMargin, GridUnitType.Star)
        |]

        self.Children.Add(child)
        Grid.SetRow(child, 1)
        Grid.SetColumn(child, 1)


type OpMode =
    | SingleChapter
    | MultiChapters

type IWizardStep =
    abstract Title : string
    abstract Description : string
    abstract Content : Control

type OperationModeSelector(title: string, description: string) =
    let baseControl = UniformGrid(Rows = 1, Columns = 2)

    let singleChapterSelectText = DynamicText(TextBlock(Text = "Single\nChapter"), 0.115)
    let multiChapterSelectText = DynamicText(TextBlock(Text = "Multiple\nChapters"), 0.115)

    let singleChapterSelect = Button(Content = singleChapterSelectText)
    let multiChapterSelect = Button(Content = multiChapterSelectText, Opacity = 0.5)

    let mutable selectedMode = OpMode.SingleChapter

    let checkButtonState (chosen: OpMode) : Unit =
        if not(selectedMode = chosen) then
            match chosen with
            | OpMode.SingleChapter ->
                singleChapterSelect.Opacity <- 1
                multiChapterSelect.Opacity <- 0.5
            | OpMode.MultiChapters ->
                singleChapterSelect.Opacity <- 0.5
                multiChapterSelect.Opacity <- 1

            selectedMode <- chosen

    do
        singleChapterSelect.Click.Add(fun _ -> checkButtonState OpMode.SingleChapter)
        multiChapterSelect.Click.Add(fun _ -> checkButtonState OpMode.MultiChapters)

        baseControl.Children.AddRange [|
            singleChapterSelect
            multiChapterSelect
        |]

    member self.Selected = selectedMode

    interface IWizardStep with
        member self.Title = title
        member self.Description = description
        member self.Content = baseControl

type TextInputField(title: string, description: string) =
    let baseControl = TextBox()

    member self.Typed = baseControl.Text

    interface IWizardStep with
        member self.Title = title
        member self.Description = description
        member self.Content = baseControl

type ChapterRangeInputField(title: string, description: string) =
    let baseControl = UniformGrid(Rows = 2, Columns = 1)
    let textInputLower = TextBox()
    let textInputHigher = TextBox()

    do
        baseControl.Children.AddRange [|
            textInputLower
            textInputHigher
        |]

    member self.LowerBound = Int32.Parse(textInputLower.Text)

    member self.UpperBound = Int32.Parse(textInputHigher.Text)

    interface IWizardStep with
        member self.Title = title
        member self.Description = description
        member self.Content = baseControl

type BackendProgressBar(title: string, description: string) =
    let baseControl = ProgressBar()

    //member self.apply(value: double) : Unit =
    //    baseControl.

    interface IWizardStep with
        member self.Title = title
        member self.Description = description
        member self.Content = baseControl

type WizardContent(initial: IWizardStep) as self =
    inherit UniformGrid(Rows = 3, Columns = 1)

    do
        self.updateWith(initial)

    member self.updateWith(step: IWizardStep) : Unit =
        self.Children.Clear()
        self.Children.AddRange [|
            DynamicText(TextBlock(Text = step.Title), 0.125)
            step.Content
            DynamicText(TextBlock(Text = step.Description), 0.06)
        |]
