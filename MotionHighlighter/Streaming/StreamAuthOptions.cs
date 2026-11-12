using System.Collections.Specialized;
using System.Net.Http.Headers;
using System.Text;

namespace MotionHighlighter.Streaming
{
    public enum AuthType
    {
        None = 0,
        BearerToken = 1,
        BasicAuth = 2,
        CustomHeaders = 3,
        QueryParameters = 4
    }

    public class StreamAuthOptions
    {
        public AuthType AuthType { get; set; } = AuthType.None;
        public string? Username { get; set; }
        public string? Password { get; set; }
        public string? BearerToken { get; set; }
        public string? UserAgent { get; set; } = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/130.0.0.0 Safari/537.36";
        public Dictionary<string, string> CustomHeaders { get; } = new(StringComparer.OrdinalIgnoreCase);
        public Dictionary<string, string> QueryParameters { get; } = new(StringComparer.OrdinalIgnoreCase);

        public void ApplyToRequest(HttpRequestMessage request, Uri? targetUri = null)
        {
            if (!string.IsNullOrWhiteSpace(UserAgent))
            {
                request.Headers.UserAgent.Clear();
                request.Headers.TryAddWithoutValidation("User-Agent", UserAgent);
            }

            switch (AuthType)
            {
                case AuthType.BearerToken:
                    if (!string.IsNullOrWhiteSpace(BearerToken))
                    {
                        string token = BearerToken.Trim();
                        if (token.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
                        {
                            token = token.Substring(7).Trim();
                        }
                        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
                    }
                    break;

                case AuthType.BasicAuth:
                    if (!string.IsNullOrEmpty(Username))
                    {
                        string credentials = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{Username}:{Password ?? string.Empty}"));
                        request.Headers.Authorization = new AuthenticationHeaderValue("Basic", credentials);
                    }
                    break;
            }

            // Apply any additional custom headers (e.g. Cookie, Referer, X-API-Key, or manual Authorization)
            foreach (KeyValuePair<string, string> kvp in CustomHeaders)
            {
                if (string.IsNullOrWhiteSpace(kvp.Key))
                {
                    continue;
                }

                if (string.Equals(kvp.Key, "Authorization", StringComparison.OrdinalIgnoreCase) && request.Headers.Authorization != null)
                {
                    // If Authorization is already set by Bearer/Basic, don't overwrite unless explicitly defined in CustomHeaders
                    continue;
                }

                request.Headers.TryAddWithoutValidation(kvp.Key.Trim(), kvp.Value?.Trim() ?? string.Empty);
            }
        }

        public Uri ApplyToUri(Uri uri)
        {
            if (QueryParameters.Count == 0)
            {
                return uri;
            }

            UriBuilder ub = new UriBuilder(uri);
            NameValueCollection query = System.Web.HttpUtility.ParseQueryString(ub.Query);

            foreach (KeyValuePair<string, string> kvp in QueryParameters)
            {
                if (!string.IsNullOrWhiteSpace(kvp.Key))
                {
                    query[kvp.Key.Trim()] = kvp.Value?.Trim() ?? string.Empty;
                }
            }

            ub.Query = query.ToString();
            return ub.Uri;
        }

        public StreamAuthOptions Clone()
        {
            StreamAuthOptions clone = new StreamAuthOptions
            {
                AuthType = AuthType,
                Username = Username,
                Password = Password,
                BearerToken = BearerToken,
                UserAgent = UserAgent
            };

            foreach (KeyValuePair<string, string> kvp in CustomHeaders)
            {
                clone.CustomHeaders[kvp.Key] = kvp.Value;
            }

            foreach (KeyValuePair<string, string> kvp in QueryParameters)
            {
                clone.QueryParameters[kvp.Key] = kvp.Value;
            }

            return clone;
        }
    }
}
