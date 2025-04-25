module DataProviderPanel

open Avalonia
open Avalonia.Controls
open Avalonia.Controls.Primitives
open Avalonia.Layout
open Avalonia.Media
open ObjectInitHelper

type DataProviderPanel(id: string, whatToDo: string) as DPP =
    inherit UniformGrid()

    let title = DataProviderPanel.initTitle id
    let inputField = TextBox()
    let description = DataProviderPanel.initDescription whatToDo
    let button = DataProviderPanel.initProceedButton ()

    do
        inputField.TextInput.Add(fun arg -> if arg.Text.Length > 0 then button.IsEnabled <- true else button.IsEnabled <- false)
        // button.Click.Add(fun _ -> )

        DPP.Rows <- 6
        DPP.Columns <- 1
        DPP.Children.AddRange [|
            Panel()
            title
            inputField
            description
            Panel()
            button
        |]

    static member private initTitle (id) : TextBlock =
        let title = Text.from id
        title.FontSize <- 30
        title.FontWeight <- FontWeight.Bold
        title

    static member private initDescription (whatToDo) : TextBlock =
        let description = Text.from whatToDo
        description.TextWrapping <- TextWrapping.WrapWithOverflow
        description.TextAlignment <- TextAlignment.Center
        description

    static member private initProceedButton () : Button =
        let button = Button()
        button.Content <- Text.from "Next"
        button.IsEnabled <- false
        button
