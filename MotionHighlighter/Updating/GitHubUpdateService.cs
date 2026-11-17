using System.Diagnostics;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace MotionHighlighter.Updating
{
    /// <summary>
    /// Communicates with the GitHub Releases API to discover new versions, download installer assets,
    /// and launch installation packages.
    /// </summary>
    public class GitHubUpdateService : IUpdateService, IDisposable
    {
        private static readonly Regex _versionRegex = new(@"(\d+)\.(\d+)(?:\.(\d+))?(?:\.(\d+))?");

        private readonly HttpClient _httpClient;
        private readonly bool _disposeClient;
        private bool _disposed;

        /// <summary>
        /// Gets the GitHub repository owner username.
        /// </summary>
        public string Owner { get; }

        /// <summary>
        /// Gets the GitHub repository name.
        /// </summary>
        public string Repository { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="GitHubUpdateService"/> class using default network settings.
        /// </summary>
        /// <param name="owner">GitHub repository owner (defaults to 'GraemeG75').</param>
        /// <param name="repository">GitHub repository name (defaults to 'motion-highlighter').</param>
        public GitHubUpdateService(string owner = "GraemeG75", string repository = "motion-highlighter")
        {
            Owner = string.IsNullOrWhiteSpace(owner) ? "GraemeG75" : owner;
            Repository = string.IsNullOrWhiteSpace(repository) ? "motion-highlighter" : repository;

            _httpClient = new HttpClient();
            _httpClient.DefaultRequestHeaders.UserAgent.Add(
                new ProductInfoHeaderValue("MotionHighlighter-App", AppVersion.VersionString));
            _httpClient.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/vnd.github.v3+json"));

            _disposeClient = true;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GitHubUpdateService"/> class with a custom <see cref="HttpClient"/> (used for unit testing).
        /// </summary>
        /// <param name="httpClient">Custom HTTP client.</param>
        /// <param name="owner">GitHub repository owner.</param>
        /// <param name="repository">GitHub repository name.</param>
        public GitHubUpdateService(HttpClient httpClient, string owner = "GraemeG75", string repository = "motion-highlighter")
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            Owner = string.IsNullOrWhiteSpace(owner) ? "GraemeG75" : owner;
            Repository = string.IsNullOrWhiteSpace(repository) ? "motion-highlighter" : repository;
            _disposeClient = false;
        }

        /// <inheritdoc />
        public async Task<UpdateInfo?> CheckForUpdatesAsync(CancellationToken cancellationToken = default)
        {
            string url = $"https://api.github.com/repos/{Owner}/{Repository}/releases/latest";

            try
            {
                using HttpResponseMessage response = await _httpClient.GetAsync(url, cancellationToken).ConfigureAwait(false);
                if (!response.IsSuccessStatusCode)
                {
                    return null;
                }

                string json = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
                return ParseReleaseJson(json);
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <summary>
        /// Parses a GitHub Release JSON payload into an <see cref="UpdateInfo"/> object.
        /// </summary>
        /// <param name="json">GitHub Release API JSON string.</param>
        /// <returns>Parsed update metadata, or <c>null</c> if invalid.</returns>
        public UpdateInfo? ParseReleaseJson(string json)
        {
            if (string.IsNullOrWhiteSpace(json))
            {
                return null;
            }

            try
            {
                using JsonDocument doc = JsonDocument.Parse(json);
                JsonElement root = doc.RootElement;

                string tagName = root.TryGetProperty("tag_name", out JsonElement tagElem) ? tagElem.GetString() ?? string.Empty : string.Empty;
                string title = root.TryGetProperty("name", out JsonElement nameElem) ? nameElem.GetString() ?? tagName : tagName;
                string body = root.TryGetProperty("body", out JsonElement bodyElem) ? bodyElem.GetString() ?? string.Empty : string.Empty;
                string htmlUrl = root.TryGetProperty("html_url", out JsonElement htmlElem) ? htmlElem.GetString() ?? string.Empty : string.Empty;

                DateTimeOffset? publishedAt = null;
                if (root.TryGetProperty("published_at", out JsonElement pubElem) && pubElem.TryGetDateTimeOffset(out DateTimeOffset pubVal))
                {
                    publishedAt = pubVal;
                }

                Version parsedVersion = ParseSemanticVersion(tagName);
                bool isNewer = parsedVersion > AppVersion.Current;

                List<GitHubReleaseAsset> assets = new();
                if (root.TryGetProperty("assets", out JsonElement assetsElem) && assetsElem.ValueKind == JsonValueKind.Array)
                {
                    foreach (JsonElement asset in assetsElem.EnumerateArray())
                    {
                        string assetName = asset.TryGetProperty("name", out JsonElement aName) ? aName.GetString() ?? string.Empty : string.Empty;
                        string downloadUrl = asset.TryGetProperty("browser_download_url", out JsonElement aUrl) ? aUrl.GetString() ?? string.Empty : string.Empty;
                        long size = asset.TryGetProperty("size", out JsonElement aSize) ? aSize.GetInt64() : 0;
                        string contentType = asset.TryGetProperty("content_type", out JsonElement aType) ? aType.GetString() ?? string.Empty : string.Empty;

                        if (!string.IsNullOrWhiteSpace(assetName) && !string.IsNullOrWhiteSpace(downloadUrl))
                        {
                            assets.Add(new GitHubReleaseAsset(assetName, downloadUrl, size, contentType));
                        }
                    }
                }

                GitHubReleaseAsset? installerAsset = SelectBestInstallerAsset(assets);

                return new UpdateInfo(tagName, parsedVersion, title, body, publishedAt, htmlUrl, isNewer, assets, installerAsset);
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <summary>
        /// Selects the most suitable installer binary asset from the list of release assets.
        /// </summary>
        /// <param name="assets">List of available release assets.</param>
        /// <returns>Selected installer asset, or <c>null</c> if none found.</returns>
        private static GitHubReleaseAsset? SelectBestInstallerAsset(IReadOnlyList<GitHubReleaseAsset> assets)
        {
            if (assets == null || assets.Count == 0)
            {
                return null;
            }

            // Priority 1: Specifically named setup or installer exe
            GitHubReleaseAsset? setupExe = assets.FirstOrDefault(a =>
                a.Name.EndsWith(".exe", StringComparison.OrdinalIgnoreCase) &&
                (a.Name.Contains("setup", StringComparison.OrdinalIgnoreCase) || a.Name.Contains("installer", StringComparison.OrdinalIgnoreCase)));

            if (setupExe != null)
            {
                return setupExe;
            }

            // Priority 2: Any .exe file
            GitHubReleaseAsset? anyExe = assets.FirstOrDefault(a => a.Name.EndsWith(".exe", StringComparison.OrdinalIgnoreCase));
            if (anyExe != null)
            {
                return anyExe;
            }

            // Priority 3: Any .msi installer
            GitHubReleaseAsset? anyMsi = assets.FirstOrDefault(a => a.Name.EndsWith(".msi", StringComparison.OrdinalIgnoreCase));
            if (anyMsi != null)
            {
                return anyMsi;
            }

            // Priority 4: Zip archive
            return assets.FirstOrDefault(a => a.Name.EndsWith(".zip", StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// Extracts and normalizes a semantic <see cref="Version"/> from a tag string (e.g. "v1.2.3" -> 1.2.3).
        /// </summary>
        /// <param name="tag">Tag string.</param>
        /// <returns>Normalized version instance.</returns>
        public static Version ParseSemanticVersion(string tag)
        {
            if (string.IsNullOrWhiteSpace(tag))
            {
                return new Version(1, 0, 0);
            }

            Match match = _versionRegex.Match(tag);
            if (match.Success)
            {
                int major = int.Parse(match.Groups[1].Value);
                int minor = int.Parse(match.Groups[2].Value);

                if (match.Groups[4].Success)
                {
                    int build = match.Groups[3].Success ? int.Parse(match.Groups[3].Value) : 0;
                    int revision = int.Parse(match.Groups[4].Value);
                    return new Version(major, minor, build, revision);
                }

                if (match.Groups[3].Success)
                {
                    int build = int.Parse(match.Groups[3].Value);
                    return new Version(major, minor, build);
                }

                return new Version(major, minor);
            }

            return new Version(1, 0, 0);
        }

        /// <inheritdoc />
        public async Task<string> DownloadInstallerAsync(UpdateInfo update, IProgress<double>? progress = null, CancellationToken cancellationToken = default)
        {
            if (update == null)
            {
                throw new ArgumentNullException(nameof(update));
            }

            if (update.InstallerAsset == null)
            {
                throw new InvalidOperationException("No downloadable installer asset is associated with this update.");
            }

            string tempDir = Path.Combine(Path.GetTempPath(), "MotionHighlighterUpdate");
            if (!Directory.Exists(tempDir))
            {
                Directory.CreateDirectory(tempDir);
            }

            string destinationFilePath = Path.Combine(tempDir, update.InstallerAsset.Name);

            using HttpResponseMessage response = await _httpClient.GetAsync(
                update.InstallerAsset.DownloadUrl,
                HttpCompletionOption.ResponseHeadersRead,
                cancellationToken).ConfigureAwait(false);

            response.EnsureSuccessStatusCode();

            long? totalBytes = response.Content.Headers.ContentLength;
            if (!totalBytes.HasValue || totalBytes.Value <= 0)
            {
                totalBytes = update.InstallerAsset.Size > 0 ? update.InstallerAsset.Size : null;
            }

            using Stream contentStream = await response.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);
            using FileStream fileStream = new(destinationFilePath, FileMode.Create, FileAccess.Write, FileShare.None, 81920, true);

            byte[] buffer = new byte[81920];
            long totalRead = 0;
            int bytesRead;

            while ((bytesRead = await contentStream.ReadAsync(buffer, 0, buffer.Length, cancellationToken).ConfigureAwait(false)) > 0)
            {
                await fileStream.WriteAsync(buffer, 0, bytesRead, cancellationToken).ConfigureAwait(false);
                totalRead += bytesRead;

                if (totalBytes.HasValue && totalBytes.Value > 0)
                {
                    double fraction = (double)totalRead / totalBytes.Value;
                    progress?.Report(Math.Min(1.0, fraction));
                }
            }

            progress?.Report(1.0);
            return destinationFilePath;
        }

        /// <inheritdoc />
        public bool LaunchInstaller(string installerFilePath, bool silent = false, bool exitCurrentApp = true)
        {
            if (string.IsNullOrWhiteSpace(installerFilePath) || !File.Exists(installerFilePath))
            {
                return false;
            }

            try
            {
                ProcessStartInfo startInfo = new()
                {
                    FileName = installerFilePath,
                    UseShellExecute = true
                };

                if (silent)
                {
                    startInfo.Arguments = "/SILENT /NORESTART";
                }

                Process? process = Process.Start(startInfo);
                if (process != null && exitCurrentApp)
                {
                    // Delay slightly to give the installer process time to spin up
                    Task.Delay(500).ContinueWith(_ =>
                    {
                        try
                        {
                            Environment.Exit(0);
                        }
                        catch
                        {
                            // Ignore exit exceptions
                        }
                    });
                }

                return process != null;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// Disposes internal HTTP resources if owned by this service.
        /// </summary>
        public void Dispose()
        {
            if (!_disposed)
            {
                if (_disposeClient)
                {
                    _httpClient.Dispose();
                }

                _disposed = true;
                GC.SuppressFinalize(this);
            }
        }
    }
}
