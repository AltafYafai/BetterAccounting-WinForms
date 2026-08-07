using System.Collections.Generic;

namespace BetterAccounting.Core.Data.Models
{
    public class Ledger
    {
        public string Name { get; set; }
        public decimal OpeningBalance { get; set; }
        public List<LedgerEntry> Entries { get; set; } = new();
    }
}
