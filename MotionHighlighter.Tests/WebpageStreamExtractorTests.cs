using MotionHighlighter.Streaming;

namespace MotionHighlighter.Tests
{
    [TestClass]
    public class WebpageStreamExtractorTests
    {
        private readonly WebpageStreamExtractor _extractor = new();

        [TestMethod]
        public void ExtractStreamsFromHtml_VideoTagAndSources_ExtractsUrls()
        {
            string html = @"
                <!DOCTYPE html>
                <html>
                <head><title>Camera Portal</title></head>
                <body>
                    <h1>Security Feed</h1>
                    <video id='player' controls poster='/thumb.jpg'>
                        <source src='/streams/cam1/playlist.m3u8' type='application/x-mpegURL'>
                        <source src='https://cdn.example.com/cam1/manifest.mpd' type='application/dash+xml'>
                        <source src='/fallback/cam1.m4v' type='video/x-m4v'>
                    </video>
                </body>
                </html>";

            Uri baseUri = new Uri("https://camera.example.com/watch");
            List<StreamCandidate> results = _extractor.ExtractStreamsFromHtml(html, baseUri);

            Assert.IsNotNull(results);
            Assert.AreEqual(3, results.Count);

            // Manifests prioritized first
            Assert.IsTrue(results.Any(r => r.Format == StreamFormat.DashMpd && r.Url == "https://cdn.example.com/cam1/manifest.mpd"));
            Assert.IsTrue(results.Any(r => r.Format == StreamFormat.HlsM3u8 && r.Url == "https://camera.example.com/streams/cam1/playlist.m3u8"));
            Assert.IsTrue(results.Any(r => r.Format == StreamFormat.M4vVideo && r.Url == "https://camera.example.com/fallback/cam1.m4v"));
        }

        [TestMethod]
        public void ExtractStreamsFromHtml_JavaScriptConfigs_ExtractsStreams()
        {
            string html = @"
                <html>
                <body>
                    <div id='player'></div>
                    <script>
                        var playerConfig = {
                            file: 'https://stream.server.org/live/manifest.mpd',
                            hls: 'https://stream.server.org/live/master.m3u8',
                            source: '/media/camera_chunk.m4s'
                        };
                        const directUrl = ""https:\/\/stream.server.org\/live\/hls_cmaf.m4s"";
                    </script>
                </body>
                </html>";

            Uri baseUri = new Uri("https://stream.server.org/");
            List<StreamCandidate> results = _extractor.ExtractStreamsFromHtml(html, baseUri);

            Assert.IsTrue(results.Any(r => r.Format == StreamFormat.DashMpd));
            Assert.IsTrue(results.Any(r => r.Format == StreamFormat.HlsM3u8));
            Assert.IsTrue(results.Any(r => r.Format == StreamFormat.FragmentedMp4));
        }

        [TestMethod]
        public void DetectFormatFromUrl_RecognizesAllSupportedFormats()
        {
            Assert.AreEqual(StreamFormat.DashMpd, WebpageStreamExtractor.DetectFormatFromUrl("https://example.com/stream.mpd"));
            Assert.AreEqual(StreamFormat.DashMpd, WebpageStreamExtractor.DetectFormatFromUrl("https://example.com/stream.mpd?token=123"));

            Assert.AreEqual(StreamFormat.HlsM3u8, WebpageStreamExtractor.DetectFormatFromUrl("https://example.com/master.m3u8"));
            Assert.AreEqual(StreamFormat.HlsM3u8, WebpageStreamExtractor.DetectFormatFromUrl("https://example.com/master.m3u8?auth=abc"));

            Assert.AreEqual(StreamFormat.FragmentedMp4, WebpageStreamExtractor.DetectFormatFromUrl("https://example.com/chunk-001.m4s"));
            Assert.AreEqual(StreamFormat.M4vVideo, WebpageStreamExtractor.DetectFormatFromUrl("https://example.com/video.m4v"));
            Assert.AreEqual(StreamFormat.StandardVideo, WebpageStreamExtractor.DetectFormatFromUrl("https://example.com/video.mp4"));
            Assert.AreEqual(StreamFormat.Mjpeg, WebpageStreamExtractor.DetectFormatFromUrl("http://camera.local/mjpeg_stream"));
            Assert.AreEqual(StreamFormat.Rtsp, WebpageStreamExtractor.DetectFormatFromUrl("rtsp://192.168.1.100:554/live"));
        }

        [TestMethod]
        public void ExtractStreamsFromHtml_ProtocolRelativeUrl_ResolvesCorrectly()
        {
            string html = "<video src='//cdn.provider.com/live/stream.m3u8'></video>";
            Uri baseUri = new Uri("https://myportal.com/");

            List<StreamCandidate> results = _extractor.ExtractStreamsFromHtml(html, baseUri);
            Assert.AreEqual(1, results.Count);
            Assert.AreEqual("https://cdn.provider.com/live/stream.m3u8", results[0].Url);
        }
    }
}
