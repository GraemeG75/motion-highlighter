using System.Reflection;

namespace MotionHighlighter
{
    /// <summary>
    /// Provides centralized version information for the Motion Highlighter application.
    /// </summary>
    public static class AppVersion
    {
        private static readonly Version _defaultVersion = new(1, 0, 0);

        /// <summary>
        /// Gets the current application <see cref="Version"/>.
        /// </summary>
        public static Version Current
        {
            get
            {
                Version? version = typeof(AppVersion).Assembly.GetName().Version;
                if (version != null)
                {
                    return version;
                }

                return _defaultVersion;
            }
        }

        /// <summary>
        /// Gets the semantic version string in "major.minor.build" format (e.g. "1.0.0").
        /// </summary>
        public static string VersionString
        {
            get
            {
                Version v = Current;
                if (v.Build >= 0)
                {
                    return $"{v.Major}.{v.Minor}.{v.Build}";
                }

                return $"{v.Major}.{v.Minor}.0";
            }
        }

        /// <summary>
        /// Gets the clean version display string containing only the version number without runtime or third-party library details.
        /// </summary>
        public static string DisplayString => $"Version {VersionString}";
    }
}
