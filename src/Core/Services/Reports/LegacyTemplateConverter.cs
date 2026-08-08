using BetterAccounting.Core.Data.Models;
using System.Text.RegularExpressions;

namespace BetterAccounting.Core.Services.Reports
{
    public static class LegacyTemplateConverter
    {
        private static readonly Regex DashLine = new(@"^\s*-{4,}\s*$", RegexOptions.Compiled);

        public static PrintTemplateLayout Convert(string content)
        {
            var layout = PrintTemplateLayout.CreateDefault();
            if (string.IsNullOrWhiteSpace(content))
                return layout;

            var y = 60.0;
            const double left = 80.0;
            const double width = 634.0;

            foreach (var raw in content.Replace("\r\n", "\n").Split('\n'))
            {
                var line = raw?.TrimEnd('\r') ?? string.Empty;
                if (string.IsNullOrWhiteSpace(line))
                {
                    y += 18;
                    continue;
                }

                if (DashLine.IsMatch(line))
                {
                    layout.Items.Add(new PrintTemplateItem
                    {
                        Kind = TemplateItemKind.Line,
                        X = left - 20,
                        Y = y,
                        Width = width + 40,
                        Height = 0,
                        BorderColor = "333333",
                        BorderThickness = 1
                    });
                    y += 24;
                    continue;
                }

                var bold = false;
                var fontSize = 12.0;
                var align = TemplateTextAlignment.Left;

                if (line.StartsWith("@T "))
                {
                    line = line.Substring(3);
                    bold = true;
                    fontSize = 18;
                    align = TemplateTextAlignment.Center;
                }
                else if (line.StartsWith("@C "))
                {
                    line = line.Substring(3);
                    align = TemplateTextAlignment.Center;
                }
                else if (line.StartsWith("@B "))
                {
                    line = line.Substring(3);
                    bold = true;
                }

                if (string.IsNullOrWhiteSpace(line))
                {
                    y += 18;
                    continue;
                }

                layout.Items.Add(new PrintTemplateItem
                {
                    Kind = TemplateItemKind.Text,
                    X = left,
                    Y = y,
                    Width = width,
                    Height = fontSize + 8,
                    Text = line,
                    FontSize = fontSize,
                    Bold = bold,
                    TextAlignment = align
                });

                y += fontSize + 10;
            }

            return layout;
        }
    }
}
