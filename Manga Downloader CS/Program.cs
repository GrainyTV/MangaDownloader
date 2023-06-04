using System;
using System.IO;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Collections.Generic;

/// <summary>
/// The Program class decides what operations to start
/// based on the user-input.
/// </summary>
///----------
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

	/// <summary>The run method creates folders to work in and invokes the appropriate processing mode.</summary>
	///--------------
	public void Run()
	{
		var temporaryImageFolder = Directory.CreateDirectory("images");
		Directory.CreateDirectory(Title);

		createChapterProcess.Invoke();

		temporaryImageFolder.Delete();
	}

	/// <summary>The method called when we need to process a single manga chapter.</summary>
	///------------------------------
	public void CreateSingleChapter()
	{
		var preparationValue = new Preparation();
		Stopwatch stopWatch = new Stopwatch();

		/// Asyncronized Solution
		/// ---------------------------------------------------------------
		Console.WriteLine("Async");
		stopWatch = Stopwatch.StartNew();
		
		preparationValue.ExtractNecessaryPageData(Url).Wait();

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

		var reader = new StreamReader("testURLs.txt");

		do
		{
			var program = new Program(1, "Shadows House", reader.ReadLine());
			program.Run();

		} while(reader.EndOfStream == false);	
	}
}
