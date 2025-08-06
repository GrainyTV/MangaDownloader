module Errors

open Serilog
open System
open System.Globalization

type BaseMessage =
    { Issue: String
      Suggestion: String }

    override self.ToString (): String =
        $"""Issue: "{self.Issue}"
Suggestion: "{self.Suggestion}"
"""

type BaseMessageWithExn =
    { Issue: String
      Suggestion: String
      Exception: String }
    override self.ToString (): String =
        $"""Issue: "{self.Issue} ({self.Exception})"
Suggestion: "{self.Suggestion}"
"""

type ErrorType =
    | CouldNotAccessFeed of BaseMessageWithExn
    | NoAvailableChapters of BaseMessage
    | FailedChapterGen of BaseMessageWithExn * CommonTypes.Chapter
    | MissingChapter of BaseMessage * Int32
    | FailedToCreateDir of BaseMessageWithExn

    override self.ToString (): String =
        match self with
        | CouldNotAccessFeed details -> details.ToString()
        | NoAvailableChapters details -> details.ToString()
        | FailedChapterGen (details, chapter) -> $"{details}Input: {chapter}\n"
        | MissingChapter (details, num) -> $"{details}Chapter: {num}\n"
        | FailedToCreateDir details -> details.ToString()

let log (error: ErrorType) (*(outDir: String)*) : Unit =
    let logFile = DateTime.Now.ToString("s", CultureInfo.InvariantCulture) + ".log"

    let logConfig = LoggerConfiguration()
    use logger = logConfig.WriteTo.File(
        path = joinPathsUnix [| AppScope.WORKING_DIRECTORY; logFile |],
        outputTemplate = "[{Level}]{NewLine}{Message}").CreateLogger()

    logger.Error(error.ToString())
