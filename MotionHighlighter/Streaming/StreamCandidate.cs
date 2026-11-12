namespace MotionHighlighter.Streaming
{
    public enum StreamFormat
    {
        DashMpd = 1,
        HlsM3u8 = 2,
        FragmentedMp4 = 3,
        M4vVideo = 4,
        StandardVideo = 5,
        Mjpeg = 6,
        Rtsp = 7,
        WebLiveFeed = 8,
        Unknown = 99
    }

    public class StreamCandidate
    {
        public string Url { get; set; } = string.Empty;
        public StreamFormat Format { get; set; } = StreamFormat.Unknown;
        public string Title { get; set; } = string.Empty;
        public string Details { get; set; } = string.Empty;
        public string SourceElement { get; set; } = string.Empty;

        public override string ToString()
        {
            return $"[{Format}] {Title} - {Url}";
        }
    }
}
