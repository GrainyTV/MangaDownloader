using ExtensionMethods;
using Optional;
using PdfSharp.Drawing;
using PdfSharp.Pdf;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

public static class Pdf
{
    public static void GenerateNew(Tuple<Guid, List<string>> process) //List<string> images, string title, string chapter)
    {
        var images = process.Item2;
        var uuid = process.Item1;

        var requestInfo = Program.Processes[uuid];
        var title = requestInfo.Title;
        var chapter = requestInfo.Chapter;

        using var document = new PdfDocument();
        document.AddPages(images.Count);

        images.ForEach((path, pageIndex) =>
        {
            using XImage image = LoadImageFromFile(path);
            PdfPage page = document.Pages[pageIndex];
            using XGraphics renderer = XGraphics.FromPdfPage(page);

            page.Width = image.PixelWidth;
            page.Height = image.PixelHeight;
            renderer.DrawImage(image, 0, 0, page.Width, page.Height);
        });

        document.Save($"{title}/{title} Chapter {chapter}.pdf");
    }

    private static XImage LoadImageFromFile(string path)
    {
        try
        {
            return XImage.FromFile(path);
        }
        catch (InvalidOperationException)
        {
            /* Broken JFIF header can cause exception */
            /* Need to apply error correction code */

            var jfifHeader = new byte[]
            {
                0xFF, 0xD8, 0xFF, 0xE0, 0x00, 0x10, 0x4A, 0x46, 0x49, 0x46,
                0x00, 0x01, 0x01, 0x01, 0x00, 0x48, 0x00, 0x48, 0x00, 0x00,
                0xFF
            };

            /* Read contents of image */
            /* Skip the first 3 bytes */
            /* Should be the same for all JPEG images */

            using var initialStream = new FileStream(path, FileMode.Open);
            const int OFFSET = 3;
            var amount = (int) initialStream.Length - OFFSET;
            var content = new byte[amount];

            /* Set starting position to 4th byte */
            /* Then read to the end */

            initialStream.Seek(OFFSET, SeekOrigin.Begin);
            initialStream.Read(content, 0, amount);

            /* Refresh file with the new data */

            using var newStream = new FileStream(path, FileMode.Create);
            newStream.Write(jfifHeader, 0, jfifHeader.Length);
            newStream.Write(content, 0, content.Length);

            return XImage.FromFile(path);
        }
    }
}