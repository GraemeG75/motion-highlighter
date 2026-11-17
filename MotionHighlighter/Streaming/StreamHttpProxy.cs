using System.Net;
using System.Net.Sockets;

namespace MotionHighlighter.Streaming
{
    public class StreamHttpProxy : IDisposable
    {
        private readonly HttpListener _listener;
        private readonly HttpClient _httpClient;
        private readonly StreamAuthOptions? _auth;
        private readonly string _targetUrl;
        private readonly int _port;
        private CancellationTokenSource? _cts;
        private Task? _listenTask;

        public string LocalProxyUrl => $"http://127.0.0.1:{_port}/stream/live.mp4";

        public StreamHttpProxy(string targetUrl, StreamAuthOptions? auth = null)
        {
            _targetUrl = targetUrl;
            _auth = auth;
            _port = GetAvailablePort();

            _listener = new HttpListener();
            _listener.Prefixes.Add($"http://127.0.0.1:{_port}/stream/");

            HttpClientHandler handler = new HttpClientHandler
            {
                AllowAutoRedirect = true,
                AutomaticDecompression = DecompressionMethods.All
            };

            _httpClient = new HttpClient(handler)
            {
                Timeout = TimeSpan.FromMinutes(30)
            };
        }

        public void Start()
        {
            _cts = new CancellationTokenSource();
            _listener.Start();
            _listenTask = Task.Run(() => ListenLoopAsync(_cts.Token));
        }

        private async Task ListenLoopAsync(CancellationToken token)
        {
            while (!token.IsCancellationRequested && _listener.IsListening)
            {
                try
                {
                    HttpListenerContext context = await _listener.GetContextAsync();
                    _ = Task.Run(() => HandleRequestAsync(context, token), token);
                }
                catch (HttpListenerException)
                {
                    break;
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch
                {
                    // Listener loop recovery
                }
            }
        }

        private async Task HandleRequestAsync(HttpListenerContext context, CancellationToken token)
        {
            try
            {
                Uri targetUri = new Uri(_targetUrl);
                if (_auth != null)
                {
                    targetUri = _auth.ApplyToUri(targetUri);
                }

                using HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, targetUri);
                _auth?.ApplyToRequest(request, targetUri);

                using HttpResponseMessage response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, token);

                context.Response.StatusCode = (int)response.StatusCode;
                if (response.Content.Headers.ContentType != null)
                {
                    context.Response.ContentType = response.Content.Headers.ContentType.ToString();
                }
                if (response.Content.Headers.ContentLength.HasValue)
                {
                    context.Response.ContentLength64 = response.Content.Headers.ContentLength.Value;
                }

                using Stream inStream = await response.Content.ReadAsStreamAsync(token);
                await inStream.CopyToAsync(context.Response.OutputStream, 81920, token);
            }
            catch
            {
                try
                {
                    context.Response.StatusCode = (int)HttpStatusCode.BadGateway;
                }
                catch { }
            }
            finally
            {
                try
                {
                    context.Response.OutputStream.Close();
                }
                catch { }
            }
        }

        private static int GetAvailablePort()
        {
            using TcpListener listener = new TcpListener(IPAddress.Loopback, 0);
            listener.Start();
            int port = ((IPEndPoint)listener.LocalEndpoint).Port;
            listener.Stop();
            return port;
        }

        public void Dispose()
        {
            try
            {
                _cts?.Cancel();
                _cts?.Dispose();
                _listener.Stop();
                _listener.Close();
                _httpClient.Dispose();
            }
            catch { }
        }
    }
}
