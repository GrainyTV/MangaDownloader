using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using System.Collections.Generic;

class Image
{
	static readonly HttpClient httpClient = new HttpClient();

	public async Task StartProcessing(string pageUrl, string title, List<string> imageLinks)
	{
		var list = new List<Task>();

		for(int i = 0; i < imageLinks.Count; ++i)
		{
			Directory.CreateDirectory(Path.Combine("images", title));

			list.Add
			(
				httpClient.DownloadFileTaskAsync
				(
					imageLinks[i], 
					Path.Combine("images", title, String.Format("{0}.png", i + 1)), 
					new Uri(pageUrl).GetLeftPart(UriPartial.Authority)
				)
			);
		}

		await Task.WhenAll(list);
	}
}