using BetterAccounting.Core.Data.Models;
using BetterAccounting.Core.Services.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace BetterAccounting.Core.Services.Reports
{
    public class TokenDefinition
    {
        public string Token { get; }
        public string Label { get; }
        public TokenDefinition(string token, string label)
        {
            Token = token;
            Label = label;
        }
    }

    public static class PrintTemplateTokens
    {
        private static readonly TokenDefinition[] Company = {
            new("CompanyName", "Company Name"),
            new("Gstin", "GSTIN"),
            new("Address", "Address"),
            new("City", "City"),
            new("State", "State"),
            new("PinCode", "PIN Code"),
            new("Phone", "Phone"),
            new("Email", "Email")
        };

        private static readonly TokenDefinition[] Invoice = Company
            .Concat(new[]
            {
                new TokenDefinition("CopyLabel", "Copy Label"),
                new TokenDefinition("VoucherType", "Voucher Type"),
                new TokenDefinition("VoucherNo", "Voucher No"),
                new TokenDefinition("Date", "Date"),
                new TokenDefinition("Account", "Account"),
                new TokenDefinition("DebitCredit", "Debit/Credit"),
                new TokenDefinition("Amount", "Amount"),
                new TokenDefinition("Narration", "Narration"),
                new TokenDefinition("Transporter", "Transporter"),
                new TokenDefinition("CreatedDate", "Printed On")
            }).ToArray();

        private static readonly TokenDefinition[] Ledger = Company
            .Concat(new[]
            {
                new TokenDefinition("CopyLabel", "Copy Label"),
                new TokenDefinition("AccountName", "Account"),
                new TokenDefinition("OpeningBalance", "Opening Balance"),
                new TokenDefinition("FromDate", "From Date"),
                new TokenDefinition("ToDate", "To Date"),
                new TokenDefinition("ClosingBalance", "Closing Balance"),
                new TokenDefinition("CreatedDate", "Created On")
            }).ToArray();

        private static readonly TokenDefinition[] Cover = Company
            .Concat(new[]
            {
                new TokenDefinition("CopyLabel", "Copy Label"),
                new TokenDefinition("ReportTitle", "Report Title"),
                new TokenDefinition("FromDate", "From Date"),
                new TokenDefinition("ToDate", "To Date"),
                new TokenDefinition("Version", "Version"),
                new TokenDefinition("PrintedBy", "Printed By"),
                new TokenDefinition("CreatedDate", "Printed On")
            }).ToArray();

        private static readonly TokenDefinition[] Report = Company
            .Concat(new[]
            {
                new TokenDefinition("CopyLabel", "Copy Label"),
                new TokenDefinition("ReportTitle", "Report Title"),
                new TokenDefinition("FromDate", "From Date"),
                new TokenDefinition("ToDate", "To Date"),
                new TokenDefinition("CreatedDate", "Printed On")
            }).ToArray();

        public static TokenDefinition[] Get(DocumentType type) => type switch
        {
            DocumentType.Invoice => Invoice,
            DocumentType.Ledger => Ledger,
            DocumentType.Cover => Cover,
            DocumentType.Report => Report,
            _ => Invoice
        };
    }

    public class PrintTemplateService
    {
        private static readonly Regex TokenRegex = new(@"\{([A-Za-z0-9_]+)\}", RegexOptions.Compiled);

        private readonly PrintTemplateRepository _repository;

        public PrintTemplateService(PrintTemplateRepository repository)
        {
            _repository = repository;
        }

        public async Task EnsureDefaultsAsync()
        {
            foreach (var type in Enum.GetValues<DocumentType>())
            {
                var existing = await _repository.GetAllAsync(type);
                if (existing.Count > 0)
                    continue;

                await _repository.AddAsync(new PrintTemplate
                {
                    Name = $"Default {type}",
                    DocumentType = type,
                    Content = GetDefaultContent(type),
                    IsDefault = true
                });
            }
        }

        public async Task<List<PrintTemplate>> GetTemplatesAsync(DocumentType type)
            => await _repository.GetAllAsync(type);

        public async Task<PrintTemplate?> GetDefaultAsync(DocumentType type)
            => await _repository.GetDefaultAsync(type);

        public async Task AddAsync(PrintTemplate template)
            => await _repository.AddAsync(template);

        public async Task UpdateAsync(PrintTemplate template)
        {
            template.UpdatedAt = DateTime.UtcNow;
            await _repository.UpdateAsync(template);
        }

        public async Task DeleteAsync(int id)
            => await _repository.DeleteAsync(id);

        public async Task SetDefaultAsync(int id, DocumentType type)
            => await _repository.SetDefaultAsync(id, type);

        public static TokenDefinition[] GetTokens(DocumentType type)
            => PrintTemplateTokens.Get(type);

        public static string GetDefaultContent(DocumentType type) => type switch
        {
            DocumentType.Invoice =>
                "Company Name : {CompanyName}\n" +
                "GSTIN        : {Gstin}\n" +
                "Address      : {Address}\n" +
                "City         : {City}, {State} - {PinCode}\n" +
                "Phone        : {Phone}\n" +
                "Email        : {Email}\n" +
                "\n--------------------------------------------------\n" +
                "Voucher Type : {VoucherType}\n" +
                "Voucher No   : {VoucherNo}          Date : {Date}\n" +
                "Account      : {Account}\n" +
                "Debit/Credit : {DebitCredit}\n" +
                "Amount       : {Amount}\n" +
                "Narration    : {Narration}\n" +
                "Transporter  : {Transporter}\n" +
                "\n--------------------------------------------------\n" +
                "Printed on   : {CreatedDate}",
            DocumentType.Ledger =>
                "Company Name     : {CompanyName}\n" +
                "GSTIN            : {Gstin}\n" +
                "LEDGER ACCOUNT\n" +
                "Account          : {AccountName}\n" +
                "Opening Balance  : {OpeningBalance}\n" +
                "Period           : {FromDate} - {ToDate}\n" +
                "Closing Balance  : {ClosingBalance}\n" +
                "Printed on       : {CreatedDate}",
            DocumentType.Cover =>
                "@C {ReportTitle}\n" +
                "\nCompany Name : {CompanyName}\n" +
                "GSTIN        : {Gstin}\n" +
                "Address      : {Address}\n" +
                "City         : {City}, {State} - {PinCode}\n" +
                "Phone        : {Phone}\n" +
                "Email        : {Email}\n" +
                "\nPeriod       : {FromDate} - {ToDate}\n" +
                "Version      : {Version}\n" +
                "Printed By   : {PrintedBy}\n" +
                "Printed on   : {CreatedDate}",
            DocumentType.Report =>
                "Company Name : {CompanyName}\n" +
                "GSTIN        : {Gstin}\n" +
                "\n@C {ReportTitle}\n" +
                "Period       : {FromDate} - {ToDate}\n" +
                "\nPrinted on   : {CreatedDate}",
            _ => ""
        };

        public static string Substitute(string text, IReadOnlyDictionary<string, string> fields)
        {
            if (string.IsNullOrEmpty(text))
                return string.Empty;
            return TokenRegex.Replace(text, m => fields.GetValueOrDefault(m.Groups[1].Value, string.Empty));
        }

        public static string[] Render(string content, IReadOnlyDictionary<string, string> fields)
        {
            if (string.IsNullOrEmpty(content))
                return Array.Empty<string>();

            var lines = content.Replace("\r\n", "\n").Split('\n');
            var rendered = new string[lines.Length];
            for (var i = 0; i < lines.Length; i++)
            {
                rendered[i] = TokenRegex.Replace(lines[i], m =>
                    fields.GetValueOrDefault(m.Groups[1].Value, string.Empty));
            }
            return rendered;
        }
    }
}