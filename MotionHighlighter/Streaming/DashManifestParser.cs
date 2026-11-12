using System.Globalization;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace MotionHighlighter.Streaming
{
    public class DashStreamInfo
    {
        public string? BaseUrl { get; set; }
        public string? InitializationUrl { get; set; }
        public List<string> SegmentUrls { get; } = new();
        public bool IsLive { get; set; }
        public int Timescale { get; set; } = 1;
        public long SegmentDurationUnits { get; set; }
        public double SegmentDurationSeconds { get; set; } = 2.0;
        public double TotalDurationSeconds { get; set; }
        public int TotalSegments { get; set; }
        public int StartNumber { get; set; } = 1;
        public string? MediaTemplate { get; set; }
        public string? RepresentationId { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public double Fps { get; set; } = 30.0;

        public IEnumerable<string> EnumerateSegmentUrls()
        {
            if (SegmentUrls.Count > 0 && (TotalSegments == 0 || TotalSegments <= SegmentUrls.Count))
            {
                foreach (string url in SegmentUrls)
                {
                    yield return url;
                }
            }
            else if (!string.IsNullOrEmpty(MediaTemplate) && !string.IsNullOrEmpty(BaseUrl))
            {
                int count = TotalSegments > 0 ? TotalSegments : Math.Max(SegmentUrls.Count, IsLive ? 50 : 250);
                Uri baseUri = new Uri(BaseUrl);
                for (int i = 0; i < count; i++)
                {
                    int n = StartNumber + i;
                    long time = i * SegmentDurationUnits;
                    string segRel = DashManifestParser.ResolveTemplate(MediaTemplate, RepresentationId ?? "0", n, time);
                    yield return new Uri(baseUri, segRel).ToString();
                }
            }
            else
            {
                foreach (string url in SegmentUrls)
                {
                    yield return url;
                }
            }
        }
    }

    public static class DashManifestParser
    {
        private static readonly XNamespace DefaultNs = "urn:mpeg:dash:schema:mpd:2011";

        public static DashStreamInfo Parse(string mpdXml, Uri manifestUri)
        {
            DashStreamInfo info = new DashStreamInfo();
            XDocument doc = XDocument.Parse(mpdXml);
            XElement? root = doc.Root;

            if (root == null)
            {
                throw new InvalidOperationException("Invalid DASH MPD document.");
            }

            string type = root.Attribute("type")?.Value ?? "static";
            info.IsLive = string.Equals(type, "dynamic", StringComparison.OrdinalIgnoreCase);

            string? durationStr = root.Attribute("mediaPresentationDuration")?.Value;
            if (!string.IsNullOrEmpty(durationStr))
            {
                try
                {
                    TimeSpan duration = System.Xml.XmlConvert.ToTimeSpan(durationStr);
                    info.TotalDurationSeconds = duration.TotalSeconds;
                }
                catch { }
            }

            // Base URL check
            string? manifestBaseUrl = root.Element(DefaultNs + "BaseURL")?.Value?.Trim() ??
                                     root.Element("BaseURL")?.Value?.Trim();

            Uri baseUri = manifestUri;
            if (!string.IsNullOrEmpty(manifestBaseUrl))
            {
                baseUri = new Uri(manifestUri, manifestBaseUrl);
            }
            info.BaseUrl = baseUri.ToString();

            // Find video AdaptationSet
            IEnumerable<XElement> periods = root.Elements(DefaultNs + "Period").Concat(root.Elements("Period"));
            XElement? period = periods.FirstOrDefault();
            if (period == null)
            {
                return info;
            }

            string? periodBase = period.Element(DefaultNs + "BaseURL")?.Value ?? period.Element("BaseURL")?.Value;
            if (!string.IsNullOrEmpty(periodBase))
            {
                baseUri = new Uri(baseUri, periodBase);
            }

            IEnumerable<XElement> adaptationSets = period.Elements(DefaultNs + "AdaptationSet").Concat(period.Elements("AdaptationSet"));
            XElement? videoAdaptation = adaptationSets.FirstOrDefault(a =>
                string.Equals(a.Attribute("contentType")?.Value, "video", StringComparison.OrdinalIgnoreCase) ||
                (a.Attribute("mimeType")?.Value ?? string.Empty).Contains("video", StringComparison.OrdinalIgnoreCase)) ??
                adaptationSets.FirstOrDefault();

            if (videoAdaptation == null)
            {
                return info;
            }

            // Find best representation (prefer <= 1080p height to keep memory and CPU usage optimal)
            List<XElement> representations = videoAdaptation.Elements(DefaultNs + "Representation").Concat(videoAdaptation.Elements("Representation")).ToList();
            XElement? rep = representations
                .Where(r =>
                {
                    int h = ParseInt(r.Attribute("height")?.Value);
                    return h > 0 && h <= 1080;
                })
                .OrderByDescending(r => ParseInt(r.Attribute("width")?.Value) * ParseInt(r.Attribute("height")?.Value))
                .ThenByDescending(r => ParseLong(r.Attribute("bandwidth")?.Value))
                .FirstOrDefault()
                ?? representations
                .OrderByDescending(r => ParseInt(r.Attribute("width")?.Value) * ParseInt(r.Attribute("height")?.Value))
                .ThenByDescending(r => ParseLong(r.Attribute("bandwidth")?.Value))
                .FirstOrDefault();

            string repId = rep?.Attribute("id")?.Value ?? "0";
            info.RepresentationId = repId;
            info.Width = ParseInt(rep?.Attribute("width")?.Value);
            info.Height = ParseInt(rep?.Attribute("height")?.Value);

            string? frameRateStr = rep?.Attribute("frameRate")?.Value ?? videoAdaptation.Attribute("frameRate")?.Value;
            if (!string.IsNullOrEmpty(frameRateStr))
            {
                info.Fps = ParseFrameRate(frameRateStr);
            }

            // SegmentTemplate can be on Representation or AdaptationSet
            XElement? segTemplate = rep?.Element(DefaultNs + "SegmentTemplate") ??
                              rep?.Element("SegmentTemplate") ??
                              videoAdaptation.Element(DefaultNs + "SegmentTemplate") ??
                              videoAdaptation.Element("SegmentTemplate");

            if (segTemplate != null)
            {
                int timescale = ParseInt(segTemplate.Attribute("timescale")?.Value);
                if (timescale <= 0) timescale = 1;
                info.Timescale = timescale;

                int startNumber = ParseInt(segTemplate.Attribute("startNumber")?.Value);
                if (startNumber <= 0) startNumber = 1;
                info.StartNumber = startNumber;

                string? initTemplate = segTemplate.Attribute("initialization")?.Value;
                if (!string.IsNullOrEmpty(initTemplate))
                {
                    string resolvedInit = ResolveTemplate(initTemplate, repId, 0, 0);
                    info.InitializationUrl = new Uri(baseUri, resolvedInit).ToString();
                }

                string? mediaTemplate = segTemplate.Attribute("media")?.Value;
                info.MediaTemplate = mediaTemplate;

                // Check SegmentTimeline
                XElement? timeline = segTemplate.Element(DefaultNs + "SegmentTimeline") ??
                               segTemplate.Element("SegmentTimeline");

                if (timeline != null && !string.IsNullOrEmpty(mediaTemplate))
                {
                    IEnumerable<XElement> sElements = timeline.Elements(DefaultNs + "S").Concat(timeline.Elements("S"));
                    long currentTime = 0;
                    int segIndex = startNumber;

                    foreach (XElement? s in sElements)
                    {
                        if (s.Attribute("t") != null)
                        {
                            currentTime = ParseLong(s.Attribute("t")?.Value);
                        }

                        long duration = ParseLong(s.Attribute("d")?.Value);
                        int repeat = ParseInt(s.Attribute("r")?.Value);

                        for (int r = 0; r <= repeat; r++)
                        {
                            string segRel = ResolveTemplate(mediaTemplate, repId, segIndex, currentTime);
                            info.SegmentUrls.Add(new Uri(baseUri, segRel).ToString());

                            currentTime += duration;
                            segIndex++;
                        }
                    }
                }
                else if (!string.IsNullOrEmpty(mediaTemplate))
                {
                    long durationVal = ParseLong(segTemplate.Attribute("duration")?.Value);
                    info.SegmentDurationUnits = durationVal;
                    if (durationVal > 0 && timescale > 0)
                    {
                        info.SegmentDurationSeconds = (double)durationVal / timescale;
                        if (info.TotalDurationSeconds > 0)
                        {
                            info.TotalSegments = (int)Math.Ceiling(info.TotalDurationSeconds / info.SegmentDurationSeconds);
                        }
                    }

                    // Pre-generate initial batch of segments for quick inspection (EnumerateSegmentUrls streams the rest on demand)
                    int count = info.IsLive ? 10 : Math.Min(30, info.TotalSegments > 0 ? info.TotalSegments : 30);
                    for (int n = startNumber; n < startNumber + count; n++)
                    {
                        string segRel = ResolveTemplate(mediaTemplate, repId, n, (long)(n - startNumber) * durationVal);
                        info.SegmentUrls.Add(new Uri(baseUri, segRel).ToString());
                    }
                }
            }

            // Check SegmentList if SegmentTemplate was not present
            XElement? segList = rep?.Element(DefaultNs + "SegmentList") ??
                          rep?.Element("SegmentList") ??
                          videoAdaptation.Element(DefaultNs + "SegmentList") ??
                          videoAdaptation.Element("SegmentList");

            if (segList != null)
            {
                XElement? initElem = segList.Element(DefaultNs + "Initialization") ?? segList.Element("Initialization");
                string? initUrl = initElem?.Attribute("sourceURL")?.Value;
                if (!string.IsNullOrEmpty(initUrl))
                {
                    info.InitializationUrl = new Uri(baseUri, initUrl).ToString();
                }

                IEnumerable<XElement> segUrls = segList.Elements(DefaultNs + "SegmentURL").Concat(segList.Elements("SegmentURL"));
                foreach (XElement? s in segUrls)
                {
                    string? media = s.Attribute("media")?.Value;
                    if (!string.IsNullOrEmpty(media))
                    {
                        info.SegmentUrls.Add(new Uri(baseUri, media).ToString());
                    }
                }
            }

            return info;
        }

        public static string ResolveTemplate(string template, string representationId, int number, long time)
        {
            string result = template.Replace("$RepresentationID$", representationId);

            // Replace $Number$ or $Number%05d$
            result = Regex.Replace(result, @"\$Number(?:%0?(\d+)d)?\$", m =>
            {
                if (m.Groups[1].Success && int.TryParse(m.Groups[1].Value, out int pad))
                {
                    return number.ToString(new string('0', pad), CultureInfo.InvariantCulture);
                }
                return number.ToString(CultureInfo.InvariantCulture);
            });

            // Replace $Time$ or $Time%05d$
            result = Regex.Replace(result, @"\$Time(?:%0?(\d+)d)?\$", m =>
            {
                if (m.Groups[1].Success && int.TryParse(m.Groups[1].Value, out int pad))
                {
                    return time.ToString(new string('0', pad), CultureInfo.InvariantCulture);
                }
                return time.ToString(CultureInfo.InvariantCulture);
            });

            return result;
        }

        private static int ParseInt(string? val)
        {
            return int.TryParse(val, NumberStyles.Integer, CultureInfo.InvariantCulture, out int result) ? result : 0;
        }

        private static long ParseLong(string? val)
        {
            return long.TryParse(val, NumberStyles.Integer, CultureInfo.InvariantCulture, out long result) ? result : 0L;
        }

        private static double ParseFrameRate(string frameRate)
        {
            if (string.IsNullOrWhiteSpace(frameRate))
            {
                return 30.0;
            }

            int slash = frameRate.IndexOf('/');
            if (slash > 0)
            {
                if (double.TryParse(frameRate.Substring(0, slash), NumberStyles.Float, CultureInfo.InvariantCulture, out double num) &&
                    double.TryParse(frameRate.Substring(slash + 1), NumberStyles.Float, CultureInfo.InvariantCulture, out double den) &&
                    den > 0)
                {
                    return num / den;
                }
            }

            return double.TryParse(frameRate, NumberStyles.Float, CultureInfo.InvariantCulture, out double fps) && fps > 0 ? fps : 30.0;
        }
    }
}
