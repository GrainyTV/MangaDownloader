namespace MangaDownloader

open Avalonia.Controls
open Avalonia.Controls.Primitives
open Avalonia.Markup.Xaml
open System
open System.Diagnostics

//|
//| Helper control to claim the inner X% of width and Y% of height from the available space
//|
type PercentageContainer() as self =
    inherit UserControl()

    let mutable widthPercentage = "100%"
    let mutable heightPercentage = "100%"
    let mutable rootGrid: Grid = null

    let calculateSections (perc: string): float * float =
        if not (perc.EndsWith '%') then
            Debug.Fail($"Percentage value `{perc}` does not end with %% sign")

        let mutable numeric = 0.0
        let parsed = Double.TryParse(perc.TrimEnd '%', &numeric)

        if not parsed then
            Debug.Fail($"Failed to parse `{perc}` as a numeric value")

        if numeric < 0 || numeric > 100 then
            Debug.Fail($"Required space%% `{numeric}` is either less than 0 or more than 100")

        (numeric, (100.0 - numeric) / 2.0)

    let applySizes (): unit =
        if not (obj.ReferenceEquals(rootGrid, null)) then
            let (wClaimed, wMargin) = calculateSections(widthPercentage)
            let (hClaimed, hMargin) = calculateSections(heightPercentage)

            rootGrid.ColumnDefinitions[0].Width <- GridLength(wMargin, GridUnitType.Star)
            rootGrid.ColumnDefinitions[1].Width <- GridLength(wClaimed, GridUnitType.Star)
            rootGrid.ColumnDefinitions[2].Width <- GridLength(wMargin, GridUnitType.Star)

            rootGrid.RowDefinitions[0].Height <- GridLength(hMargin, GridUnitType.Star)
            rootGrid.RowDefinitions[1].Height <- GridLength(hClaimed, GridUnitType.Star)
            rootGrid.RowDefinitions[2].Height <- GridLength(hMargin, GridUnitType.Star)

    do
        AvaloniaXamlLoader.Load(self)

    override self.OnApplyTemplate(e: TemplateAppliedEventArgs): unit =
        base.OnApplyTemplate(e)

        rootGrid <- e.NameScope.Find<Grid>("PercentageGrid")
        applySizes()

    member self.WidthPercentage
        with set value =
            widthPercentage <- value
            applySizes()

    member self.HeightPercentage
        with set value =
            heightPercentage <- value
            applySizes()
