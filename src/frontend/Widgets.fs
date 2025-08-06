module Widgets

open Avalonia
open Avalonia.Controls
open Avalonia.Controls.Primitives
open Avalonia.Layout
open Avalonia.Threading
open System
open System.Diagnostics
open System.Threading

open Avalonia.Media

open ObjectInitHelper

open System.IO
open System.Text.RegularExpressions

open Avalonia.Media.TextFormatting

type DynamicTB(content: TextBlock) as self =
    inherit ContentControl(Content = content)

    let MAX_FONTSIZE = 64
    let fontSizeChanged = Event<Double>()
    let mutable allowIndividualSizing = true

    let measureTextFromFontSizeMultiline (fontSize: Double) (availableWidth: Double): Size =
        let typeface = Typeface(content.FontFamily, content.FontStyle, content.FontWeight, content.FontStretch)

        use renderedText = new TextLayout(content.Text, typeface, fontSize,
            foreground = content.Foreground,
            textAlignment = content.TextAlignment,
            textWrapping = content.TextWrapping,
            textTrimming = content.TextTrimming,
            textDecorations = content.TextDecorations,
            flowDirection = content.FlowDirection,
            lineHeight = content.LineHeight,
            letterSpacing = content.LetterSpacing,
            maxLines = content.MaxLines,
            maxWidth = availableWidth)

        Size(renderedText.WidthIncludingTrailingWhitespace + renderedText.OverhangLeading + renderedText.OverhangTrailing,
             renderedText.Height + renderedText.OverhangAfter)

    [<TailCall>]
    let rec findFittingFontSize (from: Int32) (availableSize: Size): Double =
        let textSize = measureTextFromFontSizeMultiline (from) (availableSize.Width)

        if textSize.Width < availableSize.Width && textSize.Height < availableSize.Height then
            from

        else
            findFittingFontSize (from - 2) (availableSize)

    do
        self.LayoutUpdated.Add(fun _ ->
            if content.Text.IsNotEmpty &&
               self.IsEffectivelyVisible &&
               self.Bounds.Size.NearlyEquals(Size(0, 0)) = false then
                let fs = findFittingFontSize MAX_FONTSIZE self.Bounds.Size

                if allowIndividualSizing then
                    self.updateFontSize fs
                else
                    fontSizeChanged.Trigger fs)

    member self.updateFontSize (fs: Double) : Unit = content.FontSize <- fs

    member self.disallowIndividualSizing () : Unit = allowIndividualSizing <- false

    member self.Text
        with get() = content.Text
        and set value = content.Text <- value

    [<CLIEvent>]
    member self.FontSizeChanged = fontSizeChanged.Publish

    new() = DynamicTB(TextBlock(Text = String.Empty))

    new(txt: String) = DynamicTB(TextBlock(Text = txt))

type DynamicTBGroup(children: seq<DynamicTB>) =
    let tbCount = Seq.length children
    let sizes = ResizeArray<Double>()

    let appendAndEvaluate (fs: Double) : Unit =
        sizes.Add (fs)

        if sizes.Count = tbCount then
            let chosenMin = sizes |> Seq.min
            sizes.Clear()

            children |> Seq.iter (fun tb -> tb.updateFontSize chosenMin)

    do
        children |> Seq.iter (fun tb ->
            tb.disallowIndividualSizing ()
            tb.FontSizeChanged.Add appendAndEvaluate)

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
            Debug.Assert(value >= 0 && value <= 100, $"PercentageContainer values must be between 0%% and 100%%. Found: {value}%%.")
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

type WizardStep = {
    Title : string
    Description : string
    Content : Control
}

type IWizardStepProvider =
    abstract member Step: WizardStep

type OperationModeSelector(desc: String * String) =
    let baseControl = UniformGrid(Rows = 1, Columns = 2)

    let singleChapterSelectText = DynamicTB(TextBlock(Text = "Single Chapter", TextWrapping = TextWrapping.NoWrap))
    let multiChapterSelectText = DynamicTB(TextBlock(Text = "Multiple Chapters", TextWrapping = TextWrapping.NoWrap))

    let textGroup = DynamicTBGroup [| singleChapterSelectText; multiChapterSelectText |]

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

    member self.Reset() : Unit = checkButtonState (OpMode.SingleChapter)

    interface IWizardStepProvider with
        member self.Step = { Title = fst (desc); Description = snd (desc); Content = baseControl }

type TextInputField(desc: String * String) =
    let baseControl = TextBox()
    let timer = new Timers.Timer(TimeSpan.FromSeconds 0.3, AutoReset = false)
    let finishedTyping = Event<string>()

    do
        baseControl.TextChanged.Add(fun args ->
            timer.Stop()
            timer.Start()
        )

        timer.Elapsed.Add(fun _ -> Dispatcher.UIThread.Invoke(fun _ -> finishedTyping.Trigger baseControl.Text))

    [<CLIEvent>]
    member self.FinishedTyping = finishedTyping.Publish

    member self.Typed = baseControl.Text

    member self.Clear() : Unit = baseControl.Clear()

    interface IWizardStepProvider with
        member self.Step = { Title = fst (desc); Description = snd (desc); Content = baseControl }

type DoubleTextInputField(desc: String * String) =
    let baseControl = UniformGrid(Rows = 1, Columns = 2)
    let textInputLower = TextBox(Watermark = "From:")
    let textInputUpper = TextBox(Watermark = "To:")

    let timerLower = new Timers.Timer(TimeSpan.FromSeconds 0.3, AutoReset = false)
    let timerUpper = new Timers.Timer(TimeSpan.FromSeconds 0.3, AutoReset = false)
    let finishedTyping = Event<string * string>()

    do
        baseControl.Children.AddRange [|
            textInputLower
            textInputUpper
        |]

        textInputLower.TextChanged.Add(fun _ ->
            timerLower.Stop()
            timerLower.Start()
        )

        textInputUpper.TextChanged.Add(fun _ ->
            timerUpper.Stop()
            timerUpper.Start()
        )

        timerLower.Elapsed.Add(fun _ -> Dispatcher.UIThread.Invoke(fun _ -> finishedTyping.Trigger(textInputLower.Text, textInputUpper.Text)))
        timerUpper.Elapsed.Add(fun _ -> Dispatcher.UIThread.Invoke(fun _ -> finishedTyping.Trigger(textInputLower.Text, textInputUpper.Text)))

    [<CLIEvent>]
    member self.FinishedTyping = finishedTyping.Publish

    member self.Clear() : Unit =
        textInputLower.Clear()
        textInputUpper.Clear()

    interface IWizardStepProvider with
        member self.Step = { Title = fst (desc); Description = snd (desc); Content = baseControl }

type BackendProgressBar(desc: String * String) =
    let progress = ProgressBar(Minimum = 0, Maximum = 100)
    let baseControl = PercentageContainer (progress, "75%", "20%")

    member self.Reset() : Unit =
        progress.Value <- 0

    interface IWizardStepProvider with
        member self.Step = { Title = fst (desc); Description = snd (desc); Content = baseControl }

    interface CommonTypes.IBackendApplicable<Double> with
        member self.apply (toAdd: double) : Unit =
            Dispatcher.UIThread.Post(fun _ -> progress.Value <- progress.Value + toAdd)

type ResultShowcase(title: String) =
    let emoji = DynamicTB(TextBlock(FontFamily = Fonts.NOTO_COLOR_EMOJI))
    let mutable step = { Title = title; Description = String.Empty; Content = emoji }

    interface IWizardStepProvider with
        member self.Step = step

    interface CommonTypes.IBackendApplicable<CommonTypes.RequestResult> with
        member self.apply (which: CommonTypes.RequestResult) : Unit =
            Dispatcher.UIThread.Invoke(fun _ ->
                match which with
                | CommonTypes.RequestResult.TotalFailure ->
                    emoji.Text <- "🚨"
                    step <- { step with Description = "Operation failed" }

                | CommonTypes.RequestResult.PartialSuccess ->
                    emoji.Text <- "⚠️"
                    step <- { step with Description = "Operation completed partially" }

                | CommonTypes.RequestResult.PerfectCompletion ->
                    emoji.Text <- "🎉"
                    step <- { step with Description = "Operation completed successfully" }
            )

type WizardContent(initial: IWizardStepProvider) as self =
    inherit UniformGrid(Rows = 3, Columns = 1)

    do
        self.updateWith(initial.Step)

    member self.updateWith(step: WizardStep) : Unit =
        self.Children.Clear()
        self.Children.AddRange [|
            PercentageContainer(DynamicTB(TextBlock(Text = step.Title)), "75%", "85%")
            step.Content
            PercentageContainer(DynamicTB(TextBlock(Text = step.Description, TextAlignment = TextAlignment.Center)), "75%", "75%")
        |]

type WizardStepUnion =
    | Title of TextInputField
    | Mode of OperationModeSelector
    | Url of TextInputField
    | Howmany of DoubleTextInputField
    | Process
    | Finish of ResultShowcase
