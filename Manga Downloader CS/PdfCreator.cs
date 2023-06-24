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
			XImage image = null;

			try
			{
				image = XImage.FromFile(file);
			}
			catch(InvalidOperationException)
			{
				//
				// Restore stripped JFIF header of JPEG image
				// To make it work with PDFSharp
				//
				image = PdfCreator.RestoreJfifHeader(file);
			}
			
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

	public static XImage RestoreJfifHeader(string file)
	{
		//
		// Proper first 21 bytes for a JPEG image
		//
		var jfifHeader = new byte[]
		{ 
			0xFF, 0xD8, 0xFF,
			0xE0, 0x00, 0x10,
			0x4A, 0x46, 0x49, 0x46,
			0x00, 0x01, 0x01, 0x01,
			0x00, 0x48, 0x00, 0x48,
			0x00, 0x00, 0xFF 
		};
			
		using(var stream = new FileStream(file, FileMode.Open))
		{
			//
			// Skip the first 3 bytes
			// Should be the same for all JPEG images
			//
			const int OFFSET = 3;
			var amount = (int) stream.Length - OFFSET;
			var content = new byte[amount];
			
			//
			// Set starting position to 4th byte
			// Read to the end
			//
			stream.Seek(OFFSET, SeekOrigin.Begin);
			stream.Read(content, 0, amount);

			//
			// Refresh file with the new data
			//
			using(var newStream = new FileStream(file, FileMode.Create))
			{
				newStream.Write(jfifHeader, 0, jfifHeader.Length);
				newStream.Write(content, 0, content.Length);
			}
		}

		return XImage.FromFile(file);
	}
}