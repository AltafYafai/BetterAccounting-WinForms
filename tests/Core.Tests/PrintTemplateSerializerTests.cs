using BetterAccounting.Core.Data.Models;
using BetterAccounting.Core.Services.Reports;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BetterAccounting.Core.Tests
{
    [TestClass]
    public class PrintTemplateSerializerTests
    {
        [TestMethod]
        public void SerializeDeserialize_ShouldRoundTrip()
        {
            var layout = new PrintTemplateLayout();
            layout.Items.Add(new PrintTemplateItem
            {
                Kind = TemplateItemKind.Text,
                X = 10,
                Y = 20,
                Width = 300,
                Height = 40,
                Text = "{CompanyName}",
                FontSize = 18,
                Bold = true,
                Rotation = 45,
                TextColor = "Red",
                TextAlignment = TemplateTextAlignment.Center
            });
            layout.Items.Add(new PrintTemplateItem
            {
                Kind = TemplateItemKind.Rectangle,
                X = 5,
                Y = 5,
                Width = 100,
                Height = 60,
                FillColor = "Yellow",
                BorderColor = "Black",
                BorderThickness = 2
            });

            var serialized = PrintTemplateSerializer.Serialize(layout);
            var restored = PrintTemplateSerializer.TryDeserialize(serialized);

            Assert.IsNotNull(restored);
            Assert.AreEqual(layout.PageWidth, restored.PageWidth);
            Assert.AreEqual(layout.PageHeight, restored.PageHeight);
            Assert.AreEqual(2, restored.Items.Count);

            var text = restored.Items[0];
            Assert.AreEqual(TemplateItemKind.Text, text.Kind);
            Assert.AreEqual(10.0, text.X);
            Assert.AreEqual(20.0, text.Y);
            Assert.AreEqual(300.0, text.Width);
            Assert.AreEqual(40.0, text.Height);
            Assert.AreEqual("{CompanyName}", text.Text);
            Assert.AreEqual(18.0, text.FontSize);
            Assert.IsTrue(text.Bold);
            Assert.AreEqual(45.0, text.Rotation);
            Assert.AreEqual("Red", text.TextColor);
            Assert.AreEqual(TemplateTextAlignment.Center, text.TextAlignment);

            var rect = restored.Items[1];
            Assert.AreEqual(TemplateItemKind.Rectangle, rect.Kind);
            Assert.AreEqual("Yellow", rect.FillColor);
            Assert.AreEqual(2.0, rect.BorderThickness);
        }

        [TestMethod]
        public void TryDeserialize_WithLegacyText_ShouldReturnNull()
        {
            var layout = PrintTemplateSerializer.TryDeserialize("Company: {CompanyName}\nGSTIN: {Gstin}");
            Assert.IsNull(layout);
        }

        [TestMethod]
        public void TryDeserialize_WithNullOrEmpty_ShouldReturnNull()
        {
            Assert.IsNull(PrintTemplateSerializer.TryDeserialize(null));
            Assert.IsNull(PrintTemplateSerializer.TryDeserialize(string.Empty));
        }

        [TestMethod]
        public void IsLayoutTemplate_ShouldDetectLayoutContent()
        {
            var layout = DefaultLayoutFactory.Create(DocumentType.Invoice);
            var serialized = PrintTemplateSerializer.Serialize(layout);
            Assert.IsTrue(PrintTemplateSerializer.IsLayoutTemplate(serialized));
            Assert.IsFalse(PrintTemplateSerializer.IsLayoutTemplate("plain text"));
        }
    }

    [TestClass]
    public class DefaultLayoutFactoryTests
    {
        [TestMethod]
        public void Create_ShouldProduceItemsForAllDocumentTypes()
        {
            foreach (var type in new[] { DocumentType.Invoice, DocumentType.Ledger, DocumentType.Cover, DocumentType.Report })
            {
                var layout = DefaultLayoutFactory.Create(type);
                Assert.IsTrue(layout.Items.Count > 0, $"Expected items for {type}");
                Assert.AreEqual(794.0, layout.PageWidth);
                Assert.AreEqual(1123.0, layout.PageHeight);
            }
        }
    }

    [TestClass]
    public class LegacyTemplateConverterTests
    {
        [TestMethod]
        public void Convert_ShouldCreateTextAndLineItems()
        {
            var content = "@T Company Title\n" +
                          "@C {ReportTitle}\n" +
                          "Name : {CompanyName}\n" +
                          "--------------------------------------------------\n" +
                          "GSTIN : {Gstin}";

            var layout = LegacyTemplateConverter.Convert(content);

            Assert.IsTrue(layout.Items.Count >= 5);
            var title = layout.Items[0];
            Assert.AreEqual(TemplateItemKind.Text, title.Kind);
            Assert.IsTrue(title.Bold);
            Assert.AreEqual(TemplateTextAlignment.Center, title.TextAlignment);
            Assert.AreEqual(18.0, title.FontSize);

            var line = layout.Items[3];
            Assert.AreEqual(TemplateItemKind.Line, line.Kind);
        }

        [TestMethod]
        public void Convert_WithEmptyContent_ShouldReturnEmptyLayout()
        {
            var layout = LegacyTemplateConverter.Convert(string.Empty);
            Assert.AreEqual(0, layout.Items.Count);
        }
    }

    [TestClass]
    public class SubstituteTests
    {
        [TestMethod]
        public void Substitute_ShouldReplaceTokens()
        {
            var result = PrintTemplateService.Substitute(
                "Hello {CompanyName}, #{VoucherNo}",
                new System.Collections.Generic.Dictionary<string, string>
                {
                    { "CompanyName", "Acme" },
                    { "VoucherNo", "42" }
                });
            Assert.AreEqual("Hello Acme, #42", result);
        }

        [TestMethod]
        public void Substitute_WithMissingField_ShouldLeaveEmpty()
        {
            var result = PrintTemplateService.Substitute("{Missing}", new System.Collections.Generic.Dictionary<string, string>());
            Assert.AreEqual(string.Empty, result);
        }
    }
}
