using HtmlAgilityPack;

class RequiredWebData
{
	public string ClassAttribute { get; init; }
	public string ImageLink { get; init; }

	public RequiredWebData(string classAttr, string imageLink)
	{
		ClassAttribute = classAttr;
		ImageLink = imageLink;
	}
}

class Dispose
{
	public static async Task<RequiredWebData> LinkValidator(HtmlNode image)
	{
		await Task.Run(() =>
		{
			var imageLink = ExtractUrlFromImgNode(image);

			if(Uri.IsWellFormedUriString(imageLink, UriKind.Absolute))
			{
				return new RequiredWebData(FindClassAttributeOfParentDiv(image), imageLink);
			}

			throw new InvalidOperationException($"The src or data-src attribute of the image points to an invalid URL!\n-> {imageLink} <-");
		});
	}

	static string ExtractUrlFromImgNode(HtmlNode image)
	{
		var dataSrc = image.GetAttributeValue("data-src", null);
		var src = image.GetAttributeValue("src", null);

		return (dataSrc != null) ? dataSrc : 
			   (src != null) ? src : 
			   throw new InvalidOperationException("An HTML img node has neither \"src\", nor \"data-src\" attribute!");
	}

	/// <summary>A helper method to find the first parent-div, where our required images are stored.</summary>
	/// <param name="image">A singular HTML img node from the website.</param>
	/// <returns>The first parent of an img node that is of type div.</returns>
	///---------------------------------------------------------------
	static string FindClassAttributeOfParentDiv(HtmlNode image)
	{
		foreach(var parent in image.Ancestors())
		{
			if(parent.Name.Equals("div"))
			{
				var classAttr = parent.GetAttributeValue("class", null);

				if(classAttr != null)
				{
					return classAttr;
				}
			}
		}

		throw new InvalidOperationException("There is no parent node with type \"div\" and attribute \"class\" in the ancestry tree!");
	}
}