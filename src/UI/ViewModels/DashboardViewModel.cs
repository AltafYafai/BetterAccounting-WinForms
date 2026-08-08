using BetterAccounting.Core.Data.Models;
using BetterAccounting.Core.Services.Data;
using BetterAccounting.Core.Services.Reports;
using BetterAccounting.UI.Models;
using BetterAccounting.UI.Services;
using Avalonia.Controls;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace BetterAccounting.UI.ViewModels
{
    public class DashboardViewModel : ViewModelBase
    {
        private readonly CompanyManager _companyManager = CompanyManager.Instance;

        private IDataContext _context;
        private TrialBalanceService _trialBalanceService;
        private FinancialStatementService _financialStatementService;
        private AccountRepository _accountRepository;

        private ObservableCollection<TrialBalanceRecord> _trialBalance;
        private ObservableCollection<CompanyItem> _companies = new();
        private CompanyItem? _selectedCompany;
        private decimal _totalAssets;
        private decimal _totalLiabilities;
        private decimal _totalEquity;

        public DashboardViewModel()
        {
            InitializeContext();
            LoadCompanies();

            LoadDataCommand = new RelayCommand(async () => await RefreshAsync());
            AddEntryCommand = new RelayCommand(OpenVoucherEntry);
            OpenBankEntryCommand = new RelayCommand(() => OpenVoucherEntry(VoucherType.Bank));
            OpenDebitNoteCommand = new RelayCommand(() => OpenVoucherEntry(VoucherType.DebitNote));
            OpenCreditNoteCommand = new RelayCommand(() => OpenVoucherEntry(VoucherType.CreditNote));
            OpenSyncSettingsCommand = new RelayCommand(OpenSyncSettings);
            ToggleThemeCommand = new RelayCommand(SwitchTheme);
            OpenReportsCommand = new RelayCommand(OpenReports);
            OpenCatchUpCommand = new RelayCommand(OpenCatchUp);
            OpenBackupCommand = new RelayCommand(OpenBackupRestore);
            OpenAccountsCommand = new RelayCommand(OpenAccounts);
            OpenCompanyProfileCommand = new RelayCommand(OpenCompanyProfile);
            OpenGstSlabsCommand = new RelayCommand(OpenGstSlabs);
            OpenAddCustomerCommand = new RelayCommand(OpenAddCustomer);
            OpenAboutCommand = new RelayCommand(OpenAbout);
            OpenPrintFormatsCommand = new RelayCommand(OpenPrintFormats);
            OpenCompanyManagerCommand = new RelayCommand(async () => await OpenCompanyManagerAsync());

            _ = RefreshAsync();
        }

        private void InitializeContext()
        {
            (_context as IDisposable)?.Dispose();

            var dbPath = AppPaths.CurrentDbPath();
            _context = new SQLiteContext(dbPath);
            _accountRepository = new AccountRepository(((SQLiteContext)_context).Connection);
            _trialBalanceService = new TrialBalanceService(_context);
            _financialStatementService = new FinancialStatementService(_context);
        }

        private void LoadCompanies()
        {
            var activeId = _companyManager.ActiveId;
            Companies = new ObservableCollection<CompanyItem>(
                _companyManager.Companies.Select(c => new CompanyItem
                {
                    Id = c.Id,
                    Name = c.Name,
                    DbPath = c.DbFilePath,
                    IsActive = c.Id == activeId
                }));

            ActiveCompanyName = _companyManager.Active?.Name ?? "";

            _isSwitching = true;
            SelectedCompany = Companies.FirstOrDefault(i => i.Id == activeId) ?? Companies.FirstOrDefault();
            _isSwitching = false;
        }

        private async Task SwitchCompanyAsync(CompanyItem target)
        {
            if (target == null || _isSwitching)
                return;

            if (_companyManager.ActiveId == target.Id)
                return;

            _isSwitching = true;
            try
            {
                _companyManager.Switch(target.Id);
                IsBusy = true;
                await Task.Yield();
                InitializeContext();
                LoadCompanies();
                await RefreshAsync();
                StatusMessage = $"Active company: {target.Name}";
            }
            catch (Exception ex)
            {
                StatusMessage = ErrorReporter.Message($"Switch to company '{target.Name}'", ex);
            }
            finally
            {
                IsBusy = false;
                _isSwitching = false;
            }
        }

        private async Task RefreshAsync()
        {
            try
            {
                var records = await _trialBalanceService.GenerateTrialBalanceAsync();
                TrialBalance = new ObservableCollection<TrialBalanceRecord>(records);

                var (assets, liabilities, equity) = await _financialStatementService.GetBalanceSheetTotalsAsync();
                TotalAssets = assets;
                TotalLiabilities = liabilities;
                TotalEquity = equity;
            }
            catch (Exception ex)
            {
                StatusMessage = ErrorReporter.Message("Load dashboard summary", ex);
            }
        }

        private void SwitchTheme()
        {
            var newTheme = ThemeManager.CurrentTheme == "Light" ? "Dark" : "Light";
            ThemeManager.ApplyTheme(newTheme);
            ThemeManager.SaveThemePreference(newTheme);
        }

        private void OpenVoucherEntry()
        {
            OpenVoucherEntry(BetterAccounting.Core.Data.Models.VoucherType.Journal);
        }

        private void OpenVoucherEntry(VoucherType voucherType)
        {
            var view = new Views.VoucherEntryView(voucherType);
            view.Show();
        }

        private void OpenSyncSettings()
        {
            var view = new Views.SyncSettingsView();
            view.Show();
        }

        private void OpenReports()
        {
            var view = new Views.ReportViewerView();
            view.Show();
        }

        private void OpenCatchUp()
        {
            var view = new Views.CatchUpReportView();
            view.Show();
        }

        private void OpenBackupRestore()
        {
            var view = new Views.BackupRestoreView();
            view.Show();
        }

        private void OpenAccounts()
        {
            var view = new Views.ChartOfAccountsView();
            view.Show();
        }

        private void OpenCompanyProfile()
        {
            var view = new Views.CompanySettingsView();
            view.Show();
        }

        private void OpenGstSlabs()
        {
            var view = new Views.GstSlabView();
            view.Show();
        }

        private void OpenAddCustomer()
        {
            var view = new Views.AddCustomerView();
            view.Show();
        }

        private void OpenAbout()
        {
            var window = new Views.AboutWindow();
            ShowDialog(window);
        }

        private void OpenPrintFormats()
        {
            var window = new Views.PrintFormatView();
            window.Show();
        }

        private async Task OpenCompanyManagerAsync()
        {
            var window = new Views.CompanyManagerView();
            await ShowDialogAsync(window);

            // The manager may have added/switched/removed companies while it was open,
            // so resync the dashboard's context and totals with the (possibly new) active company.
            IsBusy = true;
            try
            {
                await Task.Yield();
                InitializeContext();
                LoadCompanies();
                await RefreshAsync();
                StatusMessage = $"Active company: {_companyManager.Active.Name}";
            }
            finally
            {
                IsBusy = false;
            }
        }

        public ObservableCollection<TrialBalanceRecord> TrialBalance
        {
            get => _trialBalance;
            set => SetProperty(ref _trialBalance, value);
        }

        public ObservableCollection<CompanyItem> Companies
        {
            get => _companies;
            set => SetProperty(ref _companies, value);
        }

        public CompanyItem? SelectedCompany
        {
            get => _selectedCompany;
            set
            {
                if (SetProperty(ref _selectedCompany, value) && value != null)
                    _ = SwitchCompanyAsync(value);
            }
        }

        public string ActiveCompanyName
        {
            get => _activeCompanyName;
            private set => SetProperty(ref _activeCompanyName, value);
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

        public decimal TotalAssets
        {
            get => _totalAssets;
            set => SetProperty(ref _totalAssets, value);
        }

        public decimal TotalLiabilities
        {
            get => _totalLiabilities;
            set => SetProperty(ref _totalLiabilities, value);
        }

        public decimal TotalEquity
        {
            get => _totalEquity;
            set => SetProperty(ref _totalEquity, value);
        }

        public ICommand LoadDataCommand { get; }
        public ICommand AddEntryCommand { get; }
        public ICommand OpenBankEntryCommand { get; }
        public ICommand OpenDebitNoteCommand { get; }
        public ICommand OpenCreditNoteCommand { get; }
        public ICommand OpenSyncSettingsCommand { get; }
        public ICommand ToggleThemeCommand { get; }
        public ICommand OpenReportsCommand { get; }
        public ICommand OpenCatchUpCommand { get; }
        public ICommand OpenBackupCommand { get; }
        public ICommand OpenAccountsCommand { get; }
        public ICommand OpenCompanyProfileCommand { get; }
        public ICommand OpenGstSlabsCommand { get; }
        public ICommand OpenAddCustomerCommand { get; }
        public ICommand OpenAboutCommand { get; }
        public ICommand OpenPrintFormatsCommand { get; }
        public ICommand OpenCompanyManagerCommand { get; }

        private bool _isSwitching;
        private bool _isBusy;
        private string _statusMessage = "";
        private string _activeCompanyName = "";

        private static Window? DialogOwner => AppServices.GetMainWindow();

        private static void ShowDialog(Window dialog)
        {
            var owner = DialogOwner;
            if (owner != null)
                dialog.ShowDialog(owner);
            else
                dialog.Show();
        }

        private static async Task ShowDialogAsync(Window dialog)
        {
            var owner = DialogOwner;
            if (owner != null)
                await dialog.ShowDialog(owner);
            else
                dialog.Show();
        }
    }
}
