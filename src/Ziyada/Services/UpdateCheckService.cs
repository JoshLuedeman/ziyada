using System.Net.Http.Json;
using System.Text.Json.Serialization;
using Ziyada.Helpers;
using Ziyada.Models;

namespace Ziyada.Services;

public class UpdateCheckService
{
    private const string GitHubApiUrl = "https://api.github.com/repos/JoshLuedeman/ziyada/releases/latest";
    private static readonly HttpClient _httpClient = new()
    {
        Timeout = TimeSpan.FromSeconds(5)
    };

    static UpdateCheckService()
    {
        _httpClient.DefaultRequestHeaders.Add("User-Agent", "Ziyada");
    }

    public async Task<UpdateInfo> CheckForUpdatesAsync()
    {
        var logger = LoggingService.Instance;
        
        try
        {
            logger.LogInfo("Checking for updates from GitHub");

            var response = await _httpClient.GetAsync(GitHubApiUrl);
            
            if (!response.IsSuccessStatusCode)
            {
                logger.LogWarning($"Update check failed with status code: {response.StatusCode}");
                return new UpdateInfo { IsUpdateAvailable = false };
            }

            var release = await response.Content.ReadFromJsonAsync<GitHubRelease>();
            
            if (release == null || string.IsNullOrEmpty(release.TagName))
            {
                logger.LogWarning("Update check returned no release information");
                return new UpdateInfo { IsUpdateAvailable = false };
            }

            var latestVersion = release.TagName.TrimStart('v');
            var currentVersion = AppVersion.Version;

            logger.LogInfo($"Current version: {currentVersion}, Latest version: {latestVersion}");

            var isNewer = IsNewerVersion(currentVersion, latestVersion);

            if (isNewer)
            {
                logger.LogInfo($"Update available: {latestVersion}");
            }

            return new UpdateInfo
            {
                IsUpdateAvailable = isNewer,
                LatestVersion = latestVersion,
                DownloadUrl = release.HtmlUrl ?? string.Empty,
                ReleaseNotesUrl = release.HtmlUrl ?? string.Empty
            };
        }
        catch (TaskCanceledException)
        {
            logger.LogWarning("Update check timed out");
            return new UpdateInfo { IsUpdateAvailable = false };
        }
        catch (HttpRequestException ex)
        {
            logger.LogWarning($"Update check failed due to network error: {ex.Message}");
            return new UpdateInfo { IsUpdateAvailable = false };
        }
        catch (Exception ex)
        {
            logger.LogError("Unexpected error during update check", exception: ex);
            return new UpdateInfo { IsUpdateAvailable = false };
        }
    }

    private static bool IsNewerVersion(string currentVersion, string latestVersion)
    {
        try
        {
            var current = Version.Parse(currentVersion);
            var latest = Version.Parse(latestVersion);
            return latest > current;
        }
        catch
        {
            // If parsing fails, assume no update available
            return false;
        }
    }

    private class GitHubRelease
    {
        [JsonPropertyName("tag_name")]
        public string? TagName { get; set; }

        [JsonPropertyName("html_url")]
        public string? HtmlUrl { get; set; }
    }
}
