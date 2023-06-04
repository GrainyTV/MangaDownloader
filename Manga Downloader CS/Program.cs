using System.Diagnostics;

class Program
{
	delegate void ApplicationJunction();
	ApplicationJunction createChapterProcess;
	public int FirstChapter { get; init; }
	public int FinalChapter { get; init; }
	public string Title { get; init; }
	public string Url { get; init; }

	/// <summary>Constructor for creating multiple manga chapters.</summary>
	/// <param name="firstChapter">The chosen first chapter to include from the manga.</param>
	/// <param name="finalChapter">The chosen last chapter to include from the manga.</param>
	/// <param name="title">The chosen title for the manga.</param>
	/// <param name="url">The url where the manga is hosted.</param>
	///-------------------------------------------------------------------------
	public Program(int firstChapter, int finalChapter, string title, string url)
	{
		FirstChapter = firstChapter;
		FinalChapter = finalChapter;
		Title = title;
		Url = url;
		createChapterProcess = CreateMultipleChapters;
	}

	/// <summary>Alternative constructor for creating only a single manga chapter.</summary>
	/// <param name="chapter">The chosen chapter of the manga.</param>
	/// <param name="title">The chosen title for the manga.</param>
	/// <param name="url">The url where the manga is hosted.</param>
	///--------------------------------------------------
	public Program(int chapter, string title, string url)
	{
		FirstChapter = chapter;
		FinalChapter = -1;
		Title = title;
		Url = url;
		createChapterProcess = CreateSingleChapter;
	}

	public void Run()
	{
		var temporaryImageFolder = Directory.CreateDirectory("images");
		Directory.CreateDirectory(Title);

		createChapterProcess.Invoke();

		temporaryImageFolder.Delete();
	}

	public void CreateSingleChapter()
	{
		var preparationValue = new Preparation();
		Stopwatch stopWatch = new Stopwatch();

		/// Syncronized Solution
		/// ---------------------------------------------------------------
		Console.WriteLine("Sync");
		stopWatch = Stopwatch.StartNew();
		preparationValue.ExtractNecessaryPageDataSync(Url);

		stopWatch.Stop();
		Console.WriteLine(stopWatch.Elapsed + "\n");
		/// ---------------------------------------------------------------

		/// ThreadPool Solution
		/// ---------------------------------------------------------------
		Console.WriteLine("ThreadPool");
		stopWatch = Stopwatch.StartNew();

		/*using(var countdownEvent = new CountdownEvent(1))
		{
			for(int i = 0; i < 1; ++i)
			{
				ThreadPool.QueueUserWorkItem(param => { Preparation.ExtractNecessaryPageDataPool(param); countdownEvent.Signal(); }, link.Replace("{0}", (i + 1).ToString()));
			}

			countdownEvent.Wait();
		}*/

		using(var countdownEvent = new CountdownEvent(1))
		{
			ThreadPool.QueueUserWorkItem(param => { Preparation.ExtractNecessaryPageDataPool(param); countdownEvent.Signal(); }, Url);
			countdownEvent.Wait();
		}

		
		stopWatch.Stop();
		Console.WriteLine(stopWatch.Elapsed + "\n");
		/// ---------------------------------------------------------------

		/// Asyncronized Solution
		/// ---------------------------------------------------------------
		Console.WriteLine("Async");
		stopWatch = Stopwatch.StartNew();
		
		var tasks = new List<Task>();
		tasks.Add(preparationValue.ExtractNecessaryPageDataAsync(Url));

		Task.WhenAll(tasks).Wait();

		stopWatch.Stop();
		Console.WriteLine(stopWatch.Elapsed + "\n");
		/// ---------------------------------------------------------------
	}

	public /*async Task*/void CreateMultipleChapters()
	{

	}

	static void Main(string[] args)
	{
		Program program = (args.Length == 4) ? new Program(int.Parse(args[0]), int.Parse(args[1]), args[2], args[3]) : new Program(int.Parse(args[0]), args[1], args[2]);
		program.Run();
	}
}
