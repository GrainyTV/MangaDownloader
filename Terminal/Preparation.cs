using Colors;
using ExtensionMethods;
using HelperMethods;
using HtmlAgilityPack;
using Optional;
using Optional.Collections;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

public class RequiredWebData
{
    public string ClassAttribute { get; init; } = null!;
    
    public string ImageLink { get; init; } = null!;
}

public static class Preparation
{
#region Functionality Code

    public static async Task<Tuple<Guid, Option<List<string>>>> ExtractMangaImageUrls(Guid uuid) //(string url)
    {
        var requestInfo = Program.Processes[uuid];
        var url = requestInfo.Url;

        var scraper = new HtmlWeb();
        HtmlDocument website = await scraper.LoadFromWebAsync(url);
        List<HtmlNode> images = website
            .DocumentNode
            .DescendantsAndSelf()
            .Where(node => node.OriginalName.Equals("img"))
            .ToList();

        if (images.Any())
        {
            Dictionary<string, List<string>> imageLinksGroupedByClassAttribute = images
                .Select(image => Preparation.LinkValidator(image))
                .Where(optional => optional.HasValue)
                .Select(optional => optional.Unwrap())
                .GroupBy(data => data.ClassAttribute, data => data.ImageLink, (key, group) => new
                {
                    ClassAttribute = key,
                    ImageLinks = group.ToList(),
                })
                .OrderByDescending(keyValue => keyValue.ImageLinks.Count)
                .ToDictionary(key => key.ClassAttribute, value => value.ImageLinks);

            return imageLinksGroupedByClassAttribute.Count > 0
                ? Tuple.Create(uuid, Option.Some(imageLinksGroupedByClassAttribute.First().Value))
                : Tuple.Create(uuid, Option.None<List<string>>());
        }

        return Tuple.Create(uuid, Option.None<List<string>>());
    }

    private static Option<RequiredWebData> LinkValidator(HtmlNode node)
    {
        Option<string> imageLink = OptionExtensions.SomeNotNull(node.GetAttributeValue("data-src", null) ?? node.GetAttributeValue("src", null));
        Option<string> classAttribute = node
            .Ancestors()
            .Where(node => node.OriginalName == "div")
            .Select(node => node.GetClasses().Flatten())
            .FirstOrNone();

        if (imageLink.HasValue && classAttribute.HasValue && Uri.IsWellFormedUriString(imageLink.Unwrap(), UriKind.Absolute))
        {
            return Option.Some(new RequiredWebData
            {
                ClassAttribute = classAttribute.Unwrap().Trim(),
                ImageLink = imageLink.Unwrap().Trim(),
            });
        }

        return Option.None<RequiredWebData>();
    }

#endregion

/*#region Testing Code

    [Fact]
    public static async void Test_ExtractMangaImageUrls()
    {
        IEnumerable<string> mangaLinks = File.ReadLines("mangaLinks.txt");
        
        foreach (string link in mangaLinks)
        {
            var result = await ExtractMangaImageUrls(link);
            Assert.True(result.HasValue);
        }
    }

    [Fact]
    public static void Test_LinkValidator()
    {
        HtmlDocument owner = new HtmlDocument();
        HtmlNode emptyNode = new HtmlNode(HtmlNodeType.Element, owner, 0);
        Assert.False(LinkValidator(emptyNode).HasValue);

        HtmlNode nodeWithoutAttribute = new HtmlNode(HtmlNodeType.Element, owner, 0);
        nodeWithoutAttribute.Name = "img";
        nodeWithoutAttribute.SetAttributeValue("src", null);
        nodeWithoutAttribute.SetAttributeValue("data-src", null);
        Assert.False(LinkValidator(nodeWithoutAttribute).HasValue);

        emptyNode.AddClass("html_test_class");
        nodeWithoutAttribute.SetAttributeValue("src", File.ReadLines("mangaLinks.txt").First());
        nodeWithoutAttribute.SetParent(emptyNode);
        Assert.True(LinkValidator(nodeWithoutAttribute).HasValue);
    }

#endregion*/
}