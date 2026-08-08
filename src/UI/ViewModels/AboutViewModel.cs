using BetterAccounting.Core.Data.Models;
using BetterAccounting.Core.Services;
using BetterAccounting.Core.Services.Data;
using BetterAccounting.UI.Models;
using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Input;

namespace BetterAccounting.UI.ViewModels
{
    public class AboutViewModel : ViewModelBase
    {
        private const string Owner = "AltafYafai";
        private const string Repo = "BetterAccounting-WinForms";

        private readonly UpdateService _updateService;
        private readonly string _currentVersion;

        private UpdateInfo? _updateInfo;
        private bool _isBusy;
        private string _statusMessage = string.Empty;
        private bool _updateAvailable;
        private string? _downloadedPath;

        public AboutViewModel() : this(new UpdateService(new HttpClient(), Owner, Repo))
        {
        }

        public AboutViewModel(UpdateService updateService)
        {
            _updateService = updateService;
            _currentVersion = GetCurrentVersion();
            Version = $"Version {_currentVersion}";

            CheckForUpdatesCommand = new RelayCommand(async () => await CheckForUpdatesAsync(), () => !IsBusy);
            DownloadCommand = new RelayCommand(async () => await DownloadAsync(), () => UpdateAvailable);
            ApplyUpdateCommand = new RelayCommand(async () => await ApplyUpdateAsync(), () => !string.IsNullOrEmpty(DownloadedPath));
            OpenReleasesCommand = new RelayCommand(OpenReleases);
            ShowDownloadedFileCommand = new RelayCommand(ShowDownloadedFile, () => !string.IsNullOrEmpty(DownloadedPath));
            CloseCommand = new RelayCommand(() => OnClose?.Invoke());

            _ = CheckForUpdatesAsync();
        }

        private async Task CheckForUpdatesAsync()
        {
            IsBusy = true;
            StatusMessage = "Checking for updates...";
            try
            {
                _updateInfo = await _updateService.CheckAsync(_currentVersion);
                if (!string.IsNullOrEmpty(_updateInfo.ErrorMessage))
                {
                    StatusMessage = _updateInfo.ErrorMessage;
                }
                else if (_updateInfo.IsUpdateAvailable)
                {
                    UpdateAvailable = true;
                    StatusMessage = $"Update available: version {_updateInfo.LatestVersion}";
                }
                else
                {
                    StatusMessage = "You are up to date.";
                }
            }
            catch (Exception ex)
            {
                StatusMessage = ErrorReporter.Message("Check for updates", ex);
            }
            finally
            {
                IsBusy = false;
            }
        }

        private async Task DownloadAsync()
        {
            if (_updateInfo is null || string.IsNullOrEmpty(_updateInfo.DownloadUrl))
                return;

            if (!_updateInfo.IsUpdateAvailable)
            {
                StatusMessage = "You are already running the latest version.";
                return;
            }

            IsBusy = true;
            try
            {
                var backupPath = await CreateSafetyBackupAsync();

                StatusMessage = "Downloading update...";
                var folder = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
                var targetPath = Path.Combine(folder, "Downloads", $"BetterAccounting_v{_updateInfo.LatestVersion}.exe");
                var writtenFile = await _updateService.DownloadAsync(_updateInfo, targetPath);

                DownloadedPath = writtenFile;
                StatusMessage = backupPath != null
                    ? $"Installer saved to {writtenFile}. Your data was backed up to {backupPath} before updating, so no data will be lost."
                    : $"Installer saved to {writtenFile}. Your data is stored separately and will not be lost.";
            }
            catch (Exception ex)
            {
                StatusMessage = ErrorReporter.Message("Download update installer", ex);
            }
            finally
            {
                IsBusy = false;
            }
        }

        private static async Task<string?> CreateSafetyBackupAsync()
        {
            try
            {
                var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                var dbPath = BetterAccounting.Core.Services.Data.AppPaths.CurrentDbPath();
                if (!File.Exists(dbPath))
                    return null;

                var backupDir = Path.Combine(appData, "BetterAccounting", "PreUpdateBackups");
                var service = new BackupService(dbPath, backupDir);
                var path = await service.CreateBackupAsync($"preupdate_{DateTime.Now:yyyyMMdd_HHmmss}");
                PruneDirectory(backupDir, keep: 10);
                return path;
            }
            catch (Exception ex)
            {
                ErrorReporter.Log("Create pre-update backup", ex);
                return null;
            }
        }

        // Keeps only the most recent 'keep' files, deleting older ones so the folder never grows unbounded.
        private static void PruneDirectory(string directory, int keep)
        {
            try
            {
                if (!Directory.Exists(directory))
                    return;

                var files = Directory.GetFiles(directory).OrderByDescending(f => f).Skip(keep).ToArray();
                foreach (var file in files)
                    File.Delete(file);
            }
            catch (Exception ex)
            {
                ErrorReporter.Log("Prune old update backups", ex);
            }
        }

        private async Task ApplyUpdateAsync()
        {
            var newPath = DownloadedPath;
            if (string.IsNullOrEmpty(newPath) || !File.Exists(newPath))
            {
                StatusMessage = "No downloaded installer found. Download the update first.";
                return;
            }

            var currentExe = Environment.ProcessPath;
            if (string.IsNullOrEmpty(currentExe) ||
                !currentExe.EndsWith(".exe", StringComparison.OrdinalIgnoreCase))
            {
                StatusMessage = "Could not locate this app's executable. Close the app and run the downloaded file manually.";
                return;
            }

            var directory = Path.GetDirectoryName(currentExe);
            if (string.IsNullOrEmpty(directory))
            {
                StatusMessage = "Could not locate this app's folder. Close the app and run the downloaded file manually.";
                return;
            }

            var exeName = Path.GetFileName(currentExe);
            var cmdPath = Path.Combine(directory, "apply-update.cmd");

            string[] script =
            {
                "@echo off",
                "cd /d \"%~dp0\"",
                "ping 127.0.0.1 -n 4 >nul",
                "taskkill /IM \"" + exeName + "\" /F >nul 2>&1",
                "timeout /t 1 /nobreak >nul",
                "copy /y \"" + exeName + "\" \"" + exeName + ".bak\" >nul 2>&1",
                "copy /y \"" + newPath + "\" \"" + exeName + "\" >nul 2>&1",
                "start \"\" \"" + exeName + "\"",
                "del /q /f \"%~f0\""
            };

            try
            {
                await File.WriteAllLinesAsync(cmdPath, script);
            }
            catch (Exception ex)
            {
                ErrorReporter.Log("Write apply-update script", ex);
                StatusMessage = "No write access where the app is installed, so it cannot replace itself. Close the app and run the downloaded file manually.";
                return;
            }

            try
            {
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                {
                    FileName = cmdPath,
                    UseShellExecute = false,
                    CreateNoWindow = true
                });
            }
            catch (Exception ex)
            {
                ErrorReporter.Log("Start the apply-update script", ex);
                StatusMessage = "Could not start the updater. Close the app and run the downloaded file manually.";
                return;
            }

            StatusMessage = "Applying update — this app will close and restart with the new version.";
            System.Windows.Application.Current?.Shutdown();
        }

        private void ShowDownloadedFile()
        {
            if (string.IsNullOrEmpty(DownloadedPath))
                return;

            try
            {
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                {
                    FileName = "explorer.exe",
                    Arguments = $"/select,\"{DownloadedPath}\"",
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                ErrorReporter.Log("Open downloaded update in Explorer", ex);
            }
        }

        private void OpenReleases()
        {
            var url = _updateInfo?.ReleaseUrl
                      ?? $"https://github.com/{Owner}/{Repo}/releases";
            OpenBrowser(url);
        }

        private static void OpenBrowser(string url)
        {
            try
            {
                var startInfo = new System.Diagnostics.ProcessStartInfo { FileName = url, UseShellExecute = true };
                System.Diagnostics.Process.Start(startInfo);
            }
            catch (Exception ex)
            {
                ErrorReporter.Log("Open browser", ex);
            }
        }

        private static string GetCurrentVersion()
        {
            var assembly = typeof(AboutViewModel).Assembly;
            var informational = assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion;
            var version = informational ?? assembly.GetName().Version?.ToString() ?? "1.0.0";
            return version.Split('+')[0].TrimStart('v');
        }

        public string AppName => "BetterAccounting";
        public string Tagline => "Modern Windows accounting software";
        public string Author => "Altaf Yafai";
        public string Version { get; }
        public string RepositoryUrl => $"https://github.com/{Owner}/{Repo}";

        public bool UpdateAvailable
        {
            get => _updateAvailable;
            set
            {
                if (SetProperty(ref _updateAvailable, value))
                    DownloadCommand.RaiseCanExecuteChanged();
            }
        }

        public string StatusMessage
        {
            get => _statusMessage;
            set => SetProperty(ref _statusMessage, value);
        }

        public bool IsBusy
        {
            get => _isBusy;
            set
            {
                if (SetProperty(ref _isBusy, value))
                {
                    CheckForUpdatesCommand.RaiseCanExecuteChanged();
                    DownloadCommand.RaiseCanExecuteChanged();
                }
            }
        }

        public string DownloadedPath
        {
            get => _downloadedPath ?? string.Empty;
            private set
            {
                if (SetProperty(ref _downloadedPath, value))
                    ShowDownloadedFileCommand.RaiseCanExecuteChanged();
            }
        }

        public RelayCommand CheckForUpdatesCommand { get; }
        public RelayCommand DownloadCommand { get; }
        public RelayCommand ApplyUpdateCommand { get; }
        public RelayCommand OpenReleasesCommand { get; }
        public RelayCommand ShowDownloadedFileCommand { get; }
        public RelayCommand CloseCommand { get; }

        public Action? OnClose;
    }
}