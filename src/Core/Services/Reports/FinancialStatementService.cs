using BetterAccounting.Core.Data.Models;
using BetterAccounting.Core.Services.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BetterAccounting.Core.Services.Reports
{
    public class FinancialStatementService
    {
        private readonly IDataContext _context;

        public FinancialStatementService(IDataContext context)
        {
            _context = context;
        }

        public async Task<(decimal Assets, decimal Liabilities, decimal Equity)> GetBalanceSheetTotalsAsync(DateTime? asOf = null)
        {
            var entries = await _context.GetAllEntriesAsync();
            if (asOf.HasValue)
                entries = entries.Where(e => e.Date <= asOf.Value).ToList();

            // Simplified assumption: account names starting with keywords determine category.
            // Real system would use account types/groups.
            var assets = entries
                .Where(e => e.Type == EntryType.Debit && IsAssetAccount(e.AccountName))
                .Sum(e => e.Amount) -
                entries
                .Where(e => e.Type == EntryType.Credit && IsAssetAccount(e.AccountName))
                .Sum(e => e.Amount);

            var liabilities = entries
                .Where(e => e.Type == EntryType.Credit && IsLiabilityAccount(e.AccountName))
                .Sum(e => e.Amount) -
                entries
                .Where(e => e.Type == EntryType.Debit && IsLiabilityAccount(e.AccountName))
                .Sum(e => e.Amount);

            var equity = entries
                .Where(e => e.Type == EntryType.Credit && IsEquityAccount(e.AccountName))
                .Sum(e => e.Amount) -
                entries
                .Where(e => e.Type == EntryType.Debit && IsEquityAccount(e.AccountName))
                .Sum(e => e.Amount);

            return (assets, liabilities, equity);
        }

        private static bool IsAssetAccount(string name) =>
            name.Contains("Bank", StringComparison.OrdinalIgnoreCase) ||
            name.Contains("Cash", StringComparison.OrdinalIgnoreCase) ||
            name.Contains("Stock", StringComparison.OrdinalIgnoreCase) ||
            name.Contains("Sundry Debtors", StringComparison.OrdinalIgnoreCase) ||
            name.Contains("Loan & Advances", StringComparison.OrdinalIgnoreCase);

        private static bool IsLiabilityAccount(string name) =>
            name.Contains("Loan", StringComparison.OrdinalIgnoreCase) ||
            name.Contains("Sundry Creditors", StringComparison.OrdinalIgnoreCase) ||
            name.Contains("Creditors", StringComparison.OrdinalIgnoreCase) ||
            name.Contains("Outstanding", StringComparison.OrdinalIgnoreCase);

        private static bool IsEquityAccount(string name) =>
            name.Contains("Capital", StringComparison.OrdinalIgnoreCase) ||
            name.Contains("Reserves", StringComparison.OrdinalIgnoreCase) ||
            name.Contains("Retained Earnings", StringComparison.OrdinalIgnoreCase);
    }
}
