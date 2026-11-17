namespace MotionHighlighter.Updating
{
    /// <summary>
    /// Represents a binary asset attached to a GitHub release (such as an installer executable).
    /// </summary>
    public class GitHubReleaseAsset
    {
        /// <summary>
        /// Gets the file name of the release asset (e.g. "MotionHighlighter-Setup.exe").
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Gets the direct HTTP browser download URL for the asset.
        /// </summary>
        public string DownloadUrl { get; }

        /// <summary>
        /// Gets the size of the asset file in bytes.
        /// </summary>
        public long Size { get; }

        /// <summary>
        /// Gets the MIME content type reported by GitHub.
        /// </summary>
        public string ContentType { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="GitHubReleaseAsset"/> class.
        /// </summary>
        /// <param name="name">Asset file name.</param>
        /// <param name="downloadUrl">Download URL.</param>
        /// <param name="size">File size in bytes.</param>
        /// <param name="contentType">MIME content type.</param>
        public GitHubReleaseAsset(string name, string downloadUrl, long size, string contentType)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            DownloadUrl = downloadUrl ?? throw new ArgumentNullException(nameof(downloadUrl));
            Size = size;
            ContentType = contentType ?? "application/octet-stream";
        }
    }
}
