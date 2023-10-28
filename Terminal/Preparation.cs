using HtmlAgilityPack;
using Optional;
using Optional.Collections;
using Optional.Unsafe;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

class RequiredWebData
{
    public string ClassAttribute { get; init; } = null!;
    public string ImageLink { get; init; } = null!;
}

static class Preparation
{
    public static async Task<Option<IEnumerable<string>>> ExtractMangaImageUrls(string url)
    {
        HtmlWeb scraper = new HtmlWeb();
        HtmlDocument website = await scraper.LoadFromWebAsync(url);
        List<HtmlNode> images = website
            .DocumentNode
            .DescendantsAndSelf()
            .Where(node => node.OriginalName.Equals("img"))
            .ToList();

        if (images.Count > 0)
        {
            Dictionary<string, List<string>> imageLinksGroupedByClassAttribute = images.Select(image => Preparation.LinkValidator(image))
                .Where(optional => optional.HasValue)
                .GroupBy(data => data.ValueOrFailure().ClassAttribute, data => data.ValueOrFailure().ImageLink, (key, group) => new
                {
                    ClassAttribute = key,
                    ImageLinks = group.ToList(),
                })
                .OrderByDescending(keyValue => keyValue.ImageLinks.Count)
                .ToDictionary(key => key.ClassAttribute, value => value.ImageLinks);

            return Option.Some(imageLinksGroupedByClassAttribute.FirstOrDefault().Value.AsEnumerable());
        }

        return Option.None<IEnumerable<string>>();
    }

    private static Option<RequiredWebData> LinkValidator(HtmlNode node)
    {
        Option<string> imageLink = OptionExtensions.SomeNotNull(node.GetAttributeValue("data-src", null) ?? node.GetAttributeValue("src", null));
        Option<string> classAttribute = node
            .Ancestors()
            .Where(node => node.GetAttributeValue("class", null) is not null)
            .Select(node => node.GetAttributeValue("class", null))
            .FirstOrNone();

        if (imageLink.HasValue && classAttribute.HasValue && Uri.IsWellFormedUriString(imageLink.ValueOrFailure(), UriKind.Absolute))
        {
            return Option.Some(new RequiredWebData()
            {
                ClassAttribute = classAttribute.ValueOrFailure(),
                ImageLink = imageLink.ValueOrFailure(),
            });
        }

        return Option.None<RequiredWebData>();
    }
}