using BetterAccounting.Core.Data.Models;
using System.Collections.Generic;

namespace BetterAccounting.Core.Services.Reports
{
    public static class VoucherPrintFormatter
    {
        public static IEnumerable<(string Label, string Value)> BuildFields(LedgerEntry entry, CompanyProfile? company)
        {
            yield return ("Company", company?.CompanyName ?? "");
            yield return ("GSTIN", company?.Gstin ?? "");
            yield return ("Voucher Type", entry.VoucherType.ToString());
            yield return ("Voucher No", entry.VoucherNo);
            yield return ("Date", entry.Date.ToShortDateString());
            yield return ("Account", entry.AccountName);
            yield return ("Debit/Credit", entry.Type.ToString());
            yield return ("Amount", entry.Amount.ToString("C"));
            yield return ("Narration", entry.Description);
            yield return ("Transporter", entry.Transporter);
        }
    }
}