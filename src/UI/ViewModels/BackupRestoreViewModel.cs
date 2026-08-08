using BetterAccounting.Core.Services.Data;
using BetterAccounting.UI.Models;
using BetterAccounting.UI.Services;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Input;

namespace BetterAccounting.UI.ViewModels
{
    public class BackupRestoreViewModel : ViewModelBase
    {
        private readonly BackupService _backupService;
        private ObservableCollection<string> _backups;
        private string _selectedBackup;
        private bool _isOperationInProgress;
        private string _statusMessage = "Ready";

        public BackupRestoreViewModel()
        {
            _backupService = new BackupService();
            RefreshCommand = new RelayCommand(async () => await RefreshBackupsAsync());
            BackupCommand = new RelayCommand(async () => await CreateBackupAsync(), () => !IsOperationInProgress);
            RestoreCommand = new RelayCommand(async () => await RestoreBackupAsync(), () => !IsOperationInProgress && !string.IsNullOrEmpty(SelectedBackup));
            BrowseCommand = new RelayCommand(BrowseForBackup);

            _ = RefreshBackupsAsync();
        }

        private async Task RefreshBackupsAsync()
        {
            try
            {
                var backups = _backupService.GetAvailableBackups();
                Backups = new ObservableCollection<string>(backups);
            }
            catch (Exception ex)
            {
                StatusMessage = ErrorReporter.Message("Load list of backups", ex);
            }
        }

        private async Task CreateBackupAsync()
        {
            IsOperationInProgress = true;
            StatusMessage = "Creating backup...";
            try
            {
                var path = await _backupService.CreateBackupAsync();
                StatusMessage = $"Backup created: {Path.GetFileName(path)}";
                await RefreshBackupsAsync();
            }
            catch (Exception ex)
            {
                StatusMessage = ErrorReporter.Message("Create backup", ex);
            }
            finally
            {
                ((RelayCommand)BackupCommand).RaiseCanExecuteChanged();
                IsOperationInProgress = false;
            }
        }

        private async Task RestoreBackupAsync()
        {
            if (string.IsNullOrEmpty(SelectedBackup)) return;

            IsOperationInProgress = true;
            StatusMessage = "Restoring backup...";
            try
            {
                var success = await _backupService.RestoreBackupAsync(SelectedBackup);
                StatusMessage = success ? "Backup restored successfully" : "Restore failed";
            }
            catch (Exception ex)
            {
                StatusMessage = ErrorReporter.Message($"Restore backup '{Path.GetFileName(SelectedBackup)}'", ex);
            }
            finally
            {
                ((RelayCommand)RestoreCommand).RaiseCanExecuteChanged();
                IsOperationInProgress = false;
            }
        }

        private async Task BrowseForBackup()
        {
            var file = await FileDialogService.PickFileAsync("Select backup file", ("ZIP Files", new[] { "*.zip" }));
            if (file != null)
            {
                SelectedBackup = file;
                ((RelayCommand)RestoreCommand).RaiseCanExecuteChanged();
            }
        }

        public ObservableCollection<string> Backups
        {
            get => _backups;
            set => SetProperty(ref _backups, value);
        }

        public string SelectedBackup
        {
            get => _selectedBackup;
            set => SetProperty(ref _selectedBackup, value);
        }

        public bool IsOperationInProgress
        {
            get => _isOperationInProgress;
            set
            {
                if (SetProperty(ref _isOperationInProgress, value))
                {
                    ((RelayCommand)BackupCommand).RaiseCanExecuteChanged();
                    ((RelayCommand)RestoreCommand).RaiseCanExecuteChanged();
                }
            }
        }

        public string StatusMessage
        {
            get => _statusMessage;
            set => SetProperty(ref _statusMessage, value);
        }

        public ICommand RefreshCommand { get; }
        public ICommand BackupCommand { get; }
        public ICommand RestoreCommand { get; }
        public ICommand BrowseCommand { get; }
    }
}
