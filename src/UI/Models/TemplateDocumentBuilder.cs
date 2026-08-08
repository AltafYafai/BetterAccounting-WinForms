using BetterAccounting.Core.Data.Models;
using BetterAccounting.Core.Services.Reports;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BetterAccounting.UI.Models
{
    public static class TemplateDocumentBuilder
    {
        private const double FontSize = 12;
        private const double TitleFontSize = 18;
        private const double CopyFontSize = 14;

        public static PrintDocumentModel Build(IEnumerable<string> lines)
        {
            var (width, height, _) = PrintTemplateLayout.GetPageSize("A4", false);
            return Build(lines, width, height, 40);
        }

        public static PrintDocumentModel Build(IEnumerable<string> lines, double pageWidth, double pageHeight, double padding)
        {
            var model = new PrintDocumentModel();
            var usableWidth = pageWidth - (padding * 2);
            var usableHeight = pageHeight - (padding * 2);

            StackPanel page = NewPage(pageWidth, pageHeight);
            double used = 0;

            foreach (var raw in lines)
            {
                var line = raw?.TrimEnd('\r') ?? "";
                if (string.IsNullOrWhiteSpace(line))
                    continue;

                var block = ParseLine(line, padding, usableWidth);
                var height = block.DesiredSize.Height + 2;
                if (used + height > usableHeight)
                {
                    model.Pages.Add(page);
                    page = NewPage(pageWidth, pageHeight);
                    used = 0;
                }
                page.Children.Add(block);
                used += height;
            }

            if (model.Pages.Count == 0)
                model.Pages.Add(page);

            return model;
        }

        private static StackPanel NewPage(double pageWidth, double pageHeight)
        {
            return new StackPanel
            {
                Width = pageWidth,
                Height = pageHeight,
                Background = Brushes.White
            };
        }

        private static TextBlock ParseLine(string line, double padding, double usableWidth)
        {
            var fontFamily = new FontFamily("Segoe UI");
            TextBlock block;

            if (line.StartsWith("@T "))
            {
                block = new TextBlock
                {
                    Text = line.Substring(3),
                    FontSize = TitleFontSize,
                    FontWeight = FontWeights.Bold,
                    TextAlignment = TextAlignment.Center,
                    Margin = new Thickness(padding, 6, padding, 6),
                    TextWrapping = TextWrapping.Wrap,
                    FontFamily = fontFamily
                };
            }
            else if (line.StartsWith("@C "))
            {
                block = new TextBlock
                {
                    Text = line.Substring(3),
                    FontSize = FontSize,
                    TextAlignment = TextAlignment.Center,
                    Margin = new Thickness(padding, 1, padding, 1),
                    TextWrapping = TextWrapping.Wrap,
                    FontFamily = fontFamily
                };
            }
            else if (line.StartsWith("@B "))
            {
                block = new TextBlock
                {
                    Text = line.Substring(3),
                    FontSize = FontSize,
                    FontWeight = FontWeights.Bold,
                    Margin = new Thickness(padding, 1, padding, 1),
                    TextWrapping = TextWrapping.Wrap,
                    FontFamily = fontFamily
                };
            }
            else if (line.StartsWith("@R "))
            {
                block = new TextBlock
                {
                    Text = line.Substring(3),
                    FontSize = CopyFontSize,
                    FontWeight = FontWeights.Bold,
                    TextAlignment = TextAlignment.Right,
                    Margin = new Thickness(padding, 1, padding, 1),
                    TextWrapping = TextWrapping.Wrap,
                    FontFamily = fontFamily
                };
            }
            else if (line.StartsWith("@L "))
            {
                var text = line.Substring(3);
                var separator = text.IndexOf(": ", StringComparison.Ordinal);
                block = new TextBlock
                {
                    FontSize = FontSize,
                    Margin = new Thickness(padding, 1, padding, 1),
                    TextWrapping = TextWrapping.Wrap,
                    FontFamily = fontFamily
                };
                if (separator > 0)
                {
                    block.Inlines.Add(new Run(text.Substring(0, separator + 1)) { FontWeight = FontWeights.Bold });
                    block.Inlines.Add(new Run(text.Substring(separator + 1)));
                }
                else
                {
                    block.Text = text;
                }
            }
            else
            {
                block = new TextBlock
                {
                    Text = line,
                    FontSize = FontSize,
                    Margin = new Thickness(padding, 1, padding, 1),
                    TextWrapping = TextWrapping.Wrap,
                    FontFamily = fontFamily
                };
            }

            block.Measure(new Size(usableWidth, double.PositiveInfinity));
            return block;
        }
    }
}
