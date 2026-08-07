using BetterAccounting.Core.Services.Data;
using BetterAccounting.Core.Services.Reports;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace BetterAccounting.UI.ViewModels
{
    public enum ReportType
    {
        TrialBalance,
        BalanceSheet,
        ProfitAndLoss,
        Ledger
    }

    public class ReportViewerViewModel : ViewModelBase
    {
        private readonly IDataContext _context;
        private readonly IAccountRepository _accountRepository;
        private readonly TrialBalanceService _trialBalanceService;
        private readonly FinancialStatementService _financialStatementService;
        private readonly ProfitAndLossService _profitAndLossService;
        private readonly LedgerReportService _ledgerReportService;

        private int _selectedReportIndex = 0;
        private DateTime _fromDate = DateTime.Today.AddMonths(-1);
        private DateTime _toDate = DateTime.Today;
        private string _selectedAccountName;
        private ObservableCollection<string> _accountNames;
        private ObservableCollection<ProfitAndLossRecord> _pnlData;
        private object _reportData;

        public ReportViewerViewModel()
        {
            var dbPath = Environment.GetEnvironmentVariable("BETTER_ACCOUNTING_DB_PATH");
            if (string.IsNullOrEmpty(dbPath))
            {
                var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                dbPath = System.IO.Path.Combine(appData, "BetterAccounting", "data.db");
            }

            _context = new SQLiteContext(dbPath);
            _accountRepository = new AccountRepository(((SQLiteContext)_context)._connection);
            _trialBalanceService = new TrialBalanceService(_context);
            _financialStatementService = new FinancialStatementService(_context);
            _profitAndLossService = new ProfitAndLossService(_context, _accountRepository);
            _ledgerReportService = new LedgerReportService(_context, _accountRepository);

            GenerateReportCommand = new RelayCommand(async () => await GenerateReportAsync());
            ExportCommand = new RelayCommand(async () => await ExportAsync(), () => ReportData != null);
            LoadAccountsCommand = new RelayCommand(async () => await LoadAccountsAsync());

            Reports = new ObservableCollection<string>
            {
                "Trial Balance",
                "Balance Sheet",
                "Profit & Loss",
                "Ledger"
            };

            _ = LoadAccountsAsync();
        }

        private async Task LoadAccountsAsync()
        {
            var names = await _accountRepository.GetAllAsync();
            AccountNames = new ObservableCollection<string>(names.Select(a => a.Name));
        }

        private async Task GenerateReportAsync()
        {
            ReportType reportType = (ReportType)SelectedReportIndex;
            
            switch (reportType)
            {
                case ReportType.TrialBalance:
                    var records = await _trialBalanceService.GenerateTrialBalanceAsync(FromDate, ToDate);
                    ReportData = new ObservableCollection<dynamic>(records.Select(r => new {
                        AccountName = r.AccountName,
                        TotalDebits = r.TotalDebits,
                        TotalCredits = r.TotalCredits
                    }));
                    break;
                case ReportType.ProfitAndLoss:
                    var pnl = await _profitAndLossService.GenerateAsync(FromDate, ToDate);
                    PnlData = new ObservableCollection<ProfitAndLossRecord>(pnl.Incomes.Concat(pnl.Expenses));
                    ReportData = PnlData;
                    break;
                case ReportType.Ledger:
                    if (!string.IsNullOrEmpty(SelectedAccountName))
                    {
                        var ledger = await _ledgerReportService.GenerateForAccountAsync(SelectedAccountName, FromDate, ToDate);
                        ReportData = ledger.Entries;
                    }
                    break;
                case ReportType.BalanceSheet:
                    var (assets, liabilities, equity) = await _financialStatementService.GetBalanceSheetTotalsAsync(ToDate);
                    ReportData = new[] {
                        new { Category = "Assets", Amount = assets },
                        new { Category = "Liabilities", Amount = liabilities },
                        new { Category = "Equity", Amount = equity }
                    };
                    break;
            }
            ((RelayCommand)ExportCommand).RaiseCanExecuteChanged();
        }

        private async Task ExportAsync()
        {
            var saveDialog = new Microsoft.Win32.SaveFileDialog
            {
                Filter = "Text Files|*.txt|CSV Files|*.csv"
            };
            if (saveDialog.ShowDialog() == true)
            {
                await System.IO.File.WriteAllTextAsync(saveDialog.FileName, "Report data would be exported here");
            }
        }

        public ObservableCollection<string> Reports
        {
            get => _reports;
            set => SetProperty(ref _reports, value);
        }

        private ObservableCollection<string> _reports;

        public int SelectedReportIndex
        {
            get => _selectedReportIndex;
            set => SetProperty(ref _selectedReportIndex, value);
        }

        public DateTime FromDate
        {
            get => _fromDate;
            set => SetProperty(ref _fromDate, value);
        }

        public DateTime ToDate
        {
            get => _toDate;
            set => SetProperty(ref _toDate, value);
        }

        public string SelectedAccountName
        {
            get => _selectedAccountName;
            set => SetProperty(ref _selectedAccountName, value);
        }

        public ObservableCollection<string> AccountNames
        {
            get => _accountNames;
            set => SetProperty(ref _accountNames, value);
        }

        public ObservableCollection<ProfitAndLossRecord> PnlData
        {
            get => _pnlData;
            set => SetProperty(ref _pnlData, value);
        }

        public object ReportData
        {
            get => _reportData;
            set => SetProperty(ref _reportData, value);
        }

        public ICommand GenerateReportCommand { get; }
        public ICommand ExportCommand { get; }
        public ICommand LoadAccountsCommand { get; }
    }
}
