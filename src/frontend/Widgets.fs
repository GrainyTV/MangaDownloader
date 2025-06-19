module Widgets

open Avalonia
open Avalonia.Controls
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
