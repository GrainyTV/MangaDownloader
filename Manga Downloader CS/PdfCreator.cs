using System;
using System.IO;
using System.Collections.Generic;
using PdfSharp.Pdf;
using PdfSharp.Drawing;

class PdfCreator
{
	private PdfDocument document;

	private class NumericalComparator : IComparer<string>
	{
		public int Compare(string? former, string? latter)
		{
			try
			{
				int formerI = (former == null) ? throw new InvalidOperationException("Trying to compare null values numerically.") : int.Parse(Path.GetFileNameWithoutExtension(former));
				int latterI = (latter == null) ? throw new InvalidOperationException("Trying to compare null values numerically.") : int.Parse(Path.GetFileNameWithoutExtension(latter));

				if(formerI == latterI)
				{
					return 0;
				}

				return (formerI < latterI) ? -1 : 1;
			}
			catch(FormatException)
			{
				throw new InvalidOperationException($"Trying to compare non-numeric items: {former} and {latter}.");
			}
		}
	}

	public PdfCreator()
	{
		document = new PdfDocument();
	}

	public void GenerateNewFromImages(string pathToImages, string fileName)
	{
		var files = Directory.GetFiles(pathToImages);
		var numericalCompare = new NumericalComparator();
		Array.Sort(files, numericalCompare);

		foreach(var file in files)
		{
			XImage image = default(XImage);

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

				if(image == null)
				{
					throw new InvalidOperationException($"Image: {file} could not be loaded even after error correction code.");
				}
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