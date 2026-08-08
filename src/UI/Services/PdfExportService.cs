using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Media.Imaging;
using BetterAccounting.UI.Models;
using PdfSharpCore.Drawing;
using PdfSharpCore.Pdf;

namespace BetterAccounting.UI.Services
{
    public static class PdfExportService
    {
        public static async Task<string?> ExportAsync(PrintDocumentModel document, string? suggestedName = null)
        {
            var path = await FileDialogService.SaveFileAsync("Export PDF", suggestedName,
                ("PDF Files", new[] { "*.pdf" }));
            if (string.IsNullOrEmpty(path))
                return null;

            if (!path.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase))
                path += ".pdf";

            try
            {
                BuildPdf(document, path);
            }
            catch (Exception ex)
            {
                Models.ErrorLog.Write("Export PDF", ex);
                throw;
            }

            return path;
        }

        public static void BuildPdf(PrintDocumentModel document, string outputPath)
        {
            using var pdf = new PdfDocument();

            foreach (var page in document.Pages)
            {
                if (page is not Visual visual)
                    continue;

                var widthDips = page.Width ?? page.Bounds.Width;
                var heightDips = page.Height ?? page.Bounds.Height;
                if (widthDips <= 0) widthDips = 794;
                if (heightDips <= 0) heightDips = 1123;

                const int scale = 2;
                visual.Measure(new Size(widthDips, heightDips));
                visual.Arrange(new Rect(0, 0, widthDips, heightDips));

                using var bitmap = new RenderTargetBitmap(
                    (int)Math.Ceiling(widthDips * scale),
                    (int)Math.Ceiling(heightDips * scale));
                bitmap.Render(visual);

                using var pngStream = bitmap.Encode(new PngEncoder());

                var tempPng = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName() + ".png");
                try
                {
                    using (var fs = File.Create(tempPng))
                        pngStream.CopyTo(fs);

                    using var image = XImage.FromFile(tempPng);

                    var pdfPage = pdf.AddPage();
                    pdfPage.Width = XUnit.FromPoint(widthDips * 72.0 / 96.0);
                    pdfPage.Height = XUnit.FromPoint(heightDips * 72.0 / 96.0);

                    using var g = XGraphics.FromPdfPage(pdfPage);
                    g.DrawImage(image, 0, 0, pdfPage.Width.Point, pdfPage.Height.Point);
                }
                finally
                {
                    if (File.Exists(tempPng))
                        File.Delete(tempPng);
                }
            }

            pdf.Save(outputPath);
        }

        public static void OpenDocument(string path)
        {
            if (!File.Exists(path))
                return;

            Process.Start(new ProcessStartInfo(path) { UseShellExecute = true });
        }
    }
}
