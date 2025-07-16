module Pdf

open CommonTypes
open PdfSharp.Drawing
open PdfSharp.Pdf
open System

let generateNew (chapter: Chapter) (name: string) : Unit =
    use document = new PdfDocument()
    document.AddPages (chapter.ImageInfo.Local.Length)

    chapter.ImageInfo.Local
    |> Seq.iteri (fun i path ->
        use image = XImage.FromFile(path)
        let page = document.Pages[i]

        page.Width <- XUnit.FromPoint(image.PixelWidth)
        page.Height <- XUnit.FromPoint(image.PixelHeight)

        use renderer = XGraphics.FromPdfPage(page)
        renderer.DrawImage(image, 0, 0, page.Width.Point, page.Height.Point))

    document.Info.Title <- name
    document.Info.Elements.Add("/Scanlators", PdfString("shit"))

    let file = String.Format("{0} Chapter {1}.pdf", name, chapter.Number.ToString "D4")
    document.Save (Utility.joinPathsUnix [| name; file |])
