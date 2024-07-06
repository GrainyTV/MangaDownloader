//
// .NET usings
//
using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using System.Collections.Generic;

//
// LinkDotNet usings
//
using LinkDotNet.StringBuilder;

public static class Image
{
	#region Static

	private static readonly HttpClient httpClient = new HttpClient();

	public static void StartDownloading(string pageUrl, string title, List<string> imageLinks)
	{
		string refererProperty = new Uri(pageUrl).GetLeftPart(UriPartial.Authority);
		string folder = ValueStringBuilder.Concat("images", '/', title, '/');

		Parallel.For(0, imageLinks.Count, i =>
		{
			string doesfileHaveExtension = Path.GetExtension(imageLinks[i]);
			string fileExtension = (doesfileHaveExtension == String.Empty) ? ".jpg" : doesfileHaveExtension;

			httpClient.DownloadFile(imageLinks[i], ValueStringBuilder.Concat(folder, i + 1, fileExtension), refererProperty);
		});
	}

	#endregion
}