using BetterAccounting.Core.Data.Models;
using System.Text.Json;

namespace BetterAccounting.Core.Services.Reports
{
    public static class PrintTemplateSerializer
    {
        public const string LayoutMarker = "#BA-LAYOUT#1#";

        private static readonly JsonSerializerOptions Options = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true
        };

        public static string Serialize(PrintTemplateLayout layout)
        {
            return LayoutMarker + "\n" + JsonSerializer.Serialize(layout, Options);
        }

        public static PrintTemplateLayout? TryDeserialize(string content)
        {
            if (string.IsNullOrWhiteSpace(content))
                return null;

            var json = content;
            if (json.StartsWith(LayoutMarker, StringComparison.Ordinal))
                json = json.Substring(LayoutMarker.Length).TrimStart('\r', '\n');
            else if (!json.TrimStart().StartsWith("{"))
                return null;

            try
            {
                return JsonSerializer.Deserialize<PrintTemplateLayout>(json, Options);
            }
            catch (JsonException)
            {
                return null;
            }
        }

        public static bool IsLayoutTemplate(string content)
            => TryDeserialize(content) != null;
    }
}
