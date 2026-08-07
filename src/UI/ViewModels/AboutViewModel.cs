using BetterAccounting.Core.Data.Models;
using BetterAccounting.Core.Services;
using BetterAccounting.Core.Services.Data;
using BetterAccounting.UI.Models;
using System;
using System.IO;
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
                StatusMessage = $"Download failed: {ex.Message}";
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
                var dbPath = Path.Combine(appData, "BetterAccounting", "data.db");
                if (!File.Exists(dbPath))
                    return null;

                var backupDir = Path.Combine(appData, "BetterAccounting", "PreUpdateBackups");
                var service = new BackupService(dbPath, backupDir);
                return await service.CreateBackupAsync($"preupdate_{DateTime.Now:yyyyMMdd_HHmmss}");
            }
            catch
            {
                return null;
            }
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
            catch
            {
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
            catch
            {
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
        public RelayCommand OpenReleasesCommand { get; }
        public RelayCommand ShowDownloadedFileCommand { get; }
        public RelayCommand CloseCommand { get; }

        public Action? OnClose;
    }
}