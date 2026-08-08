using BetterAccounting.Core.Data.Models;
using BetterAccounting.Core.Services.Reports;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BetterAccounting.UI.Models
{
    public static class VoucherDocumentBuilder
    {
        public static PrintDocumentModel Build(LedgerEntry entry, CompanyProfile? company, string copyLabel,
            PrintTemplate? template = null)
        {
            if (template != null && !string.IsNullOrWhiteSpace(template.Content))
            {
                var fields = BuildFields(entry, company);
                var lines = PrintTemplateService.Render(template.Content, fields).ToList();
                lines.Insert(0, "@C " + copyLabel);
                return TemplateDocumentBuilder.Build(lines);
            }

            return BuildDefault(entry, company, copyLabel);
        }

        public static PrintDocumentModel BuildDefault(LedgerEntry entry, CompanyProfile? company, string copyLabel)
        {
            var lines = new List<string>
            {
                "@R " + copyLabel
            };

            var companyName = company?.CompanyName ?? "BetterAccounting";
            lines.Add("@T " + companyName);

            if (!string.IsNullOrEmpty(company?.Gstin))
                lines.Add("@C GSTIN: " + company.Gstin);

            foreach (var (label, value) in VoucherPrintFormatter.BuildFields(entry, company))
            {
                if (label == "Company" || label == "GSTIN")
                    continue;

                lines.Add("@L " + label + ": " + value);
            }

            return TemplateDocumentBuilder.Build(lines);
        }

        public static Dictionary<string, string> BuildFields(LedgerEntry entry, CompanyProfile? company)
            => BuildFieldDictionary(entry, company);

        private static Dictionary<string, string> BuildFieldDictionary(LedgerEntry entry, CompanyProfile? company)
        {
            return new Dictionary<string, string>
            {
                { "CompanyName", company?.CompanyName ?? "" },
                { "Gstin", company?.Gstin ?? "" },
                { "Address", company?.Address ?? "" },
                { "City", company?.City ?? "" },
                { "State", company?.State ?? "" },
                { "PinCode", company?.PinCode ?? "" },
                { "Phone", company?.Phone ?? "" },
                { "Email", company?.Email ?? "" },
                { "VoucherType", entry.VoucherType.ToString() },
                { "VoucherNo", entry.VoucherNo },
                { "Date", entry.Date.ToShortDateString() },
                { "Account", entry.AccountName },
                { "DebitCredit", entry.Type.ToString() },
                { "Amount", entry.Amount.ToString("C") },
                { "Narration", entry.Description ?? "" },
                { "Transporter", entry.Transporter ?? "" },
                { "CreatedDate", DateTime.Now.ToString("g") }
            };
        }
    }
}
