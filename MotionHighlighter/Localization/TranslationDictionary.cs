using System.Globalization;

namespace MotionHighlighter.Localization
{
    /// <summary>
    /// Stores key-value translation strings for a specific locale and provides formatted string lookup.
    /// </summary>
    public class TranslationDictionary
    {
        private readonly Dictionary<string, string> _strings = new(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// Gets the locale metadata associated with this translation dictionary.
        /// </summary>
        public LocaleInfo Locale { get; }

        /// <summary>
        /// Gets the number of translation keys defined in this dictionary.
        /// </summary>
        public int Count => _strings.Count;

        /// <summary>
        /// Gets all translation keys stored in this dictionary.
        /// </summary>
        public IReadOnlyCollection<string> Keys => _strings.Keys;

        /// <summary>
        /// Initializes a new instance of the <see cref="TranslationDictionary"/> class.
        /// </summary>
        /// <param name="locale">Locale metadata describing the dictionary.</param>
        public TranslationDictionary(LocaleInfo locale)
        {
            Locale = locale ?? throw new ArgumentNullException(nameof(locale));
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TranslationDictionary"/> class with pre-loaded entries.
        /// </summary>
        /// <param name="locale">Locale metadata describing the dictionary.</param>
        /// <param name="entries">Initial key-value mappings.</param>
        public TranslationDictionary(LocaleInfo locale, IDictionary<string, string> entries) : this(locale)
        {
            if (entries != null)
            {
                foreach (KeyValuePair<string, string> kvp in entries)
                {
                    _strings[kvp.Key] = kvp.Value;
                }
            }
        }

        /// <summary>
        /// Sets or updates a translation string.
        /// </summary>
        /// <param name="key">Unique translation key.</param>
        /// <param name="value">Localized string value.</param>
        public void Set(string key, string value)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                throw new ArgumentException("Translation key cannot be null or empty.", nameof(key));
            }

            _strings[key] = value ?? string.Empty;
        }

        /// <summary>
        /// Determines whether the dictionary contains a translation for the specified key.
        /// </summary>
        /// <param name="key">The translation key to check.</param>
        /// <returns><c>true</c> if the key is defined; otherwise, <c>false</c>.</returns>
        public bool ContainsKey(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                return false;
            }

            return _strings.ContainsKey(key);
        }

        /// <summary>
        /// Retrieves the localized string for a key, or returns the fallback key if not found.
        /// </summary>
        /// <param name="key">The translation key.</param>
        /// <returns>The localized text, or the key itself if missing.</returns>
        public string Get(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                return string.Empty;
            }

            if (_strings.TryGetValue(key, out string? value))
            {
                return value;
            }

            return key;
        }

        /// <summary>
        /// Retrieves the localized string formatted with positional arguments.
        /// </summary>
        /// <param name="key">The translation key.</param>
        /// <param name="args">Format arguments to substitute into placeholders.</param>
        /// <returns>The formatted localized text.</returns>
        public string Format(string key, params object[] args)
        {
            string raw = Get(key);
            if (args == null || args.Length == 0)
            {
                return raw;
            }

            try
            {
                return string.Format(CultureInfo.CurrentCulture, raw, args);
            }
            catch (FormatException)
            {
                return raw;
            }
        }
    }
}
