using Microsoft.VisualStudio.TestTools.UnitTesting;
using MotionHighlighter.Streaming;

namespace MotionHighlighter.Tests
{
    [TestClass]
    public class StreamHistoryManagerTests
    {
        [TestMethod]
        public void BuiltInPresets_ContainsCameraPortalsAndTestStreams()
        {
            IReadOnlyList<(string Category, string Name, string Url, StreamFormat Format)> presets = StreamHistoryManager.BuiltInPresets;
            Assert.IsTrue(presets.Count >= 10);

            Assert.IsTrue(presets.Any(p => p.Name.Contains("Google Home")));
            Assert.IsTrue(presets.Any(p => p.Name.Contains("Nest")));
            Assert.IsTrue(presets.Any(p => p.Name.Contains("Ring")));
            Assert.IsTrue(presets.Any(p => p.Name.Contains("Arlo")));
            Assert.IsTrue(presets.Any(p => p.Name.Contains("Wyze")));
            Assert.IsTrue(presets.Any(p => p.Name.Contains("Eufy")));
            Assert.IsTrue(presets.Any(p => p.Name.Contains("UniFi")));
            Assert.IsTrue(presets.Any(p => p.Name.Contains("Big Buck Bunny")));
        }

        [TestMethod]
        public void SaveAndLoadHistory_PersistsItem()
        {
            string testUrl = "https://my-test-camera.example.com/live/cam1";
            string testTitle = "Office Camera";

            StreamHistoryManager.SaveHistoryItem(testUrl, StreamFormat.WebLiveFeed, testTitle);
            List<StreamHistoryItem> history = StreamHistoryManager.LoadHistory();

            StreamHistoryItem? item = history.FirstOrDefault(h => h.Url == testUrl);
            Assert.IsNotNull(item);
            Assert.AreEqual(testTitle, item.Title);
            Assert.AreEqual(StreamFormat.WebLiveFeed, item.Format);
        }

        [TestMethod]
        public void DetectFormatFromUrl_RecognizesVariousCameraWebsites()
        {
            Assert.AreEqual(StreamFormat.WebLiveFeed, WebpageStreamExtractor.DetectFormatFromUrl("https://home.google.com/cameras/list/12345"));
            Assert.AreEqual(StreamFormat.WebLiveFeed, WebpageStreamExtractor.DetectFormatFromUrl("https://home.nest.com/camera/live"));
            Assert.AreEqual(StreamFormat.WebLiveFeed, WebpageStreamExtractor.DetectFormatFromUrl("https://ring.com/users/sign_in"));
            Assert.AreEqual(StreamFormat.WebLiveFeed, WebpageStreamExtractor.DetectFormatFromUrl("https://my.arlo.com/#/cameras"));
            Assert.AreEqual(StreamFormat.WebLiveFeed, WebpageStreamExtractor.DetectFormatFromUrl("https://view.wyze.com/live"));
            Assert.AreEqual(StreamFormat.WebLiveFeed, WebpageStreamExtractor.DetectFormatFromUrl("https://mysecurity.eufylife.com/camera"));
            Assert.AreEqual(StreamFormat.WebLiveFeed, WebpageStreamExtractor.DetectFormatFromUrl("https://unifi.ui.com/protect/devices"));
            Assert.AreEqual(StreamFormat.WebLiveFeed, WebpageStreamExtractor.DetectFormatFromUrl("https://my-custom-cctv.local:8443/view"));
        }

        [TestMethod]
        public void LoadConfiguredCameras_LoadsFromConfigFile()
        {
            List<ConfiguredCamera> cameras = StreamHistoryManager.LoadConfiguredCameras();
            Assert.IsNotNull(cameras);
            Assert.IsTrue(cameras.Count >= 10);

            List<ConfiguredCamera> myCams = cameras.Where(c => c.Category == "My Cameras").ToList();
            Assert.IsTrue(myCams.Count >= 2);
            ConfiguredCamera? rtspCam = myCams.FirstOrDefault(c => c.Format == StreamFormat.Rtsp);
            Assert.IsNotNull(rtspCam);
            Assert.AreEqual(AuthType.BasicAuth, rtspCam.AuthType);
            Assert.AreEqual("admin", rtspCam.Username);
            Assert.AreEqual("password123", rtspCam.Password);

            List<ConfiguredCamera> portals = cameras.Where(c => c.Category == "Camera Portals").ToList();
            Assert.IsTrue(portals.Any(p => p.Name.Contains("Google Home")));

            List<ConfiguredCamera> tests = cameras.Where(c => c.Category == "Public Test Streams").ToList();
            Assert.IsTrue(tests.Any(t => t.Url.Contains(".mpd")));
            Assert.IsTrue(tests.Any(t => t.Url.Contains(".m3u8")));
        }

        [TestMethod]
        public void ConfiguredCameras_UrlsAreValidUris()
        {
            List<ConfiguredCamera> cameras = StreamHistoryManager.LoadConfiguredCameras();
            foreach (ConfiguredCamera cam in cameras)
            {
                Assert.IsTrue(Uri.TryCreate(cam.Url, UriKind.Absolute, out Uri? uri), $"Invalid URI: {cam.Url}");
                Assert.IsTrue(uri.Scheme == "https" || uri.Scheme == "http" || uri.Scheme == "rtsp", $"Unexpected scheme: {uri.Scheme}");
            }
        }

        [TestMethod]
        public void Forms_InitializeComponentsWithoutExceptions()
        {
            Thread thread = new Thread(() =>
            {
                using FormMain formMain = new FormMain();
                Assert.IsNotNull(formMain);

                using FormStreamDialog formStream = new FormStreamDialog();
                Assert.IsNotNull(formStream);
            });
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
            thread.Join();
        }
    }
}
