module Program
open CommonTypes
open System
open System.IO
open System.Threading.RateLimiting
//open type Network.ChapterDescriptor
//open type Network.ImageDescriptor
//open type Network.PdfDescriptor

type UserRequest() =
    member val Title: string = String.Empty with get, set
    member val Url: string = String.Empty with get, set
    member val FirstChapter: int = -1 with get, set
    member val FinalChapter: int = -1 with get, set

type private PipelineContext(request: UserRequest) =
    member self.OutDirectory: string = request.Title
    member self.BatchSize: int = Math.Min(int (Math.Ceiling(double request.FinalChapter * 1.15)), 500)
    member self.FeedUrl: string = Utility.assembleIntermediateFeedUrl (request.Url) (self.BatchSize)
    member self.LeadingZeroCount: int = Utility.calculateLeadingZeros (request.FinalChapter)

    member self.Chapters: seq<ChapterEntry> = 
        Network.collectChaptersFromFeed
            (self.FeedUrl)
            (self.BatchSize)
            ({ request.FirstChapter .. request.FinalChapter } |> Seq.map (fun number -> ChapterEntry(number, request.Title, self.LeadingZeroCount)))

let createChaptersFrom (request: UserRequest) : Unit =    
    Network.setUserAgent ()

    let context = PipelineContext(request)
    Utility.createDirectoryIfNeeded (context.OutDirectory)
    
    let rateLimiterOptions = FixedWindowRateLimiterOptions ()
    rateLimiterOptions.AutoReplenishment <- true
    rateLimiterOptions.PermitLimit <- 40
    rateLimiterOptions.QueueLimit <- Int32.MaxValue
    rateLimiterOptions.QueueProcessingOrder <- QueueProcessingOrder.OldestFirst
    rateLimiterOptions.Window <- TimeSpan.FromMinutes(1)

    use rateLimiter = new FixedWindowRateLimiter(rateLimiterOptions)

    context.Chapters
    |> Seq.filter (fun chapter -> chapter.Id <> String.Empty)
    |> Seq.map (fun chapter -> async {
        use! lease = rateLimiter.AcquireAsync().AsTask() |> Async.AwaitTask

        // Process will wait here after exhausting the quota

        let! imageUrls = Network.collectImageUrlsOfChapterAsync (chapter.Id)
        chapter.RemoteFiles <- imageUrls

        Network.startImageDownloads (chapter)
        Pdf.generateNew (chapter)

        //let imageDescriptor = { Number = chapter.Number; Urls = imageUrls }
        //let pdfDescriptor = Network.startImageDownloads (chapter) //(imageDescriptor) (context.OutDirectory)
        
        //Pdf.generateNew pdfDescriptor.Files (PathJoin.from [
        //    context.OutDirectory
        //    $"{request.Title} Chapter {Preprocess.addLeadingZerosIfNecessary imageDescriptor.Number request.FinalChapter}.pdf"
        //]) //(Path.Join(context.OutDirectory, $"{request.Title} Chapter {Preprocess.addLeadingZerosIfNecessary imageDescriptor.Number context.Count}.pdf"))
    })
    |> fun computations -> Async.Parallel(computations, Environment.ProcessorCount)
    |> Async.Ignore
    |> Async.RunSynchronously

    //async {
        //let imageUrls = context.ChapterDescriptors |> Seq.map(fun desc -> { Number = desc.Number; Urls = Network.collectImageUrlsOfChapter desc.Id })
        //context.ImageDescriptors <- imageUrls
        
        //context.ChapterDescriptors
        //|> Seq.map(fun desc -> async { { Number = desc.Number; Urls = Network.collectImageUrlsOfChapter desc.Id } })
        //|> Seq.map(fun desc -> async { Network.startImageDownloads desc context.OutDirectory })
        //|> Seq.iter (fun image -> async { Pdf.generateNew image.Files (Path.Join(context.OutDirectory, $"{request.Title} Chapter {Preprocess.addLeadingZerosIfNecessary image.Number context.Count}.pdf" )) })

        //let localImageFiles = context.ImageDescriptors |> Seq.map(fun desc -> Network.startImageDownloads desc context.OutDirectory)
    
        //localImageFiles |> Seq.iter (fun image -> Pdf.generateNew image.Files (Path.Join(context.OutDirectory, $"{request.Title} Chapter {Preprocess.addLeadingZerosIfNecessary image.Number context.Count}.pdf")))

    //}


//let private failedArguments () : Unit =
//    printfn "You have not supplied the necessary command line arguments!"

//let startFromArgs (args: array<string>) : Unit =
//    match args.Length with
//    | 3 -> createChaptersFrom { Title = args[0]; Url = args[1]; FirstChapter = Int32.Parse(args[2]); FinalChapter = Int32.Parse(args[2]) }
//    | 4 -> createChaptersFrom { Title = args[0]; Url = args[1]; FirstChapter = Int32.Parse(args[2]); FinalChapter = Int32.Parse(args[3]) }
//    | _ -> failedArguments ()
