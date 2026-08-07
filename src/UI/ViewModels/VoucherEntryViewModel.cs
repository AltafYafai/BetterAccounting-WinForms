using BetterAccounting.Core.Data.Models;
using BetterAccounting.Core.Services.Data;
using BetterAccounting.Core.Services.Reports;
using BetterAccounting.UI.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace BetterAccounting.UI.ViewModels
{
    public class VoucherEntryViewModel : ViewModelBase, INotifyDataErrorInfo
    {
        private readonly IDataContext _context;
        private readonly IAccountRepository _accountRepository;
        private readonly FinancialStatementService _financialStatementService;

        private DateTime _date = DateTime.Today;
        private string _voucherNumber = "";
        private string _narration = "";
        private EntryType _entryType = EntryType.Debit;
        private string _selectedAccount = "";
        private decimal _amount;
        private ObservableCollection<Account> _accounts;
        private bool _isDirty;
        private VoucherType _selectedVoucherType = VoucherType.Journal;
        private string _transporter = "";

        public VoucherEntryViewModel() : this(VoucherType.Journal)
        {
        }

        public VoucherEntryViewModel(VoucherType initialType)
        {
            var dbPath = BetterAccounting.Core.Services.Data.AppPaths.CurrentDbPath();

            _context = new SQLiteContext(dbPath);
            _accountRepository = new AccountRepository(((SQLiteContext)_context).Connection);
            _financialStatementService = new FinancialStatementService(_context);

            SaveCommand = new RelayCommand(async () => await SaveAsync(), () => IsFormValid());
            CancelCommand = new RelayCommand(() => OnCancel?.Invoke());
            LoadAccountsCommand = new RelayCommand(async () => await LoadAccountsAsync());
            OpenPrintPreviewCommand = new RelayCommand(OpenPrintPreview);

            EntryTypes = new ObservableCollection<EntryType>(Enum.GetValues<EntryType>());
            VoucherTypes = new ObservableCollection<VoucherType>(Enum.GetValues<VoucherType>());
            SelectedVoucherType = initialType;

            _ = LoadAccountsAsync();
        }

        private async Task LoadAccountsAsync()
        {
            var allAccounts = await _accountRepository.GetAllAsync();
            Accounts = new ObservableCollection<Account>(allAccounts);
        }

        public async System.Threading.Tasks.Task SaveAsync()
        {
            var entry = new LedgerEntry
            {
                Date = Date,
                VoucherNo = VoucherNumber,
                AccountName = SelectedAccount,
                Type = EntryType,
                Amount = Amount,
                Description = Narration,
                VoucherType = SelectedVoucherType,
                Transporter = Transporter,
                CreatedAt = DateTime.UtcNow
            };

            await _context.AddEntryAsync(entry);
            OnSaveSuccess?.Invoke();
        }

        private void OpenPrintPreview()
        {
            var entry = new LedgerEntry
            {
                Date = Date,
                VoucherNo = VoucherNumber,
                AccountName = SelectedAccount,
                Type = EntryType,
                Amount = Amount,
                Description = Narration,
                VoucherType = SelectedVoucherType,
                Transporter = Transporter
            };
            var view = new Views.PrintPreviewWindow(entry);
            view.Show();
        }

        private bool IsFormValid()
        {
            ValidateAllProperties();
            return !HasErrors;
        }

        public void ValidateAllProperties()
        {
            ValidateProperty(nameof(Date), Date);
            ValidateProperty(nameof(VoucherNumber), VoucherNumber);
            ValidateProperty(nameof(SelectedAccount), SelectedAccount);
            ValidateProperty(nameof(Amount), Amount);
        }

        private void ValidateProperty(string propertyName, object value)
        {
            var validationContext = new ValidationContext(this) { MemberName = propertyName };
            var validationResults = new List<ValidationResult>();
            Validator.TryValidateProperty(value, validationContext, validationResults);

            if (validationResults.Any())
            {
                _errors[propertyName] = validationResults.Select(v => v.ErrorMessage).ToList();
            }
            else
            {
                _errors.Remove(propertyName);
            }

            ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(propertyName));
        }

        #region INotifyDataErrorInfo Implementation

        private readonly Dictionary<string, List<string>> _errors = new();

        public bool HasErrors => _errors.Count > 0;

        /// <inheritdoc />
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Nullable", "CS8768")]
        public event EventHandler<DataErrorsChangedEventArgs>? ErrorsChanged;

        /// <inheritdoc />
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Nullable", "CS8768")]
        System.Collections.IEnumerable? INotifyDataErrorInfo.GetErrors(string? propertyName)
        {
            if (string.IsNullOrEmpty(propertyName))
                return _errors.SelectMany(e => e.Value).ToArray();
            
            return _errors.GetValueOrDefault(propertyName, new List<string>());
        }

        #endregion

        public async void OnPropertyChangedHandler(string propertyName)
        {
            if (propertyName == nameof(Date) || propertyName == nameof(VoucherNumber) || 
                propertyName == nameof(SelectedAccount) || propertyName == nameof(Amount))
            {
                IsDirty = true;
                ((RelayCommand)SaveCommand).RaiseCanExecuteChanged();
                ValidateAllProperties();
            }
        }

        protected override void OnPropertyChanged(string? propertyName = null)
        {
            base.OnPropertyChanged(propertyName);
            OnPropertyChangedHandler(propertyName);
        }

        public DateTime Date
        {
            get => _date;
            set => SetProperty(ref _date, value);
        }

        [Required(ErrorMessage = "Voucher number is required")]
        public string VoucherNumber
        {
            get => _voucherNumber;
            set => SetProperty(ref _voucherNumber, value);
        }

        public string Narration
        {
            get => _narration;
            set => SetProperty(ref _narration, value);
        }

        public EntryType EntryType
        {
            get => _entryType;
            set => SetProperty(ref _entryType, value);
        }

        [Required(ErrorMessage = "Please select an account")]
        public string SelectedAccount
        {
            get => _selectedAccount;
            set => SetProperty(ref _selectedAccount, value);
        }

        [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than zero")]
        public decimal Amount
        {
            get => _amount;
            set => SetProperty(ref _amount, value);
        }

        public ObservableCollection<Account> Accounts
        {
            get => _accounts;
            set => SetProperty(ref _accounts, value);
        }

        public ObservableCollection<EntryType> EntryTypes
        {
            get => _entryTypes;
            set => SetProperty(ref _entryTypes, value);
        }

        private ObservableCollection<EntryType> _entryTypes;

        public ObservableCollection<VoucherType> VoucherTypes
        {
            get => _voucherTypes;
            set => SetProperty(ref _voucherTypes, value);
        }

        private ObservableCollection<VoucherType> _voucherTypes;

        public VoucherType SelectedVoucherType
        {
            get => _selectedVoucherType;
            set => SetProperty(ref _selectedVoucherType, value);
        }

        public string Transporter
        {
            get => _transporter;
            set => SetProperty(ref _transporter, value);
        }

        public bool IsDirty
        {
            get => _isDirty;
            set => SetProperty(ref _isDirty, value);
        }

        public ICommand SaveCommand { get; }
        public ICommand CancelCommand { get; }
        public ICommand LoadAccountsCommand { get; }
        public ICommand OpenPrintPreviewCommand { get; }

        public Action OnSaveSuccess;
        public Action OnCancel;
    }
}
