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
	public static RequiredWebData? LinkValidator(HtmlNode image)
	{
		string? imageLink = Dispose.ExtractUrlFromImgNode(image);

		if (Uri.IsWellFormedUriString(imageLink, UriKind.Absolute))
		{
			string? classAttribute = Dispose.FindClassAttributeOfParentDiv(image);

			if (classAttribute is not null)
			{
				return new RequiredWebData()
				{
					ClassAttribute = classAttribute,
					ImageLink = imageLink,
				};
			}
		}

		return default(RequiredWebData);
	}

	private static string? ExtractUrlFromImgNode(HtmlNode image)
	{
		return image.GetAttributeValue("data-src", null) ??
			   image.GetAttributeValue("src", null) ??
			   default(string);
	}

	private static string? FindClassAttributeOfParentDiv(HtmlNode image)
	{
		foreach (HtmlNode parent in image.Ancestors())
		{
			if (parent.Name.Equals("div"))
			{
				string classAttr = parent.GetAttributeValue("class", null);

				if (classAttr is null)
				{
					continue;
				}

				return classAttr;
			}
		}

		return default(string);
	}
}