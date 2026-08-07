using System;
using System.IO;
using System.Windows.Input;
using BetterAccounting.UI.Models;

namespace BetterAccounting.UI.ViewModels
{
    public class SyncSettingsViewModel : ViewModelBase
    {
        private string _syncFolderPath;
        private bool _isSyncEnabled;
        private bool _useOneDrive;
        private bool _useGoogleDrive;

        public SyncSettingsViewModel()
        {
            LoadSettings();
            SaveCommand = new RelayCommand(Save);
            BrowseCommand = new RelayCommand(BrowseForFolder);
        }

        private void LoadSettings()
        {
            var configPath = GetConfigPath();
            if (File.Exists(configPath))
            {
                var lines = File.ReadAllLines(configPath);
                foreach (var line in lines)
                {
                    var parts = line.Split('=');
                    if (parts.Length != 2) continue;
                    switch (parts[0])
                    {
                        case "SyncFolder": _syncFolderPath = parts[1]; break;
                        case "IsSyncEnabled": _isSyncEnabled = bool.Parse(parts[1]); break;
                        case "UseOneDrive": _useOneDrive = bool.Parse(parts[1]); break;
                        case "UseGoogleDrive": _useGoogleDrive = bool.Parse(parts[1]); break;
                    }
                }
            }

            if (string.IsNullOrEmpty(_syncFolderPath))
            {
                DetectDefaultSyncFolder();
            }

            IsSyncEnabled = _isSyncEnabled;
            SyncFolderPath = _syncFolderPath;
            UseOneDrive = _useOneDrive;
            UseGoogleDrive = _useGoogleDrive;
        }

        private void DetectDefaultSyncFolder()
        {
            // Try common OneDrive locations
            var oneDrive = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            var oneDrivePath = Path.Combine(oneDrive, "OneDrive");
            if (Directory.Exists(oneDrivePath))
            {
                _syncFolderPath = oneDrivePath;
                _useOneDrive = true;
                return;
            }

            // Try Google Drive
            var googleDrivePath = Path.Combine(oneDrive, "Google Drive");
            if (Directory.Exists(googleDrivePath))
            {
                _syncFolderPath = googleDrivePath;
                _useGoogleDrive = true;
            }
        }

        private void Save()
        {
            var configPath = GetConfigPath();
            var dir = Path.GetDirectoryName(configPath);
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            var content = string.Join("\n", new[]
            {
                $"SyncFolder={SyncFolderPath}",
                $"IsSyncEnabled={IsSyncEnabled}",
                $"UseOneDrive={UseOneDrive}",
                $"UseGoogleDrive={UseGoogleDrive}"
            });

            File.WriteAllText(configPath, content);
        }

        private void BrowseForFolder()
        {
            var dialog = new System.Windows.Forms.FolderBrowserDialog
            {
                Description = "Select Sync Folder",
                SelectedPath = _syncFolderPath
            };
            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                SyncFolderPath = dialog.SelectedPath;
            }
        }

        private string GetConfigPath()
        {
            var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            return Path.Combine(appData, "BetterAccounting", "sync.cfg");
        }

        public string SyncFolderPath
        {
            get => _syncFolderPath;
            set => SetProperty(ref _syncFolderPath, value);
        }

        public bool IsSyncEnabled
        {
            get => _isSyncEnabled;
            set => SetProperty(ref _isSyncEnabled, value);
        }

        public bool UseOneDrive
        {
            get => _useOneDrive;
            set => SetProperty(ref _useOneDrive, value);
        }

        public bool UseGoogleDrive
        {
            get => _useGoogleDrive;
            set => SetProperty(ref _useGoogleDrive, value);
        }

        public ICommand SaveCommand { get; }
        public ICommand BrowseCommand { get; }
    }
}
