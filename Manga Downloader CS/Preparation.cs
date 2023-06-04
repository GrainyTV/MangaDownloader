using HtmlAgilityPack;

class Preparation
{
	static readonly HtmlWeb httpClient = new HtmlWeb();
	static Dictionary<string, List<string>> htmlImageBlocks = new Dictionary<string, List<string>>();

	public Preparation()
	{
		httpClient.UserAgent = "Mozilla/5.0 (X11; Linux x86_64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/114.0.0.0 Safari/537.36";
	}

	public static void ExtractNecessaryPageDataPool(object param)
	{
		var url = (string) param;
		
		var website = httpClient.Load(url);
		var images = website.DocumentNode.SelectNodes("//img");

		if(images != null)
		{
			DisposeUnnecessaryImages(images);

			foreach(var key in htmlImageBlocks.Keys)
			{
				Console.WriteLine($"{key} - {htmlImageBlocks[key].Count}");
			}

			htmlImageBlocks.Clear();
		}
	}

	public void ExtractNecessaryPageDataSync(string url)
	{
		var website = httpClient.Load(url);
		var images = website.DocumentNode.SelectNodes("//img");

		if(images != null)
		{
			DisposeUnnecessaryImages(images);

			foreach(var key in htmlImageBlocks.Keys)
			{
				Console.WriteLine($"{key} - {htmlImageBlocks[key].Count}");
			}

			htmlImageBlocks.Clear();
		}
	}

	public Task ExtractNecessaryPageDataAsync(string url)
	{
		return Task.Run(() => 
		{ 
			var website = httpClient.Load(url);
			var images = website.DocumentNode.SelectNodes("//img");

			if(images != null)
			{
				DisposeUnnecessaryImages(images);

				foreach(var key in htmlImageBlocks.Keys)
				{
					Console.WriteLine($"{key} - {htmlImageBlocks[key].Count}");
				}

				htmlImageBlocks.Clear();
			}	
		});
	}

	public static async void DisposeUnnecessaryImages(HtmlNodeCollection images)
	{
		var list = new List<Task<RequiredWebData>>();

		foreach(var image in images)
		{
			list.Add(Dispose.LinkValidator(image));
		}

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
			catch(InvalidOperationException)
			{
				// Expected behavior for invalid images.
				list.Remove(finishedTask);
			}
		}
	} 
}