using Colors;
using ExtensionMethods;
using HelperMethods;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading;
using System.IO;

static class Program
{
    private const Byte SingleChapter = 3;
    private const Byte MultipleChapters = 4;

    // ┏━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━┓
    // ┃ args[0] = Title                          ┃
    // ┃ args[1] = URL                            ┃
    // ┃ args[2] = Chapter Number                 ┃
    // ┃ args[3] = Optional<Final Chapter Number> ┃
    // ┗━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━┛
    
    private static void Main(string[] args)
    {
        Action application = args.Length switch
        {
            SingleChapter => () => CreateChapters(args[0], args[1], args[2], args[2]),
            MultipleChapters => () => CreateChapters(args[0], args[1], args[2], args[3]),
            _ => () => FailedArguments(),
        };

        application.Invoke();
    }

    private static void CreateChapters(string title, string url, string firstChapter, string finalChapter)
    {
        Int32 firstChapterAsInt = Int32.Parse(firstChapter);
        Int32 finalChapterAsInt = Int32.Parse(finalChapter);
        Int32 chapterCount = finalChapterAsInt - firstChapterAsInt + 1;

        IEnumerable<Int32> chapters = Enumerable.Range(firstChapterAsInt, chapterCount);
        var creationProcess = new PipeLine();
        string processStartedAt = DateTime.Now.ToString("s");

        chapters.ForEach(chapter =>
        {
            var userInput = new RequestInfo
            {
                Time = processStartedAt,
                Id = chapter,
                CancellationSource = new System.Threading.CancellationTokenSource(),
                Title = title,
                Url = chapterCount == 1 ? url : String.Format(url, chapter),
            };
            creationProcess.AppendNewEntry(userInput);
        });

        creationProcess.Begin();
    }

    private static void FailedArguments()
    {
        Helpers.WriteLineColor(Color.DarkOrange, "[WARNING]", "Your command line arguments are not properly formatted.");
        Helpers.WriteLineColor(Color.DarkOrange, """ - use "./{Executable} {Title} {Url} {Chapter}" for a single chapter""");
        Helpers.WriteLineColor(Color.DarkOrange, """ - use "./{Executable} {Title} {Url} {FirstChapter} {FinalChapter}" for multiple chapters""");
    }

    public static void LogFailedProcess(RequestInfo requestInfo, string causeMsg, string suggestionMsg)
    {
        CancellationTokenSource source = requestInfo.CancellationSource;
        source.Cancel();

        using (StreamWriter logFile = new StreamWriter(path: $"logfile-{requestInfo.Time}.log", append: true))
        {
            logFile.WriteLine("[ERROR]");
            logFile.WriteLine($"Cause: {causeMsg}");
            logFile.WriteLine($"Suggestion: {suggestionMsg}");
            logFile.WriteLine(requestInfo.ToString());
            logFile.WriteLine();
        }

        source.Token.ThrowIfCancellationRequested();
    }
}