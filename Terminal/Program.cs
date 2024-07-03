using Colors;
using ExtensionMethods;
using HelperMethods;
using Optional;
using Optional.Unsafe;
using System;
using System.Linq;
using System.Collections.Generic;

static class Program
{
    private const Byte SingleChapter = 3;
    private const Byte MultipleChapters = 4; 
 
    // args[0] = Title
    // args[1] = URL
    // args[2] = Chapter Number
    // args[3] = Optional<Final Chapter Number>
    // ----------------------------------------
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
        int firstChapterAsInt = Int32.Parse(firstChapter);
        int finalChapterAsInt = Int32.Parse(finalChapter);
        var creationProcess = new PipeLine();
        
        if (firstChapterAsInt == finalChapterAsInt)
        {
            var userInput = new RequestInfo { Id = firstChapterAsInt, Title = title, Url = url, };
            creationProcess.AppendNewEntry(userInput);
        }
        else
        {
            IEnumerable<int> chapters = Enumerable.Range(firstChapterAsInt, finalChapterAsInt);
            
            chapters.ForEach((chapter, _) =>
            {
                var userInput = new RequestInfo { Id = chapter, Title = title, Url = String.Format(url, chapter), };      
                creationProcess.AppendNewEntry(userInput);
            });
        }

        creationProcess.Begin();
    }

    private static void FailedArguments()
    {
        Helpers.WriteLineColor(Color.DarkOrange, "[WARNING]", "Your command line arguments are not properly formatted.");
        Helpers.WriteLineColor(Color.DarkOrange, """ - use "./{Executable} {Title} {Url} {Chapter}" for a single chapter""");
        Helpers.WriteLineColor(Color.DarkOrange, """ - use "./{Executable} {Title} {Url} {FirstChapter} {FinalChapter}" for multiple chapters""");
    }
}