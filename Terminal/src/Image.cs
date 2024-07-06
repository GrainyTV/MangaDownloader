using ExtensionMethods;
using Optional;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

public static class Image
{
    private static readonly HttpClient httpClient = new HttpClient();

    public static PathBundle StartDownloads(ImageUrlBundle imageUrlBundle)
    {
        if (imageUrlBundle.Images.HasValue == false)
        {
            return new PathBundle { Metadata = imageUrlBundle.Metadata, Paths = Enumerable.Empty<string>().ToList(), };
        }

        IEnumerable<string> images = imageUrlBundle.Images.Unwrap();
        RequestInfo requestInfo = imageUrlBundle.Metadata;
        string title = requestInfo.Title;
        int chapter = requestInfo.Id;
        
        Directory.CreateDirectory(title);
        Directory.CreateDirectory($"{title}/{chapter}");

        List<string> imagePaths = images
            .AsParallel()
            .Select(async (url, page) =>
            {
                string savePath = $"{title}/{chapter}/{page}";

                var baseAddress = new Uri(requestInfo.Url);
                var httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, url);
                httpRequestMessage.Headers.Add("User-Agent", "Mozilla/5.0 (Windows; U; Windows NT 5.1; en-US; rv:x.x.x) Gecko/20041107 Firefox/x.x");
                httpRequestMessage.Headers.Add("Referer", baseAddress.GetLeftPart(UriPartial.Authority));

                using HttpResponseMessage response = await httpClient.SendAsync(httpRequestMessage);
                using var fileStream = new FileStream(savePath, FileMode.OpenOrCreate);

                response.EnsureSuccessStatusCode();
                await response.Content.CopyToAsync(fileStream);

                return savePath;
            })
            .Select(path => path.Result)
            .ToList();

        return new PathBundle { Metadata = requestInfo, Paths = imagePaths, };
    }
}