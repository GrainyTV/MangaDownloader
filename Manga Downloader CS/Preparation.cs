using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using HtmlAgilityPack;

/// <summary>
/// The Preparation class collects 
/// the required data to work on.
/// </summary>
///--------------
class Preparation
{
	static readonly HtmlWeb httpClient = new HtmlWeb();
	static Dictionary<string, List<string>> htmlImageBlocks = new Dictionary<string, List<string>>();

	/// <summary>The constructor sets the User-Agent to Google Chrome's.</summary>
	///-----------------
	public Preparation()
	{
		httpClient.UserAgent = "Mozilla/5.0 (X11; Linux x86_64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/114.0.0.0 Safari/537.36";
	}

	/// <summary>An async method that request a website, collects all the images and validates them.</summary>
	/// <param name="url">The URL of the website.</param>
	///---------------------------------------------------
	public async Task ExtractNecessaryPageData(string url)
	{
		var website = httpClient.Load(url);
		var images = website.DocumentNode.SelectNodes("//img");

		if(images != null)
		{
			await DisposeUnnecessaryImages(images);

			foreach(var key in htmlImageBlocks.Keys)
			{
				Console.WriteLine($"{key} - {htmlImageBlocks[key].Count}");
			}

			htmlImageBlocks.Clear();
		}	
	}

	/// <summary>An async method to store all valid (div-image) pairs in a dictionary.</summary>
	/// <param name="images">All the img nodes found on the website.</param>
	///------------------------------------------------------------------
	static async Task DisposeUnnecessaryImages(HtmlNodeCollection images)
	{
		var list = new List<Task<RequiredWebData>>();

		// Push all image tasks to container

		foreach(var image in images)
		{
			list.Add(Dispose.LinkValidator(image));
		}

		// Check the result of all the finished tasks

		while(list.Count > 0)
		{
			var finishedTask = await Task.WhenAny(list);

			try
			{		
				var query = await finishedTask;

				if(htmlImageBlocks.ContainsKey(query.ClassAttribute))
				{
					htmlImageBlocks[query.ClassAttribute].Add(query.ImageLink);
				}
				else
				{
					htmlImageBlocks.Add(query.ClassAttribute, new List<string>());
					htmlImageBlocks[query.ClassAttribute].Add(query.ImageLink);
				}

				list.Remove(finishedTask);
			}
			catch(InvalidOperationException ex)
			{
				// Print exception messages for debugging purposes

				Console.WriteLine(ex.Message);

				// Expected behavior for invalid images.
				
				list.Remove(finishedTask);
			}
		}
	} 
}