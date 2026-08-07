using System.Collections.Generic;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;

namespace BetterAccounting.UI.Models
{
    public static class TemplateDocumentBuilder
    {
        public static FlowDocument Build(IEnumerable<string> lines)
        {
            var doc = new FlowDocument
            {
                PagePadding = new Thickness(40),
                FontFamily = new FontFamily("Segoe UI"),
                FontSize = 12
            };

            foreach (var raw in lines)
            {
                var line = raw?.TrimEnd('\r') ?? "";
                if (string.IsNullOrWhiteSpace(line))
                    continue;

                Paragraph paragraph;
                if (line.StartsWith("@T "))
                {
                    paragraph = new Paragraph(new Run(line.Substring(3))
                    {
                        FontSize = 18,
                        FontWeight = FontWeights.Bold
                    })
                    {
                        TextAlignment = TextAlignment.Center,
                        Margin = new Thickness(0, 6, 0, 6)
                    };
                }
                else if (line.StartsWith("@C "))
                {
                    paragraph = new Paragraph(new Run(line.Substring(3)))
                    {
                        TextAlignment = TextAlignment.Center
                    };
                }
                else if (line.StartsWith("@B "))
                {
                    paragraph = new Paragraph(new Run(line.Substring(3))
                    {
                        FontWeight = FontWeights.Bold
                    });
                }
                else
                {
                    paragraph = new Paragraph(new Run(line));
                }

                doc.Blocks.Add(paragraph);
            }

            return doc;
        }
    }
}