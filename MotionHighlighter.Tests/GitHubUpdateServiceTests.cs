using System.Net;
using System.Text.Json;
using System.Windows.Forms;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MotionHighlighter.Updating;

namespace MotionHighlighter.Tests
{
    /// <summary>
    /// Unit tests verifying GitHub Releases API checking, semantic version parsing,
    /// installer asset detection, and update dialog presentation.
    /// </summary>
    [TestClass]
    public class GitHubUpdateServiceTests
    {
        private class MockHttpMessageHandler : HttpMessageHandler
        {
            private readonly Func<HttpRequestMessage, HttpResponseMessage> _responder;

            public MockHttpMessageHandler(Func<HttpRequestMessage, HttpResponseMessage> responder)
            {
                _responder = responder;
            }

            protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            {
                return Task.FromResult(_responder(request));
            }
        }

        [TestMethod]
        public void ParseSemanticVersion_HandlesVariousTagFormats()
        {
            Assert.AreEqual(new Version(1, 2, 3), GitHubUpdateService.ParseSemanticVersion("v1.2.3"));
            Assert.AreEqual(new Version(2, 0, 0), GitHubUpdateService.ParseSemanticVersion("2.0.0"));
            Assert.AreEqual(new Version(3, 4, 5, 6), GitHubUpdateService.ParseSemanticVersion("v3.4.5.6"));
            Assert.AreEqual(new Version(10, 5), GitHubUpdateService.ParseSemanticVersion("10.5"));
            Assert.AreEqual(new Version(1, 0, 0), GitHubUpdateService.ParseSemanticVersion(""));
            Assert.AreEqual(new Version(1, 0, 0), GitHubUpdateService.ParseSemanticVersion(null!));
        }

        [TestMethod]
        public void ParseReleaseJson_ExtractsReleaseAndInstallerAssetCorrectly()
        {
            string json = @"
            {
                ""tag_name"": ""v2.1.0"",
                ""name"": ""Motion Highlighter 2.1.0"",
                ""body"": ""## What's New\n- Added multi-language translations\n- Automated installer"",
                ""published_at"": ""2026-11-17T12:00:00Z"",
                ""html_url"": ""https://github.com/GraemeG75/motion-highlighter/releases/tag/v2.1.0"",
                ""assets"": [
                    {
                        ""name"": ""Source.zip"",
                        ""browser_download_url"": ""https://github.com/GraemeG75/motion-highlighter/releases/download/v2.1.0/Source.zip"",
                        ""size"": 10240,
                        ""content_type"": ""application/zip""
                    },
                    {
                        ""name"": ""MotionHighlighter-Setup.exe"",
                        ""browser_download_url"": ""https://github.com/GraemeG75/motion-highlighter/releases/download/v2.1.0/MotionHighlighter-Setup.exe"",
                        ""size"": 45000000,
                        ""content_type"": ""application/x-msdos-program""
                    }
                ]
            }";

            using HttpClient client = new(new MockHttpMessageHandler(_ => new HttpResponseMessage(HttpStatusCode.OK)));
            GitHubUpdateService service = new(client);

            UpdateInfo? update = service.ParseReleaseJson(json);
            Assert.IsNotNull(update);
            Assert.AreEqual("v2.1.0", update.TagName);
            Assert.AreEqual(new Version(2, 1, 0), update.Version);
            Assert.AreEqual("Motion Highlighter 2.1.0", update.Title);
            Assert.IsTrue(update.IsNewer, "v2.1.0 should be marked as newer than current version.");
            Assert.IsNotNull(update.InstallerAsset, "InstallerAsset should identify MotionHighlighter-Setup.exe.");
            Assert.AreEqual("MotionHighlighter-Setup.exe", update.InstallerAsset.Name);
            Assert.IsTrue(update.InstallerAsset.DownloadUrl.EndsWith("MotionHighlighter-Setup.exe"));
        }

        [TestMethod]
        public async Task CheckForUpdatesAsync_WithMockHttpClient_DetectsNewerRelease()
        {
            string json = @"
            {
                ""tag_name"": ""v9.9.9"",
                ""name"": ""Major Update 9.9.9"",
                ""body"": ""Exciting new features!"",
                ""published_at"": ""2026-11-17T12:00:00Z"",
                ""html_url"": ""https://github.com/GraemeG75/motion-highlighter/releases/tag/v9.9.9"",
                ""assets"": [
                    {
                        ""name"": ""MotionHighlighter-Setup.exe"",
                        ""browser_download_url"": ""https://example.com/MotionHighlighter-Setup.exe"",
                        ""size"": 12345,
                        ""content_type"": ""application/octet-stream""
                    }
                ]
            }";

            using HttpClient client = new(new MockHttpMessageHandler(_ => new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(json)
            }));

            GitHubUpdateService service = new(client);
            UpdateInfo? update = await service.CheckForUpdatesAsync();

            Assert.IsNotNull(update);
            Assert.IsTrue(update.IsNewer);
            Assert.AreEqual(new Version(9, 9, 9), update.Version);
        }

        [TestMethod]
        public async Task CheckForUpdatesAsync_HandlesNetworkErrorGracefully()
        {
            using HttpClient client = new(new MockHttpMessageHandler(_ => new HttpResponseMessage(HttpStatusCode.NotFound)));
            GitHubUpdateService service = new(client);

            UpdateInfo? update = await service.CheckForUpdatesAsync();
            Assert.IsNull(update, "Update check should return null on HTTP 404 without throwing exceptions.");
        }

        [TestMethod]
        public void FormUpdate_InitializesWithMascotAndPrideTheming()
        {
            GitHubReleaseAsset asset = new("MotionHighlighter-Setup.exe", "https://example.com/setup.exe", 1000, "application/octet-stream");
            UpdateInfo update = new("v2.0.0", new Version(2, 0, 0), "Release 2.0.0", "Changelog text", DateTimeOffset.UtcNow, "https://github.com/GraemeG75/motion-highlighter", true, new[] { asset }, asset);

            using HttpClient client = new(new MockHttpMessageHandler(_ => new HttpResponseMessage(HttpStatusCode.OK)));
            GitHubUpdateService service = new(client);

            using FormUpdate dlg = new(update, service);

            Assert.IsNotNull(dlg.Controls);

            PictureBox? picMascot = null;
            Panel? prideBanner = null;

            foreach (Control c in dlg.Controls)
            {
                if (c is Panel p && p.Name == "panelPrideBanner")
                {
                    prideBanner = p;
                }
                else if (c is Panel content)
                {
                    foreach (Control child in content.Controls)
                    {
                        if (child is PictureBox pb && pb.Name == "picMascot")
                        {
                            picMascot = pb;
                        }
                    }
                }
            }

            Assert.IsNotNull(prideBanner, "FormUpdate should contain panelPrideBanner.");
            Assert.IsNotNull(picMascot, "FormUpdate should contain picMascot.");
            Assert.AreEqual("Generated Pixel kitten mascot", picMascot.AccessibleName);
            Assert.AreEqual("Generated Pixel kitten mascot", picMascot.AccessibleDescription);
        }
    }
}
