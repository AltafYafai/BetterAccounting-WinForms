using BetterAccounting.Core.Data.Models;
using BetterAccounting.Core.Services.Reports;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Linq;

namespace BetterAccounting.Core.Tests
{
    [TestClass]
    public class PrintTemplateServiceTests
    {
        [TestMethod]
        public void Render_ShouldSubstituteTokens()
        {
            var content = "Company: {CompanyName}\nGSTIN: {Gstin}";
            var fields = new Dictionary<string, string>
            {
                { "CompanyName", "Acme Pvt Ltd" },
                { "Gstin", "27AAACS1234A1Z5" }
            };

            var rendered = PrintTemplateService.Render(content, fields);

            Assert.AreEqual("Company: Acme Pvt Ltd", rendered[0]);
            Assert.AreEqual("GSTIN: 27AAACS1234A1Z5", rendered[1]);
        }

        [TestMethod]
        public void Render_WithMissingToken_ShouldRenderEmpty()
        {
            var rendered = PrintTemplateService.Render("Amount: {Amount}", new Dictionary<string, string>());
            Assert.AreEqual("Amount: ", rendered[0]);
        }

        [TestMethod]
        public void Render_WithLiteralText_ShouldPreserveText()
        {
            var rendered = PrintTemplateService.Render("Invoice Copy", new Dictionary<string, string>());
            Assert.AreEqual("Invoice Copy", rendered[0]);
        }

        [TestMethod]
        public void Render_WithNullContent_ShouldReturnEmpty()
        {
            var rendered = PrintTemplateService.Render(null, new Dictionary<string, string>());
            Assert.AreEqual(0, rendered.Length);
        }

        [TestMethod]
        public void GetTokens_ShouldReturnInvoiceTokens()
        {
            var tokens = PrintTemplateService.GetTokens(DocumentType.Invoice);
            var keys = tokens.Select(t => t.Token).ToArray();
            CollectionAssert.Contains(keys, "CompanyName");
            CollectionAssert.Contains(keys, "VoucherNo");
            CollectionAssert.Contains(keys, "Transporter");
        }

        [TestMethod]
        public void GetDefaultContent_ShouldNotBeEmptyForAllTypes()
        {
            foreach (var type in new[] { DocumentType.Invoice, DocumentType.Ledger, DocumentType.Cover, DocumentType.Report })
            {
                Assert.IsFalse(string.IsNullOrWhiteSpace(PrintTemplateService.GetDefaultContent(type)));
            }
        }

        [TestMethod]
        public void Render_ShouldAcceptTokenSurroundedByText()
        {
            var rendered = PrintTemplateService.Render(
                "Voucher #{VoucherNo} dated {Date}",
                new Dictionary<string, string> { { "VoucherNo", "100" }, { "Date", "07" } });
            Assert.AreEqual("Voucher #100 dated 07", rendered[0]);
        }
    }
}