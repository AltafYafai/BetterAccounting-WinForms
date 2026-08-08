using System;
using System.Reflection;
using System.Text;
using Avalonia;
using BetterAccounting.UI.Services;

namespace BetterAccounting.UI.Models
{
    public static class ErrorReporter
    {
        public const string SupportEmail = "altaf.alu@gmail.com";

        public static void Log(string operation, Exception ex) => ErrorLog.Write(operation, ex);

        public static string Message(string operation, Exception ex)
        {
            ErrorLog.Write(operation, ex);
            return $"{operation} failed. {Describe(ex)}";
        }

        public static void Show(string operation, Exception ex)
        {
            ErrorLog.Write(operation, ex);
            if (Application.Current != null)
            {
                var report = BuildReport(operation, ex);
                var message = $"{operation} failed.\n\n{Describe(ex)}";
                _ = Views.ErrorDialogWindow.ShowAsync(operation, message, report,
                    AppServices.GetMainWindow());
            }
        }

        public static string DescribeForDialog(Exception ex) => Describe(ex);

        public static string BuildReport(string operation, Exception ex)
        {
            var sb = new StringBuilder();
            sb.AppendLine("BetterAccounting Error Report");
            sb.AppendLine($"Version: {GetVersion()}");
            sb.AppendLine($"Time: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            sb.AppendLine($"Operation: {operation}");
            sb.AppendLine($"Error: {Describe(ex)}");
            sb.AppendLine();
            sb.AppendLine("Stack trace:");
            sb.AppendLine(ex.StackTrace ?? "(none)");
            sb.AppendLine();
            sb.AppendLine($"Full log: {ErrorLog.LogFilePath}");
            return sb.ToString();
        }

        public static string BuildMailtoUrl(string report, string subject = "BetterAccounting Error Report")
        {
            var body = report;
            const int maxBodyChars = 1500;
            if (body.Length > maxBodyChars)
                body = body.Substring(0, maxBodyChars) + $"\n...(truncated; full log at {ErrorLog.LogFilePath})";

            return $"mailto:{SupportEmail}?subject={Uri.EscapeDataString(subject)}&body={Uri.EscapeDataString(body)}";
        }

        private static string Describe(Exception ex)
        {
            if (ex.InnerException != null && !string.IsNullOrWhiteSpace(ex.InnerException.Message))
                return $"{ex.Message} (Reason: {ex.InnerException.Message})";
            return ex.Message;
        }

        private static string GetVersion()
        {
            return typeof(ErrorReporter).Assembly
                .GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion?.Split('+')[0]
                ?? "unknown";
        }
    }
}
