using BetterAccounting.Core.Data.Models;
using BetterAccounting.Core.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace BetterAccounting.Core.Tests
{
    [TestClass]
    public class UpdateServiceTests
    {
        private sealed class FakeHandler : HttpMessageHandler
        {
            private readonly (HttpStatusCode, string)[] _responses;
            public FakeHandler(params (HttpStatusCode Status, string Body)[] responses) => _responses = responses;

            public int CallCount { get; private set; }

            protected override async Task<HttpResponseMessage> SendAsync(
                HttpRequestMessage request, CancellationToken cancellationToken)
            {
                var (status, body) = _responses[Math.Min(CallCount, _responses.Length - 1)];
                CallCount++;
                return await Task.FromResult(new HttpResponseMessage(status)
                {
                    Content = new StringContent(body, Encoding.UTF8, "application/json")
                });
            }
        }

        private const string LatestReleaseJson = @"{
            ""tag_name"": ""v1.0.1"",
            ""html_url"": ""https://github.com/AltafYafai/BetterAccounting-WinForms/releases/tag/v1.0.1"",
            ""body"": ""Bug fixes"",
            ""assets"": [
                { ""name"": ""BetterAccounting.exe"", ""browser_download_url"": ""https://example.test/BetterAccounting.exe"" },
                { ""name"": ""checksum.txt"", ""browser_download_url"": ""https://example.test/checksum.txt"" }
            ]
        }";

        private static UpdateService CreateService(FakeHandler handler)
        {
            return new UpdateService(new HttpClient(handler), "AltafYafai", "BetterAccounting-WinForms", "https://example.test");
        }

        [TestMethod]
        public async Task CheckAsync_WhenNewerReleaseExists_ShouldFlagUpdateAvailable()
        {
            var service = CreateService(new FakeHandler((HttpStatusCode.OK, LatestReleaseJson)));
            var result = await service.CheckAsync("1.0.0");

            Assert.IsTrue(result.IsUpdateAvailable);
            Assert.AreEqual("1.0.1", result.LatestVersion);
            Assert.AreEqual("https://example.test/BetterAccounting.exe", result.DownloadUrl);
        }

        [TestMethod]
        public async Task CheckAsync_WhenSameVersion_ShouldNotFlagUpdate()
        {
            var service = CreateService(new FakeHandler((HttpStatusCode.OK, LatestReleaseJson)));
            var result = await service.CheckAsync("1.0.1");

            Assert.IsFalse(result.IsUpdateAvailable);
        }

        [TestMethod]
        public async Task CheckAsync_OnHttpError_ShouldSetErrorMessage()
        {
            var service = CreateService(new FakeHandler((HttpStatusCode.Forbidden, "")));
            var result = await service.CheckAsync("1.0.0");

            Assert.IsFalse(result.IsUpdateAvailable);
            Assert.IsFalse(string.IsNullOrEmpty(result.ErrorMessage));
        }

        [TestMethod]
        public async Task CheckAsync_WhenNoExeAsset_ShouldLeaveDownloadUrlNull()
        {
            var noExe = @"{
                ""tag_name"": ""v1.0.1"",
                ""html_url"": ""https://example.test/r"",
                ""assets"": [ { ""name"": ""readme.txt"", ""browser_download_url"": ""https://example.test/readme.txt"" } ]
            }";

            var service = CreateService(new FakeHandler((HttpStatusCode.OK, noExe)));
            var result = await service.CheckAsync("1.0.0");

            Assert.IsTrue(result.IsUpdateAvailable);
            Assert.IsTrue(string.IsNullOrEmpty(result.DownloadUrl));
        }

        [TestMethod]
        public async Task DownloadAsync_ShouldWriteFileContent()
        {
            var handler = new FakeHandler((HttpStatusCode.OK, "binary-data"));
            var service = CreateService(handler);
            var info = new UpdateInfo
            {
                DownloadUrl = "https://example.test/BetterAccounting.exe"
            };
            var target = Path.Combine(Path.GetTempPath(), $"update_{Guid.NewGuid():N}.exe");

            try
            {
                await service.DownloadAsync(info, target);
                Assert.IsTrue(File.Exists(target));
            }
            finally
            {
                if (File.Exists(target))
                    File.Delete(target);
            }
        }
    }
}