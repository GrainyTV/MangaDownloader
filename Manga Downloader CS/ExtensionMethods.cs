//
// .NET usings
//
using System;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Diagnostics;
using System.Collections.Generic;

//
// PDFSharp usings
//
using PdfSharp.Pdf;

public static class Methods
{
	public static void DownloadFile(this HttpClient httpClient, string url, string path, string referer)
	{
		using HttpRequestMessage httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, url);
		httpRequestMessage.Headers.Add("Referer", referer);

		using HttpResponseMessage response = httpClient.Send(httpRequestMessage);

		if (response.IsSuccessStatusCode)
		{
			using FileStream fileStream = new FileStream(path, FileMode.OpenOrCreate);
			response.Content.CopyTo(fileStream, null, CancellationToken.None);
		}
		else
		{

		}
	}

	public static void AddOrUpdate<TKey, TValue>(this IDictionary<TKey, List<TValue>> map, TKey newKey, TValue newValue)
	{
		if (map.ContainsKey(newKey))
		{
			map[newKey].Add(newValue);
			return;
		}

		map.Add(newKey, new List<TValue>());
		map[newKey].Add(newValue);
	}

	public static void AddPages(this PdfDocument pdfDocument, Int32 amount)
	{
		Debug.Assert(amount > 0, "Trying to add less than 1 page to a document.");

		for (int i = 0; i < amount; ++i)
		{
			pdfDocument.AddPage();
		}
	}
}