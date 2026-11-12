using System.Net.Http.Headers;
using System.Text;
using MotionHighlighter.Streaming;

namespace MotionHighlighter.Tests
{
    [TestClass]
    public class StreamAuthOptionsTests
    {
        [TestMethod]
        public void ApplyToRequest_BearerToken_SetsAuthorizationHeader()
        {
            StreamAuthOptions auth = new StreamAuthOptions
            {
                AuthType = AuthType.BearerToken,
                BearerToken = "test_token_12345"
            };

            using HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, "https://example.com/video.mpd");
            auth.ApplyToRequest(request);

            Assert.IsNotNull(request.Headers.Authorization);
            Assert.AreEqual("Bearer", request.Headers.Authorization.Scheme);
            Assert.AreEqual("test_token_12345", request.Headers.Authorization.Parameter);
        }

        [TestMethod]
        public void ApplyToRequest_BearerTokenWithPrefix_StripsDuplicateBearer()
        {
            StreamAuthOptions auth = new StreamAuthOptions
            {
                AuthType = AuthType.BearerToken,
                BearerToken = "Bearer secret_xyz"
            };

            using HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, "https://example.com/stream");
            auth.ApplyToRequest(request);

            Assert.IsNotNull(request.Headers.Authorization);
            Assert.AreEqual("secret_xyz", request.Headers.Authorization.Parameter);
        }

        [TestMethod]
        public void ApplyToRequest_BasicAuth_EncodesBase64()
        {
            StreamAuthOptions auth = new StreamAuthOptions
            {
                AuthType = AuthType.BasicAuth,
                Username = "admin",
                Password = "password123"
            };

            using HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, "https://example.com/stream.m3u8");
            auth.ApplyToRequest(request);

            Assert.IsNotNull(request.Headers.Authorization);
            Assert.AreEqual("Basic", request.Headers.Authorization.Scheme);

            string expectedBase64 = Convert.ToBase64String(Encoding.UTF8.GetBytes("admin:password123"));
            Assert.AreEqual(expectedBase64, request.Headers.Authorization.Parameter);
        }

        [TestMethod]
        public void ApplyToRequest_CustomHeaders_AddsAllCustomHeaders()
        {
            StreamAuthOptions auth = new StreamAuthOptions
            {
                AuthType = AuthType.CustomHeaders
            };
            auth.CustomHeaders["Cookie"] = "session_id=abc; token=def";
            auth.CustomHeaders["X-Custom-Auth"] = "MyApiKey";
            auth.CustomHeaders["Referer"] = "https://mysite.com/viewer";

            using HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, "https://example.com/stream.m4s");
            auth.ApplyToRequest(request);

            Assert.IsTrue(request.Headers.Contains("Cookie"));
            Assert.IsTrue(request.Headers.Contains("X-Custom-Auth"));
            Assert.IsTrue(request.Headers.Contains("Referer"));
        }

        [TestMethod]
        public void ApplyToUri_AppendsQueryParameters()
        {
            StreamAuthOptions auth = new StreamAuthOptions();
            auth.QueryParameters["token"] = "abc123xyz";
            auth.QueryParameters["exp"] = "1800000";

            Uri baseUri = new Uri("https://example.com/live/segment.m4s");
            Uri result = auth.ApplyToUri(baseUri);

            Assert.IsTrue(result.Query.Contains("token=abc123xyz"));
            Assert.IsTrue(result.Query.Contains("exp=1800000"));
        }

        [TestMethod]
        public void Clone_CreatesIndependentCopy()
        {
            StreamAuthOptions auth = new StreamAuthOptions
            {
                AuthType = AuthType.BearerToken,
                BearerToken = "initial_token",
                UserAgent = "CustomUA/1.0"
            };
            auth.CustomHeaders["Header1"] = "Val1";

            StreamAuthOptions clone = auth.Clone();
            clone.BearerToken = "modified_token";
            clone.CustomHeaders["Header1"] = "ModifiedVal";

            Assert.AreEqual("initial_token", auth.BearerToken);
            Assert.AreEqual("Val1", auth.CustomHeaders["Header1"]);
        }
    }
}
