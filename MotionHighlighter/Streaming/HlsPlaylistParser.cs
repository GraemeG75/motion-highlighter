using System.Globalization;
using System.Text.RegularExpressions;

namespace MotionHighlighter.Streaming
{
    public class HlsPlaylistInfo
    {
        public bool IsMasterPlaylist { get; set; }
        public bool IsLive { get; set; }
        public string? InitializationUrl { get; set; }
        public List<string> SegmentUrls { get; } = new();
        public List<HlsStreamVariant> Variants { get; } = new();
        public double TargetDurationSeconds { get; set; } = 2.0;
        public long MediaSequence { get; set; }
    }

    public class HlsStreamVariant
    {
        public string Url { get; set; } = string.Empty;
        public int Bandwidth { get; set; }
        public string Resolution { get; set; } = string.Empty;
        public string Codecs { get; set; } = string.Empty;
    }

    public static class HlsPlaylistParser
    {
        private static readonly Regex ExtInfRegex = new(@"#EXTINF:\s*([0-9.]+)", RegexOptions.Compiled);
        private static readonly Regex ExtMapRegex = new(@"#EXT-X-MAP:URI=""([^""]+)""", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        private static readonly Regex StreamInfRegex = new(@"#EXT-X-STREAM-INF:(.+)", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        private static readonly Regex BandwidthRegex = new(@"BANDWIDTH=(\d+)", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        private static readonly Regex ResolutionRegex = new(@"RESOLUTION=(\d+x\d+)", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        public static HlsPlaylistInfo Parse(string m3u8Content, Uri playlistUri)
        {
            HlsPlaylistInfo info = new HlsPlaylistInfo();
            if (string.IsNullOrWhiteSpace(m3u8Content))
            {
                return info;
            }

            string[] lines = m3u8Content.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries);
            bool hasEndList = false;

            for (int i = 0; i < lines.Length; i++)
            {
                string line = lines[i].Trim();
                if (string.IsNullOrEmpty(line))
                {
                    continue;
                }

                if (line.StartsWith("#EXT-X-STREAM-INF:", StringComparison.OrdinalIgnoreCase))
                {
                    info.IsMasterPlaylist = true;
                    Match match = StreamInfRegex.Match(line);
                    int bandwidth = 0;
                    string res = string.Empty;

                    if (match.Success)
                    {
                        Match bwMatch = BandwidthRegex.Match(line);
                        if (bwMatch.Success) int.TryParse(bwMatch.Groups[1].Value, out bandwidth);

                        Match resMatch = ResolutionRegex.Match(line);
                        if (resMatch.Success) res = resMatch.Groups[1].Value;
                    }

                    // Next non-comment line is the variant URL
                    while (i + 1 < lines.Length && lines[i + 1].StartsWith("#"))
                    {
                        i++;
                    }

                    if (i + 1 < lines.Length)
                    {
                        i++;
                        string variantRel = lines[i].Trim();
                        if (Uri.TryCreate(playlistUri, variantRel, out Uri? variantAbs))
                        {
                            info.Variants.Add(new HlsStreamVariant
                            {
                                Url = variantAbs.ToString(),
                                Bandwidth = bandwidth,
                                Resolution = res
                            });
                        }
                    }
                }
                else if (line.StartsWith("#EXT-X-MAP:", StringComparison.OrdinalIgnoreCase))
                {
                    Match mapMatch = ExtMapRegex.Match(line);
                    if (mapMatch.Success)
                    {
                        string initRel = mapMatch.Groups[1].Value;
                        if (Uri.TryCreate(playlistUri, initRel, out Uri? initAbs))
                        {
                            info.InitializationUrl = initAbs.ToString();
                        }
                    }
                }
                else if (line.StartsWith("#EXT-X-TARGETDURATION:", StringComparison.OrdinalIgnoreCase))
                {
                    string durStr = line.Substring("#EXT-X-TARGETDURATION:".Length).Trim();
                    if (double.TryParse(durStr, NumberStyles.Float, CultureInfo.InvariantCulture, out double td))
                    {
                        info.TargetDurationSeconds = td;
                    }
                }
                else if (line.StartsWith("#EXT-X-MEDIA-SEQUENCE:", StringComparison.OrdinalIgnoreCase))
                {
                    string seqStr = line.Substring("#EXT-X-MEDIA-SEQUENCE:".Length).Trim();
                    if (long.TryParse(seqStr, NumberStyles.Integer, CultureInfo.InvariantCulture, out long seq))
                    {
                        info.MediaSequence = seq;
                    }
                }
                else if (line.StartsWith("#EXT-X-ENDLIST", StringComparison.OrdinalIgnoreCase))
                {
                    hasEndList = true;
                }
                else if (line.StartsWith("#EXTINF:", StringComparison.OrdinalIgnoreCase))
                {
                    // Next non-comment line is the segment URL
                    while (i + 1 < lines.Length && lines[i + 1].StartsWith("#"))
                    {
                        i++;
                    }

                    if (i + 1 < lines.Length)
                    {
                        i++;
                        string segRel = lines[i].Trim();
                        if (Uri.TryCreate(playlistUri, segRel, out Uri? segAbs))
                        {
                            info.SegmentUrls.Add(segAbs.ToString());
                        }
                    }
                }
            }

            if (!info.IsMasterPlaylist)
            {
                info.IsLive = !hasEndList;
            }

            return info;
        }
    }
}
