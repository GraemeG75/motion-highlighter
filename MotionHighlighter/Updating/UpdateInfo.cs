namespace MotionHighlighter.Updating
{
    /// <summary>
    /// Encapsulates version details and installer assets for a GitHub release.
    /// </summary>
    public class UpdateInfo
    {
        /// <summary>
        /// Gets the raw GitHub release tag (e.g. "v1.1.0").
        /// </summary>
        public string TagName { get; }

        /// <summary>
        /// Gets the parsed semantic <see cref="Version"/>.
        /// </summary>
        public Version Version { get; }

        /// <summary>
        /// Gets the release title or name.
        /// </summary>
        public string Title { get; }

        /// <summary>
        /// Gets the release changelog / body text.
        /// </summary>
        public string ReleaseNotes { get; }

        /// <summary>
        /// Gets the UTC publication timestamp, if available.
        /// </summary>
        public DateTimeOffset? PublishedAt { get; }

        /// <summary>
        /// Gets the HTML web page URL for this release on GitHub.
        /// </summary>
        public string ReleasePageUrl { get; }

        /// <summary>
        /// Gets a value indicating whether this release version is strictly newer than the currently executing version.
        /// </summary>
        public bool IsNewer { get; }

        /// <summary>
        /// Gets the primary executable installer asset, if present.
        /// </summary>
        public GitHubReleaseAsset? InstallerAsset { get; }

        /// <summary>
        /// Gets all assets attached to this release.
        /// </summary>
        public IReadOnlyList<GitHubReleaseAsset> Assets { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="UpdateInfo"/> class.
        /// </summary>
        /// <param name="tagName">GitHub tag name.</param>
        /// <param name="version">Parsed semantic version.</param>
        /// <param name="title">Release title.</param>
        /// <param name="releaseNotes">Markdown/text release notes.</param>
        /// <param name="publishedAt">Publication date.</param>
        /// <param name="releasePageUrl">Release URL on GitHub.</param>
        /// <param name="isNewer">Whether release is newer than current app.</param>
        /// <param name="assets">List of attached binary assets.</param>
        /// <param name="installerAsset">Designated installer asset.</param>
        public UpdateInfo(
            string tagName,
            Version version,
            string title,
            string releaseNotes,
            DateTimeOffset? publishedAt,
            string releasePageUrl,
            bool isNewer,
            IEnumerable<GitHubReleaseAsset>? assets = null,
            GitHubReleaseAsset? installerAsset = null)
        {
            TagName = tagName ?? string.Empty;
            Version = version ?? new Version(1, 0, 0);
            Title = title ?? string.Empty;
            ReleaseNotes = releaseNotes ?? string.Empty;
            PublishedAt = publishedAt;
            ReleasePageUrl = releasePageUrl ?? string.Empty;
            IsNewer = isNewer;

            List<GitHubReleaseAsset> list = assets != null ? new List<GitHubReleaseAsset>(assets) : new List<GitHubReleaseAsset>();
            Assets = list.AsReadOnly();
            InstallerAsset = installerAsset ?? list.FirstOrDefault(a => a.Name.EndsWith(".exe", StringComparison.OrdinalIgnoreCase));
        }
    }
}
