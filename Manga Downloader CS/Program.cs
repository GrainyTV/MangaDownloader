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
	private delegate void ApplicationJunction();
	private ApplicationJunction createChapterProcess;
	private UserInput userGivenData;

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
		var imageDirectory = Directory.CreateDirectory(Path.Combine("images", userGivenData.Title));
		var resultDirectory = Directory.CreateDirectory(userGivenData.Title);

		createChapterProcess.Invoke();
		imageDirectory.Delete(true);
		temporaryImageFolder.Delete();
	}

	/// <summary>The method called when we need to process a single manga chapter.</summary>
	///------------------------------
	public void CreateSingleChapter()
	{
		var preparationValue = new Preparation();
		var imageHandler = new Image();
		var pdfCreator = new PdfCreator();

		try
		{
			Stopwatch st = new Stopwatch();
			st = Stopwatch.StartNew();

			var listOfImageUrls = preparationValue.ExtractNecessaryPageData(userGivenData.Url).Result;

			st.Stop();
			Console.WriteLine("Web scraping: " + st.Elapsed);
			st = Stopwatch.StartNew();

			imageHandler.StartProcessing(userGivenData.Url, userGivenData.Title, listOfImageUrls).Wait();

			st.Stop();
			Console.WriteLine("Image downloading: " + st.Elapsed);
			st = Stopwatch.StartNew();
			
			pdfCreator.GenerateNewFromImages(
				Path.Combine("images", userGivenData.Title),
				Path.Combine(userGivenData.Title, string.Format("{0} Chapter {1}.pdf", userGivenData.Title, userGivenData.FirstChapter))
			);

			st.Stop();
			Console.WriteLine("PDF creating: " + st.Elapsed + "\n");
		}
		catch(AggregateException ex)
		{
			if(ex.InnerException != null)
			{
				Console.WriteLine(ex.InnerException.Message);
				Console.WriteLine(ex.StackTrace);
			}
			
			Directory.Delete(userGivenData.Title, false);
		}
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
		var program = new Program(1, 10, "Shadows House", "https://chapmanganato.com/manga-mn989748/chapter-{0}");
		program.Run();

		// Testing
		// |
		// V

		/*using(var reader = new StreamReader("testURLs.txt"))
		{
			do
			{
				var line = reader.ReadLine();

				if(line != null)
				{
					string[] split = line.Split(';');
					var program = new Program(int.Parse(split[2]), split[1], split[0]);
					program.Run();
				}	

			} while(reader.EndOfStream == false);
		}*/
	}
}
