using ExtensionMethods;
using Optional;
using PdfSharp.Drawing;
using PdfSharp.Pdf;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using System.Text;
using ImageMagick;

public static class Pdf
{
    public static void GenerateNew(PathBundle pathBundle)
    {
        List<string> images = pathBundle.Paths;
        RequestInfo requestInfo = pathBundle.Metadata;
        string title = requestInfo.Title;
        int chapter = requestInfo.Id;

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
        Directory.Delete($"{title}/{chapter}", true);
    }

    private static XImage LoadImageFromFile(string path)
    {
        try
        {
            return XImage.FromFile(path);
        }
        catch (InvalidOperationException)
        {
            using var image = new MagickImage(path);
            image.Write(path, MagickFormat.Png24);

            return XImage.FromFile(path);
        }
    }
}