using BetterAccounting.Core.Data.Models;
using BetterAccounting.Core.Services.Data;
using BetterAccounting.UI.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Input;

namespace BetterAccounting.UI.ViewModels
{
    public class AccountEditorViewModel : ViewModelBase
    {
        private readonly AccountRepository _repository;
        private readonly Account? _existing;

        private string _name = "";
        private AccountGroup _group = AccountGroup.Assets;
        private string _description = "";
        private decimal _openingBalance;
        private EntryType _openingBalanceType = EntryType.Debit;
        private bool _isBusy;
        private string _statusMessage = "";

        public AccountEditorViewModel(AccountRepository repository, Account? existing = null)
        {
            _repository = repository;
            _existing = existing;

            Groups = new List<AccountGroup>(Enum.GetValues<AccountGroup>());
            BalanceTypes = new List<EntryType> { EntryType.Debit, EntryType.Credit };

            if (existing != null)
            {
                _name = existing.Name;
                _group = existing.Group;
                _description = existing.Description ?? "";
                _openingBalance = existing.OpeningBalance;
                _openingBalanceType = existing.OpeningBalanceType;
            }

            SaveCommand = new RelayCommand(async () => await SaveAsync(), () => !string.IsNullOrWhiteSpace(Name));
            CancelCommand = new RelayCommand(() => OnClose?.Invoke());
            Title = existing == null ? "Add Account" : $"Edit Account: {existing.Name}";
        }

        private async Task SaveAsync()
        {
            if (string.IsNullOrWhiteSpace(Name))
            {
                StatusMessage = "Account name is required.";
                return;
            }

            IsBusy = true;
            try
            {
                if (_existing != null)
                {
                    _existing.Name = Name.Trim();
                    _existing.Group = Group;
                    _existing.Description = string.IsNullOrWhiteSpace(Description) ? null : Description.Trim();
                    _existing.OpeningBalance = OpeningBalance;
                    _existing.OpeningBalanceType = OpeningBalanceType;
                    await _repository.UpdateAsync(_existing);
                }
                else
                {
                    await _repository.AddAsync(new Account
                    {
                        Name = Name.Trim(),
                        Group = Group,
                        Description = string.IsNullOrWhiteSpace(Description) ? null : Description.Trim(),
                        OpeningBalance = OpeningBalance,
                        OpeningBalanceType = OpeningBalanceType
                    });
                }

                OnSaved?.Invoke();
                OnClose?.Invoke();
            }
            catch (Exception ex)
            {
                StatusMessage = ErrorReporter.Message($"Save account '{Name}'", ex);
            }
            finally
            {
                IsBusy = false;
            }
        }

        public string Title { get; }

        public IReadOnlyList<AccountGroup> Groups { get; }
        public List<EntryType> BalanceTypes { get; }

        public string Name
        {
            get => _name;
            set
            {
                if (SetProperty(ref _name, value))
                    SaveCommand.RaiseCanExecuteChanged();
            }
        }

        public AccountGroup Group
        {
            get => _group;
            set => SetProperty(ref _group, value);
        }

        public string Description
        {
            get => _description;
            set => SetProperty(ref _description, value);
        }

        public decimal OpeningBalance
        {
            get => _openingBalance;
            set => SetProperty(ref _openingBalance, value);
        }

        public EntryType OpeningBalanceType
        {
            get => _openingBalanceType;
            set => SetProperty(ref _openingBalanceType, value);
        }

        public bool IsBusy
        {
            get => _isBusy;
            set => SetProperty(ref _isBusy, value);
        }

        public string StatusMessage
        {
            get => _statusMessage;
            set => SetProperty(ref _statusMessage, value);
        }

        public RelayCommand SaveCommand { get; }
        public RelayCommand CancelCommand { get; }

        public Action? OnSaved;
        public Action? OnClose;
    }
}