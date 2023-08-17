//
// .NET usings
//
using System;
using System.IO;
using System.Linq;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Collections.Concurrent;

//
// PDFSharp usings
//
using PdfSharp.Pdf;
using PdfSharp.Drawing;

class PdfCreator
{
	private static readonly object documentWriterLock = new object();

	private class NumericalComparator : IComparer<string>
	{
		public int Compare(string? former, string? latter)
		{
			Debug.Assert(former != null && latter != null, "Passing null value to NumericalComparator class.");

			UInt32 formerI = 0;
			UInt32 latterI = 0;

			UInt32.TryParse(Path.GetFileNameWithoutExtension(former), out formerI);
			UInt32.TryParse(Path.GetFileNameWithoutExtension(latter), out latterI);

			Debug.Assert(formerI > 0 && latterI > 0, "0 index found in NumericalComparator class.");

			if (formerI == latterI)
			{
				return 0;
			}

			return (formerI < latterI) ? -1 : 1;
		}
	}

	public static void GenerateNewFromImages(string pathToImages, string fileName)
	{
		string[] files = Directory.GetFiles(pathToImages);
		NumericalComparator numericalCompare = new NumericalComparator();
		Array.Sort(files, numericalCompare);

		using PdfDocument document = new PdfDocument();
		document.AddPages(files.Length);

		Parallel.For(0, files.Length, i =>
		{
			XImage? image = null;

			try
			{
				image = XImage.FromFile(files[i]);
			}
			catch (InvalidOperationException)
			{
				//
				// Restore stripped JFIF header of JPEG image
				// To make it work with PDFSharp
				//

				image = PdfCreator.RestoreJfifHeader(files[i]);
				Debug.Assert(image != null, $"{files[i]} could not be loaded even after error correction code.");
			}

			PdfPage page = document.Pages[i];
			page.Width = image.PixelWidth;
			page.Height = image.PixelHeight;

			lock (documentWriterLock)
			{
				XGraphics renderer = XGraphics.FromPdfPage(page);
				renderer.DrawImage(image, 0, 0, page.Width, page.Height);
			}
		});

		document.Save(fileName);
	}

	public static XImage RestoreJfifHeader(string file)
	{
		//
		// Proper first 21 bytes for a JPEG image
		//

		byte[] jfifHeader = new byte[]
		{
			0xFF, 0xD8, 0xFF,
			0xE0, 0x00, 0x10,
			0x4A, 0x46, 0x49, 0x46,
			0x00, 0x01, 0x01, 0x01,
			0x00, 0x48, 0x00, 0x48,
			0x00, 0x00, 0xFF
		};

		using (var stream = new FileStream(file, FileMode.Open))
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

			using (var newStream = new FileStream(file, FileMode.Create))
			{
				newStream.Write(jfifHeader, 0, jfifHeader.Length);
				newStream.Write(content, 0, content.Length);
			}
		}

		return XImage.FromFile(file);
	}
}