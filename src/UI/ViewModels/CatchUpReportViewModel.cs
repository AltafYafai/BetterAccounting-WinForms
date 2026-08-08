using BetterAccounting.Core.Data.Models;
using BetterAccounting.Core.Services.Data;
using BetterAccounting.Core.Services.Reports;
using BetterAccounting.UI.Models;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace BetterAccounting.UI.ViewModels
{
    public class CatchUpReportViewModel : ViewModelBase
    {
        private readonly IDataContext _context;
        private readonly IAccountRepository _accountRepository;
        private readonly CatchUpReportService _catchUpService;

        private ObservableCollection<CatchUpRecord> _records = new();
        private DateTime _fromDate = DateTime.Today.AddMonths(-1);
        private DateTime _toDate = DateTime.Today;
        private decimal _totalOverPayment;
        private string _statusMessage = string.Empty;

        public CatchUpReportViewModel()
        {
            var dbPath = BetterAccounting.Core.Services.Data.AppPaths.CurrentDbPath();

            _context = new SQLiteContext(dbPath);
            _accountRepository = new AccountRepository(((SQLiteContext)_context).Connection);
            _catchUpService = new CatchUpReportService(_context, _accountRepository);

            GenerateCommand = new RelayCommand(async () => await GenerateAsync());
            _ = GenerateAsync();
        }

        private async Task GenerateAsync()
        {
            StatusMessage = "Loading...";
            try
            {
                var records = await _catchUpService.GenerateCatchUpAsync(FromDate, ToDate);
                Records = new ObservableCollection<CatchUpRecord>(records);
                TotalOverPayment = records.Sum(r => r.OverPayment);
                StatusMessage = $"Show accounts; overpaid (catch-up) total: {TotalOverPayment:C}";
            }
            catch (Exception ex)
            {
                StatusMessage = ErrorReporter.Message("Generate catch-up report", ex);
            }
        }

        public ObservableCollection<CatchUpRecord> Records
        {
            get => _records;
            set => SetProperty(ref _records, value);
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

        public decimal TotalOverPayment
        {
            get => _totalOverPayment;
            set => SetProperty(ref _totalOverPayment, value);
        }

        public string StatusMessage
        {
            get => _statusMessage;
            set => SetProperty(ref _statusMessage, value);
        }

        public ICommand GenerateCommand { get; }
    }
}