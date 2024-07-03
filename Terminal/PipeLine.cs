using System;
using System.Threading.Tasks.Dataflow;

public class PipeLine
{
    private TransformBlock<RequestInfo, ImageUrlBundle> extractImagesFromWebsite;
    private TransformBlock<ImageUrlBundle, PathBundle> downloadImages;
    private ActionBlock<PathBundle> mergeToPdf;

    public PipeLine()
    {
        // Specify parameters for pipeline behaviour
        // -----------------------------------------
        var executionOptions = new ExecutionDataflowBlockOptions { MaxDegreeOfParallelism = Environment.ProcessorCount, SingleProducerConstrained = true, };
        var linkageOptions = new DataflowLinkOptions { PropagateCompletion = true, };

        // Instantiate the individual pipe pieces
        // --------------------------------------
        extractImagesFromWebsite = new TransformBlock<RequestInfo, ImageUrlBundle>(Preparation.ExtractMangaImageUrls, executionOptions);
        downloadImages = new TransformBlock<ImageUrlBundle, PathBundle>(Image.StartDownloads, executionOptions);
        mergeToPdf = new ActionBlock<PathBundle>(Pdf.GenerateNew, executionOptions);

        // Then connect them together
        // --------------------------
        extractImagesFromWebsite.LinkTo(downloadImages, linkageOptions);
        downloadImages.LinkTo(mergeToPdf, linkageOptions);
    }

    public void AppendNewEntry(RequestInfo requestInfo)
    {
        extractImagesFromWebsite.Post(requestInfo);
    }

    public void Begin()
    {
        extractImagesFromWebsite.Complete();
        mergeToPdf.Completion.Wait();
    }
}