using Colors;
using ExtensionMethods;
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

    public static Dictionary<Guid, RequestInfo> Processes = new Dictionary<Guid, RequestInfo>(); 

    /**
     * 
     * args[0] = Title
     * args[1] = URL
     * args[2] = Chapter Number
     * args[3] = Optional<Final Chapter Number>
     * 
     */
    private static void Main(string[] args)
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
        /* Define the different sequences of the pipeline */
        
       // var acquireImageUrls = new TransformBlock<string, Option<List<string>>>(url => Preparation.ExtractMangaImageUrls(url));
       // var downloadImages = new TransformBlock<Option<List<string>>, List<string>>(acquiredUrls => Image.StartDownloads(acquiredUrls, title, chapter));
       // var mergeToPdf = new ActionBlock<List<string>>(downloadedImages => Pdf.GenerateNew(downloadedImages, title, chapter));
 
        /* Specify parameters for pipeline behaviour */
        
       // var linkOptions = new DataflowLinkOptions{ PropagateCompletion = true, };

        /* Join pipeline sequences together in order */

        //acquireImageUrls.LinkTo(downloadImages, linkOptions);
       // downloadImages.LinkTo(mergeToPdf, linkOptions);
        
        /* Provide input to pipeline then signal when you ran out of values */

       // acquireImageUrls.Post(url);
        //acquireImageUrls.Complete();
 
        /* Wait for pipeline to finish */
        /* Should be invoked on last sequence of pipeline */

        //mergeToPdf.Completion.Wait();
    }

    private static void CreateMultipleChapters(string title, string url, string firstChapter, string finalChapter)
    {
        var pipelineOptions = new ExecutionDataflowBlockOptions{ MaxDegreeOfParallelism = -1, };

        /* Define the different sequences of the pipeline */
        
        var acquireImageUrls = new TransformBlock<Guid, Tuple<Guid, Option<List<string>>>>(Preparation.ExtractMangaImageUrls, pipelineOptions);
        var downloadImages = new TransformBlock<Tuple<Guid, Option<List<string>>>, Tuple<Guid, List<string>>>(Image.StartDownloads, pipelineOptions);
        var mergeToPdf = new ActionBlock<Tuple<Guid, List<string>>>(Pdf.GenerateNew, pipelineOptions);
 
        /* Specify parameters for pipeline behaviour */
        
        var linkOptions = new DataflowLinkOptions{ PropagateCompletion = true, };

        /* Join pipeline sequences together in order */

        acquireImageUrls.LinkTo(downloadImages, linkOptions);
        downloadImages.LinkTo(mergeToPdf, linkOptions);
        
        /* Provide input to pipeline then signal when you ran out of values */
        Enumerable.Range(Int32.Parse(firstChapter), Int32.Parse(finalChapter))
        .ForEach((chapter, _) =>
        {
            Guid processIdentifier = Guid.NewGuid();
            var userInput = new RequestInfo
            {
                Url = String.Format(url, chapter),
                Title = title,
                Chapter = chapter,
            };
            
            Program.Processes[processIdentifier] = userInput;
            acquireImageUrls.Post(processIdentifier);
        });

        //acquireImageUrls.Post("");
        acquireImageUrls.Complete();
 
        /* Wait for pipeline to finish */
        /* Should be invoked on last sequence of pipeline */

        mergeToPdf.Completion.Wait();
    }

    private static void FailedArguments()
    {
        Helpers.WriteLineColor(Color.DarkOrange, "[WARNING]", "Your command line arguments are not properly formatted.");
        Helpers.WriteLineColor(Color.DarkOrange, """ - use "./{Executable} {Title} {Url} {Chapter}" for a single chapter""");
        Helpers.WriteLineColor(Color.DarkOrange, """ - use "./{Executable} {Title} {Url} {FirstChapter} {FinalChapter}" for multiple chapters""");
    }
}