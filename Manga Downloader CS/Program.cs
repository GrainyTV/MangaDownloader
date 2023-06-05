using System;
using System.IO;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Collections.Generic;

struct UserInput
{
	public int FirstChapter { get; init; }
	public int FinalChapter { get; init; }
	public string Title { get; init; }
	public string Url { get; init; }

	public UserInput(int firstChapter, int finalChapter, string title, string url)
	{
		FirstChapter = firstChapter;
		FinalChapter = finalChapter;
		Title = title;
		Url = url;
	}
}

/// <summary>
/// The Program class decides what operations to start
/// based on the user-input.
/// </summary>
///----------
class Program
{
	delegate void ApplicationJunction();
	ApplicationJunction createChapterProcess;
	UserInput userGivenData;

	/// <summary>Constructor for creating multiple manga chapters.</summary>
	/// <param name="firstChapter">The chosen first chapter to include from the manga.</param>
	/// <param name="finalChapter">The chosen last chapter to include from the manga.</param>
	/// <param name="title">The chosen title for the manga.</param>
	/// <param name="url">The url where the manga is hosted.</param>
	///-------------------------------------------------------------------------
	public Program(int firstChapter, int finalChapter, string title, string url)
	{
		userGivenData = new UserInput(firstChapter, finalChapter, title, url);
		createChapterProcess = CreateMultipleChapters;
	}

	/// <summary>Alternative constructor for creating only a single manga chapter.</summary>
	/// <param name="chapter">The chosen chapter of the manga.</param>
	/// <param name="title">The chosen title for the manga.</param>
	/// <param name="url">The url where the manga is hosted.</param>
	///--------------------------------------------------
	public Program(int chapter, string title, string url)
	{
		userGivenData = new UserInput(chapter, -1, title, url);
		createChapterProcess = CreateSingleChapter;
	}

	/// <summary>The run method creates folders to work in and invokes the appropriate processing mode.</summary>
	///--------------
	public void Run()
	{
		var temporaryImageFolder = Directory.CreateDirectory("images");
		Directory.CreateDirectory(userGivenData.Title);

		createChapterProcess.Invoke();

		//temporaryImageFolder.Delete();
	}

	/// <summary>The method called when we need to process a single manga chapter.</summary>
	///------------------------------
	public void CreateSingleChapter()
	{
		var preparationValue = new Preparation();
		var imageHandler = new Image();

		Stopwatch stopWatch = new Stopwatch();

		/// Asyncronized Solution
		/// ---------------------------------------------------------------
		Console.WriteLine("Async");
		stopWatch = Stopwatch.StartNew();
		
		try
		{
			var listOfImageUrls = preparationValue.ExtractNecessaryPageData(userGivenData.Url).Result;
			imageHandler.StartProcessing(userGivenData.Url, listOfImageUrls).Wait();
		}
		catch(AggregateException ex)
		{
			Console.WriteLine(ex.InnerException.Message);
		}

		stopWatch.Stop();
		Console.WriteLine(stopWatch.Elapsed + "\n");
		/// ---------------------------------------------------------------
	}

	/// <summary>The method called when we need to process multiple manga chapters.</summary>
	///---------------------------------
	public void CreateMultipleChapters()
	{
		//var tasks = new List<Task>();
		//tasks.Add(preparationValue.ExtractNecessaryPageDataAsync(Url));
		//Task.WhenAll(tasks).Wait();
	}

	/// <summary>The entry point of our application.</summary>
	///---------------
	static void Main()
	{
		// Program program = (args.Length == 4) ? new Program(int.Parse(args[0]), int.Parse(args[1]), args[2], args[3]) : new Program(int.Parse(args[0]), args[1], args[2]);
		// program.Run();

		/*using(var reader = new StreamReader("testURLs.txt"))
		{
			do
			{
				var program = new Program(1, Guid.NewGuid().ToString("N"), reader.ReadLine());
				program.Run();

			} while(reader.EndOfStream == false);
		}*/

		//var program = new Program(1, "Shadows House", "https://ww5.mangakakalot.tv/chapter/manga-di980617/chapter-1");
		//var program = new Program(1, "Shadows House", "https://lfe.org/");

		var program = new Program(1, "Shadows House", "https://chapmanganato.com/manga-mn989748/chapter-165");
		program.Run();
	}
}
