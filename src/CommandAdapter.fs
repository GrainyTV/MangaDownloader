namespace MangaDownloader

open System
open System.Windows.Input

type CommandAdapter(action: Action) =
    let executeChanged = Event<EventHandler, EventArgs>()

    interface ICommand with
        member self.CanExecute(parameter: obj): bool =
            true

        member self.Execute(parameter: obj): unit =
            action.Invoke()

        [<CLIEvent>]
        member self.CanExecuteChanged = executeChanged.Publish
