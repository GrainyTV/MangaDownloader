using ExtensionMethods;
using HtmlAgilityPack;
using Optional;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using System.Diagnostics;
using System.Net.Http;

public static partial class Preparation
{
    private record class RequiredWebData(string ClassAttribute, string ImageLink);

    [GeneratedRegex(@"https://([a-z0-9-]+\.){1,}([a-z0-9]+/)[^,'\n]+")]
    private static partial Regex MangaImageUrlRegex();

    private static Option<IEnumerable<RequiredWebData>> ImgNodeToRequired(HtmlNode img)
    {
        string link = img.GetAttributeValue("data-src", img.GetAttributeValue("src", String.Empty));

        if (link.IsNonEmpty())
        {
            Match regexResult = MangaImageUrlRegex().Match(link);

            if (regexResult.Success)
            {
                Option<string> classAttribute = img.GetClassesOfFirstParentDiv();

                if (classAttribute.HasValue)
                {
                    return new RequiredWebData(
                        ClassAttribute: classAttribute.Unwrap(), 
                        ImageLink: link
                    )
                    .Yield()
                    .Some();
                }
            }
        }

        return Option.None<IEnumerable<RequiredWebData>>();
    }

    private static Option<IEnumerable<RequiredWebData>> ScriptNodeToRequired(HtmlNode script)
    {
        if (script.InnerText.IsNonEmpty())
        {
            string content = script.InnerText.Trim();
            MatchCollection regexResult = MangaImageUrlRegex().Matches(content);

            if (regexResult.Count > 0)
            {
                Option<string> classAttribute = script.GetClassesOfFirstParentDiv();

                if (classAttribute.HasValue)
                {
                    return regexResult
                        .Select(match =>
                        {
                            return new RequiredWebData(
                                ClassAttribute: classAttribute.Unwrap(),
                                ImageLink: match.Value
                            );
                        })
                        .Some();
                }
            }
        }

        return Option.None<IEnumerable<RequiredWebData>>();
    }

    private static Option<IEnumerable<RequiredWebData>> HtmlNodeToRequired(HtmlNode node)
    {
        return node.OriginalName switch
        {
            "img" => ImgNodeToRequired(node),
            "script" => ScriptNodeToRequired(node),
            _ => Option.None<IEnumerable<RequiredWebData>>(),
        };
    }

    private static IEnumerable<string> FindImageUrlsOnSite(HtmlNode root)
    {
        IEnumerable<RequiredWebData> requiredData = root
            .DescendantsAndSelf()
            .Select(node => HtmlNodeToRequired(node))
            .Where(optRequired => optRequired.HasValue)
            .SelectMany(optRequired => optRequired.Unwrap());

        if (requiredData.Any())
        {
            return requiredData
                .GroupBy(required => required.ClassAttribute, required => required.ImageLink)
                .OrderByDescending(group => group.Count())
                .First()
                .Select(group => group);
        }

        return Enumerable.Empty<string>();
    }

    public static async Task<ImageUrlBundle> ExtractMangaImageUrls(RequestInfo requestInfo)
    {
        var scraper = new HtmlWeb();
        var optWebsite = Option.None<HtmlDocument>();

        try
        {
            HtmlDocument content = await scraper.LoadFromWebAsync(requestInfo.Url);
            optWebsite = content.Some();
        }
        catch (HttpRequestException)
        {
            ErrorMessages.LogFailedProcess(requestInfo, ErrorValue.NetworkFailure);
        }
        
        IEnumerable<string> foundUrls = optWebsite
            .Map(website => FindImageUrlsOnSite(website.DocumentNode))
            .Unwrap();

        if (foundUrls.Any())
        {
            return new ImageUrlBundle { Metadata = requestInfo, Images = foundUrls };
        }

        ErrorMessages.LogFailedProcess(requestInfo, ErrorValue.NoImageResourceFound);
        throw new UnreachableException();
    }
}