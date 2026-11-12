using MotionHighlighter.Streaming;

namespace MotionHighlighter.Tests
{
    [TestClass]
    public class CurlParserTests
    {
        [TestMethod]
        public void TryParse_GoogleHomeCurlRequest_ExtractsUrlAndHeaders()
        {
            string curl = @"curl 'https://nexusapi-us1.dropcam.com/get_image?uuid=95477349dadeda4a92eff459c0167a354257ecd49a6ce457dd13bfa9fd7ea287&width=1280' \
  -H 'authority: nexusapi-us1.dropcam.com' \
  -H 'accept: */*' \
  -H 'cookie: __Secure-1PSID=test_session_id; SAPISID=test_sapisid; SID=test_sid' \
  -H 'referer: https://home.google.com/' \
  -H 'user-agent: Mozilla/5.0 (Windows NT 10.0; Win64; x64) Chrome/130.0.0.0'";

            bool success = CurlParser.TryParse(curl, out string url, out StreamAuthOptions auth);

            Assert.IsTrue(success);
            Assert.AreEqual("https://nexusapi-us1.dropcam.com/get_image?uuid=95477349dadeda4a92eff459c0167a354257ecd49a6ce457dd13bfa9fd7ea287&width=1280", url);
            Assert.AreEqual("Mozilla/5.0 (Windows NT 10.0; Win64; x64) Chrome/130.0.0.0", auth.UserAgent);
            Assert.IsTrue(auth.CustomHeaders.ContainsKey("cookie"));
            Assert.AreEqual("__Secure-1PSID=test_session_id; SAPISID=test_sapisid; SID=test_sid", auth.CustomHeaders["cookie"]);
            Assert.AreEqual("https://home.google.com/", auth.CustomHeaders["referer"]);
        }

        [TestMethod]
        public void TryParse_BearerAuthCurl_SetsBearerToken()
        {
            string curl = @"curl ""https://streaming.example.com/live.m4s"" -H ""Authorization: Bearer my_jwt_token_123""";

            bool success = CurlParser.TryParse(curl, out string url, out StreamAuthOptions auth);

            Assert.IsTrue(success);
            Assert.AreEqual("https://streaming.example.com/live.m4s", url);
            Assert.AreEqual(AuthType.BearerToken, auth.AuthType);
            Assert.AreEqual("my_jwt_token_123", auth.BearerToken);
        }
    }
}
