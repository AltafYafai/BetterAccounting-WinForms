using System;
using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;

namespace BetterAccounting.Core.Services.Data
{
    public class BackupService
    {
        private readonly string _dbPath;
        private readonly string _backupDirectory;

        public BackupService(string? dbPath = null, string? backupDirectory = null)
        {
            _dbPath = dbPath ?? GetDefaultDbPath();
            _backupDirectory = backupDirectory ?? GetDefaultBackupDirectory();
        }

        public async Task<string> CreateBackupAsync(string? backupName = null)
        {
            if (!File.Exists(_dbPath))
                throw new FileNotFoundException("Database file not found");

            Directory.CreateDirectory(_backupDirectory);

            backupName = backupName ?? $"backup_{DateTime.Now:yyyyMMdd_HHmmss}";
            var backupPath = Path.Combine(_backupDirectory, $"{backupName}.zip");

            using (var archive = ZipFile.Open(backupPath, ZipArchiveMode.Create))
            {
                archive.CreateEntryFromFile(_dbPath, "data.db");
                var configPath = GetConfigPath();
                if (File.Exists(configPath))
                    archive.CreateEntryFromFile(configPath, "sync.cfg");
            }

            return backupPath;
        }

        public async Task<bool> RestoreBackupAsync(string backupFilePath)
        {
            if (!File.Exists(backupFilePath))
                return false;

            using (var archive = ZipFile.OpenRead(backupFilePath))
            {
                foreach (var entry in archive.Entries)
                {
                    if (entry.FullName == "data.db")
                    {
                        entry.ExtractToFile(_dbPath, true);
                    }
                    else if (entry.FullName == "sync.cfg")
                    {
                        var configPath = GetConfigPath();
                        var configDir = Path.GetDirectoryName(configPath);
                        if (!string.IsNullOrEmpty(configDir))
                            Directory.CreateDirectory(configDir);
                        entry.ExtractToFile(configPath, true);
                    }
                }
            }

            return true;
        }

        public string[] GetAvailableBackups()
        {
            if (!Directory.Exists(_backupDirectory))
                return Array.Empty<string>();

            return Directory.GetFiles(_backupDirectory, "*.zip");
        }

        private static string GetDefaultDbPath()
        {
            return AppPaths.CurrentDbPath();
        }

        private static string GetDefaultBackupDirectory()
        {
            var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            return Path.Combine(appData, "BetterAccounting", "Backups");
        }

        private static string GetConfigPath()
        {
            var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            return Path.Combine(appData, "BetterAccounting", "sync.cfg");
        }
    }
}
