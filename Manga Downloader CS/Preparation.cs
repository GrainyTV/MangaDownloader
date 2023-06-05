using System;
using System.Linq;
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
	Dictionary<string, List<string>> htmlImageBlocks;

	/// <summary>The constructor sets the User-Agent to Google Chrome's and creates a dictionary.</summary>
	///-----------------
	public Preparation()
	{
		httpClient.UserAgent = "Mozilla/5.0 (X11; Linux x86_64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/111.0.0.0 Safari/537.36";
		htmlImageBlocks = new Dictionary<string, List<string>>();
	}

	/// <summary>An async method that request a website, collects all the images and validates them.</summary>
	/// <param name="url">The URL of the website.</param>
	///-----------------------------------------------------------------
	public async Task<List<string>> ExtractNecessaryPageData(string url)
	{
		var website = await httpClient.LoadFromWebAsync(url);
		var images = website.DocumentNode.SelectNodes("//img");

		if(images != null)
		{
			await DisposeUnnecessaryImages(images);

			/*foreach(var key in htmlImageBlocks.Keys)
			{
				Console.WriteLine($"{key} - {htmlImageBlocks[key].Count}");
			}*/

			// Query for the longest entry we have (We need the div with the most images.)

			return htmlImageBlocks.OrderByDescending(dictionary => dictionary.Value.Count).First().Value;
		}

		throw new InvalidOperationException("The provided website has no available images.");
	}

	/// <summary>An async method to store all valid (div-image) pairs in a dictionary.</summary>
	/// <param name="images">All the img nodes found on the website.</param>
	///-----------------------------------------------------------
	async Task DisposeUnnecessaryImages(HtmlNodeCollection images)
	{
		var list = new List<Task<RequiredWebData>>();

		// Push all image tasks to container

		foreach(var image in images)
		{
			list.Add(Dispose.LinkValidator(image));
		}

		// Check the result of all the finished tasks
		try
		{
			Task.WaitAll(list.ToArray());
		}
		catch(Exception)
		{
		}

		foreach(var result in list)
		{
			if(result.IsFaulted == false)
			{
				var query = await result;

				if(htmlImageBlocks.ContainsKey(query.ClassAttribute))
				{
					htmlImageBlocks[query.ClassAttribute].Add(query.ImageLink);
				}
				else
				{
					htmlImageBlocks.Add(query.ClassAttribute, new List<string>());
					htmlImageBlocks[query.ClassAttribute].Add(query.ImageLink);
				}
			}
		}
	} 
}