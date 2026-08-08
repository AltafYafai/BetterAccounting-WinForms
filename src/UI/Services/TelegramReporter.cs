using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace BetterAccounting.UI.Services
{
    public static class TelegramReporter
    {
        private const string BotToken = "8157097626:AAGt4JlDyTBO-rm5XX9CCycY4ys1RuZKnbU";
        private const string ChatId = "-1003813498680";
        private const int MaxMessageLength = 4000;

        private static readonly HttpClient HttpClient = new();

        public static async Task<bool> SendAsync(string message)
        {
            try
            {
                var text = message ?? string.Empty;
                if (text.Length > MaxMessageLength)
                    text = text.Substring(0, MaxMessageLength) + "\n...(truncated)";

                var payload = "{\"chat_id\":\"" + ChatId + "\",\"text\":\"" + EscapeJson(text)
                    + "\",\"disable_web_page_preview\":true}";
                var content = new StringContent(payload, Encoding.UTF8, "application/json");
                var response = await HttpClient.PostAsync(
                    $"https://api.telegram.org/bot{BotToken}/sendMessage", content);
                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }

        private static string EscapeJson(string s)
        {
            return s.Replace("\\", "\\\\")
                .Replace("\"", "\\\"")
                .Replace("\r", "")
                .Replace("\n", "\\n")
                .Replace("\t", "\\t");
        }
    }
}
