using System.Diagnostics;
using System.Net;
using OpenCvSharp;

namespace MotionHighlighter.Streaming
{
    public class StreamGrabber : IStreamGrabber
    {
        private const int MaxQueueSize = 5;

        private readonly string _streamUrl;
        private readonly StreamFormat _format;
        private readonly StreamAuthOptions? _auth;
        private readonly HttpClient _httpClient;
        private readonly Queue<Mat> _frameQueue = new();
        private readonly object _queueLock = new();
        private readonly SemaphoreSlim _frameSignal = new(0);

        // Bounded sliding window for tracking processed stream segments (prevents memory leaks on long streams)
        private readonly Queue<string> _recentSegmentsQueue = new();
        private readonly HashSet<string> _recentSegmentsSet = new(StringComparer.OrdinalIgnoreCase);

        private StreamHttpProxy? _proxy;
        private CancellationTokenSource? _grabberCts;
        private Task? _workerTask;
        private readonly Stopwatch _elapsedStopwatch = new();

        public string StreamUrl => _streamUrl;
        public string StreamTitle { get; private set; } = string.Empty;
        public StreamFormat Format => _format;
        public bool IsRunning { get; private set; }
        public bool IsLive { get; private set; }
        public double Fps { get; private set; } = 30.0;
        public int Width { get; private set; }
        public int Height { get; private set; }
        public long FramesReceived { get; private set; }
        public TimeSpan ElapsedStreamTime => _elapsedStopwatch.Elapsed;

        public StreamGrabber(string streamUrl, StreamFormat? format = null, StreamAuthOptions? auth = null, string? title = null)
        {
            _streamUrl = streamUrl ?? throw new ArgumentNullException(nameof(streamUrl));
            _format = format ?? WebpageStreamExtractor.DetectFormatFromUrl(streamUrl);
            _auth = auth?.Clone();
            StreamTitle = title ?? Path.GetFileName(new Uri(streamUrl).AbsolutePath);
            if (string.IsNullOrWhiteSpace(StreamTitle))
            {
                StreamTitle = streamUrl;
            }

            HttpClientHandler handler = new HttpClientHandler
            {
                AllowAutoRedirect = true,
                AutomaticDecompression = DecompressionMethods.All
            };

            _httpClient = new HttpClient(handler)
            {
                Timeout = TimeSpan.FromSeconds(25)
            };
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            if (IsRunning)
            {
                return Task.CompletedTask;
            }

            IsRunning = true;
            _elapsedStopwatch.Restart();
            _grabberCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            CancellationToken token = _grabberCts.Token;

            _workerTask = Task.Run(() => WorkerLoopAsync(token), token);
            return Task.CompletedTask;
        }

        public async Task<Mat?> ReadNextFrameAsync(CancellationToken cancellationToken)
        {
            while (IsRunning || _frameQueue.Count > 0)
            {
                if (await _frameSignal.WaitAsync(100, cancellationToken))
                {
                    lock (_queueLock)
                    {
                        if (_frameQueue.TryDequeue(out Mat? frame))
                        {
                            return frame;
                        }
                    }
                }

                if (!IsRunning && _frameQueue.Count == 0)
                {
                    break;
                }
            }

            return null;
        }

        private bool TryMarkSegmentProcessed(string segUrl)
        {
            lock (_queueLock)
            {
                if (_recentSegmentsSet.Contains(segUrl))
                {
                    return false;
                }

                _recentSegmentsSet.Add(segUrl);
                _recentSegmentsQueue.Enqueue(segUrl);

                while (_recentSegmentsQueue.Count > 100)
                {
                    string old = _recentSegmentsQueue.Dequeue();
                    _recentSegmentsSet.Remove(old);
                }

                return true;
            }
        }

        private async Task WorkerLoopAsync(CancellationToken token)
        {
            try
            {
                switch (_format)
                {
                    case StreamFormat.DashMpd:
                        await RunDashStreamAsync(token);
                        break;

                    case StreamFormat.HlsM3u8:
                        await RunHlsStreamAsync(token);
                        break;

                    case StreamFormat.FragmentedMp4:
                        await RunSegmentOrVideoAsync(token);
                        break;

                    case StreamFormat.WebLiveFeed:
                        await RunWebLiveFeedAsync(token);
                        break;

                    default:
                        // RTSP, MJPEG, direct video files (.mp4, .m4v, .webm, .mkv)
                        // Stream frame-by-frame on the fly via OpenCV without buffering movies in RAM
                        await RunDirectStreamAsync(token);
                        break;
                }
            }
            catch (OperationCanceledException)
            {
                // Clean shutdown
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[StreamGrabber] Worker error: {ex.Message}");
            }
            finally
            {
                IsRunning = false;
                _elapsedStopwatch.Stop();
            }
        }

        private async Task RunDashStreamAsync(CancellationToken token)
        {
            Uri manifestUri = new Uri(_streamUrl);
            string mpdXml = await FetchStringAsync(manifestUri, token);
            DashStreamInfo dashInfo = DashManifestParser.Parse(mpdXml, manifestUri);

            IsLive = dashInfo.IsLive;
            if (dashInfo.Fps > 0) Fps = dashInfo.Fps;
            if (dashInfo.Width > 0) Width = dashInfo.Width;
            if (dashInfo.Height > 0) Height = dashInfo.Height;

            byte[]? initBytes = null;
            if (!string.IsNullOrEmpty(dashInfo.InitializationUrl))
            {
                initBytes = await FetchBytesAsync(new Uri(dashInfo.InitializationUrl), token, maxBytes: 1024 * 1024);
            }

            do
            {
                foreach (string segUrl in dashInfo.EnumerateSegmentUrls())
                {
                    if (token.IsCancellationRequested) break;
                    if (!TryMarkSegmentProcessed(segUrl)) continue;

                    // Backpressure: wait if queue already has buffered frames before fetching next chunk
                    while (_frameQueue.Count >= 2 && !token.IsCancellationRequested)
                    {
                        await Task.Delay(25, token);
                    }

                    if (token.IsCancellationRequested) break;

                    string tempFile = Path.Combine(Path.GetTempPath(), $"mh_dash_{Guid.NewGuid():N}.mp4");
                    try
                    {
                        await DownloadChunkToFileAsync(initBytes, new Uri(segUrl), tempFile, token);
                        await DecodeFileToQueueAsync(tempFile, token);
                    }
                    finally
                    {
                        try
                        {
                            if (File.Exists(tempFile))
                            {
                                File.Delete(tempFile);
                            }
                        }
                        catch { }
                    }
                }

                if (IsLive && !token.IsCancellationRequested)
                {
                    int delayMs = (int)Math.Max(1000, dashInfo.SegmentDurationSeconds * 800);
                    await Task.Delay(delayMs, token);

                    try
                    {
                        string refreshedXml = await FetchStringAsync(manifestUri, token);
                        dashInfo = DashManifestParser.Parse(refreshedXml, manifestUri);
                    }
                    catch
                    {
                        // Retry on transient network errors
                    }
                }
            } while (IsLive && !token.IsCancellationRequested);
        }

        private async Task RunHlsStreamAsync(CancellationToken token)
        {
            Uri playlistUri = new Uri(_streamUrl);
            string m3u8Content = await FetchStringAsync(playlistUri, token);
            HlsPlaylistInfo hlsInfo = HlsPlaylistParser.Parse(m3u8Content, playlistUri);

            if (hlsInfo.IsMasterPlaylist && hlsInfo.Variants.Count > 0)
            {
                // Prefer variants with height <= 1080p
                HlsStreamVariant bestVariant = hlsInfo.Variants
                    .Where(v =>
                    {
                        if (string.IsNullOrEmpty(v.Resolution)) return true;
                        string[] parts = v.Resolution.Split('x');
                        return parts.Length == 2 && int.TryParse(parts[1], out int h) && h <= 1080;
                    })
                    .OrderByDescending(v => v.Bandwidth)
                    .FirstOrDefault()
                    ?? hlsInfo.Variants.OrderByDescending(v => v.Bandwidth).First();

                playlistUri = new Uri(bestVariant.Url);
                m3u8Content = await FetchStringAsync(playlistUri, token);
                hlsInfo = HlsPlaylistParser.Parse(m3u8Content, playlistUri);
            }

            IsLive = hlsInfo.IsLive;

            byte[]? initBytes = null;
            if (!string.IsNullOrEmpty(hlsInfo.InitializationUrl))
            {
                initBytes = await FetchBytesAsync(new Uri(hlsInfo.InitializationUrl), token, maxBytes: 1024 * 1024);
            }

            do
            {
                foreach (string segUrl in hlsInfo.SegmentUrls)
                {
                    if (token.IsCancellationRequested) break;
                    if (!TryMarkSegmentProcessed(segUrl)) continue;

                    // Backpressure: wait if queue already has buffered frames before fetching next chunk
                    while (_frameQueue.Count >= 2 && !token.IsCancellationRequested)
                    {
                        await Task.Delay(25, token);
                    }

                    if (token.IsCancellationRequested) break;

                    string tempFile = Path.Combine(Path.GetTempPath(), $"mh_hls_{Guid.NewGuid():N}.mp4");
                    try
                    {
                        await DownloadChunkToFileAsync(initBytes, new Uri(segUrl), tempFile, token);
                        await DecodeFileToQueueAsync(tempFile, token);
                    }
                    finally
                    {
                        try
                        {
                            if (File.Exists(tempFile))
                            {
                                File.Delete(tempFile);
                            }
                        }
                        catch { }
                    }
                }

                if (IsLive && !token.IsCancellationRequested)
                {
                    int delayMs = (int)Math.Max(1000, hlsInfo.TargetDurationSeconds * 600);
                    await Task.Delay(delayMs, token);

                    try
                    {
                        string refreshedM3u8 = await FetchStringAsync(playlistUri, token);
                        hlsInfo = HlsPlaylistParser.Parse(refreshedM3u8, playlistUri);
                    }
                    catch
                    {
                        // Retry on transient network errors
                    }
                }
            } while (IsLive && !token.IsCancellationRequested);
        }

        private async Task RunSegmentOrVideoAsync(CancellationToken token)
        {
            Uri mediaUri = new Uri(_streamUrl);

            byte[]? initBytes = null;
            // Check if there is an init.m4s or init.mp4 in the same directory for .m4s fragments
            if (_format == StreamFormat.FragmentedMp4)
            {
                try
                {
                    Uri initUri = new Uri(mediaUri, "init.m4s");
                    initBytes = await FetchBytesAsync(initUri, token, maxBytes: 1024 * 1024);
                }
                catch
                {
                    try
                    {
                        Uri initUri = new Uri(mediaUri, "init.mp4");
                        initBytes = await FetchBytesAsync(initUri, token, maxBytes: 1024 * 1024);
                    }
                    catch { }
                }
            }

            string tempFile = Path.Combine(Path.GetTempPath(), $"mh_frag_{Guid.NewGuid():N}.mp4");
            try
            {
                await DownloadChunkToFileAsync(initBytes, mediaUri, tempFile, token);
                await DecodeFileToQueueAsync(tempFile, token);
            }
            finally
            {
                try
                {
                    if (File.Exists(tempFile))
                    {
                        File.Delete(tempFile);
                    }
                }
                catch { }
            }
        }

        private async Task RunDirectStreamAsync(CancellationToken token)
        {
            string captureUrl = _streamUrl;

            // If custom auth or headers are required, route through our local StreamHttpProxy
            if (_auth != null && (_auth.AuthType != AuthType.None || _auth.CustomHeaders.Count > 0))
            {
                _proxy = new StreamHttpProxy(_streamUrl, _auth);
                _proxy.Start();
                captureUrl = _proxy.LocalProxyUrl;
            }

            using VideoCapture cap = new VideoCapture(captureUrl);
            if (!cap.IsOpened())
            {
                throw new InvalidOperationException($"Failed to open stream at {captureUrl}");
            }

            double capFps = cap.Get(VideoCaptureProperties.Fps);
            if (capFps > 0 && !double.IsNaN(capFps))
            {
                Fps = capFps;
            }
            Width = (int)cap.Get(VideoCaptureProperties.FrameWidth);
            Height = (int)cap.Get(VideoCaptureProperties.FrameHeight);

            int delayMs = (int)Math.Max(1.0, 1000.0 / Fps);

            while (!token.IsCancellationRequested)
            {
                while (_frameQueue.Count >= MaxQueueSize && !token.IsCancellationRequested)
                {
                    await Task.Delay(15, token);
                }
                if (token.IsCancellationRequested) break;

                Mat frame = new Mat();
                if (!cap.Read(frame) || frame.Empty())
                {
                    frame.Dispose();
                    break;
                }

                if (Width == 0 || Height == 0)
                {
                    Width = frame.Width;
                    Height = frame.Height;
                }

                EnqueueFrame(frame);
                await Task.Delay(delayMs, token);
            }
        }

        private async Task RunWebLiveFeedAsync(CancellationToken token)
        {
            using WebPageFrameGrabber webGrabber = new WebPageFrameGrabber(_streamUrl, _auth, StreamTitle);
            await webGrabber.StartAsync(token);

            while (!token.IsCancellationRequested && webGrabber.IsRunning)
            {
                Mat? frame = await webGrabber.ReadNextFrameAsync(token);
                if (frame != null)
                {
                    if (Width == 0 || Height == 0)
                    {
                        Width = webGrabber.Width;
                        Height = webGrabber.Height;
                    }
                    Fps = webGrabber.Fps;
                    EnqueueFrame(frame);
                }
            }

            webGrabber.Stop();
        }

        private async Task DownloadChunkToFileAsync(byte[]? initBytes, Uri chunkUri, string targetFilePath, CancellationToken token)
        {
            if (_auth != null) chunkUri = _auth.ApplyToUri(chunkUri);
            using HttpRequestMessage req = new HttpRequestMessage(HttpMethod.Get, chunkUri);
            _auth?.ApplyToRequest(req, chunkUri);

            // Stream response directly to file without buffering giant payloads in RAM
            using HttpResponseMessage resp = await _httpClient.SendAsync(req, HttpCompletionOption.ResponseHeadersRead, token);
            resp.EnsureSuccessStatusCode();

            using FileStream fileStream = new FileStream(targetFilePath, FileMode.Create, FileAccess.Write, FileShare.None, 65536, useAsync: true);
            if (initBytes != null && initBytes.Length > 0)
            {
                await fileStream.WriteAsync(initBytes, 0, initBytes.Length, token);
            }

            using Stream responseStream = await resp.Content.ReadAsStreamAsync(token);
            await responseStream.CopyToAsync(fileStream, 65536, token);
        }

        private async Task DecodeFileToQueueAsync(string tempFile, CancellationToken token)
        {
            try
            {
                using VideoCapture cap = new VideoCapture(tempFile);
                if (!cap.IsOpened())
                {
                    return;
                }

                double capFps = cap.Get(VideoCaptureProperties.Fps);
                if (capFps > 0 && !double.IsNaN(capFps))
                {
                    Fps = capFps;
                }

                while (!token.IsCancellationRequested)
                {
                    // Backpressure: wait if queue is full before decoding next frame
                    while (_frameQueue.Count >= MaxQueueSize && !token.IsCancellationRequested)
                    {
                        await Task.Delay(15, token);
                    }
                    if (token.IsCancellationRequested) break;

                    Mat frame = new Mat();
                    if (!cap.Read(frame) || frame.Empty())
                    {
                        frame.Dispose();
                        break;
                    }

                    if (Width == 0 || Height == 0)
                    {
                        Width = frame.Width;
                        Height = frame.Height;
                    }

                    EnqueueFrame(frame);
                }
            }
            catch (OperationCanceledException) { }
            catch (Exception ex)
            {
                Debug.WriteLine($"[StreamGrabber] Chunk decode error: {ex.Message}");
            }
        }

        private void EnqueueFrame(Mat frame)
        {
            lock (_queueLock)
            {
                while (_frameQueue.Count >= MaxQueueSize)
                {
                    Mat dropped = _frameQueue.Dequeue();
                    dropped.Dispose();
                }

                _frameQueue.Enqueue(frame);
                FramesReceived++;
            }

            _frameSignal.Release();
        }

        private async Task<string> FetchStringAsync(Uri uri, CancellationToken token)
        {
            if (_auth != null) uri = _auth.ApplyToUri(uri);
            using HttpRequestMessage req = new HttpRequestMessage(HttpMethod.Get, uri);
            _auth?.ApplyToRequest(req, uri);

            using HttpResponseMessage resp = await _httpClient.SendAsync(req, HttpCompletionOption.ResponseContentRead, token);
            resp.EnsureSuccessStatusCode();
            return await resp.Content.ReadAsStringAsync(token);
        }

        private async Task<byte[]> FetchBytesAsync(Uri uri, CancellationToken token, long maxBytes = 5 * 1024 * 1024)
        {
            if (_auth != null) uri = _auth.ApplyToUri(uri);
            using HttpRequestMessage req = new HttpRequestMessage(HttpMethod.Get, uri);
            _auth?.ApplyToRequest(req, uri);

            using HttpResponseMessage resp = await _httpClient.SendAsync(req, HttpCompletionOption.ResponseHeadersRead, token);
            resp.EnsureSuccessStatusCode();

            if (resp.Content.Headers.ContentLength.HasValue && resp.Content.Headers.ContentLength.Value > maxBytes)
            {
                throw new InvalidOperationException($"Refusing to buffer resource of size {resp.Content.Headers.ContentLength.Value:N0} bytes into memory (limit: {maxBytes:N0} bytes). Streams must be processed frame-by-frame.");
            }

            return await resp.Content.ReadAsByteArrayAsync(token);
        }

        public void Stop()
        {
            IsRunning = false;
            _grabberCts?.Cancel();
            _elapsedStopwatch.Stop();
            _proxy?.Dispose();
            _proxy = null;

            lock (_queueLock)
            {
                while (_frameQueue.Count > 0)
                {
                    _frameQueue.Dequeue().Dispose();
                }
                _recentSegmentsQueue.Clear();
                _recentSegmentsSet.Clear();
            }
        }

        public void Dispose()
        {
            Stop();
            _grabberCts?.Dispose();
            _frameSignal.Dispose();
            _httpClient.Dispose();
        }
    }
}
