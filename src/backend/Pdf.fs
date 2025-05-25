module Pdf

open CommonTypes
open PdfSharp.Drawing
open PdfSharp.Pdf

//let generateNew (images: seq<string>) (outFile: string) : Unit =
let generateNew (chapter: ChapterEntry) : Unit =
    //let pageCount = images |> Seq.length

    use document = new PdfDocument()
    document.AddPages(chapter.LocalFiles.Length)

    chapter.LocalFiles
    |> Seq.iteri (fun i path ->
        use image = XImage.FromFile(path)
        let page = document.Pages[i]

        page.Width <- XUnit.FromPoint(image.PixelWidth)
        page.Height <- XUnit.FromPoint(image.PixelHeight)

        use renderer = XGraphics.FromPdfPage(page)
        renderer.DrawImage(image, 0, 0, page.Width.Point, page.Height.Point))

    document.Save(chapter.OutFile)
