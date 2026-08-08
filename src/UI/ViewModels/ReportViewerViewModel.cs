using BetterAccounting.Core.Data.Models;
using BetterAccounting.Core.Services.Data;
using BetterAccounting.Core.Services.Reports;
using BetterAccounting.UI.Models;
using BetterAccounting.UI.Services;
using System;
using System.Collections.Generic;
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
        private readonly PrintTemplateService _printTemplateService;
        private readonly CompanyProfileRepository _companyRepository;

        private int _selectedReportIndex = 0;
        private DateTime _fromDate = DateTime.Today.AddMonths(-1);
        private DateTime _toDate = DateTime.Today;
        private string _selectedAccountName;
        private ObservableCollection<string> _accountNames;
        private ObservableCollection<ProfitAndLossRecord> _pnlData;
        private object _reportData;
        private string _errorMessage = string.Empty;

        public ReportViewerViewModel()
        {
            var dbPath = BetterAccounting.Core.Services.Data.AppPaths.CurrentDbPath();

            _context = new SQLiteContext(dbPath);
            _accountRepository = new AccountRepository(((SQLiteContext)_context).Connection);
            _trialBalanceService = new TrialBalanceService(_context);
            _financialStatementService = new FinancialStatementService(_context);
            _profitAndLossService = new ProfitAndLossService(_context, _accountRepository);
            _ledgerReportService = new LedgerReportService(_context, _accountRepository);
            _printTemplateService = new PrintTemplateService(new PrintTemplateRepository(((SQLiteContext)_context).Connection));
            _companyRepository = new CompanyProfileRepository(((SQLiteContext)_context).Connection);

            GenerateReportCommand = new RelayCommand(async () => await GenerateReportAsync());
            ExportCommand = new RelayCommand(async () => await ExportAsync(), () => ReportData != null);
            LoadAccountsCommand = new RelayCommand(async () => await LoadAccountsAsync());
            PrintCommand = new RelayCommand(async () => await PrintAsync(), () => ReportData != null);

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
            try
            {
                var names = await _accountRepository.GetAllAsync();
                AccountNames = new ObservableCollection<string>(names.Select(a => a.Name));
            }
            catch (Exception ex)
            {
                ErrorReporter.Log("Load account list for report viewer", ex);
            }
        }

        private async Task GenerateReportAsync()
        {
            try
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
                ErrorMessage = string.Empty;
            }
            catch (Exception ex)
            {
                ErrorMessage = ErrorReporter.Message($"Generate '{Reports[SelectedReportIndex]}' report", ex);
            }
            ((RelayCommand)ExportCommand).RaiseCanExecuteChanged();
        }

        private async Task ExportAsync()
        {
            try
            {
                var path = await FileDialogService.SaveFileAsync("Export report",
                    "report.txt", ("Text Files", new[] { "*.txt" }), ("CSV Files", new[] { "*.csv" }));
                if (string.IsNullOrEmpty(path))
                    return;

                await System.IO.File.WriteAllTextAsync(path, "Report data would be exported here");
            }
            catch (Exception ex)
            {
                ErrorMessage = ErrorReporter.Message($"Export '{Reports[SelectedReportIndex]}' report", ex);
            }
        }

        private async Task PrintAsync()
        {
            try
            {
                var reportTitle = SelectedReportIndex < Reports.Count ? Reports[SelectedReportIndex] : "Report";
                var company = await LoadCompanyAsync();
                var template = await _printTemplateService.GetDefaultAsync(DocumentType.Report);
                var content = template?.Content ?? PrintTemplateService.GetDefaultContent(DocumentType.Report);

                var fields = new Dictionary<string, string>
                {
                    { "CompanyName", company?.CompanyName ?? "" },
                    { "Gstin", company?.Gstin ?? "" },
                    { "Address", company?.Address ?? "" },
                    { "City", company?.City ?? "" },
                    { "State", company?.State ?? "" },
                    { "PinCode", company?.PinCode ?? "" },
                    { "Phone", company?.Phone ?? "" },
                    { "Email", company?.Email ?? "" },
                    { "ReportTitle", reportTitle },
                    { "FromDate", FromDate.ToShortDateString() },
                    { "ToDate", ToDate.ToShortDateString() },
                    { "CreatedDate", DateTime.Now.ToString("g") }
                };

                var reportRows = await BuildReportRowsAsync((ReportType)SelectedReportIndex);

                var layout = PrintTemplateSerializer.TryDeserialize(content);
                PrintDocumentModel document;
                if (layout != null)
                {
                    document = PrintLayoutRenderer.BuildLayoutDocument(layout, fields, reportRows);
                }
                else
                {
                    var lines = PrintTemplateService.Render(content, fields).Concat(reportRows).ToList();
                    document = TemplateDocumentBuilder.Build(lines);
                }

                var preview = new Views.DocumentPreviewWindow(document, $"Print - {reportTitle}");
                preview.Show();
            }
            catch (Exception ex)
            {
                ErrorMessage = ErrorReporter.Message($"Print '{Reports[SelectedReportIndex]}' report", ex);
            }
        }

        private async Task<CompanyProfile?> LoadCompanyAsync()
        {
            try
            {
                return await _companyRepository.GetAsync();
            }
            catch (Exception ex)
            {
                ErrorReporter.Log("Load company profile for report printing", ex);
                return null;
            }
        }

        private async Task<IReadOnlyList<string>> BuildReportRowsAsync(ReportType type)
        {
            var rows = new List<string>();
            switch (type)
            {
                case ReportType.TrialBalance:
                    var records = await _trialBalanceService.GenerateTrialBalanceAsync(FromDate, ToDate);
                    rows.Add("@B Account          Debits          Credits");
                    foreach (var r in records)
                        rows.Add($"{r.AccountName,-20}{r.TotalDebits:C14}{r.TotalCredits:C14}");
                    break;
                case ReportType.BalanceSheet:
                    var (assets, liabilities, equity) = await _financialStatementService.GetBalanceSheetTotalsAsync(ToDate);
                    rows.Add($"Assets       : {assets:C}");
                    rows.Add($"Liabilities  : {liabilities:C}");
                    rows.Add($"Equity       : {equity:C}");
                    break;
                case ReportType.ProfitAndLoss:
                    var pnl = await _profitAndLossService.GenerateAsync(FromDate, ToDate);
                    rows.Add("@B  Income");
                    foreach (var i in pnl.Incomes)
                        rows.Add($"{i.AccountName,-20}{i.Amount:C}");
                    rows.Add($"Total Income     {pnl.TotalIncome:C}");
                    rows.Add("@B  Expenses");
                    foreach (var e in pnl.Expenses)
                        rows.Add($"{e.AccountName,-20}{e.Amount:C}");
                    rows.Add($"Total Expenses   {pnl.TotalExpense:C}");
                    rows.Add($"Net Profit       {pnl.NetProfit:C}");
                    break;
                case ReportType.Ledger:
                    if (!string.IsNullOrEmpty(SelectedAccountName))
                    {
                        var ledger = await _ledgerReportService.GenerateForAccountAsync(SelectedAccountName, FromDate, ToDate);
                        rows.Add($"Account: {ledger.AccountName}");
                        rows.Add($"Opening Balance: {ledger.OpeningBalance:C}");
                        rows.Add("@B Date        Voucher      Description        Debit      Credit     Balance");
                        foreach (var e in ledger.Entries)
                            rows.Add($"{e.Date.ToShortDateString(),-12}{e.VoucherNo,-12}{e.Description,-16}{e.Debit:C9}{e.Credit:C9}{e.RunningBalance:C9}");
                        rows.Add($"Closing Balance: {ledger.ClosingBalance:C}");
                    }
                    break;
            }
            return rows;
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

        public string ErrorMessage
        {
            get => _errorMessage;
            set => SetProperty(ref _errorMessage, value);
        }

        public ICommand GenerateReportCommand { get; }
        public ICommand ExportCommand { get; }
        public ICommand LoadAccountsCommand { get; }
        public ICommand PrintCommand { get; }
    }
}
