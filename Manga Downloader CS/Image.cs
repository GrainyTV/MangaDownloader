using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using System.Collections.Generic;

class Image
{
	static readonly HttpClient httpClient = new HttpClient();

	public async Task StartProcessing(string pageUrl, List<string> imageLinks)
	{
		var list = new List<Task>();
		var index = 0;

		foreach(var imageLink in imageLinks)
		{
			list.Add
			(
				httpClient.DownloadFileTaskAsync
				(
					imageLink, 
					Path.Combine("images", String.Format("{0}.png", index + 1)), 
					new Uri(pageUrl).GetLeftPart(UriPartial.Authority)
				)
			);
			
			++index;
		}

		await Task.WhenAll(list);
	}
}