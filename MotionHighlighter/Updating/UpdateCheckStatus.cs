namespace MotionHighlighter.Updating
{
    /// <summary>
    /// Represents the status resulting from an update check attempt.
    /// </summary>
    public enum UpdateCheckStatus
    {
        /// <summary>
        /// An update check is actively in progress.
        /// </summary>
        Checking,

        /// <summary>
        /// The currently running application version is up to date.
        /// </summary>
        UpToDate,

        /// <summary>
        /// A newer release version is available on GitHub.
        /// </summary>
        UpdateAvailable,

        /// <summary>
        /// The update check encountered a network, parsing, or server error.
        /// </summary>
        Error
    }
}
