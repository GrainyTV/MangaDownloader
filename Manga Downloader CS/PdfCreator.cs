using System;
using System.IO;
using PdfSharp.Pdf;
using PdfSharp.Drawing;

class PdfCreator
{
	private PdfDocument document;

	public PdfCreator()
	{
		document = new PdfDocument();
	}

	public void GenerateNewFromImages(string pathToImages, string fileName)
	{
		var files = Directory.GetFiles(pathToImages);

		foreach(var file in files)
		{
			var image = XImage.FromFile(file);
			var page = new PdfPage()
			{
				Width = image.PixelWidth,
				Height = image.PixelHeight
			};	
	
			document.AddPage(page);
			var renderer = XGraphics.FromPdfPage(page);
			renderer.DrawImage(image, 0, 0, page.Width, page.Height);	
		}

		document.Save(fileName);
	}
}