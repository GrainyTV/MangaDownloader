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
		Directory.CreateDirectory(Path.Combine("images", title));
		var list = new List<Task>();

		for(int i = 0; i < imageLinks.Count; ++i)
		{
			var extension = Path.GetExtension(imageLinks[i]);			

			list.Add(
				httpClient.DownloadFileTaskAsync(
					imageLinks[i], 
					Path.Combine("images", title, String.Format("{0}{1}", i + 1, (extension != String.Empty) ? extension : ".jpg")), 
					new Uri(pageUrl).GetLeftPart(UriPartial.Authority)
				)
			);
		}

		await Task.WhenAll(list);
	}
}