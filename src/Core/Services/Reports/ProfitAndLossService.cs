using BetterAccounting.Core.Data.Models;
using BetterAccounting.Core.Services.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BetterAccounting.Core.Services.Reports
{
    public class ProfitAndLossRecord
    {
        public string AccountName { get; set; }
        public decimal Amount { get; set; }
        public EntryType Type { get; set; }
    }

    public class ProfitAndLossResult
    {
        public List<ProfitAndLossRecord> Incomes { get; set; } = new();
        public List<ProfitAndLossRecord> Expenses { get; set; } = new();
        public decimal TotalIncome => Incomes.Sum(i => i.Amount);
        public decimal TotalExpense => Expenses.Sum(e => e.Amount);
        public decimal NetProfit => TotalIncome - TotalExpense;
        public bool IsProfitable => NetProfit >= 0;
    }

    public class ProfitAndLossService
    {
        private readonly IDataContext _context;
        private readonly IAccountRepository _accountRepository;

        public ProfitAndLossService(IDataContext context, IAccountRepository accountRepository)
        {
            _context = context;
            _accountRepository = accountRepository;
        }

        public async Task<ProfitAndLossResult> GenerateAsync(DateTime from, DateTime to)
        {
            var entries = await _context.GetEntriesByDateRangeAsync(from, to);
            var accounts = await _accountRepository.GetAllAsync();

            var result = new ProfitAndLossResult();

            foreach (var entry in entries)
            {
                var account = accounts.FirstOrDefault(a => a.Name == entry.AccountName);
                if (account == null) continue;

                var record = new ProfitAndLossRecord
                {
                    AccountName = entry.AccountName,
                    Amount = entry.Amount,
                    Type = entry.Type
                };

                if (account.Group == AccountGroup.Income)
                    result.Incomes.Add(record);
                else if (account.Group == AccountGroup.Expenses)
                    result.Expenses.Add(record);
            }

            return result;
        }
    }
}
