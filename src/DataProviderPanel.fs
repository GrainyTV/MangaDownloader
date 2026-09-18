namespace MangaDownloader

open Avalonia.Controls
open Avalonia.Markup.Xaml

type DataProviderPanel() as self =
    inherit UserControl()

    static let ProceedButtonLabelProperty = Avalonia.AvaloniaProperty.Register<DataProviderPanel, string>("ProceedButtonLabel", defaultValue="unset")
    static let TitleProperty = Avalonia.AvaloniaProperty.Register<DataProviderPanel, string>("Title", defaultValue="unset")
    static let CommandProperty = Avalonia.AvaloniaProperty.Register<DataProviderPanel, CommandAdapter>("Command")

    let closeEvent = Event<unit>()

    do
        AvaloniaXamlLoader.Load(self)

    member self.ProceedButtonLabel
        with get()    = self.GetValue(ProceedButtonLabelProperty)
        and set value = self.SetValue(ProceedButtonLabelProperty, value) |> ignore

    member self.Title
        with get()    = self.GetValue(TitleProperty)
        and set value = self.SetValue(TitleProperty, value) |> ignore

    member self.Command
        with get()    = self.GetValue(CommandProperty)
        and set value = self.SetValue(CommandProperty, value) |> ignore

    member self.OnCloseDataProviderPanel =
        CommandAdapter(fun () -> closeEvent.Trigger())

    [<CLIEvent>]
    member self.Close =
        closeEvent.Publish
