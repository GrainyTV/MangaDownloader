using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

static class ImageDownloadExtension
{
	public static async Task DownloadFileTaskAsync(this HttpClient httpClient, string url, string path, string referer)
	{
		var httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, url);
		httpRequestMessage.Headers.Add("Referer", referer);

		using(var request = await httpClient.SendAsync(httpRequestMessage))
		{
			using(var fileStream = new FileStream(path, FileMode.OpenOrCreate))
			{
				request.EnsureSuccessStatusCode();
				await request.Content.CopyToAsync(fileStream);
			}
		}
	}
}