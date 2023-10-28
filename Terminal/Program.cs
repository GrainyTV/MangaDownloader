using Colors;
using HelperMethods;
using Optional;
using Optional.Unsafe;
using System;
using System.Linq;
using System.Threading.Tasks.Dataflow;
using System.Collections.Generic;

static class Program
{
    private const Byte SingleChapter = 3;

    private const Byte MultipleChapters = 4; 

    /**
     * 
     * args[0] = Title
     * args[1] = URL
     * args[2] = Chapter Number
     * args[3] = Optional<Final Chapter Number>
     * 
     */
    static void Main(string[] args)
    {
        Action application = args.Length switch
        {
            SingleChapter => () => CreateSingleChapter(args[0], args[1], args[2]),
            MultipleChapters => () => CreateMultipleChapters(args[0], args[1], args[2], args[3]),
            _ => () => FailedArguments(),
        };

        application.Invoke();
    }

    private static void CreateSingleChapter(string title, string url, string chapter)
    {
        var acquireMangaImageUrls = new TransformBlock<string, Option<IEnumerable<string>>>(Preparation.ExtractMangaImageUrls);
        acquireMangaImageUrls.Post(url);
        acquireMangaImageUrls.Complete();

        var results = acquireMangaImageUrls.Receive().ValueOrFailure();

        foreach (string result in results)
        {
            Console.WriteLine(result);
        }
    }

    private static void CreateMultipleChapters(ReadOnlySpan<char> title, ReadOnlySpan<char> url, ReadOnlySpan<char> firstChapter, ReadOnlySpan<char> finalChapter)
    {
    }

    private static void FailedArguments()
    {
        Helpers.WriteLineColor(Color.Orange, "[WARNING]", "Your command line arguments are not properly formatted.");
        Helpers.WriteLineColor(Color.Orange, """ - use "./{Executable} {Title} {Url} {Chapter}" for a single chapter""");
        Helpers.WriteLineColor(Color.Orange, """ - use "./{Executable} {Title} {Url} {FirstChapter} {FinalChapter}" for multiple chapters""");
    }
}