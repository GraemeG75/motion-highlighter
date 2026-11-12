using System.Net;
using System.Text.RegularExpressions;
using HtmlAgilityPack;

namespace MotionHighlighter.Streaming
{
    public class WebpageStreamExtractor
    {
        private static readonly HttpClient DefaultHttpClient = new(new HttpClientHandler
        {
            AllowAutoRedirect = true,
            AutomaticDecompression = DecompressionMethods.All
        })
        {
            Timeout = TimeSpan.FromSeconds(15)
        };

        private static readonly Regex MediaUrlRegex = new(
            @"(?:https?:\\?/\\?/[^\s""'<>\\]+|(?:\/[^\s""'<>\\]+))\.(?:m3u8|mpd|m4s|m4v|mp4|webm|ts)(?:\?[^\s""'<>\\]*)?",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private static readonly Regex JsPlayerConfigRegex = new(
            @"(?:source|file|src|url|hls|dash|manifest|playlist|streamUrl|videoUrl)\s*[:=]\s*[""']([^""']+\.(?:m3u8|mpd|m4s|m4v|mp4|webm|ts)[^""']*)[""']",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

        public async Task<List<StreamCandidate>> ExtractStreamsFromUrlAsync(
            string pageUrl,
            StreamAuthOptions? auth = null,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(pageUrl))
            {
                return new List<StreamCandidate>();
            }

            if (!Uri.TryCreate(pageUrl, UriKind.Absolute, out Uri? baseUri))
            {
                throw new ArgumentException("Invalid URL format.", nameof(pageUrl));
            }

            // If the URL itself directly points to a media stream / manifest, return it directly
            StreamFormat directFormat = DetectFormatFromUrl(pageUrl);
            if (directFormat != StreamFormat.Unknown)
            {
                return new List<StreamCandidate>
                {
                    new StreamCandidate
                    {
                        Url = pageUrl,
                        Format = directFormat,
                        Title = $"Direct Stream ({directFormat})",
                        Details = "Provided URL points directly to media resource",
                        SourceElement = "Direct URL"
                    }
                };
            }

            // Apply auth to target URI if query params are present
            if (auth != null)
            {
                baseUri = auth.ApplyToUri(baseUri);
            }

            using HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, baseUri);
            auth?.ApplyToRequest(request, baseUri);

            using HttpResponseMessage response = await DefaultHttpClient.SendAsync(request, HttpCompletionOption.ResponseContentRead, cancellationToken);
            response.EnsureSuccessStatusCode();

            string contentType = response.Content.Headers.ContentType?.MediaType ?? string.Empty;

            // If response is already an m3u8 or mpd stream content
            if (contentType.Contains("mpegurl", StringComparison.OrdinalIgnoreCase) ||
                contentType.Contains("dash+xml", StringComparison.OrdinalIgnoreCase) ||
                contentType.Contains("video/", StringComparison.OrdinalIgnoreCase))
            {
                StreamFormat responseFormat = DetectFormatFromContentType(contentType, baseUri.ToString());
                return new List<StreamCandidate>
                {
                    new StreamCandidate
                    {
                        Url = baseUri.ToString(),
                        Format = responseFormat,
                        Title = $"Direct Stream ({responseFormat})",
                        Details = $"HTTP Content-Type: {contentType}",
                        SourceElement = "HTTP Response"
                    }
                };
            }

            string html = await response.Content.ReadAsStringAsync(cancellationToken);
            return ExtractStreamsFromHtml(html, baseUri);
        }

        public List<StreamCandidate> ExtractStreamsFromHtml(string html, Uri baseUri)
        {
            List<StreamCandidate> results = new List<StreamCandidate>();
            HashSet<string> seenUrls = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            if (string.IsNullOrWhiteSpace(html))
            {
                return results;
            }

            HtmlAgilityPack.HtmlDocument doc = new HtmlAgilityPack.HtmlDocument();
            doc.LoadHtml(html);

            // 1. Scan HTML5 <video> elements
            HtmlNodeCollection? videoNodes = doc.DocumentNode.SelectNodes("//video");
            if (videoNodes != null)
            {
                foreach (HtmlNode? video in videoNodes)
                {
                    string src = video.GetAttributeValue("src", string.Empty);
                    if (!string.IsNullOrWhiteSpace(src))
                    {
                        AddCandidate(results, seenUrls, src, baseUri, "<video src>", video.GetAttributeValue("type", null));
                    }

                    // Check child <source> elements inside <video>
                    HtmlNodeCollection? sources = video.SelectNodes(".//source");
                    if (sources != null)
                    {
                        foreach (HtmlNode? source in sources)
                        {
                            string sourceSrc = source.GetAttributeValue("src", string.Empty);
                            string type = source.GetAttributeValue("type", string.Empty);
                            AddCandidate(results, seenUrls, sourceSrc, baseUri, "<source src>", type);
                        }
                    }
                }
            }

            // 2. Scan standalone <source> tags anywhere in DOM
            HtmlNodeCollection? allSourceNodes = doc.DocumentNode.SelectNodes("//source[@src]");
            if (allSourceNodes != null)
            {
                foreach (HtmlNode? source in allSourceNodes)
                {
                    string sourceSrc = source.GetAttributeValue("src", string.Empty);
                    string type = source.GetAttributeValue("type", string.Empty);
                    AddCandidate(results, seenUrls, sourceSrc, baseUri, "<source src>", type);
                }
            }

            // 3. Scan Open Graph and Twitter video tags
            HtmlNodeCollection? metaNodes = doc.DocumentNode.SelectNodes("//meta[@property='og:video' or @property='og:video:url' or @name='twitter:player:stream']");
            if (metaNodes != null)
            {
                foreach (HtmlNode? meta in metaNodes)
                {
                    string content = meta.GetAttributeValue("content", string.Empty);
                    AddCandidate(results, seenUrls, content, baseUri, $"<meta {meta.Name}>", null);
                }
            }

            // 4. Scan <link> preload / alternate tags
            HtmlNodeCollection? linkNodes = doc.DocumentNode.SelectNodes("//link[@rel='alternate' or @rel='preload']");
            if (linkNodes != null)
            {
                foreach (HtmlNode? link in linkNodes)
                {
                    string href = link.GetAttributeValue("href", string.Empty);
                    string type = link.GetAttributeValue("type", string.Empty);
                    AddCandidate(results, seenUrls, href, baseUri, $"<link rel='{link.GetAttributeValue("rel", "")}'>", type);
                }
            }

            // 5. Scan <script> blocks for JavaScript player configs and URLs
            HtmlNodeCollection? scriptNodes = doc.DocumentNode.SelectNodes("//script");
            if (scriptNodes != null)
            {
                foreach (HtmlNode? script in scriptNodes)
                {
                    string scriptContent = script.InnerText;
                    if (string.IsNullOrWhiteSpace(scriptContent))
                    {
                        continue;
                    }

                    // Unescape JSON / JS slashes (e.g. "\/" -> "/" and "\u002F" -> "/")
                    string unescaped = scriptContent
                        .Replace("\\/", "/")
                        .Replace("\\u002F", "/")
                        .Replace("\\u0026", "&");

                    // Match player config keys (file: "...", source: "...")
                    foreach (Match m in JsPlayerConfigRegex.Matches(unescaped))
                    {
                        if (m.Groups.Count > 1)
                        {
                            string url = m.Groups[1].Value.Trim();
                            AddCandidate(results, seenUrls, url, baseUri, "JavaScript Player Config", null);
                        }
                    }

                    // Match all regex media URLs in scripts
                    foreach (Match m in MediaUrlRegex.Matches(unescaped))
                    {
                        string url = m.Value.Trim();
                        AddCandidate(results, seenUrls, url, baseUri, "JavaScript Embedded URL", null);
                    }
                }
            }

            // 6. Generic regex pass over entire HTML for any remaining stream URLs
            string cleanHtml = html.Replace("\\/", "/");
            foreach (Match m in MediaUrlRegex.Matches(cleanHtml))
            {
                string url = m.Value.Trim();
                AddCandidate(results, seenUrls, url, baseUri, "HTML Text Scan", null);
            }

            // Sort: prioritize DASH (.mpd) and HLS (.m3u8), then fragmented MP4 (.m4s), then .m4v / .mp4
            return results.OrderBy(c => GetFormatPriority(c.Format)).ToList();
        }

        private static int GetFormatPriority(StreamFormat format)
        {
            return format switch
            {
                StreamFormat.DashMpd => 1,
                StreamFormat.HlsM3u8 => 2,
                StreamFormat.FragmentedMp4 => 3,
                StreamFormat.M4vVideo => 4,
                StreamFormat.StandardVideo => 5,
                StreamFormat.Mjpeg => 6,
                StreamFormat.Rtsp => 7,
                _ => 10
            };
        }

        private static void AddCandidate(
            List<StreamCandidate> list,
            HashSet<string> seen,
            string? rawUrl,
            Uri baseUri,
            string sourceDesc,
            string? mimeType)
        {
            if (string.IsNullOrWhiteSpace(rawUrl))
            {
                return;
            }

            rawUrl = WebUtility.HtmlDecode(rawUrl.Trim());

            // Handle protocol-relative URLs
            if (rawUrl.StartsWith("//"))
            {
                rawUrl = baseUri.Scheme + ":" + rawUrl;
            }

            if (!Uri.TryCreate(baseUri, rawUrl, out Uri? absoluteUri))
            {
                return;
            }

            string fullUrl = absoluteUri.ToString();
            if (seen.Contains(fullUrl))
            {
                return;
            }

            StreamFormat format = DetectFormatFromContentType(mimeType ?? string.Empty, fullUrl);
            if (format == StreamFormat.Unknown)
            {
                format = DetectFormatFromUrl(fullUrl);
            }

            if (format == StreamFormat.Unknown)
            {
                return;
            }

            seen.Add(fullUrl);

            string title = Path.GetFileName(absoluteUri.AbsolutePath);
            if (string.IsNullOrEmpty(title))
            {
                title = absoluteUri.Host;
            }

            list.Add(new StreamCandidate
            {
                Url = fullUrl,
                Format = format,
                Title = title,
                Details = $"{format} via {sourceDesc}",
                SourceElement = sourceDesc
            });
        }

        public static StreamFormat DetectFormatFromUrl(string url)
        {
            if (string.IsNullOrWhiteSpace(url))
            {
                return StreamFormat.Unknown;
            }

            if (url.StartsWith("webpage://", StringComparison.OrdinalIgnoreCase) ||
                url.Contains("home.google.com", StringComparison.OrdinalIgnoreCase) ||
                url.Contains("nest.com", StringComparison.OrdinalIgnoreCase) ||
                url.Contains("ring.com", StringComparison.OrdinalIgnoreCase) ||
                url.Contains("arlo.com", StringComparison.OrdinalIgnoreCase) ||
                url.Contains("wyze.com", StringComparison.OrdinalIgnoreCase) ||
                url.Contains("eufylife.com", StringComparison.OrdinalIgnoreCase) ||
                url.Contains("unifi.ui.com", StringComparison.OrdinalIgnoreCase))
            {
                return StreamFormat.WebLiveFeed;
            }

            if (url.StartsWith("rtsp://", StringComparison.OrdinalIgnoreCase))
            {
                return StreamFormat.Rtsp;
            }

            string path = url;
            int qIndex = path.IndexOf('?');
            if (qIndex >= 0)
            {
                path = path.Substring(0, qIndex);
            }

            if (path.EndsWith(".mpd", StringComparison.OrdinalIgnoreCase))
            {
                return StreamFormat.DashMpd;
            }
            if (path.EndsWith(".m3u8", StringComparison.OrdinalIgnoreCase))
            {
                return StreamFormat.HlsM3u8;
            }
            if (path.EndsWith(".m4s", StringComparison.OrdinalIgnoreCase))
            {
                return StreamFormat.FragmentedMp4;
            }
            if (path.EndsWith(".m4v", StringComparison.OrdinalIgnoreCase))
            {
                return StreamFormat.M4vVideo;
            }
            if (path.EndsWith(".mp4", StringComparison.OrdinalIgnoreCase))
            {
                return StreamFormat.StandardVideo;
            }
            if (path.EndsWith(".webm", StringComparison.OrdinalIgnoreCase) || path.EndsWith(".ts", StringComparison.OrdinalIgnoreCase))
            {
                return StreamFormat.StandardVideo;
            }
            if (path.Contains("mjpeg", StringComparison.OrdinalIgnoreCase) || path.Contains("mjpg", StringComparison.OrdinalIgnoreCase))
            {
                return StreamFormat.Mjpeg;
            }

            // If it's a web URL without a direct video file extension, it represents a web camera portal or live page feed
            if (url.StartsWith("http://", StringComparison.OrdinalIgnoreCase) || url.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
            {
                return StreamFormat.WebLiveFeed;
            }

            return StreamFormat.Unknown;
        }

        public static StreamFormat DetectFormatFromContentType(string contentType, string url)
        {
            if (string.IsNullOrWhiteSpace(contentType))
            {
                return DetectFormatFromUrl(url);
            }

            if (contentType.Contains("dash+xml", StringComparison.OrdinalIgnoreCase))
            {
                return StreamFormat.DashMpd;
            }
            if (contentType.Contains("mpegurl", StringComparison.OrdinalIgnoreCase) || contentType.Contains("x-mpegurl", StringComparison.OrdinalIgnoreCase))
            {
                return StreamFormat.HlsM3u8;
            }
            if (contentType.Contains("video/iso.segment", StringComparison.OrdinalIgnoreCase))
            {
                return StreamFormat.FragmentedMp4;
            }
            if (contentType.Contains("video/x-m4v", StringComparison.OrdinalIgnoreCase))
            {
                return StreamFormat.M4vVideo;
            }
            if (contentType.Contains("video/mp4", StringComparison.OrdinalIgnoreCase))
            {
                return DetectFormatFromUrl(url) == StreamFormat.FragmentedMp4 ? StreamFormat.FragmentedMp4 : StreamFormat.StandardVideo;
            }
            if (contentType.Contains("multipart/x-mixed-replace", StringComparison.OrdinalIgnoreCase))
            {
                return StreamFormat.Mjpeg;
            }

            return DetectFormatFromUrl(url);
        }
    }
}
