using System;

namespace BetterAccounting.Core.Data.Models
{
    public enum EntryType { Debit, Credit }

    public class LedgerEntry
    {
        public long Id { get; set; }
        public DateTime Date { get; set; }
        public string VoucherNo { get; set; }
        public string AccountName { get; set; }
        public EntryType Type { get; set; }
        public decimal Amount { get; set; }
        public string Description { get; set; }
        public string ReferenceVoucherId { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
