using System;
using System.Threading.Tasks;
using HtmlAgilityPack;

/// <summary>
/// The RequiredWebData struct is used to return a key-value pair 
/// of a div's class attribute and the URL of the image found in it.
/// </summary>
///-------------------
struct RequiredWebData
{
	public string ClassAttribute { get; init; }
	public string ImageLink { get; init; }

	public RequiredWebData(string classAttr, string imageLink)
	{
		ClassAttribute = classAttr;
		ImageLink = imageLink;
	}
}

/// <summary>
/// The Dispose class provides methods to remove all the invalid 
/// and unnecessary HTML img nodes from our operation.
/// </summary>
///----------
class Dispose
{
	/// <summary>An async method that checks validity of an img node.</summary>
	/// <param name="image">A singular HTML img node from the website.</param>
	/// <returns>The location of the img node combined with the name of the div where it is found.</returns>
	/// <exception cref="InvalidOperationException">An exception if the node doesn't have a valid location / URL.</exception> 
	///--------------------------------------------------------------------
	public static RequiredWebData LinkValidator(HtmlNode image)
	{
		var imageLink = ExtractUrlFromImgNode(image);

		if(Uri.IsWellFormedUriString(imageLink, UriKind.Absolute))
		{
			var classAttribute = FindClassAttributeOfParentDiv(image);
			return new RequiredWebData(classAttribute, imageLink);
		}

		throw new InvalidOperationException($"The src or data-src attribute of the image points to an invalid URL!\n-> {imageLink} <-");
	}

	/// <summary>A helper method to find the attribute, which contains the location / URL of our image.</summary>
	/// <param name="image">A singular HTML img node from the website.</param>
	/// <returns>The location / URL of the img node.</returns>
	/// <exception cref="InvalidOperationException">Exception if the node doesn't have an "src" or "data-src" attribute.</exception> 
	///------------------------------------------------
	private static string ExtractUrlFromImgNode(HtmlNode image)
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
	/// <exception cref="InvalidOperationException">Exception if the node doesn't have a parent with type "div" and "class" attribute.</exception> 
	///--------------------------------------------------------
	private static string FindClassAttributeOfParentDiv(HtmlNode image)
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