using BetterAccounting.Core.Data.Models;
using BetterAccounting.Core.Services.Data;
using BetterAccounting.Core.Services.Reports;
using BetterAccounting.UI.Models;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;

namespace BetterAccounting.UI.ViewModels
{
    public class DashboardViewModel : ViewModelBase
    {
        private readonly IDataContext _context;
        private readonly TrialBalanceService _trialBalanceService;
        private readonly FinancialStatementService _financialStatementService;
        private readonly AccountRepository _accountRepository;

        private ObservableCollection<TrialBalanceRecord> _trialBalance;
        private decimal _totalAssets;
        private decimal _totalLiabilities;
        private decimal _totalEquity;

        public DashboardViewModel()
        {
            var dbPath = Environment.GetEnvironmentVariable("BETTER_ACCOUNTING_DB_PATH");
            if (string.IsNullOrEmpty(dbPath))
            {
                var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                dbPath = System.IO.Path.Combine(appData, "BetterAccounting", "data.db");
            }

            _context = new SQLiteContext(dbPath);
            _accountRepository = new AccountRepository(((SQLiteContext)_context).Connection);
            _trialBalanceService = new TrialBalanceService(_context);
            _financialStatementService = new FinancialStatementService(_context);

            LoadDataCommand = new RelayCommand(async () => await RefreshAsync());
            AddEntryCommand = new RelayCommand(OpenVoucherEntry);
            OpenSyncSettingsCommand = new RelayCommand(OpenSyncSettings);
            ToggleThemeCommand = new RelayCommand(SwitchTheme);
            OpenReportsCommand = new RelayCommand(OpenReports);
            OpenBackupCommand = new RelayCommand(OpenBackupRestore);
            OpenAccountsCommand = new RelayCommand(OpenAccounts);
        }

        private async Task RefreshAsync()
        {
            var records = await _trialBalanceService.GenerateTrialBalanceAsync();
            TrialBalance = new ObservableCollection<TrialBalanceRecord>(records);

            var (assets, liabilities, equity) = await _financialStatementService.GetBalanceSheetTotalsAsync();
            TotalAssets = assets;
            TotalLiabilities = liabilities;
            TotalEquity = equity;
        }

        private void SwitchTheme()
        {
            var newTheme = ThemeManager.CurrentTheme == "Light" ? "Dark" : "Light";
            ThemeManager.ApplyTheme(newTheme);
            ThemeManager.SaveThemePreference(newTheme);
        }

        private void OpenVoucherEntry()
        {
            var view = new Views.VoucherEntryView();
            var window = new Window { Content = view, Title = "Voucher Entry", Width = 700, Height = 500 };
            window.Show();
        }

        private void OpenSyncSettings()
        {
            var view = new Views.SyncSettingsView();
            var window = new Window { Content = view, Title = "Sync Settings", Width = 600, Height = 400 };
            window.Show();
        }

        private void OpenReports()
        {
            var view = new Views.ReportViewerView();
            var window = new Window { Content = view, Title = "Reports", Width = 900, Height = 600 };
            window.Show();
        }

        private void OpenBackupRestore()
        {
            var view = new Views.BackupRestoreView();
            var window = new Window { Content = view, Title = "Backup & Restore", Width = 700, Height = 500 };
            window.Show();
        }

        private void OpenAccounts()
        {
            var view = new Views.ChartOfAccountsView();
            var window = new Window { Content = view, Title = "Chart of Accounts", Width = 800, Height = 500 };
            window.Show();
        }

        public ObservableCollection<TrialBalanceRecord> TrialBalance
        {
            get => _trialBalance;
            set => SetProperty(ref _trialBalance, value);
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
        public ICommand OpenSyncSettingsCommand { get; }
        public ICommand ToggleThemeCommand { get; }
        public ICommand OpenReportsCommand { get; }
        public ICommand OpenBackupCommand { get; }
        public ICommand OpenAccountsCommand { get; }
    }
}
