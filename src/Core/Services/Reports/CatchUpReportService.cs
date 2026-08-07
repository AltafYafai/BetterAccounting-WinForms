using BetterAccounting.Core.Data.Models;
using BetterAccounting.Core.Services.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BetterAccounting.Core.Services.Reports
{
    public class CatchUpRecord
    {
        public string AccountName { get; set; } = string.Empty;
        public decimal OpeningBalance { get; set; }
        public decimal Invoiced { get; set; }
        public decimal Paid { get; set; }
        public decimal ClosingBalance { get; set; }
        public decimal OverPayment { get; set; }
        public string Status { get; set; } = string.Empty;
    }

    public class CatchUpReportService
    {
        private readonly IDataContext _context;
        private readonly IAccountRepository _accountRepository;

        public CatchUpReportService(IDataContext context, IAccountRepository accountRepository)
        {
            _context = context;
            _accountRepository = accountRepository;
        }

        public async Task<List<CatchUpRecord>> GenerateCatchUpAsync(DateTime from, DateTime to)
        {
            var accounts = await _accountRepository.GetAllAsync();
            var entries = await _context.GetEntriesByDateRangeAsync(from, to);

            var groups = entries
                .GroupBy(e => e.AccountName)
                .ToDictionary(g => g.Key, g => g.ToList());

            var results = new List<CatchUpRecord>();

            foreach (var account in accounts.OrderBy(a => a.Name))
            {
                if (!groups.TryGetValue(account.Name, out var accountEntries))
                    continue;

                var opening = account.OpeningBalance * (account.OpeningBalanceType == EntryType.Debit ? 1 : -1);

                var debits = accountEntries.Where(e => e.Type == EntryType.Debit).Sum(e => e.Amount);
                var credits = accountEntries.Where(e => e.Type == EntryType.Credit).Sum(e => e.Amount);

                var closing = opening + debits - credits;

                var overPayment = closing < 0 ? Math.Abs(closing) : 0m;

                results.Add(new CatchUpRecord
                {
                    AccountName = account.Name,
                    OpeningBalance = opening,
                    Invoiced = debits,
                    Paid = credits,
                    ClosingBalance = closing,
                    OverPayment = overPayment,
                    Status = overPayment > 0 ? "Overpaid" : "In balance"
                });
            }

            return results
                .OrderByDescending(r => r.OverPayment)
                .ToList();
        }
    }
}