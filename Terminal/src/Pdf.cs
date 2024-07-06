using ExtensionMethods;
using SkiaSharp;
using PdfSharp.Drawing;
using PdfSharp.Pdf;
using System;
using System.Collections.Generic;
using System.IO;

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

            page.Width = XUnit.FromPoint(image.PixelWidth);
            page.Height = XUnit.FromPoint(image.PixelHeight);
            renderer.DrawImage(image, 0, 0, page.Width.Point, page.Height.Point);
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
            SKData encodedPixelData;

            using (FileStream originalFileStream = File.OpenRead(path))
            {
                using SKImage image = SKImage.FromEncodedData(originalFileStream);
                encodedPixelData = image.Encode(SKEncodedImageFormat.Jpeg, 100);
            }

            using (FileStream newFileStream = File.Open(path, FileMode.Create))
            {
                encodedPixelData.SaveTo(newFileStream);
            }

            return XImage.FromFile(path);
        }
    }
}