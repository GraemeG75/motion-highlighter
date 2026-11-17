namespace MotionHighlighter.Localization
{
    /// <summary>
    /// Represents metadata describing a supported or dynamically loaded application locale.
    /// </summary>
    public class LocaleInfo
    {
        /// <summary>
        /// Gets the ISO two-letter language code (e.g. "en", "de", "fr", "es").
        /// </summary>
        public string Code { get; }

        /// <summary>
        /// Gets the localized display name of the language (e.g. "English", "German", "French", "Spanish").
        /// </summary>
        public string DisplayName { get; }

        /// <summary>
        /// Gets the native endonym of the language (e.g. "English", "Deutsch", "Français", "Español").
        /// </summary>
        public string NativeName { get; }

        /// <summary>
        /// Gets the flag emoji or icon identifier associated with the locale.
        /// </summary>
        public string FlagEmoji { get; }

        /// <summary>
        /// Gets the strongly-typed <see cref="AppLanguage"/> enumeration value.
        /// </summary>
        public AppLanguage LanguageEnum { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="LocaleInfo"/> class.
        /// </summary>
        /// <param name="code">ISO two-letter language code.</param>
        /// <param name="displayName">English display name.</param>
        /// <param name="nativeName">Native name in the language.</param>
        /// <param name="flagEmoji">Flag emoji string.</param>
        /// <param name="languageEnum">Strongly-typed language enum.</param>
        public LocaleInfo(string code, string displayName, string nativeName, string flagEmoji, AppLanguage languageEnum)
        {
            Code = code ?? throw new ArgumentNullException(nameof(code));
            DisplayName = displayName ?? throw new ArgumentNullException(nameof(displayName));
            NativeName = nativeName ?? throw new ArgumentNullException(nameof(nativeName));
            FlagEmoji = flagEmoji ?? string.Empty;
            LanguageEnum = languageEnum;
        }

        /// <inheritdoc />
        public override string ToString()
        {
            if (string.IsNullOrWhiteSpace(FlagEmoji))
            {
                return $"{NativeName} ({Code})";
            }

            return $"{FlagEmoji} {NativeName}";
        }
    }
}
