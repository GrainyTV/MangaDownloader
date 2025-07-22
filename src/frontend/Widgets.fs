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

type DynamicTB(content: TextBlock) as self =
    inherit Viewbox(Stretch = Stretch.Uniform)

    do
        self.Child <- content

    member self.Text
        with get() = content.Text
        and set value = content.Text <- value

    new() = DynamicTB(TextBlock(Text = String.Empty))

    new(txt: String) = DynamicTB(TextBlock(Text = txt))

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

    let singleChapterSelectText = DynamicTB("Single\nChapter")
    let multiChapterSelectText = DynamicTB("Multiple\nChapters")

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

    member self.Typed = baseControl.Text

    [<CLIEvent>]
    member self.FinishedTyping = finishedTyping.Publish

    interface IWizardStepProvider with
        member self.Step = { Title = fst (desc); Description = snd (desc); Content = baseControl }

type DoubleInputField(desc: String * String) =
    let baseControl = UniformGrid(Rows = 1, Columns = 2)
    let textInputLower = TextBox(Watermark = "From:")
    let textInputUpper = TextBox(Watermark = "To:")

    let timerLower = new Timers.Timer(TimeSpan.FromSeconds 0.3, AutoReset = false)
    let timerUpper = new Timers.Timer(TimeSpan.FromSeconds 0.3, AutoReset = false)
    let finishedTyping = Event<string * string>()

    do
        textInputLower.TextChanged.Add(fun args ->
            timerLower.Stop()
            timerLower.Start()
        )

        textInputUpper.TextChanged.Add(fun args ->
            timerUpper.Stop()
            timerUpper.Start()
        )

        timerLower.Elapsed.Add(fun _ -> Dispatcher.UIThread.Invoke(fun _ -> finishedTyping.Trigger(textInputLower.Text, textInputUpper.Text)))
        timerUpper.Elapsed.Add(fun _ -> Dispatcher.UIThread.Invoke(fun _ -> finishedTyping.Trigger(textInputLower.Text, textInputUpper.Text)))

    do
        baseControl.Children.AddRange [|
            textInputLower
            textInputUpper
        |]

    [<CLIEvent>]
    member self.FinishedTyping = finishedTyping.Publish

    interface IWizardStepProvider with
        member self.Step = { Title = fst (desc); Description = snd (desc); Content = baseControl }

type BackendProgressBar(desc: String * String) =
    let progress = ProgressBar(Minimum = 0, Maximum = 100)
    let baseControl = PercentageContainer (progress, "75%", "20%")

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
                    emoji.Text <- "🛑"
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
            PercentageContainer(DynamicTB(step.Title), "25%", "100%")
            step.Content
            PercentageContainer(DynamicTB(step.Description), "75%", "100%")
        |]

type WizardStepUnion =
    | Title of TextInputField
    | Mode of OperationModeSelector
    | Url of TextInputField
    | Howmany of DoubleInputField
    | Process
    | Finish of ResultShowcase

type StackLayout() =
    let titleDefine = TextInputField("Title", "Enter a title for your manga")
    let opModeSelector = OperationModeSelector("Mode", "Select an operation mode")
    let urlDefine = TextInputField("Url", "Paste the URL of the desired manga")
    let chapterRangeDefine = DoubleInputField("How Many", "Provide an inclusive range of chapters")
    let backendProcess = BackendProgressBar("In Progress", "Your files are being generated")
    let resultShowcase = ResultShowcase("Done")

    let mutable request = Program.EMPTY_REQUEST
    let mutable selectedWizardStep = Title titleDefine

    [<Literal>]
    let MANGADEX_URL_PATTERN = @"^https://mangadex\.org/title/[a-z0-9]{8}(-[a-z0-9]{4}){3}-[a-z0-9]{12}(?=/|$)"

    let content = WizardContent(titleDefine)
    let stepChanged = Event<WizardStepUnion>()
    let allowProceed = Event<Unit>()
    let disallowProceed = Event<Unit>()
    let failedInput = Event<String>()

    let tryValidateTitle (text: String) : Option<String> =
        let allowedMin = 1
        let allowedMax = 99
        let invalidFilenameChars = Path.GetInvalidFileNameChars()

        if text.Length < allowedMin || text.Length > allowedMax then None
        elif invalidFilenameChars |> Array.exists (fun inv -> text.Contains(inv)) then None
        else Some (text)

    let tryValidateUrl (text: String) : Option<String> =
        let validate = Regex.Match(text, MANGADEX_URL_PATTERN, RegexOptions.Compiled)

        if validate.Success then
            Some (validate.Value)

        else
            None

    let tryValidateChapterRange (lower: String) (upper: String) : Option<Int32 * Int32> =
        let allowedMin = 0
        let allowedMax = 9999

        match Int32.TryParse(lower), Int32.TryParse(upper) with
        | (true, lowerValue), (true, upperValue) ->
            if lowerValue < allowedMin || lowerValue > allowedMax then None
            elif upperValue < allowedMin || upperValue > allowedMax then None
            elif lowerValue >= upperValue then None
            else Some(lowerValue, upperValue)

        | _ -> None

    let changeStep (newStep: WizardStepUnion) (contentToUse: IWizardStepProvider) : Unit =
        selectedWizardStep <- newStep
        stepChanged.Trigger (selectedWizardStep)
        content.updateWith (contentToUse.Step)

    do
        // ┏━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━┓
        // ┃ Discard events that arrive after the user has already proceeded ┃
        // ┗━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━┛

        titleDefine.FinishedTyping.Add(fun title ->
            match selectedWizardStep with
            | Title _ ->
                let titleTrimmed = title.Trim()

                if not (titleTrimmed.IsEmpty) then
                    let validation = tryValidateTitle (titleTrimmed)

                    if validation.IsSome then
                        request <- { request with Title = validation.Value }
                        allowProceed.Trigger()

                    else
                        disallowProceed.Trigger()
                        failedInput.Trigger("Title must be raw text without invalid filename characters and with length [1; 100)")

                else
                    disallowProceed.Trigger()
                    failedInput.Trigger(String.Empty)

            | _ -> ()
        )

        urlDefine.FinishedTyping.Add(fun url ->
            match selectedWizardStep with
            | Url _ ->
                let urlTrimmed = url.Trim()

                if not (urlTrimmed.IsEmpty) then
                    let validation = tryValidateUrl (urlTrimmed)

                    if validation.IsSome then
                        request <- { request with Url = validation.Value }
                        allowProceed.Trigger()

                    else
                        disallowProceed.Trigger()
                        failedInput.Trigger("Url must use this form: https://mangadex.org/title/<UUID>/...")

                else
                    disallowProceed.Trigger ()
                    failedInput.Trigger (String.Empty)

            | _ -> ()
        )

        chapterRangeDefine.FinishedTyping.Add(fun (lower, upper) ->
            match selectedWizardStep with
            | Howmany _ ->
                let lowerTrimmed = lower.Trim()
                let upperTrimmed = upper.Trim()

                if not (lowerTrimmed.IsEmpty) && not (upperTrimmed.IsEmpty) then
                    let validation = tryValidateChapterRange (lowerTrimmed) (upperTrimmed)

                    if validation.IsSome then
                        request <- {
                        request with
                            FirstChapter = fst validation.Value
                            FinalChapter = snd validation.Value
                        }
                        allowProceed.Trigger()

                    else
                        disallowProceed.Trigger()
                        failedInput.Trigger("Chapter range must be two distinct integers")

                else
                    disallowProceed.Trigger ()
                    failedInput.Trigger(String.Empty)

            | _ -> ()
        )

    [<CLIEvent>]
    member self.AllowProceed = allowProceed.Publish

    [<CLIEvent>]
    member self.DisallowProceed = disallowProceed.Publish

    [<CLIEvent>]
    member self.StepChanged = stepChanged.Publish

    [<CLIEvent>]
    member self.FailedInput = failedInput.Publish

    member self.Active = content

    member self.ProceedButtonText =
        match selectedWizardStep with
        | Url _ | Process -> "Start"
        | _ -> "Next"

    member self.FinishProcessing() : Unit = Dispatcher.UIThread.Invoke(fun _ -> self.Proceed())

    member self.Proceed() : Unit =
        match selectedWizardStep with
        | Title t -> changeStep (Mode opModeSelector) (opModeSelector)
        | Mode m ->
            disallowProceed.Trigger()

            match m.Selected with
            | SingleChapter -> ()
            | MultiChapters -> changeStep (Howmany chapterRangeDefine) (chapterRangeDefine)

        | Url u ->
            disallowProceed.Trigger ()
            changeStep (Process) (backendProcess)
            Program.createChaptersFrom (request) (backendProcess) (resultShowcase) (self.FinishProcessing) |> Async.StartImmediate

        | Howmany h ->
            disallowProceed.Trigger ()
            changeStep (Url urlDefine) (urlDefine)

        | Process ->
            changeStep (Finish resultShowcase) (resultShowcase)

        | Finish f -> ()
