namespace MangaDownloader

open Avalonia.Collections
open Avalonia.Controls
open Avalonia.Markup.Xaml

// TODO: introduce enum instead of magic int values
type DataProviderStackPanelViewModel(panelChangedEvent: Event<int>) =
    let mutable titleText = ""
    let mutable urlText = ""

    member self.TitleText
        with get()    = titleText
        and set value = titleText <- value

    member self.TitleNext =
        CommandAdapter(fun () -> panelChangedEvent.Trigger(1))

    member self.ModeNext =
        CommandAdapter(fun () -> panelChangedEvent.Trigger(1))

    member self.UrlText
        with get()    = urlText
        and set value = urlText <- value

//|
//| Helper widget that keeps one of its children active at a time
//|
type DataProviderStackPanel() as self =
    inherit UserControl()

    static let ActivePanelProperty = Avalonia.AvaloniaProperty.Register<DataProviderStackPanel, DataProviderPanel>("ActivePanel")

    let panels = AvaloniaList<DataProviderPanel>()
    let panelChanged = Event<int>()

    let hidePanel(): unit =
        self.IsVisible <- false
        self.IsEnabled <- false

    do
        AvaloniaXamlLoader.Load(self)
        self.DataContext <- DataProviderStackPanelViewModel(panelChanged)
        self.ActivePanel <- panels[0]

        // PanelChanged event changes the active view
        // ---
        self.PanelChanged.Add(fun idx -> self.ActivePanel <- panels[idx])

        // Panel is HIDDEN by default
        // ---
        hidePanel()

    member self.Panels =
        panels

    member self.PanelChanged: IEvent<int> =
        panelChanged.Publish

    member self.ActivePanel
        with get()    = self.GetValue(ActivePanelProperty)
        and set value =
            value.Close.Add(hidePanel)
            self.SetValue(ActivePanelProperty, value) |> ignore
