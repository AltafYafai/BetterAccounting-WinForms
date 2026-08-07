using BetterAccounting.Core.Data.Models;

namespace BetterAccounting.Core.Services.Data
{
    public enum AccountGroup
    {
        Assets,
        Liabilities,
        Equity,
        Income,
        Expenses
    }

    public class Account
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public AccountGroup Group { get; set; }
        public string? Description { get; set; }
        public decimal OpeningBalance { get; set; }
        public EntryType OpeningBalanceType { get; set; } = EntryType.Debit;
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
