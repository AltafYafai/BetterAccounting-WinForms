using BetterAccounting.Core.Services.Data;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace BetterAccounting.Core.Tests
{
    [TestClass]
    public class GstinLookupServiceTests
    {
        private sealed class FakeHandler : HttpMessageHandler
        {
            private readonly string _json;
            public FakeHandler(string json) => _json = json;

            protected override Task<HttpResponseMessage> SendAsync(
                HttpRequestMessage request, CancellationToken cancellationToken)
            {
                var response = new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(_json, Encoding.UTF8, "application/json")
                };
                return Task.FromResult(response);
            }
        }

        private static GstinLookupService CreateService(string json)
        {
            var handler = new FakeHandler(json);
            return new GstinLookupService(new HttpClient(handler), "https://example.test/getgstin?gstin=");
        }

        [TestMethod]
        public async Task LookupAsync_WithSuccess_ShouldMapCompanyDetails()
        {
            var json = @"{
                ""Success"": true,
                ""Result"": {
                    ""Gstin"": ""27AAACS1234A1Z5"",
                    ""LegalName"": ""ACME PRIVATE LIMITED"",
                    ""TradeName"": ""ACME"",
                    ""PrincipalAddr"": {
                        ""BuildingName"": ""Regus Tower"",
                        ""Street"": ""1 Main Road"",
                        ""Locality"": ""Industrial Area"",
                        ""City"": ""Pune"",
                        ""District"": ""Pune"",
                        ""StateName"": ""Maharashtra"",
                        ""PinCode"": ""411001""
                    }
                }
            }";

            var service = CreateService(json);
            var result = await service.LookupAsync("27AAACS1234A1Z5");

            Assert.AreEqual(string.Empty, result.ErrorMessage);
            Assert.AreEqual("ACME PRIVATE LIMITED", result.LegalName);
            Assert.AreEqual("Maharashtra", result.State);
            Assert.AreEqual("411001", result.PinCode);
            Assert.IsTrue(result.Address.Contains("1 Main Road"));
        }

        [TestMethod]
        public async Task LookupAsync_WithInvalidGstinLength_ShouldReturnError()
        {
            var service = CreateService("{}");
            var result = await service.LookupAsync("123");

            Assert.AreNotEqual(string.Empty, result.ErrorMessage);
            Assert.AreEqual(string.Empty, result.LegalName);
        }

        [TestMethod]
        public async Task LookupAsync_WithNotFound_ShouldReturnErrorMessage()
        {
            var json = @"{ ""Success"": false, ""Errors"": [ { ""Message"": ""Invalid GSTIN"" } ] }";
            var service = CreateService(json);

            var result = await service.LookupAsync("27AAACS1234A1Z5");

            Assert.AreEqual("Invalid GSTIN", result.ErrorMessage);
        }
    }
}