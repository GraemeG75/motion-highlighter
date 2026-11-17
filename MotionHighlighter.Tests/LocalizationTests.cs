using System.Reflection;
using System.Text.Json;
using System.Text.RegularExpressions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MotionHighlighter.Localization;

namespace MotionHighlighter.Tests
{
    /// <summary>
    /// Tests for application internationalization, locale completeness, key parity, and dynamic loading.
    /// </summary>
    [TestClass]
    public class LocalizationTests
    {
        private static readonly string[] RequiredLocaleCodes = new string[] { "en", "de", "fr", "es" };

        private static Dictionary<string, string> LoadLocaleDictionary(string code)
        {
            Assembly assembly = typeof(LocalizationManager).Assembly;
            string targetSuffix = $".Localization.Locales.{code}.json";
            string? resourceName = assembly.GetManifestResourceNames()
                .FirstOrDefault(r => r.EndsWith(targetSuffix, StringComparison.OrdinalIgnoreCase));

            Assert.IsNotNull(resourceName, $"Embedded resource for locale '{code}' was not found.");

            using Stream? stream = assembly.GetManifestResourceStream(resourceName);
            Assert.IsNotNull(stream, $"Stream for locale '{code}' was null.");

            using StreamReader reader = new(stream);
            string json = reader.ReadToEnd();
            Dictionary<string, string>? dict = JsonSerializer.Deserialize<Dictionary<string, string>>(json);
            Assert.IsNotNull(dict, $"Failed to deserialize JSON for locale '{code}'.");
            return dict;
        }

        [TestMethod]
        public void Locales_AllFourRequiredLocalesExist()
        {
            foreach (string code in RequiredLocaleCodes)
            {
                Dictionary<string, string> dict = LoadLocaleDictionary(code);
                Assert.IsTrue(dict.Count > 0, $"Locale '{code}' contains no translations.");
            }
        }

        [TestMethod]
        public void Locales_HaveCompleteKeyParityWithEnglish()
        {
            Dictionary<string, string> english = LoadLocaleDictionary("en");
            HashSet<string> englishKeys = new(english.Keys, StringComparer.OrdinalIgnoreCase);

            Assert.IsTrue(englishKeys.Count >= 70, $"English locale should contain comprehensive keys (found {englishKeys.Count}).");

            foreach (string code in new string[] { "de", "fr", "es" })
            {
                Dictionary<string, string> target = LoadLocaleDictionary(code);
                HashSet<string> targetKeys = new(target.Keys, StringComparer.OrdinalIgnoreCase);

                // Missing keys check
                List<string> missingKeys = englishKeys.Except(targetKeys).ToList();
                Assert.AreEqual(0, missingKeys.Count,
                    $"Locale '{code}' is missing {missingKeys.Count} keys present in English: {string.Join(", ", missingKeys.Take(10))}");

                // Extra keys check
                List<string> extraKeys = targetKeys.Except(englishKeys).ToList();
                Assert.AreEqual(0, extraKeys.Count,
                    $"Locale '{code}' has {extraKeys.Count} unexpected keys not in English: {string.Join(", ", extraKeys.Take(10))}");
            }
        }

        [TestMethod]
        public void Locales_PlaceholdersMatchEnglishDefinitions()
        {
            Dictionary<string, string> english = LoadLocaleDictionary("en");
            Regex placeholderRegex = new(@"\{(\d+)(:[^}]+)?\}");

            foreach (string code in new string[] { "de", "fr", "es" })
            {
                Dictionary<string, string> target = LoadLocaleDictionary(code);

                foreach (KeyValuePair<string, string> kvp in english)
                {
                    MatchCollection enMatches = placeholderRegex.Matches(kvp.Value);
                    if (enMatches.Count > 0)
                    {
                        Assert.IsTrue(target.TryGetValue(kvp.Key, out string? targetVal), $"Missing key {kvp.Key} in {code}");
                        MatchCollection targetMatches = placeholderRegex.Matches(targetVal!);

                        HashSet<string> enPlaceholders = enMatches.Select(m => m.Groups[1].Value).ToHashSet();
                        HashSet<string> targetPlaceholders = targetMatches.Select(m => m.Groups[1].Value).ToHashSet();

                        bool setsEqual = enPlaceholders.SetEquals(targetPlaceholders);
                        Assert.IsTrue(setsEqual,
                            $"Placeholder mismatch for key '{kvp.Key}' in locale '{code}'. English: '{kvp.Value}' vs Target: '{targetVal}'");
                    }
                }
            }
        }

        [TestMethod]
        public void LocalizationManager_SwitchesLanguagesCorrectly()
        {
            LocalizationManager mgr = new();

            bool switchedToGerman = mgr.SetLanguage(AppLanguage.German);
            Assert.IsTrue(switchedToGerman);
            Assert.AreEqual("de", mgr.CurrentLanguageCode);
            Assert.AreEqual("ℹ Über", mgr.GetString("main.btn_about"));

            bool switchedToFrench = mgr.SetLanguage(AppLanguage.French);
            Assert.IsTrue(switchedToFrench);
            Assert.AreEqual("fr", mgr.CurrentLanguageCode);
            Assert.AreEqual("ℹ À propos", mgr.GetString("main.btn_about"));

            bool switchedToSpanish = mgr.SetLanguage(AppLanguage.Spanish);
            Assert.IsTrue(switchedToSpanish);
            Assert.AreEqual("es", mgr.CurrentLanguageCode);
            Assert.AreEqual("ℹ Acerca de", mgr.GetString("main.btn_about"));

            bool switchedToEnglish = mgr.SetLanguage(AppLanguage.English);
            Assert.IsTrue(switchedToEnglish);
            Assert.AreEqual("en", mgr.CurrentLanguageCode);
            Assert.AreEqual("ℹ About", mgr.GetString("main.btn_about"));
        }

        [TestMethod]
        public void LocalizationManager_FallsBackToEnglishForMissingKey()
        {
            LocalizationManager mgr = new();
            mgr.SetLanguage(AppLanguage.English);

            // Register temporary locale with missing key
            LocaleInfo testLocale = new("test", "Test", "Test", "🧪", AppLanguage.Custom);
            mgr.RegisterDictionary(testLocale, new Dictionary<string, string>
            {
                ["about.title"] = "Custom About"
            });

            mgr.SetLanguage("test");
            Assert.AreEqual("Custom About", mgr.GetString("about.title"));
            // Fallback key:
            Assert.AreEqual("ℹ About", mgr.GetString("main.btn_about"));
        }

        [TestMethod]
        public void LocalizationManager_SupportsLoadingCustomExternalLocaleFile()
        {
            string tempDir = Path.Combine(Path.GetTempPath(), "mh_locale_test_" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(tempDir);

            try
            {
                // Create an Italian locale file
                string italianJson = JsonSerializer.Serialize(new Dictionary<string, string>
                {
                    ["app.name"] = "Evidenziatore di movimento",
                    ["main.btn_about"] = "Informazioni"
                });

                string itFile = Path.Combine(tempDir, "it.json");
                File.WriteAllText(itFile, italianJson);

                LocalizationManager mgr = new();
                mgr.LoadExternalLocales(tempDir);

                LocaleInfo? itLocale = mgr.AvailableLocales.FirstOrDefault(l => l.Code == "it");
                Assert.IsNotNull(itLocale, "Italian locale should be dynamically registered from external folder.");

                bool switched = mgr.SetLanguage("it");
                Assert.IsTrue(switched);
                Assert.AreEqual("Evidenziatore di movimento", mgr.GetString("app.name"));
                Assert.AreEqual("Informazioni", mgr.GetString("main.btn_about"));
            }
            finally
            {
                if (Directory.Exists(tempDir))
                {
                    Directory.Delete(tempDir, true);
                }
            }
        }
    }
}
