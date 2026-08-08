using BetterAccounting.Core.Data.Models;
using BetterAccounting.Core.Services.Reports;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Documents;

namespace BetterAccounting.UI.Models
{
    public static class VoucherDocumentBuilder
    {
        public static FlowDocument Build(LedgerEntry entry, CompanyProfile? company, string copyLabel,
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

        public static FlowDocument BuildDefault(LedgerEntry entry, CompanyProfile? company, string copyLabel)
        {
            var doc = new FlowDocument
            {
                PagePadding = new Thickness(40),
                FontFamily = new System.Windows.Media.FontFamily("Segoe UI"),
                FontSize = 12
            };

            doc.Blocks.Add(new Paragraph(new Run(copyLabel))
            {
                FontWeight = FontWeights.Bold,
                FontSize = 14,
                TextAlignment = TextAlignment.Right,
                Margin = new Thickness(0, 0, 0, 8)
            });

            var companyPara = new Paragraph
            {
                TextAlignment = TextAlignment.Center,
                Margin = new Thickness(0, 0, 0, 6)
            };
            companyPara.Inlines.Add(new Run(company?.CompanyName ?? "BetterAccounting")
            {
                FontSize = 18,
                FontWeight = FontWeights.Bold
            });
            doc.Blocks.Add(companyPara);

            if (!string.IsNullOrEmpty(company?.Gstin))
            {
                doc.Blocks.Add(new Paragraph(new Run($"GSTIN: {company.Gstin}"))
                {
                    TextAlignment = TextAlignment.Center,
                    Margin = new Thickness(0, 0, 0, 12)
                });
            }

            foreach (var (label, value) in VoucherPrintFormatter.BuildFields(entry, company))
            {
                if (label == "Company" || label == "GSTIN")
                    continue;

                var para = new Paragraph { Margin = new Thickness(0, 1, 0, 1) };
                para.Inlines.Add(new Run($"{label}: ") { FontWeight = FontWeights.Bold });
                para.Inlines.Add(new Run(value));
                doc.Blocks.Add(para);
            }

            return doc;
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