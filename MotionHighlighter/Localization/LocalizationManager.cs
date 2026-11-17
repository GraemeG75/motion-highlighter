using System.Globalization;
using System.Reflection;
using System.Text.Json;

namespace MotionHighlighter.Localization
{
    /// <summary>
    /// Central manager for application internationalization, providing translations for English,
    /// German, French, Spanish, and dynamically loaded external locales.
    /// </summary>
    public class LocalizationManager : ILocalizationProvider
    {
        private static readonly Lazy<LocalizationManager> _lazyInstance = new(() => new LocalizationManager());

        private readonly object _syncLock = new();
        private readonly Dictionary<string, TranslationDictionary> _dictionaries = new(StringComparer.OrdinalIgnoreCase);
        private readonly List<LocaleInfo> _availableLocales = new();
        private TranslationDictionary? _currentDictionary;
        private TranslationDictionary? _fallbackDictionary;

        /// <summary>
        /// Gets the singleton instance of the <see cref="LocalizationManager"/>.
        /// </summary>
        public static LocalizationManager Instance => _lazyInstance.Value;

        /// <summary>
        /// Default language code when none is set (English).
        /// </summary>
        public const string DefaultLanguageCode = "en";

        /// <inheritdoc />
        public string CurrentLanguageCode
        {
            get
            {
                lock (_syncLock)
                {
                    return _currentDictionary?.Locale.Code ?? DefaultLanguageCode;
                }
            }
        }

        /// <inheritdoc />
        public AppLanguage CurrentLanguage
        {
            get
            {
                lock (_syncLock)
                {
                    return _currentDictionary?.Locale.LanguageEnum ?? AppLanguage.English;
                }
            }
        }

        /// <summary>
        /// Gets the currently active <see cref="LocaleInfo"/>.
        /// </summary>
        public LocaleInfo CurrentLocale
        {
            get
            {
                lock (_syncLock)
                {
                    return _currentDictionary?.Locale ?? _availableLocales.First(l => l.Code == DefaultLanguageCode);
                }
            }
        }

        /// <inheritdoc />
        public IReadOnlyList<LocaleInfo> AvailableLocales
        {
            get
            {
                lock (_syncLock)
                {
                    return _availableLocales.AsReadOnly();
                }
            }
        }

        /// <inheritdoc />
        public event EventHandler? LanguageChanged;

        /// <summary>
        /// Initializes a new instance of the <see cref="LocalizationManager"/> class.
        /// </summary>
        public LocalizationManager()
        {
            RegisterStandardLocales();
            LoadEmbeddedTranslations();
            TryLoadExternalLocales();
            DetectInitialLanguage();
        }

        /// <summary>
        /// Registers the standard built-in language descriptors.
        /// </summary>
        private void RegisterStandardLocales()
        {
            _availableLocales.Clear();
            _availableLocales.Add(new LocaleInfo("en", "English", "English", "🇬🇧", AppLanguage.English));
            _availableLocales.Add(new LocaleInfo("de", "German", "Deutsch", "🇩🇪", AppLanguage.German));
            _availableLocales.Add(new LocaleInfo("fr", "French", "Français", "🇫🇷", AppLanguage.French));
            _availableLocales.Add(new LocaleInfo("es", "Spanish", "Español", "🇪🇸", AppLanguage.Spanish));
        }

        /// <summary>
        /// Loads translations from embedded JSON resources.
        /// </summary>
        private void LoadEmbeddedTranslations()
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            string[] resourceNames = assembly.GetManifestResourceNames();

            foreach (LocaleInfo locale in _availableLocales)
            {
                string targetSuffix = $".Localization.Locales.{locale.Code}.json";
                string? matchedResource = resourceNames.FirstOrDefault(r => r.EndsWith(targetSuffix, StringComparison.OrdinalIgnoreCase) || r.EndsWith($".{locale.Code}.json", StringComparison.OrdinalIgnoreCase));

                if (matchedResource != null)
                {
                    using Stream? stream = assembly.GetManifestResourceStream(matchedResource);
                    if (stream != null)
                    {
                        LoadDictionaryFromStream(locale, stream);
                    }
                }
            }

            if (_dictionaries.TryGetValue(DefaultLanguageCode, out TranslationDictionary? fallbackDict))
            {
                _fallbackDictionary = fallbackDict;
            }
        }

        /// <summary>
        /// Deserializes a JSON translation stream and registers the resulting dictionary.
        /// </summary>
        /// <param name="locale">Locale metadata.</param>
        /// <param name="stream">Stream containing UTF-8 JSON key-value pairs.</param>
        public void LoadDictionaryFromStream(LocaleInfo locale, Stream stream)
        {
            if (locale == null)
            {
                throw new ArgumentNullException(nameof(locale));
            }

            if (stream == null)
            {
                throw new ArgumentNullException(nameof(stream));
            }

            try
            {
                using StreamReader reader = new(stream);
                string json = reader.ReadToEnd();
                Dictionary<string, string>? dict = JsonSerializer.Deserialize<Dictionary<string, string>>(json);
                if (dict != null)
                {
                    TranslationDictionary translationDict = new(locale, dict);
                    _dictionaries[locale.Code] = translationDict;

                    if (!_availableLocales.Any(l => l.Code.Equals(locale.Code, StringComparison.OrdinalIgnoreCase)))
                    {
                        _availableLocales.Add(locale);
                    }
                }
            }
            catch (Exception)
            {
                // Silently maintain existing translations if stream is malformed
            }
        }

        /// <summary>
        /// Searches standard application folders for external *.json locale files to allow adding new languages at runtime.
        /// </summary>
        private void TryLoadExternalLocales()
        {
            try
            {
                string baseDir = AppDomain.CurrentDomain.BaseDirectory;
                string localesFolder = Path.Combine(baseDir, "locales");
                if (Directory.Exists(localesFolder))
                {
                    LoadExternalLocales(localesFolder);
                }
            }
            catch
            {
                // Non-fatal if folder cannot be accessed
            }
        }

        /// <summary>
        /// Scans a directory for external translation files named '{code}.json' (e.g. 'it.json', 'pt.json').
        /// </summary>
        /// <param name="directoryPath">Directory path containing custom translation JSON files.</param>
        public void LoadExternalLocales(string directoryPath)
        {
            if (string.IsNullOrWhiteSpace(directoryPath) || !Directory.Exists(directoryPath))
            {
                return;
            }

            string[] jsonFiles = Directory.GetFiles(directoryPath, "*.json");
            foreach (string file in jsonFiles)
            {
                try
                {
                    string fileName = Path.GetFileNameWithoutExtension(file);
                    string code = fileName.ToLowerInvariant();

                    LocaleInfo? existingLocale = _availableLocales.FirstOrDefault(l => l.Code.Equals(code, StringComparison.OrdinalIgnoreCase));
                    if (existingLocale == null)
                    {
                        string displayName = CultureInfo.GetCultureInfo(code).DisplayName;
                        string nativeName = CultureInfo.GetCultureInfo(code).NativeName;
                        existingLocale = new LocaleInfo(code, displayName, nativeName, "🌐", AppLanguage.Custom);
                        _availableLocales.Add(existingLocale);
                    }

                    using FileStream fs = File.OpenRead(file);
                    LoadDictionaryFromStream(existingLocale, fs);
                }
                catch
                {
                    // Ignore invalid custom files
                }
            }
        }

        /// <summary>
        /// Detects the initial language using saved preferences or system culture.
        /// </summary>
        private void DetectInitialLanguage()
        {
            string systemCode = CultureInfo.CurrentUICulture.TwoLetterISOLanguageName.ToLowerInvariant();
            if (systemCode == "de" || systemCode == "fr" || systemCode == "es")
            {
                SetLanguage(systemCode);
            }
            else
            {
                SetLanguage(DefaultLanguageCode);
            }
        }

        /// <inheritdoc />
        public bool SetLanguage(string code)
        {
            if (string.IsNullOrWhiteSpace(code))
            {
                return false;
            }

            lock (_syncLock)
            {
                if (_dictionaries.TryGetValue(code, out TranslationDictionary? dict))
                {
                    _currentDictionary = dict;
                    LanguageChanged?.Invoke(this, EventArgs.Empty);
                    return true;
                }
            }

            return false;
        }

        /// <inheritdoc />
        public bool SetLanguage(AppLanguage language)
        {
            LocaleInfo? matched;
            lock (_syncLock)
            {
                matched = _availableLocales.FirstOrDefault(l => l.LanguageEnum == language);
            }

            if (matched != null)
            {
                return SetLanguage(matched.Code);
            }

            return false;
        }

        /// <inheritdoc />
        public string GetString(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                return string.Empty;
            }

            lock (_syncLock)
            {
                if (_currentDictionary != null && _currentDictionary.ContainsKey(key))
                {
                    return _currentDictionary.Get(key);
                }

                if (_fallbackDictionary != null && _fallbackDictionary.ContainsKey(key))
                {
                    return _fallbackDictionary.Get(key);
                }
            }

            return key;
        }

        /// <inheritdoc />
        public string GetString(string key, params object[] args)
        {
            string raw = GetString(key);
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

        /// <summary>
        /// Global shortcut for <see cref="GetString(string)"/>.
        /// </summary>
        /// <param name="key">The translation key.</param>
        /// <returns>The localized text.</returns>
        public static string T(string key)
        {
            return Instance.GetString(key);
        }

        /// <summary>
        /// Global shortcut for <see cref="GetString(string, object[])"/>.
        /// </summary>
        /// <param name="key">The translation key.</param>
        /// <param name="args">Format arguments.</param>
        /// <returns>The formatted localized text.</returns>
        public static string T(string key, params object[] args)
        {
            return Instance.GetString(key, args);
        }

        /// <summary>
        /// Registers a programmatic dictionary directly (useful for tests or plugin extensions).
        /// </summary>
        /// <param name="locale">Locale metadata.</param>
        /// <param name="entries">Key-value mappings.</param>
        public void RegisterDictionary(LocaleInfo locale, IDictionary<string, string> entries)
        {
            if (locale == null)
            {
                throw new ArgumentNullException(nameof(locale));
            }

            TranslationDictionary dict = new(locale, entries);
            _dictionaries[locale.Code] = dict;

            if (!_availableLocales.Any(l => l.Code.Equals(locale.Code, StringComparison.OrdinalIgnoreCase)))
            {
                _availableLocales.Add(locale);
            }

            if (locale.Code.Equals(DefaultLanguageCode, StringComparison.OrdinalIgnoreCase))
            {
                _fallbackDictionary = dict;
            }

            if (_currentDictionary == null || _currentDictionary.Locale.Code.Equals(locale.Code, StringComparison.OrdinalIgnoreCase))
            {
                _currentDictionary = dict;
            }
        }
    }
}
