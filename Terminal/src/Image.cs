using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;

public static class Image
{
    private static readonly HttpClient httpClient = new HttpClient();

    public static PathBundle StartDownloads(ImageUrlBundle imageUrlBundle)
    {
        IEnumerable<string> images = imageUrlBundle.Images;
        RequestInfo requestInfo = imageUrlBundle.Metadata;
        string title = requestInfo.Title;
        string chapter = requestInfo.UniqueId;

        Directory.CreateDirectory($"{title}/{chapter}");
        List<string> imagePaths = images
            .AsParallel()
            .Select(async (imageUrl, page) =>
            {
                var baseAddress = new Uri(requestInfo.Url);
                string savePath = $"{title}/{chapter}/{page}";
                
                using var httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, imageUrl);
                httpRequestMessage.Headers.Add("User-Agent", "Mozilla/5.0 (Windows; U; Windows NT 5.1; en-US; rv:x.x.x) Gecko/20041107 Firefox/x.x");
                httpRequestMessage.Headers.Add("Referer", baseAddress.GetLeftPart(UriPartial.Authority));

                using HttpResponseMessage response = await httpClient.SendAsync(httpRequestMessage);

                if (response.IsSuccessStatusCode)
                {
                    using var fileStream = new FileStream(savePath, FileMode.CreateNew);
                    await response.Content.CopyToAsync(fileStream);
                }

                return savePath;
            })
            .Select(task => task.Result)
            .ToList();

        return new PathBundle { Metadata = requestInfo, Paths = imagePaths, };
    }
}