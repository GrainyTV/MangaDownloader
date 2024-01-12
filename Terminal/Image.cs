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
    private readonly static HttpClient httpClient = new HttpClient();

    public static Tuple<Guid, List<string>> StartDownloads(Tuple<Guid, Option<List<string>>> process) //Option<List<string>> acquiredUrls, string title, string chapter)
    {
        var acquiredUrls = process.Item2;
        var uuid = process.Item1;

        var requestInfo = Program.Processes[uuid];
        var title = requestInfo.Title;
        var chapter = requestInfo.Chapter;
        
        Directory.CreateDirectory(title);
        Directory.CreateDirectory($"{title}/{chapter}");

        var imagePaths = acquiredUrls
            .Unwrap()
            .AsParallel()
            .Select(async (url, page) =>
            {
                var uriFromUrl = new Uri(url);
                string hasExtension = Path.GetExtension(url);
                string extension = (hasExtension == String.Empty) ? ".jpg" : hasExtension;
                string savePath = $"{title}/{chapter}/{page}{extension}";
                
                var httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, url);
                httpRequestMessage.Headers.Add("Referer", uriFromUrl.GetLeftPart(UriPartial.Authority));

                using HttpResponseMessage response = await httpClient.SendAsync(httpRequestMessage);
                using var fileStream = new FileStream(savePath, FileMode.OpenOrCreate);

                response.EnsureSuccessStatusCode();
                await response.Content.CopyToAsync(fileStream);

                return savePath;
            })
            .Select(path => path.Result)
            .ToList();

        return Tuple.Create(uuid, imagePaths);
    }
}