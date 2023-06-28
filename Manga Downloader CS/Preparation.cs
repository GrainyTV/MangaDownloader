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
	private static readonly HtmlWeb httpClient = new HtmlWeb();
	private Dictionary<string, List<string>> htmlImageBlocks;

	/// <summary>The constructor sets the User-Agent to Google Chrome's and creates a dictionary.</summary>
	///-----------------
	public Preparation()
	{
		httpClient.UserAgent = "Mozilla/5.0 (X11; Linux x86_64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/111.0.0.0 Safari/537.36";
		htmlImageBlocks = new Dictionary<string, List<string>>();
	}

	/// <summary>An async method that requests a website, collects all the images and validates them.</summary>
	/// <param name="url">The URL of the website.</param>
	///-----------------------------------------------------------------
	public async Task<List<string>> ExtractNecessaryPageData(string url)
	{
		var website = await httpClient.LoadFromWebAsync(url);
		var images = website.DocumentNode.SelectNodes("//img");

		//
		// There are images on the provided website
		//

		if(images != null)
		{
			DisposeUnnecessaryImages(images);

			/*foreach(var key in htmlImageBlocks.Keys)
			{
				Console.WriteLine($"{key} - {htmlImageBlocks[key].Count}");
			}*/

			//
			// Query for the longest entry we have (Return div with the most images.)
			//

			return htmlImageBlocks.OrderByDescending(dictionary => dictionary.Value.Count).First().Value;
		}

		throw new InvalidOperationException("The provided website has no available images.");
	}

	/// <summary>An async method to store all valid (div-image) pairs in a dictionary.</summary>
	/// <param name="images">All the img nodes found on the website.</param>
	///-----------------------------------------------------------
	private void DisposeUnnecessaryImages(HtmlNodeCollection images)
	{
		foreach(var image in images)
		{
			try
			{
				var query = Dispose.LinkValidator(image);

				htmlImageBlocks.AddOrUpdate(query.ClassAttribute, query.ImageLink);
			}
			catch(InvalidOperationException)
			{
				//
				// Expected behavior for invalid images
				// Possible causes:
				//  -no src or data-src attribute
				//  -invalid URL in source attribute
				//  -no parent div found with class attribute
				//
			}
		}
	} 
}