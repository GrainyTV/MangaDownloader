namespace MangaDownloader

open Avalonia.Controls
open Avalonia.Markup.Xaml

type FancyButton() as self =
    inherit UserControl()

    static let CornerRadiusProperty = Avalonia.AvaloniaProperty.Register<FancyButton, Avalonia.CornerRadius>("CornerRadius")
    static let LabelProperty = Avalonia.AvaloniaProperty.Register<FancyButton, string>("Label", defaultValue="unset")
    static let CommandProperty = Avalonia.AvaloniaProperty.Register<FancyButton, CommandAdapter>("Command")

    do
        AvaloniaXamlLoader.Load(self)

    member self.CornerRadius
        with get()    = self.GetValue(CornerRadiusProperty)
        and set value = self.SetValue(CornerRadiusProperty, value) |> ignore

    member self.Label
        with get()    = self.GetValue(LabelProperty)
        and set value = self.SetValue(LabelProperty, value) |> ignore

    member self.Command
        with get()    = self.GetValue(CommandProperty)
        and set value = self.SetValue(CommandProperty, value) |> ignore
