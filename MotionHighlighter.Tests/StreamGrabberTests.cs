using System.Diagnostics;
using System.Net;
using MotionHighlighter.Streaming;
using OpenCvSharp;

namespace MotionHighlighter.Tests
{
    [TestClass]
    public class StreamGrabberTests
    {
        private static string? _testDir;
        private static byte[]? _initBytes;
        private static byte[]? _chunkBytes;

        [ClassInitialize]
        public static void SetupClass(TestContext context)
        {
            _testDir = Path.Combine(Path.GetTempPath(), "MH_StreamTests_" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(_testDir);

            // Generate sample test video and DASH fMP4 chunks using ffmpeg
            string mp4Path = Path.Combine(_testDir, "sample.mp4");
            ProcessStartInfo psi = new ProcessStartInfo
            {
                FileName = "ffmpeg.exe",
                Arguments = $"-y -f lavfi -i testsrc=duration=2:size=320x240:rate=10 -c:v libx264 -pix_fmt yuv420p -f dash -seg_duration 1 -init_seg_name init.m4s -media_seg_name chunk-$Number%02d$.m4s manifest.mpd",
                WorkingDirectory = _testDir,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using Process proc = Process.Start(psi)!;
            proc.WaitForExit(10000);

            string initFile = Path.Combine(_testDir, "init.m4s");
            string chunkFile = Path.Combine(_testDir, "chunk-01.m4s");

            if (File.Exists(initFile)) _initBytes = File.ReadAllBytes(initFile);
            if (File.Exists(chunkFile)) _chunkBytes = File.ReadAllBytes(chunkFile);
        }

        [ClassCleanup]
        public static void CleanupClass()
        {
            try
            {
                if (_testDir != null && Directory.Exists(_testDir))
                {
                    Directory.Delete(_testDir, true);
                }
            }
            catch { }
        }

        [TestMethod]
        public async Task StreamGrabber_AuthenticatedDashStream_DownloadsAndDecodesFrames()
        {
            if (_initBytes == null || _chunkBytes == null)
            {
                Assert.Inconclusive("FFmpeg not available to generate test fMP4 segments.");
                return;
            }

            int port = 59123;
            HttpListener listener = new HttpListener();
            listener.Prefixes.Add($"http://127.0.0.1:{port}/");
            listener.Start();

            string expectedToken = "secret_stream_token_123";
            bool authVerified = false;

            string mpdXml = $@"<?xml version=""1.0"" encoding=""utf-8""?>
<MPD xmlns=""urn:mpeg:dash:schema:mpd:2011"" profiles=""urn:mpeg:dash:profile:isoff-live:2011"" type=""static"">
    <Period id=""0"" start=""PT0.0S"">
        <AdaptationSet id=""0"" contentType=""video"" frameRate=""10/1"">
            <Representation id=""0"" mimeType=""video/mp4"" width=""320"" height=""240"">
                <SegmentTemplate timescale=""1000"" initialization=""init.m4s"" media=""chunk-$Number%02d$.m4s"" startNumber=""1"">
                    <SegmentTimeline>
                        <S t=""0"" d=""1000"" r=""0"" />
                    </SegmentTimeline>
                </SegmentTemplate>
            </Representation>
        </AdaptationSet>
    </Period>
</MPD>";

            CancellationTokenSource cts = new CancellationTokenSource();

            _ = Task.Run(async () =>
            {
                while (listener.IsListening && !cts.IsCancellationRequested)
                {
                    try
                    {
                        HttpListenerContext ctx = await listener.GetContextAsync();
                        string authHeader = ctx.Request.Headers["Authorization"] ?? string.Empty;
                        if (authHeader.Contains(expectedToken))
                        {
                            authVerified = true;
                        }

                        string path = ctx.Request.Url?.AbsolutePath ?? string.Empty;
                        byte[] body;

                        if (path.EndsWith("manifest.mpd"))
                        {
                            body = System.Text.Encoding.UTF8.GetBytes(mpdXml);
                            ctx.Response.ContentType = "application/dash+xml";
                        }
                        else if (path.EndsWith("init.m4s"))
                        {
                            body = _initBytes;
                            ctx.Response.ContentType = "video/iso.segment";
                        }
                        else if (path.EndsWith("chunk-01.m4s"))
                        {
                            body = _chunkBytes;
                            ctx.Response.ContentType = "video/iso.segment";
                        }
                        else
                        {
                            ctx.Response.StatusCode = 404;
                            ctx.Response.Close();
                            continue;
                        }

                        ctx.Response.ContentLength64 = body.Length;
                        await ctx.Response.OutputStream.WriteAsync(body, 0, body.Length);
                        ctx.Response.OutputStream.Close();
                    }
                    catch { }
                }
            });

            try
            {
                StreamAuthOptions auth = new StreamAuthOptions
                {
                    AuthType = AuthType.BearerToken,
                    BearerToken = expectedToken
                };

                string manifestUrl = $"http://127.0.0.1:{port}/manifest.mpd";
                using StreamGrabber grabber = new StreamGrabber(manifestUrl, StreamFormat.DashMpd, auth, "TestCameraFeed");

                await grabber.StartAsync(CancellationToken.None);

                List<Mat> receivedFrames = new();
                using CancellationTokenSource readCts = new CancellationTokenSource(TimeSpan.FromSeconds(5));

                while (receivedFrames.Count < 5 && !readCts.IsCancellationRequested)
                {
                    Mat? frame = await grabber.ReadNextFrameAsync(readCts.Token);
                    if (frame != null)
                    {
                        receivedFrames.Add(frame);
                    }
                }

                Assert.IsTrue(authVerified, "Authentication header was not verified on server.");
                Assert.IsTrue(receivedFrames.Count > 0, "No frames were decoded from the authenticated DASH stream.");

                Mat firstFrame = receivedFrames[0];
                Assert.AreEqual(320, firstFrame.Width);
                Assert.AreEqual(240, firstFrame.Height);

                // Test screenshot saving
                string screenshotFolder = Path.Combine(_testDir!, "Screenshots");
                string savedScreenshot = ScreenshotHelper.SaveScreenshot(firstFrame, screenshotFolder, "TestShot");

                Assert.IsTrue(File.Exists(savedScreenshot), "Screenshot file was not saved.");
                FileInfo fileInfo = new FileInfo(savedScreenshot);
                Assert.IsTrue(fileInfo.Length > 100, "Screenshot file is too small.");

                foreach (Mat f in receivedFrames)
                {
                    f.Dispose();
                }

                grabber.Stop();
                Assert.IsFalse(grabber.IsRunning);
            }
            finally
            {
                cts.Cancel();
                listener.Stop();
            }
        }

        [TestMethod]
        public void StreamGrabber_DirectVideoFormat_SetsFormatCorrectlyWithoutBufferingFullMovie()
        {
            string videoUrl = "https://example.com/movies/giant_movie.m4v";
            using StreamGrabber grabber = new StreamGrabber(videoUrl);

            Assert.AreEqual(StreamFormat.M4vVideo, grabber.Format);
            Assert.AreEqual("giant_movie.m4v", grabber.StreamTitle);
            Assert.IsFalse(grabber.IsRunning);
        }
    }
}
