using BetterAccounting.Core.Data.Models;
using BetterAccounting.Core.Services.Reports;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Markup;
using System.Windows.Media;

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

        public static FixedDocument BuildFixedDocument(PrintTemplateLayout layout,
            IReadOnlyDictionary<string, string> fields, IReadOnlyList<string>? trailingLines = null)
        {
            var document = new FixedDocument();
            var pageSize = new Size(layout.PageWidth, layout.PageHeight);

            var canvas = BuildCanvas(layout, fields);
            canvas.Measure(pageSize);
            canvas.Arrange(new Rect(0, 0, pageSize.Width, pageSize.Height));
            AddPage(document, CreateFixedPage(layout, canvas));

            if (trailingLines != null && trailingLines.Count > 0)
            {
                const double fontSize = 11;
                const double lineHeight = 17;
                const double padding = 50;
                var usableHeight = layout.PageHeight - (padding * 2);
                var perPage = (int)Math.Max(1, Math.Floor(usableHeight / lineHeight));

                for (var i = 0; i < trailingLines.Count; i += perPage)
                {
                    var stack = new StackPanel { Margin = new Thickness(padding) };
                    foreach (var raw in trailingLines.Skip(i).Take(perPage))
                    {
                        var line = StripLegacyMarker(raw);
                        stack.Children.Add(new TextBlock
                        {
                            Text = line,
                            FontSize = fontSize,
                            Margin = new Thickness(0, 0, 0, 3)
                        });
                    }
                    AddPage(document, CreateFixedPage(layout, stack));
                }
            }

            return document;
        }

        private static FixedPage CreateFixedPage(PrintTemplateLayout layout, UIElement content)
        {
            var page = new FixedPage
            {
                Width = layout.PageWidth,
                Height = layout.PageHeight,
                Background = Brushes.White
            };
            page.Children.Add(content);
            FixedPage.SetLeft(content, 0);
            FixedPage.SetTop(content, 0);
            return page;
        }

        private static void AddPage(FixedDocument document, FixedPage page)
        {
            var pageContent = new PageContent();
            ((IAddChild)pageContent).AddChild(page);
            document.Pages.Add(pageContent);
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
