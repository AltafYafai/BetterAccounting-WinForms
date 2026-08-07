using BetterAccounting.Core.Data.Models;
using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace BetterAccounting.Core.Services
{
    public class UpdateService
    {
        private readonly HttpClient _httpClient;
        private readonly string _releaseUrl;

        public UpdateService(HttpClient httpClient, string owner, string repo, string? apiBase = null)
        {
            _httpClient = httpClient ?? new HttpClient();
            apiBase ??= "https://api.github.com";
            _releaseUrl = $"{apiBase.TrimEnd('/')}/repos/{owner}/{repo}/releases/latest";
        }

        public async Task<UpdateInfo> CheckAsync(string currentVersion)
        {
            var info = new UpdateInfo { LatestVersion = currentVersion };
            try
            {
                var request = new HttpRequestMessage(HttpMethod.Get, _releaseUrl);
                request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/vnd.github+json"));
                request.Headers.UserAgent.ParseAdd("BetterAccounting-Updater");
                request.Headers.Add("X-GitHub-Api-Version", "2022-11-28");

                using var response = await _httpClient.SendAsync(request);
                var json = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    info.ErrorMessage = $"Update check failed (HTTP {(int)response.StatusCode}).";
                    return info;
                }

                var release = JsonSerializer.Deserialize<GitHubRelease>(json);
                if (release == null || string.IsNullOrEmpty(release.TagName))
                {
                    info.ErrorMessage = "Could not read release information.";
                    return info;
                }

                var latest = release.TagName.TrimStart('v');
                info.LatestVersion = latest;
                info.TagName = release.TagName;
                info.ReleaseUrl = release.HtmlUrl;
                info.ReleaseNotes = release.Body;
                info.DownloadUrl = release.Assets?
                    .FirstOrDefault(a => a.Name?.EndsWith(".exe", StringComparison.OrdinalIgnoreCase) == true)?
                    .BrowserDownloadUrl;
                info.IsUpdateAvailable = CompareVersions(latest, currentVersion) > 0;
            }
            catch (Exception ex)
            {
                info.ErrorMessage = $"Update check failed: {ex.Message}";
            }

            return info;
        }

        public async Task<string> DownloadAsync(UpdateInfo info, string targetPath)
        {
            if (string.IsNullOrEmpty(info?.DownloadUrl))
                throw new InvalidOperationException("No download available.");

            var request = new HttpRequestMessage(HttpMethod.Get, info.DownloadUrl);
            request.Headers.UserAgent.ParseAdd("BetterAccounting-Updater");
            using var response = await _httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();

            var directory = Path.GetDirectoryName(targetPath);
            if (!string.IsNullOrEmpty(directory))
                Directory.CreateDirectory(directory);

            var tempPath = targetPath + ".part";
            if (File.Exists(tempPath))
                File.Delete(tempPath);

            try
            {
                await using (var responseStream = await response.Content.ReadAsStreamAsync())
                await using (var file = File.Create(tempPath))
                {
                    await responseStream.CopyToAsync(file);
                }

                var size = new FileInfo(tempPath).Length;
                if (size <= 0)
                    throw new IOException("Downloaded file is empty.");

                if (File.Exists(targetPath))
                    File.Delete(targetPath);
                File.Move(tempPath, targetPath);
            }
            finally
            {
                if (File.Exists(tempPath))
                    File.Delete(tempPath);
            }

            return targetPath;
        }

        private static int CompareVersions(string left, string right)
        {
            var leftParsed = Version.TryParse(left, out var lv);
            var rightParsed = Version.TryParse(right, out var rv);

            if (leftParsed && rightParsed && lv != null && rv != null)
                return lv.CompareTo(rv);
            return string.Compare(left, right, StringComparison.OrdinalIgnoreCase);
        }

        private sealed class GitHubRelease
        {
            [JsonPropertyName("tag_name")]
            public string? TagName { get; set; }

            [JsonPropertyName("html_url")]
            public string? HtmlUrl { get; set; }

            [JsonPropertyName("body")]
            public string? Body { get; set; }

            [JsonPropertyName("assets")]
            public GitHubAsset[]? Assets { get; set; }
        }

        private sealed class GitHubAsset
        {
            [JsonPropertyName("name")]
            public string? Name { get; set; }

            [JsonPropertyName("browser_download_url")]
            public string? BrowserDownloadUrl { get; set; }
        }
    }
}