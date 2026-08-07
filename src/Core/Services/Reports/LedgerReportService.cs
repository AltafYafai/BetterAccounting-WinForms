using BetterAccounting.Core.Data.Models;
using BetterAccounting.Core.Services.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BetterAccounting.Core.Services.Reports
{
    public class LedgerEntryViewModel
    {
        public DateTime Date { get; set; }
        public string VoucherNo { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal Debit { get; set; }
        public decimal Credit { get; set; }
        public decimal RunningBalance { get; set; }
    }

    public class LedgerReportResult
    {
        public string AccountName { get; set; } = string.Empty;
        public decimal OpeningBalance { get; set; }
        public List<LedgerEntryViewModel> Entries { get; set; } = new();
        public decimal ClosingBalance => Entries.LastOrDefault()?.RunningBalance ?? OpeningBalance;
    }

    public class LedgerReportService
    {
        private readonly IDataContext _context;
        private readonly IAccountRepository _accountRepository;

        public LedgerReportService(IDataContext context, IAccountRepository accountRepository)
        {
            _context = context;
            _accountRepository = accountRepository;
        }

        public async Task<LedgerReportResult> GenerateForAccountAsync(string accountName, DateTime? from = null, DateTime? to = null)
        {
            var account = await _accountRepository.GetByNameAsync(accountName);
            if (account == null) throw new ArgumentException($"Account '{accountName}' not found");

            var entries = from.HasValue && to.HasValue
                ? await _context.GetEntriesByDateRangeAsync(from.Value, to.Value)
                : await _context.GetAllEntriesAsync();

            var accountEntries = entries
                .Where(e => e.AccountName == accountName)
                .OrderBy(e => e.Date)
                .ToList();

            var result = new LedgerReportResult
            {
                AccountName = accountName,
                OpeningBalance = account.OpeningBalance * (account.OpeningBalanceType == EntryType.Debit ? 1 : -1)
            };

            decimal runningBalance = result.OpeningBalance;

            foreach (var entry in accountEntries)
            {
                var vmEntry = new LedgerEntryViewModel
                {
                    Date = entry.Date,
                    VoucherNo = entry.VoucherNo,
                    Description = entry.Description,
                    Debit = entry.Type == EntryType.Debit ? entry.Amount : 0,
                    Credit = entry.Type == EntryType.Credit ? entry.Amount : 0,
                };
                runningBalance += vmEntry.Debit - vmEntry.Credit;
                vmEntry.RunningBalance = runningBalance;
                result.Entries.Add(vmEntry);
            }

            return result;
        }
    }
}
