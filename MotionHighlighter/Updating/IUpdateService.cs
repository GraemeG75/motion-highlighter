namespace MotionHighlighter.Updating
{
    /// <summary>
    /// Contract for discovering, downloading, and launching software updates.
    /// </summary>
    public interface IUpdateService
    {
        /// <summary>
        /// Checks the configured release repository for the latest published application release.
        /// </summary>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>An <see cref="UpdateInfo"/> instance describing the release, or <c>null</c> if unreachable.</returns>
        Task<UpdateInfo?> CheckForUpdatesAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Downloads the installer asset associated with an update to a temporary directory.
        /// </summary>
        /// <param name="update">Update metadata containing the installer asset.</param>
        /// <param name="progress">Progress callback reporting completion fraction from 0.0 to 1.0.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Local file path to the downloaded installer executable.</returns>
        Task<string> DownloadInstallerAsync(UpdateInfo update, IProgress<double>? progress = null, CancellationToken cancellationToken = default);

        /// <summary>
        /// Spawns the downloaded installer process.
        /// </summary>
        /// <param name="installerFilePath">Path to the installer executable.</param>
        /// <param name="silent">Whether to pass silent unattended execution flags.</param>
        /// <param name="exitCurrentApp">Whether to terminate the current process so files can be updated.</param>
        /// <returns><c>true</c> if the installer process was successfully launched; otherwise, <c>false</c>.</returns>
        bool LaunchInstaller(string installerFilePath, bool silent = false, bool exitCurrentApp = true);
    }
}
