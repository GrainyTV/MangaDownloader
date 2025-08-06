module StackLayout

open Avalonia.Threading
open System
open System.IO
open System.Text.RegularExpressions
open Widgets

type StackLayout() =
    let titleDefine = TextInputField("Title", "Enter a title for your manga")
    let opModeSelector = OperationModeSelector("Mode", "Select an operation mode")
    let urlDefine = TextInputField("Url", "Paste the URL of the chosen manga")
    let chapterRangeDefine = DoubleTextInputField("How Many", "Provide an inclusive range of chapters")
    let backendProcess = BackendProgressBar("In Progress", "Your files are being generated")
    let resultShowcase = ResultShowcase("Done")

    let mutable request = CommonTypes.EMPTY_REQUEST
    let mutable selectedWizardStep = Title titleDefine

    [<Literal>]
    let MANGADEX_CHAPTER_GROUP_PATTERN = @"^https://mangadex\.org/title/[a-z0-9]{8}(-[a-z0-9]{4}){3}-[a-z0-9]{12}(?=/|$)"

    [<Literal>]
    let MANGADEX_CHAPTER_PATTERN = @"^https://mangadex\.org/chapter/[a-z0-9]{8}(-[a-z0-9]{4}){3}-[a-z0-9]{12}(?=$)"

    let content = WizardContent(titleDefine)
    let stepChanged = Event<WizardStepUnion>()
    let allowProceed = Event<Unit>()
    let disallowProceed = Event<Unit>()
    let failedInput = Event<String>()
    let closePanel = Event<Unit>()

    let tryValidateTitle (text: String) : Option<String> =
        let allowedMin = 1
        let allowedMax = 99
        let invalidFilenameChars = Path.GetInvalidFileNameChars()

        if text.Length < allowedMin || text.Length > allowedMax then None
        elif invalidFilenameChars |> Array.exists (fun inv -> text.Contains inv) then None
        else Some (text)

    let tryValidateUrl (text: String) : Option<String> =
        let pattern =
            if opModeSelector.Selected = OpMode.SingleChapter then
                MANGADEX_CHAPTER_PATTERN

            else
                MANGADEX_CHAPTER_GROUP_PATTERN

        let validate = Regex.Match(text, pattern, RegexOptions.Compiled)

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

                if titleTrimmed.IsNotEmpty then
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

                if urlTrimmed.IsNotEmpty then
                    let validation = tryValidateUrl (urlTrimmed)

                    if validation.IsSome then
                        request <- { request with Url = validation.Value }
                        allowProceed.Trigger()

                    else
                        disallowProceed.Trigger()

                        if opModeSelector.Selected = OpMode.SingleChapter then
                            failedInput.Trigger("Url must use this form: https://mangadex.org/chapter/<UUID>")

                        else
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

                if lowerTrimmed.IsNotEmpty && upperTrimmed.IsNotEmpty then
                    let validation = tryValidateChapterRange (lowerTrimmed) (upperTrimmed)

                    if validation.IsSome then
                        let lowerBound = fst validation.Value
                        let upperBound = snd validation.Value
                        request <- { request with ChapterRange = Some({ lowerBound .. upperBound }) }
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

    [<CLIEvent>]
    member self.Close = closePanel.Publish

    member self.Active = content

    member self.ProceedButtonText =
        match selectedWizardStep with
        | Finish _ -> "Finish"
        | Url _ | Process -> "Start"
        | _ -> "Next"

    member private self.FinishProcessing() : Unit = Dispatcher.UIThread.Invoke(fun _ -> self.Proceed())

    member private self.Reset() : Unit =
        titleDefine.Clear()
        chapterRangeDefine.Clear()
        urlDefine.Clear()

        opModeSelector.Reset()
        backendProcess.Reset()

        request <- CommonTypes.EMPTY_REQUEST
        changeStep (Title titleDefine) (titleDefine)

        closePanel.Trigger ()

    member self.Proceed() : Unit =
        match selectedWizardStep with
        | Title t -> changeStep (Mode opModeSelector) (opModeSelector)
        | Mode m ->
            disallowProceed.Trigger()

            match m.Selected with
            | SingleChapter -> changeStep (Url urlDefine) (urlDefine)
            | MultiChapters -> changeStep (Howmany chapterRangeDefine) (chapterRangeDefine)

        | Url u ->
            disallowProceed.Trigger ()
            changeStep (Process) (backendProcess)
            Program.createChaptersFrom
                (request)
                ({ ProgressBar = backendProcess
                   FinalVerdict = resultShowcase })
                (self.FinishProcessing) |> Async.StartImmediate

        | Howmany h ->
            disallowProceed.Trigger ()
            changeStep (Url urlDefine) (urlDefine)

        | Process ->
            changeStep (Finish resultShowcase) (resultShowcase)
            allowProceed.Trigger ()

        | Finish f -> self.Reset()
