//
// .NET usings
//
using System;
using System.IO;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Runtime.InteropServices;

public class UserInput
{
	public int FirstChapter { get; init; }
	public int FinalChapter { get; init; }
	public string Title { get; init; }
	public string Url { get; init; }

	public UserInput(out Action<UserInput> processingMode, string title, string url, int firstChapter, [Optional] int? finalChapter)
	{
		Title = title;
		Url = url;
		FirstChapter = firstChapter;
		FinalChapter = finalChapter ?? firstChapter;
		processingMode = (FirstChapter == FinalChapter) ? Program.CreateSingleChapter : Program.CreateMultipleChapters;
	}
}

public class Program
{
	public static void CreateSingleChapter(UserInput userGivenData)
	{
		DirectoryInfo temporaryImageFolder = Directory.CreateDirectory("images");
		DirectoryInfo imageDirectory = Directory.CreateDirectory(Path.Combine("images", userGivenData.Title));
		DirectoryInfo resultDirectory = Directory.CreateDirectory(userGivenData.Title);

		List<string> listOfImageUrls = Preparation.ExtractMangaImageUrls(userGivenData.Url);

		if (listOfImageUrls.Count > 0)
		{
			Image.StartDownloading(userGivenData.Url, userGivenData.Title, listOfImageUrls);
			PdfCreator.GenerateNewFromImages(Path.Combine("images", userGivenData.Title),
											 Path.Combine(userGivenData.Title, string.Format("{0} Chapter {1}.pdf", userGivenData.Title, userGivenData.FirstChapter)));
		}
	}

	public static void CreateMultipleChapters(UserInput userGivenData)
	{
	}

	static void Main()
	{
		Action<UserInput>? createChapterProcess = null;
		UserInput userGivenData = new UserInput(out createChapterProcess, "The Promised Neverland", "https://ww4.readneverland.com/chapter/the-promised-neverland-chapter-169/", 169);
		createChapterProcess.Invoke(userGivenData);
	}
}