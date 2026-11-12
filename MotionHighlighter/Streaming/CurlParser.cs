using System.Text.RegularExpressions;

namespace MotionHighlighter.Streaming
{
    public static class CurlParser
    {
        private static readonly Regex HeaderRegex = new(
            @"-H\s+[""']([^""']+)[""']",
            RegexOptions.Compiled | RegexOptions.IgnoreCase);

        private static readonly Regex UrlRegex = new(
            @"curl(?:\.exe)?\s+(?:--[a-z0-9-]+\s+|-[a-zA-Z0-9]\s+(?:[""'][^""']*[""']|\S+)\s+)*[""']?(https?://[^\s""']+)[""']?",
            RegexOptions.Compiled | RegexOptions.IgnoreCase);

        public static bool TryParse(string curlCommand, out string url, out StreamAuthOptions auth)
        {
            url = string.Empty;
            auth = new StreamAuthOptions();

            if (string.IsNullOrWhiteSpace(curlCommand))
            {
                return false;
            }

            string clean = curlCommand
                .Replace("\\\r\n", " ")
                .Replace("\\\n", " ")
                .Replace("^\r\n", " ")
                .Replace("^\n", " ");

            // Extract URL
            Match urlMatch = UrlRegex.Match(clean);
            if (urlMatch.Success && urlMatch.Groups.Count > 1)
            {
                url = urlMatch.Groups[1].Value.Trim();
            }
            else
            {
                // Fallback: look for any http(s) URL in command
                Match genericMatch = Regex.Match(clean, @"[""'](https?://[^""'\s]+)[""']", RegexOptions.IgnoreCase);
                if (genericMatch.Success)
                {
                    url = genericMatch.Groups[1].Value.Trim();
                }
                else
                {
                    return false;
                }
            }

            // Extract Headers
            MatchCollection headerMatches = HeaderRegex.Matches(clean);
            foreach (Match m in headerMatches)
            {
                if (m.Groups.Count <= 1) continue;
                string headerLine = m.Groups[1].Value.Trim();
                int colonIdx = headerLine.IndexOf(':');
                if (colonIdx > 0)
                {
                    string name = headerLine.Substring(0, colonIdx).Trim();
                    string value = headerLine.Substring(colonIdx + 1).Trim();

                    if (string.Equals(name, "Authorization", StringComparison.OrdinalIgnoreCase))
                    {
                        if (value.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
                        {
                            auth.AuthType = AuthType.BearerToken;
                            auth.BearerToken = value.Substring(7).Trim();
                        }
                        else if (value.StartsWith("Basic ", StringComparison.OrdinalIgnoreCase))
                        {
                            auth.AuthType = AuthType.BasicAuth;
                            auth.CustomHeaders["Authorization"] = value;
                        }
                        else
                        {
                            auth.CustomHeaders["Authorization"] = value;
                        }
                    }
                    else if (string.Equals(name, "User-Agent", StringComparison.OrdinalIgnoreCase))
                    {
                        auth.UserAgent = value;
                    }
                    else
                    {
                        auth.CustomHeaders[name] = value;
                    }
                }
            }

            if (auth.AuthType == AuthType.None && auth.CustomHeaders.Count > 0)
            {
                auth.AuthType = AuthType.CustomHeaders;
            }

            return !string.IsNullOrEmpty(url);
        }
    }
}
