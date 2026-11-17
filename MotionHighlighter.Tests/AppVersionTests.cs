using System.Text.RegularExpressions;
using System.Windows.Forms;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MotionHighlighter.Tests
{
    /// <summary>
    /// Unit tests verifying versioning accuracy, clean semantic version formatting,
    /// and that third-party library names (OpenCV, .NET) are omitted from the display.
    /// </summary>
    [TestClass]
    public class AppVersionTests
    {
        [TestMethod]
        public void AppVersion_CurrentIsNotNullAndAtLeastOneZeroZero()
        {
            Version current = AppVersion.Current;
            Assert.IsNotNull(current);
            Assert.IsTrue(current.Major >= 1, "AppVersion major version should be at least 1.");
        }

        [TestMethod]
        public void AppVersion_VersionString_MatchesSemanticVersionFormat()
        {
            string versionStr = AppVersion.VersionString;
            Assert.IsFalse(string.IsNullOrWhiteSpace(versionStr));
            Assert.IsTrue(Regex.IsMatch(versionStr, @"^\d+\.\d+\.\d+$"),
                $"Version string '{versionStr}' does not match standard semver major.minor.patch format.");
        }

        [TestMethod]
        public void AppVersion_DisplayString_ContainsOnlyVersionWithoutOpenCvOrDotNet()
        {
            string display = AppVersion.DisplayString;

            Assert.IsTrue(display.StartsWith("Version ", StringComparison.OrdinalIgnoreCase),
                $"Display string '{display}' should begin with 'Version '.");
            Assert.IsFalse(display.Contains("OpenCV", StringComparison.OrdinalIgnoreCase),
                "Version display must not mention OpenCV.");
            Assert.IsFalse(display.Contains(".NET", StringComparison.OrdinalIgnoreCase),
                "Version display must not mention .NET.");
        }

        [TestMethod]
        public void FormAbout_VersionLabel_ContainsOnlyVersionWithoutOpenCvOrDotNet()
        {
            using FormAbout dlg = new();

            Label? lblVersion = null;
            foreach (Control c in dlg.Controls)
            {
                if (c is Panel p)
                {
                    foreach (Control child in p.Controls)
                    {
                        if (child is Label lbl && lbl.Name == "lblVersion")
                        {
                            lblVersion = lbl;
                            break;
                        }
                    }
                }
            }

            Assert.IsNotNull(lblVersion, "lblVersion should exist in FormAbout.");
            Assert.IsFalse(lblVersion.Text.Contains("OpenCV", StringComparison.OrdinalIgnoreCase),
                "lblVersion must not mention OpenCV.");
            Assert.IsFalse(lblVersion.Text.Contains(".NET", StringComparison.OrdinalIgnoreCase),
                "lblVersion must not mention .NET.");
            Assert.IsTrue(lblVersion.Text.Contains(AppVersion.VersionString),
                $"lblVersion should display current version string '{AppVersion.VersionString}'.");
        }
    }
}
