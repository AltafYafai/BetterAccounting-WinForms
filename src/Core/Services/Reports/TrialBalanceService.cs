using BetterAccounting.Core.Data.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BetterAccounting.Core.Services.Reports
{
    public class TrialBalanceRecord
    {
        public string AccountName { get; set; }
        public decimal TotalDebits { get; set; }
        public decimal TotalCredits { get; set; }
    }

    public class TrialBalanceService
    {
        private readonly IDataContext _context;

        public TrialBalanceService(IDataContext context)
        {
            _context = context;
        }

        public async Task<List<TrialBalanceRecord>> GenerateTrialBalanceAsync(DateTime? from = null, DateTime? to = null)
        {
            var entries = from.HasValue && to.HasValue
                ? await _context.GetEntriesByDateRangeAsync(from.Value, to.Value)
                : await _context.GetAllEntriesAsync();

            var grouped = entries
                .GroupBy(e => e.AccountName)
                .Select(g => new TrialBalanceRecord
                {
                    AccountName = g.Key,
                    TotalDebits = g.Where(e => e.Type == EntryType.Debit).Sum(e => e.Amount),
                    TotalCredits = g.Where(e => e.Type == EntryType.Credit).Sum(e => e.Amount)
                })
                .OrderBy(r => r.AccountName)
                .ToList();

            return grouped;
        }
    }
}
