namespace MangaDownloader

open Avalonia
open Avalonia.Controls
open Avalonia.Input
open Avalonia.Markup.Xaml
open System.Diagnostics

type MainWindowViewModel(window: Window) =
    let unhideDataProviderPanel = CommandAdapter(fun () ->
        let dpp = window.FindControl("DataProviderPanelMain")

        if not(obj.ReferenceEquals(dpp, null)) && not dpp.IsVisible then
            dpp.IsVisible <- true
            dpp.IsEnabled <- true
        else
            Debug.WriteLine($"StackPanel not found! ({dpp})")
    )

    member self.UnhideDataProviderPanel =
        unhideDataProviderPanel

type MainWindow() as self =
    inherit Window()

    do
        AvaloniaXamlLoader.Load(self)
        self.DataContext <- MainWindowViewModel(self)

    member self.AppBarDoubleTapped(sender: obj, e: Input.TappedEventArgs): unit =
        match self.WindowState with
        | WindowState.FullScreen ->
            self.WindowState <- WindowState.Normal
        | _ ->
            self.WindowState <- WindowState.FullScreen

    member self.AppBarPressed(sender: obj, e: Input.PointerPressedEventArgs): unit =
        self.BeginMoveDrag(e)
