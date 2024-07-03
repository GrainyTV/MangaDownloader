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

using System.Text.RegularExpressions;

public static class Preparation
{
    private class RequiredWebData
    {
        public string ClassAttribute { get; init; } = null!;
        public string ImageLink { get; init; } = null!;
    }

    public static async Task<ImageUrlBundle> ExtractMangaImageUrls(RequestInfo requestInfo)
    {
        var scraper = new HtmlWeb();
        HtmlDocument website = await scraper.LoadFromWebAsync(requestInfo.Url);

        List<HtmlNode> scriptNodes = website
            .DocumentNode
            .DescendantsAndSelf()
            .Where(node => node.OriginalName.Equals("script") && node.InnerText.Trim().Equals(String.Empty) == false)
            .ToList();

        /*Console.WriteLine($"Found {scriptNodes.Count} non-empty scripts.");

        foreach (var node in scriptNodes)
        {
            Console.WriteLine(node.InnerText);
            Console.WriteLine("===================================================================");
        }*/

        string pattern = @"https:\/\/([a-zA-Z0-9]*\.)?([a-zA-Z0-9]*\.)[a-zA-Z]*[a-zA-Z0-9_\-\.\/\?\=\&]*";
        string pattern2 = @"(?<=\/)\d+(?=[a-z-_.]*$)|(?<=\/[a-z]*)\d+(?=\.)";
        var regex = new Regex(pattern, RegexOptions.Compiled);

        List<MatchCollection> foundUrls = scriptNodes
            .Where(node => regex.IsMatch(node.InnerText.Trim()))
            .Select(node => regex.Matches(node.InnerText.Trim()))
            .ToList();

        Option<MatchCollection> imageLinks = OptionExtensions.SomeNotNull(foundUrls.MaxBy(match => match.Count));

        if (imageLinks.HasValue)
        {
            return new ImageUrlBundle { Metadata = requestInfo, Images = imageLinks.Unwrap().Select(match => match.Value).Some(), };
        }

        /*

        List<HtmlNode> imageNodes = website
            .DocumentNode
            .DescendantsAndSelf()
            .Where(node => node.OriginalName.Equals("img"))
            .ToList();

        if (imageNodes.Any())
        {
            IEnumerable<RequiredWebData> imageNodesWithValidUrl = imageNodes
                .Select(node => Preparation.LinkValidator(node))
                .Where(data => data.HasValue)
                .Select(data => data.Unwrap());
            
            Dictionary<string, List<string>> groupedImageLinks = imageNodesWithValidUrl
                .GroupBy(data => data.ClassAttribute, data => data.ImageLink, (key, group) => new
                {
                    Key = key,
                    Value = group.ToList(),
                })
                .ToDictionary(group => group.Key, group => group.Value);
            
            Option<IEnumerable<string>> imageLinks = groupedImageLinks
                .Values
                .OrderByDescending(list => list.Count)
                .Select(list => list.AsEnumerable())
                .FirstOrNone();

            return new ImageUrlBundle { Metadata = requestInfo, Images = imageLinks, };
        }*/

        return new ImageUrlBundle { Metadata = requestInfo, Images = Option.None<IEnumerable<string>>(), };
    }

    private static Option<RequiredWebData> LinkValidator(HtmlNode node)
    {
        Option<string> imageLink = OptionExtensions.SomeNotNull(node.GetAttributeValue("data-src", null) ?? node.GetAttributeValue("src", null));
        Option<string> classAttribute = node
            .Ancestors()
            .Where(node => node.OriginalName.Equals("div"))
            .Select(div => div.GetClasses().Flatten())
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
}