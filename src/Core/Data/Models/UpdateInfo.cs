namespace BetterAccounting.Core.Data.Models
{
    public class UpdateInfo
    {
        public string? LatestVersion { get; set; }
        public string? TagName { get; set; }
        public string? ReleaseUrl { get; set; }
        public string? DownloadUrl { get; set; }
        public string? ReleaseNotes { get; set; }
        public bool IsUpdateAvailable { get; set; }
        public string? ErrorMessage { get; set; }
    }
}