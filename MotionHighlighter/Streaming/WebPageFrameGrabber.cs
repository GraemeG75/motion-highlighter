using System.Diagnostics;
using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.WinForms;
using OpenCvSharp;

namespace MotionHighlighter.Streaming
{
    public class WebPageFrameGrabber : IStreamGrabber
    {
        private const int MaxQueueSize = 5;

        private readonly string _streamUrl;
        private readonly string _pageUrl;
        private readonly StreamAuthOptions? _auth;
        private readonly Queue<Mat> _frameQueue = new();
        private readonly object _queueLock = new();
        private readonly SemaphoreSlim _frameSignal = new(0);
        private readonly Stopwatch _elapsedStopwatch = new();

        private Form? _hostForm;
        private WebView2? _webView;
        private Thread? _staThread;
        private CancellationTokenSource? _grabberCts;

        public string StreamUrl => _streamUrl;
        public string StreamTitle { get; private set; }
        public StreamFormat Format => StreamFormat.WebLiveFeed;
        public bool IsRunning { get; private set; }
        public bool IsLive => true;
        public double Fps { get; private set; } = 15.0;
        public int Width { get; private set; }
        public int Height { get; private set; }
        public long FramesReceived { get; private set; }
        public TimeSpan ElapsedStreamTime => _elapsedStopwatch.Elapsed;

        public WebPageFrameGrabber(string streamUrl, StreamAuthOptions? auth = null, string? title = null)
        {
            _streamUrl = streamUrl ?? throw new ArgumentNullException(nameof(streamUrl));
            _pageUrl = streamUrl.StartsWith("webpage://", StringComparison.OrdinalIgnoreCase)
                ? "https://" + streamUrl.Substring("webpage://".Length)
                : streamUrl;

            _auth = auth?.Clone();
            StreamTitle = title ?? Path.GetFileName(new Uri(_pageUrl).AbsolutePath);
            if (string.IsNullOrWhiteSpace(StreamTitle))
            {
                StreamTitle = _pageUrl;
            }
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

            TaskCompletionSource<bool> tcs = new TaskCompletionSource<bool>();

            _staThread = new Thread(() =>
            {
                try
                {
                    _hostForm = new Form
                    {
                        Width = 1280,
                        Height = 720,
                        ShowInTaskbar = false,
                        StartPosition = FormStartPosition.Manual,
                        Location = new System.Drawing.Point(-20000, -20000),
                        FormBorderStyle = FormBorderStyle.None,
                        Opacity = 0.01
                    };

                    _webView = new WebView2
                    {
                        Dock = DockStyle.Fill
                    };
                    _hostForm.Controls.Add(_webView);

                    _hostForm.Load += async (_, _) =>
                    {
                        try
                        {
                            string profileDir = Path.Combine(
                                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                                "MotionHighlighter", "WebViewProfile");

                            CoreWebView2EnvironmentOptions options = new CoreWebView2EnvironmentOptions(
                                "--disable-background-timer-throttling --disable-backgrounding-occluded-windows --disable-renderer-backgrounding --autoplay-policy=no-user-gesture-required");
                            CoreWebView2Environment env = await CoreWebView2Environment.CreateAsync(userDataFolder: profileDir, options: options);
                            await _webView.EnsureCoreWebView2Async(env);

                            _webView.CoreWebView2.WebMessageReceived += OnWebMessageReceived;

                            await _webView.CoreWebView2.AddScriptToExecuteOnDocumentCreatedAsync(@"
                                (function() {
                                    if (window.__mhGrabberInjected) return;
                                    window.__mhGrabberInjected = true;

                                    function findActiveFeed() {
                                        const videos = Array.from(document.querySelectorAll('video'));
                                        let best = null;
                                        let maxScore = -1;

                                        for (const v of videos) {
                                            if (!v || v.videoWidth === 0 || v.videoHeight === 0) continue;

                                            const w = v.videoWidth;
                                            const h = v.videoHeight;

                                            // Skip tiny thumbnail cards (like 240x136 preview cards on Google Home)
                                            if ((w < 320 || h < 180) && videos.length > 1) {
                                                continue;
                                            }

                                            const isLiveStream = (v.srcObject !== null && v.srcObject !== undefined) || !isFinite(v.duration);
                                            const isPlaying = !v.paused && v.currentTime > 0;
                                            const area = w * h;

                                            let score = area;
                                            if (isLiveStream) score *= 1000;
                                            if (isPlaying) score *= 10;
                                            if (isFinite(v.duration) && v.duration > 0 && v.duration < 5) {
                                                score = 1;
                                            }

                                            if (score > maxScore) {
                                                maxScore = score;
                                                best = v;
                                            }
                                        }

                                        if (best) {
                                            if (best.paused) {
                                                best.play().catch(() => {});
                                            }
                                            return { el: best, isVideo: true, width: best.videoWidth, height: best.videoHeight };
                                        }

                                        const canvases = Array.from(document.querySelectorAll('canvas'));
                                        for (const c of canvases) {
                                            if (c && c.width >= 320 && c.height >= 180) {
                                                return { el: c, isVideo: false, width: c.width, height: c.height };
                                            }
                                        }
                                        return null;
                                    }

                                    function startLoop() {
                                        if (window.__mhLoopStarted) return;
                                        window.__mhLoopStarted = true;

                                        setInterval(() => {
                                            const feed = findActiveFeed();
                                            if (!feed) return;

                                            if (!window.__mhCanvas) {
                                                window.__mhCanvas = document.createElement('canvas');
                                            }
                                            const c = window.__mhCanvas;
                                            if (c.width !== feed.width || c.height !== feed.height) {
                                                c.width = feed.width;
                                                c.height = feed.height;
                                            }
                                            const ctx = c.getContext('2d');
                                            ctx.drawImage(feed.el, 0, 0);
                                            const dataUrl = c.toDataURL('image/jpeg', 0.80);
                                            window.chrome.webview.postMessage(dataUrl);
                                        }, 66); // ~15 FPS
                                    }

                                    const checker = setInterval(() => {
                                        const feed = findActiveFeed();
                                        if (feed) {
                                            clearInterval(checker);
                                            startLoop();
                                        }
                                    }, 400);
                                })();
                            ");

                            _webView.CoreWebView2.Navigate(_pageUrl);
                            tcs.TrySetResult(true);
                        }
                        catch (Exception ex)
                        {
                            tcs.TrySetException(ex);
                        }
                    };

                    Application.Run(_hostForm);
                }
                catch (Exception ex)
                {
                    tcs.TrySetException(ex);
                }
            })
            {
                IsBackground = true
            };

            _staThread.SetApartmentState(ApartmentState.STA);
            _staThread.Start();

            return tcs.Task;
        }

        private void OnWebMessageReceived(object? sender, CoreWebView2WebMessageReceivedEventArgs e)
        {
            try
            {
                string msg = e.TryGetWebMessageAsString();
                if (string.IsNullOrEmpty(msg) || !msg.StartsWith("data:image/jpeg;base64,"))
                {
                    return;
                }

                // If queue already has buffered frames, drop to avoid decoding and memory build-up
                lock (_queueLock)
                {
                    if (_frameQueue.Count >= MaxQueueSize)
                    {
                        return;
                    }
                }

                string base64 = msg.Substring("data:image/jpeg;base64,".Length);
                byte[] bytes = Convert.FromBase64String(base64);

                Mat frame = Cv2.ImDecode(bytes, ImreadModes.Color);
                if (frame.Empty())
                {
                    frame.Dispose();
                    return;
                }

                if (Width == 0 || Height == 0)
                {
                    Width = frame.Width;
                    Height = frame.Height;
                }

                EnqueueFrame(frame);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[WebPageFrameGrabber] Frame decode error: {ex.Message}");
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

        public void Stop()
        {
            IsRunning = false;
            _grabberCts?.Cancel();
            _elapsedStopwatch.Stop();

            if (_hostForm != null && !_hostForm.IsDisposed)
            {
                try
                {
                    _hostForm.BeginInvoke(() =>
                    {
                        try
                        {
                            _webView?.Dispose();
                            _hostForm.Close();
                            _hostForm.Dispose();
                        }
                        catch { }
                    });
                }
                catch { }
            }

            lock (_queueLock)
            {
                while (_frameQueue.Count > 0)
                {
                    _frameQueue.Dequeue().Dispose();
                }
            }
        }

        public void Dispose()
        {
            Stop();
            _grabberCts?.Dispose();
            _frameSignal.Dispose();
        }
    }
}
