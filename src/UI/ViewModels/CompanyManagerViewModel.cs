using BetterAccounting.Core.Data.Models;
using BetterAccounting.Core.Services.Data;
using BetterAccounting.UI.Models;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace BetterAccounting.UI.ViewModels
{
    public class CompanyManagerViewModel : ViewModelBase
    {
        private readonly CompanyManager _manager;

        private ObservableCollection<CompanyItem> _items = new();
        private ObservableCollection<RemovedCompanyInfo> _removedItems = new();
        private CompanyItem? _selectedItem;
        private RemovedCompanyInfo? _selectedRemoved;
        private string _newCompanyName = "";
        private string _renameName = "";
        private string _statusMessage = "Manage your companies. Each company keeps its own data.";
        private bool _isBusy;

        public CompanyManagerViewModel(CompanyManager? manager = null)
        {
            _manager = manager ?? CompanyManager.Instance;

            AddCompanyCommand = new RelayCommand(async () => await AddCompanyAsync(), () => !string.IsNullOrWhiteSpace(NewCompanyName));
            ActivateCommand = new RelayCommand(ActivateSelected, () => SelectedItem != null && !SelectedItem.IsActive);
            RenameCommand = new RelayCommand(RenameSelected, () => SelectedItem != null && !string.IsNullOrWhiteSpace(RenameName));
            RestoreCommand = new RelayCommand(RestoreSelectedRemoved, () => SelectedRemoved != null);
            CloseCommand = new RelayCommand(() => OnClose?.Invoke());

            LoadItems();
        }

        private void LoadItems()
        {
            var activeId = _manager.ActiveId;
            Items = new ObservableCollection<CompanyItem>(
                _manager.Companies.Select(c => new CompanyItem
                {
                    Id = c.Id,
                    Name = c.Name,
                    DbPath = c.DbFilePath,
                    IsActive = c.Id == activeId
                }));

            SelectedItem = Items.FirstOrDefault(i => i.Id == activeId) ?? Items.FirstOrDefault();

            RemovedItems = new ObservableCollection<RemovedCompanyInfo>(_manager.GetRemovedCompanies());
            SelectedRemoved = RemovedItems.FirstOrDefault();
        }

        private void RestoreSelectedRemoved()
        {
            if (SelectedRemoved == null)
                return;

            var restored = _manager.RestoreRemovedCompany(SelectedRemoved);
            if (restored != null)
                StatusMessage = $"Restored '{restored.Name}' back into your companies. Use Activate to make it current.";
            else
                StatusMessage = "Could not restore that company.";

            LoadItems();
        }

        private async Task AddCompanyAsync()
        {
            IsBusy = true;
            try
            {
                var company = _manager.CreateCompany(NewCompanyName);
                _manager.Switch(company.Id);
                NewCompanyName = "";
                StatusMessage = $"Added and switched to '{company.Name}'. Fill in its Company Profile to personalise invoices.";
                LoadItems();
            }
            finally
            {
                IsBusy = false;
            }
        }

        private void ActivateSelected()
        {
            if (SelectedItem == null)
                return;

            _manager.Switch(SelectedItem.Id);
            StatusMessage = $"Active company is now '{SelectedItem.Name}'.";
            LoadItems();
        }

        private void RenameSelected()
        {
            if (SelectedItem == null)
                return;

            if (_manager.Rename(SelectedItem.Id, RenameName))
            {
                StatusMessage = "Company renamed.";
                RenameName = "";
                LoadItems();
            }
            else
            {
                StatusMessage = "Could not rename company.";
            }
        }

        public void RemoveSelected()
        {
            if (SelectedItem == null)
                return;

            var removedName = SelectedItem.Name;
            _manager.Remove(SelectedItem.Id);
            StatusMessage = $"Removed '{removedName}'. Its data was moved to RemovedCompanies (not deleted), so it can be recovered later.";
            LoadItems();
        }

        public ObservableCollection<CompanyItem> Items
        {
            get => _items;
            set => SetProperty(ref _items, value);
        }

        public ObservableCollection<RemovedCompanyInfo> RemovedItems
        {
            get => _removedItems;
            set => SetProperty(ref _removedItems, value);
        }

        public RemovedCompanyInfo? SelectedRemoved
        {
            get => _selectedRemoved;
            set
            {
                if (SetProperty(ref _selectedRemoved, value))
                    RestoreCommand.RaiseCanExecuteChanged();
            }
        }

        public CompanyItem? SelectedItem
        {
            get => _selectedItem;
            set
            {
                if (SetProperty(ref _selectedItem, value) && value != null)
                {
                    RenameName = value.Name;
                    ActivateCommand.RaiseCanExecuteChanged();
                    RenameCommand.RaiseCanExecuteChanged();
                }
            }
        }

        public string NewCompanyName
        {
            get => _newCompanyName;
            set
            {
                if (SetProperty(ref _newCompanyName, value))
                    AddCompanyCommand.RaiseCanExecuteChanged();
            }
        }

        public string RenameName
        {
            get => _renameName;
            set
            {
                if (SetProperty(ref _renameName, value))
                    RenameCommand.RaiseCanExecuteChanged();
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
            set => SetProperty(ref _isBusy, value);
        }

        public RelayCommand AddCompanyCommand { get; }
        public RelayCommand ActivateCommand { get; }
        public RelayCommand RenameCommand { get; }
        public RelayCommand RestoreCommand { get; }
        public RelayCommand CloseCommand { get; }

        public Action? OnClose;
    }
}