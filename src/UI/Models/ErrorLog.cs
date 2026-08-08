using System;
using System.IO;
using System.Text;

namespace BetterAccounting.UI.Models
{
    public static class ErrorLog
    {
        private static readonly object Gate = new();
        private const long MaxLogBytes = 512 * 1024;

        public static string LogDirectory { get; } = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "BetterAccounting", "logs");

        public static string LogFilePath => Path.Combine(LogDirectory, "error.log");

        public static void Write(string message)
        {
            try
            {
                lock (Gate)
                {
                    Directory.CreateDirectory(LogDirectory);
                    RotateIfNeeded();
                    File.AppendAllText(LogFilePath, $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {message}{Environment.NewLine}");
                }
            }
            catch
            {
                // Logging must never take the app down.
            }
        }

        public static void Write(string operation, Exception ex)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"Operation: {operation}");
            sb.AppendLine($"Error: {ex.Message}");
            if (ex.InnerException != null)
                sb.AppendLine($"Inner: {ex.InnerException.Message}");
            sb.AppendLine("Stack trace:");
            sb.AppendLine(ex.StackTrace ?? "(none)");
            Write(sb.ToString());
        }

        public static string ReadAll()
        {
            try
            {
                lock (Gate)
                {
                    return File.Exists(LogFilePath) ? File.ReadAllText(LogFilePath) : string.Empty;
                }
            }
            catch
            {
                return string.Empty;
            }
        }

        private static void RotateIfNeeded()
        {
            var file = new FileInfo(LogFilePath);
            if (!file.Exists || file.Length < MaxLogBytes)
                return;

            var rotated = LogFilePath + ".1";
            if (File.Exists(rotated))
                File.Delete(rotated);
            File.Move(LogFilePath, rotated);
        }
    }
}
