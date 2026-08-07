using BetterAccounting.Core.Data.Models;
using BetterAccounting.Core.Services.Reports;
using System.Windows;
using System.Windows.Documents;

namespace BetterAccounting.UI.Models
{
    public static class VoucherDocumentBuilder
    {
        public static FlowDocument Build(LedgerEntry entry, CompanyProfile? company, string copyLabel)
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
    }
}