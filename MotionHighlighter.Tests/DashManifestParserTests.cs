using MotionHighlighter.Streaming;

namespace MotionHighlighter.Tests
{
    [TestClass]
    public class DashManifestParserTests
    {
        [TestMethod]
        public void Parse_SegmentTemplateWithTimeline_ExtractsInitAndChunks()
        {
            string mpd = @"<?xml version=""1.0"" encoding=""utf-8""?>
<MPD xmlns=""urn:mpeg:dash:schema:mpd:2011"" profiles=""urn:mpeg:dash:profile:isoff-live:2011"" type=""static"">
    <Period id=""0"" start=""PT0.0S"">
        <AdaptationSet id=""0"" contentType=""video"" frameRate=""30/1"">
            <Representation id=""video_1080p"" mimeType=""video/mp4"" width=""1920"" height=""1080"">
                <SegmentTemplate timescale=""1000"" initialization=""$RepresentationID$-init.m4s"" media=""$RepresentationID$-chunk-$Number%04d$.m4s"" startNumber=""1"">
                    <SegmentTimeline>
                        <S t=""0"" d=""2000"" r=""2"" />
                    </SegmentTimeline>
                </SegmentTemplate>
            </Representation>
        </AdaptationSet>
    </Period>
</MPD>";

            Uri manifestUri = new Uri("https://streaming.example.com/live/manifest.mpd");
            DashStreamInfo info = DashManifestParser.Parse(mpd, manifestUri);

            Assert.IsFalse(info.IsLive);
            Assert.AreEqual("https://streaming.example.com/live/video_1080p-init.m4s", info.InitializationUrl);
            Assert.AreEqual(3, info.SegmentUrls.Count);
            Assert.AreEqual("https://streaming.example.com/live/video_1080p-chunk-0001.m4s", info.SegmentUrls[0]);
            Assert.AreEqual("https://streaming.example.com/live/video_1080p-chunk-0002.m4s", info.SegmentUrls[1]);
            Assert.AreEqual("https://streaming.example.com/live/video_1080p-chunk-0003.m4s", info.SegmentUrls[2]);
            Assert.AreEqual(1920, info.Width);
            Assert.AreEqual(1080, info.Height);
            Assert.AreEqual(30.0, info.Fps);
        }

        [TestMethod]
        public void ResolveTemplate_HandlesNumberAndPadding()
        {
            string template = "chunk-$Number%05d$.m4s";
            string resolved = DashManifestParser.ResolveTemplate(template, "0", 42, 0);
            Assert.AreEqual("chunk-00042.m4s", resolved);

            string repTemplate = "$RepresentationID$_$Number$.m4s";
            string repResolved = DashManifestParser.ResolveTemplate(repTemplate, "hd", 7, 0);
            Assert.AreEqual("hd_7.m4s", repResolved);
        }

        [TestMethod]
        public void Parse_DynamicMpd_IdentifiesAsLiveStream()
        {
            string mpd = @"<?xml version=""1.0"" encoding=""utf-8""?>
<MPD xmlns=""urn:mpeg:dash:schema:mpd:2011"" type=""dynamic"">
    <Period id=""0"">
        <AdaptationSet id=""0"" contentType=""video"">
            <Representation id=""1"">
                <SegmentTemplate timescale=""1000"" duration=""2000"" initialization=""init.m4s"" media=""chunk-$Number$.m4s"" startNumber=""100"" />
            </Representation>
        </AdaptationSet>
    </Period>
</MPD>";

            Uri manifestUri = new Uri("https://live.example.com/feed.mpd");
            DashStreamInfo info = DashManifestParser.Parse(mpd, manifestUri);

            Assert.IsTrue(info.IsLive);
            Assert.AreEqual("https://live.example.com/init.m4s", info.InitializationUrl);
            Assert.IsTrue(info.SegmentUrls.Count > 0);
            Assert.AreEqual("https://live.example.com/chunk-100.m4s", info.SegmentUrls[0]);
        }

        [TestMethod]
        public void Parse_Prefers1080pOver4K_ToAvoidHighRam()
        {
            string mpd = @"<?xml version=""1.0"" encoding=""utf-8""?>
<MPD xmlns=""urn:mpeg:dash:schema:mpd:2011"" profiles=""urn:mpeg:dash:profile:isoff-live:2011"" type=""static"">
    <Period id=""0"">
        <AdaptationSet id=""0"" contentType=""video"">
            <Representation id=""video_4k"" width=""3840"" height=""2160"" bandwidth=""12000000"">
                <SegmentTemplate timescale=""1000"" initialization=""init_4k.m4s"" media=""chunk_4k_$Number$.m4s"" startNumber=""1"" duration=""2000"" />
            </Representation>
            <Representation id=""video_1080p"" width=""1920"" height=""1080"" bandwidth=""4000000"">
                <SegmentTemplate timescale=""1000"" initialization=""init_1080p.m4s"" media=""chunk_1080p_$Number$.m4s"" startNumber=""1"" duration=""2000"" />
            </Representation>
            <Representation id=""video_720p"" width=""1280"" height=""720"" bandwidth=""2000000"">
                <SegmentTemplate timescale=""1000"" initialization=""init_720p.m4s"" media=""chunk_720p_$Number$.m4s"" startNumber=""1"" duration=""2000"" />
            </Representation>
        </AdaptationSet>
    </Period>
</MPD>";

            Uri manifestUri = new Uri("https://streaming.example.com/live/manifest.mpd");
            DashStreamInfo info = DashManifestParser.Parse(mpd, manifestUri);

            Assert.AreEqual("video_1080p", info.RepresentationId);
            Assert.AreEqual(1920, info.Width);
            Assert.AreEqual(1080, info.Height);
            Assert.AreEqual("https://streaming.example.com/live/init_1080p.m4s", info.InitializationUrl);
        }

        [TestMethod]
        public void EnumerateSegmentUrls_LazilyYieldsSegmentsBasedOnPresentationDuration()
        {
            string mpd = @"<?xml version=""1.0"" encoding=""utf-8""?>
<MPD xmlns=""urn:mpeg:dash:schema:mpd:2011"" mediaPresentationDuration=""PT600S"" type=""static"">
    <Period id=""0"">
        <AdaptationSet id=""0"" contentType=""video"">
            <Representation id=""1080p"" width=""1920"" height=""1080"">
                <SegmentTemplate timescale=""1000"" duration=""2000"" initialization=""init.m4s"" media=""chunk_$Number%04d$.m4s"" startNumber=""1"" />
            </Representation>
        </AdaptationSet>
    </Period>
</MPD>";

            Uri manifestUri = new Uri("https://streaming.example.com/movie/manifest.mpd");
            DashStreamInfo info = DashManifestParser.Parse(mpd, manifestUri);

            Assert.AreEqual(600.0, info.TotalDurationSeconds);
            Assert.AreEqual(2.0, info.SegmentDurationSeconds);
            Assert.AreEqual(300, info.TotalSegments);

            // Verify lazy enumeration yields all 300 segments on demand
            int count = 0;
            string? lastUrl = null;
            foreach (string url in info.EnumerateSegmentUrls())
            {
                count++;
                lastUrl = url;
            }

            Assert.AreEqual(300, count);
            Assert.AreEqual("https://streaming.example.com/movie/chunk_0300.m4s", lastUrl);
        }
    }
}
