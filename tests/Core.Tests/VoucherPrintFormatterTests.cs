using BetterAccounting.Core.Data.Models;
using BetterAccounting.Core.Services.Reports;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;

namespace BetterAccounting.Core.Tests
{
    [TestClass]
    public class VoucherPrintFormatterTests
    {
        private static LedgerEntry CreateEntry() => new()
        {
            Date = new DateTime(2026, 8, 7),
            VoucherNo = "V-100",
            AccountName = "Cash",
            Type = EntryType.Debit,
            Amount = 500.25m,
            Description = "Test narration",
            VoucherType = VoucherType.DebitNote,
            Transporter = "Transporter Co."
        };

        [TestMethod]
        public void BuildFields_ShouldIncludeCompanyNameAndGstin()
        {
            var company = new CompanyProfile { CompanyName = "Acme Pvt Ltd", Gstin = "27AAACS1234A1Z5" };
            var fields = VoucherPrintFormatter.BuildFields(CreateEntry(), company).ToList();

            Assert.AreEqual("Acme Pvt Ltd", fields.First(f => f.Label == "Company").Value);
            Assert.AreEqual("27AAACS1234A1Z5", fields.First(f => f.Label == "GSTIN").Value);
        }

        [TestMethod]
        public void BuildFields_ShouldIncludeVoucherDetails()
        {
            var fields = VoucherPrintFormatter.BuildFields(CreateEntry(), null).ToList();

            Assert.AreEqual("DebitNote", fields.First(f => f.Label == "Voucher Type").Value);
            Assert.AreEqual("V-100", fields.First(f => f.Label == "Voucher No").Value);
            Assert.AreEqual("Cash", fields.First(f => f.Label == "Account").Value);
            Assert.AreEqual("Debit", fields.First(f => f.Label == "Debit/Credit").Value);
            Assert.AreEqual("Test narration", fields.First(f => f.Label == "Narration").Value);
        }

        [TestMethod]
        public void BuildFields_ShouldIncludeTransporter()
        {
            var fields = VoucherPrintFormatter.BuildFields(CreateEntry(), null).ToList();
            Assert.AreEqual("Transporter Co.", fields.First(f => f.Label == "Transporter").Value);
        }

        [TestMethod]
        public void BuildFields_ShouldFormatAmount()
        {
            var fields = VoucherPrintFormatter.BuildFields(CreateEntry(), null).ToList();
            var amount = fields.First(f => f.Label == "Amount").Value;
            Assert.IsTrue(amount.Contains("500"));
        }

        [TestMethod]
        public void BuildFields_WithNoCompany_ShouldUseEmptyStrings()
        {
            var fields = VoucherPrintFormatter.BuildFields(CreateEntry(), null).ToList();
            Assert.AreEqual(string.Empty, fields.First(f => f.Label == "Company").Value);
            Assert.AreEqual(string.Empty, fields.First(f => f.Label == "GSTIN").Value);
        }
    }
}