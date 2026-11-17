namespace MotionHighlighter.Localization
{
    /// <summary>
    /// Defines operations for querying localized strings and handling locale changes.
    /// </summary>
    public interface ILocalizationProvider
    {
        /// <summary>
        /// Gets the currently active language code (e.g. "en", "de", "fr", "es").
        /// </summary>
        string CurrentLanguageCode { get; }

        /// <summary>
        /// Gets the currently active <see cref="AppLanguage"/>.
        /// </summary>
        AppLanguage CurrentLanguage { get; }

        /// <summary>
        /// Gets the list of all available locales registered in the system.
        /// </summary>
        IReadOnlyList<LocaleInfo> AvailableLocales { get; }

        /// <summary>
        /// Retrieves the localized string for the specified key.
        /// </summary>
        /// <param name="key">The translation key.</param>
        /// <returns>The localized text.</returns>
        string GetString(string key);

        /// <summary>
        /// Retrieves and formats the localized string for the specified key.
        /// </summary>
        /// <param name="key">The translation key.</param>
        /// <param name="args">Format arguments.</param>
        /// <returns>The formatted localized text.</returns>
        string GetString(string key, params object[] args);

        /// <summary>
        /// Sets the active locale by language code.
        /// </summary>
        /// <param name="code">ISO two-letter code.</param>
        /// <returns><c>true</c> if successfully switched; otherwise, <c>false</c>.</returns>
        bool SetLanguage(string code);

        /// <summary>
        /// Sets the active locale by <see cref="AppLanguage"/> enumeration.
        /// </summary>
        /// <param name="language">Language enumeration value.</param>
        /// <returns><c>true</c> if successfully switched; otherwise, <c>false</c>.</returns>
        bool SetLanguage(AppLanguage language);

        /// <summary>
        /// Occurs when the active language changes.
        /// </summary>
        event EventHandler? LanguageChanged;
    }
}
