//
// .NET usings
//
using System;
using System.Threading.Tasks;

//
// HtmlAgilityPack usings
//
using HtmlAgilityPack;

struct RequiredWebData
{
	public string ClassAttribute { get; init; }
	public string ImageLink { get; init; }
}

class Dispose
{
	public static RequiredWebData LinkValidator(HtmlNode image)
	{
		var imageLink = ExtractUrlFromImgNode(image);

		if (Uri.IsWellFormedUriString(imageLink, UriKind.Absolute))
		{
			var classAttribute = FindClassAttributeOfParentDiv(image);
			return new RequiredWebData() {
					   ClassAttribute = classAttribute, ImageLink = imageLink
			};
		}

		throw new InvalidOperationException($"The src or data-src attribute of the image points to an invalid URL!\n-> {imageLink} <-");
	}

	private static string ExtractUrlFromImgNode(HtmlNode image)
	{
		var dataSrc = image.GetAttributeValue("data-src", null);
		var src = image.GetAttributeValue("src", null);

		return (dataSrc != null) ? dataSrc :
		       (src != null) ? src :
		       throw new InvalidOperationException("An HTML img node has neither \"src\", nor \"data-src\" attribute!");
	}

	private static string FindClassAttributeOfParentDiv(HtmlNode image)
	{
		foreach (var parent in image.Ancestors())
		{
			if (parent.Name.Equals("div"))
			{
				var classAttr = parent.GetAttributeValue("class", null);

				if (classAttr != null)
				{
					return classAttr;
				}
			}
		}

		throw new InvalidOperationException("There is no parent node with type \"div\" and attribute \"class\" in the ancestry tree!");
	}
}