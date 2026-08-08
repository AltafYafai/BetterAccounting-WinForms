using BetterAccounting.Core.Data.Models;
using BetterAccounting.Core.Services.Reports;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BetterAccounting.UI.Models
{
    public static class PrintLayoutRenderer
    {
        public static Canvas BuildCanvas(PrintTemplateLayout layout, IReadOnlyDictionary<string, string> fields)
        {
            var canvas = new Canvas
            {
                Width = layout.PageWidth,
                Height = layout.PageHeight,
                Background = Brushes.White
            };

            foreach (var item in layout.Items.OrderBy(i => i.ZIndex))
            {
                var shape = TemplateItemShapeFactory.Create(item,
                    text => PrintTemplateService.Substitute(text, fields));
                var (x, y, w, h) = TemplateItemShapeFactory.Normalize(item);
                Canvas.SetLeft(shape, x);
                Canvas.SetTop(shape, y);
                shape.Width = w;
                shape.Height = h;
                Canvas.SetZIndex(shape, item.ZIndex);
                canvas.Children.Add(shape);
            }

            return canvas;
        }

        public static PrintDocumentModel BuildLayoutDocument(PrintTemplateLayout layout,
            IReadOnlyDictionary<string, string> fields, IReadOnlyList<string>? trailingLines = null)
        {
            var model = new PrintDocumentModel();
            model.Pages.Add(BuildCanvas(layout, fields));

            if (trailingLines != null && trailingLines.Count > 0)
            {
                var cleaned = trailingLines.Select(StripLegacyMarker).ToList();
                var textDoc = TemplateDocumentBuilder.Build(cleaned, layout.PageWidth, layout.PageHeight, 50);
                foreach (var page in textDoc.Pages)
                    model.Pages.Add(page);
            }

            return model;
        }

        private static string StripLegacyMarker(string line)
        {
            var text = line?.TrimEnd('\r') ?? string.Empty;
            if (text.Length >= 3 && text[0] == '@' && text[1] is >= 'A' and <= 'Z' && text[2] == ' ')
                text = text.Substring(3);
            return text;
        }
    }
}
