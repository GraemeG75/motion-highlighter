using MotionHighlighter.Streaming;

namespace MotionHighlighter.Tests
{
    [TestClass]
    public class HlsPlaylistParserTests
    {
        [TestMethod]
        public void Parse_MasterPlaylist_ExtractsVariants()
        {
            string master = @"#EXTM3U
#EXT-X-STREAM-INF:BANDWIDTH=800000,RESOLUTION=640x360
low/index.m3u8
#EXT-X-STREAM-INF:BANDWIDTH=2500000,RESOLUTION=1280x720
mid/index.m3u8
#EXT-X-STREAM-INF:BANDWIDTH=5000000,RESOLUTION=1920x1080
high/index.m3u8";

            Uri uri = new Uri("https://video.example.com/hls/master.m3u8");
            HlsPlaylistInfo info = HlsPlaylistParser.Parse(master, uri);

            Assert.IsTrue(info.IsMasterPlaylist);
            Assert.AreEqual(3, info.Variants.Count);
            Assert.AreEqual("https://video.example.com/hls/high/index.m3u8", info.Variants[2].Url);
            Assert.AreEqual(5000000, info.Variants[2].Bandwidth);
            Assert.AreEqual("1920x1080", info.Variants[2].Resolution);
        }

        [TestMethod]
        public void Parse_Fmp4MediaPlaylist_ExtractsInitMapAndM4sChunks()
        {
            string playlist = @"#EXTM3U
#EXT-X-VERSION:7
#EXT-X-TARGETDURATION:2
#EXT-X-MEDIA-SEQUENCE:1
#EXT-X-MAP:URI=""init.mp4""
#EXTINF:2.000,
chunk-001.m4s
#EXTINF:2.000,
chunk-002.m4s
#EXTINF:2.000,
chunk-003.m4s
#EXT-X-ENDLIST";

            Uri uri = new Uri("https://live.example.com/streams/cam1/playlist.m3u8");
            HlsPlaylistInfo info = HlsPlaylistParser.Parse(playlist, uri);

            Assert.IsFalse(info.IsMasterPlaylist);
            Assert.IsFalse(info.IsLive); // Has #EXT-X-ENDLIST
            Assert.AreEqual("https://live.example.com/streams/cam1/init.mp4", info.InitializationUrl);
            Assert.AreEqual(3, info.SegmentUrls.Count);
            Assert.AreEqual("https://live.example.com/streams/cam1/chunk-001.m4s", info.SegmentUrls[0]);
            Assert.AreEqual("https://live.example.com/streams/cam1/chunk-002.m4s", info.SegmentUrls[1]);
            Assert.AreEqual("https://live.example.com/streams/cam1/chunk-003.m4s", info.SegmentUrls[2]);
        }

        [TestMethod]
        public void Parse_LivePlaylistWithoutEndList_IdentifiesAsLive()
        {
            string playlist = @"#EXTM3U
#EXT-X-VERSION:3
#EXT-X-TARGETDURATION:3
#EXT-X-MEDIA-SEQUENCE:42
#EXTINF:3.000,
segment42.ts
#EXTINF:3.000,
segment43.ts";

            Uri uri = new Uri("https://live.example.com/live.m3u8");
            HlsPlaylistInfo info = HlsPlaylistParser.Parse(playlist, uri);

            Assert.IsTrue(info.IsLive);
            Assert.AreEqual(2, info.SegmentUrls.Count);
            Assert.AreEqual("https://live.example.com/segment42.ts", info.SegmentUrls[0]);
        }
    }
}
