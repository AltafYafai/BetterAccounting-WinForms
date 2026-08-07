using System;

namespace BetterAccounting.Core.Data.Models
{
    public enum EntryType { Debit, Credit }

    public enum VoucherType { Cash, Bank, Journal, Receipt, Payment, Contra, DebitNote, CreditNote }

    public class LedgerEntry
    {
        public long Id { get; set; }
        public DateTime Date { get; set; }
        public string VoucherNo { get; set; } = string.Empty;
        public string AccountName { get; set; } = string.Empty;
        public EntryType Type { get; set; }
        public decimal Amount { get; set; }
        public string Description { get; set; } = string.Empty;
        public string ReferenceVoucherId { get; set; } = string.Empty;
        public VoucherType VoucherType { get; set; } = VoucherType.Journal;
        public string Transporter { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
