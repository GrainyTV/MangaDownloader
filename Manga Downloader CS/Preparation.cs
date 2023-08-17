//
// .NET usings
//
using System;
using System.Linq;
using System.Collections.Generic;

//
// HtmlAgilityPack usings
//
using HtmlAgilityPack;

public class Preparation
{
	#region Instance

	private Dictionary<string, List<string>> htmlImageBlocks = new Dictionary<string, List<string>>();

	public void GroupByDiv(HtmlNodeCollection images)
	{
		foreach (HtmlNode image in images)
		{
			try
			{
				RequiredWebData query = Dispose.LinkValidator(image);
				htmlImageBlocks.AddOrUpdate(query.ClassAttribute, query.ImageLink);
			}
			catch (InvalidOperationException)
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

	public List<string> EntryWithMostItems()
	{
		//
		// Query for the longest entry we have (Return div with the most images.)
		//

		return htmlImageBlocks.OrderByDescending(dictionary => dictionary.Value.Count).First().Value;
	}

	#endregion

	#region Static

	private static readonly HtmlWeb httpClient = new HtmlWeb();

	public static List<string> ExtractMangaImageUrls(string url)
	{
		HtmlDocument website = httpClient.Load(url);
		HtmlNodeCollection images = website.DocumentNode.SelectNodes("//img");

		//
		// There are images on the provided website
		//

		if (images != null)
		{
			Preparation imageFilter = new Preparation();
			imageFilter.GroupByDiv(images);

			return imageFilter.EntryWithMostItems();
		}

		throw new InvalidOperationException("The provided website has no available images.");
	}

	#endregion
}