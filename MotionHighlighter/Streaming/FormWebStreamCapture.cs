using System.Text.Json;
using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.WinForms;

namespace MotionHighlighter.Streaming
{
    public class FormWebStreamCapture : Form
    {
        private readonly WebView2 _webView = new();
        private readonly Panel _panelTop = new();
        private readonly Panel _panelNav = new();
        private readonly Label _lblStatus = new();
        private readonly Button _btnUseStream = new();
        private readonly Button _btnCancel = new();

        private readonly Button _btnBack = new();
        private readonly Button _btnForward = new();
        private readonly Button _btnReload = new();
        private readonly TextBox _txtNavUrl = new();
        private readonly Button _btnGo = new();
        private readonly ComboBox _cmbQuickPortals = new();

        private readonly string _initialUrl;

        public string CapturedStreamUrl { get; private set; } = string.Empty;
        public StreamFormat CapturedFormat { get; private set; } = StreamFormat.Unknown;
        public StreamAuthOptions CapturedAuth { get; private set; } = new();

        public FormWebStreamCapture(string? initialUrl = null)
        {
            _initialUrl = string.IsNullOrWhiteSpace(initialUrl)
                ? "https://home.google.com/"
                : initialUrl;

            InitializeUi();
            Shown += async (_, _) => await InitializeWebViewAsync();
        }

        private void InitializeUi()
        {
            Text = "Sign In & Auto-Capture Camera Stream";
            ClientSize = new Size(1150, 780);
            StartPosition = FormStartPosition.CenterParent;
            MinimumSize = new Size(850, 600);

            // 1. Top status banner
            _panelTop.Dock = DockStyle.Top;
            _panelTop.Height = 54;
            _panelTop.BackColor = Color.FromArgb(240, 244, 248);
            _panelTop.Padding = new Padding(12, 6, 12, 6);

            FlowLayoutPanel panelTopButtons = new FlowLayoutPanel
            {
                Dock = DockStyle.Right,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                FlowDirection = FlowDirection.RightToLeft,
                WrapContents = false,
                Padding = new Padding(0, 4, 0, 4),
                BackColor = Color.Transparent
            };

            _btnCancel.Text = "Cancel";
            _btnCancel.Size = new Size(85, 34);
            _btnCancel.Margin = new Padding(6, 0, 0, 0);
            _btnCancel.Click += (_, _) => { DialogResult = DialogResult.Cancel; Close(); };

            _btnUseStream.Text = "✓ Use Captured Stream";
            _btnUseStream.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            _btnUseStream.Size = new Size(220, 34);
            _btnUseStream.Margin = new Padding(0);
            _btnUseStream.Enabled = true;
            _btnUseStream.BackColor = Color.FromArgb(22, 163, 74);
            _btnUseStream.ForeColor = Color.White;
            _btnUseStream.UseVisualStyleBackColor = false;
            _btnUseStream.Cursor = Cursors.Hand;
            _btnUseStream.Click += async (_, _) => await OnUseStreamClickedAsync();

            panelTopButtons.Controls.Add(_btnCancel);
            panelTopButtons.Controls.Add(_btnUseStream);

            _lblStatus.Text = "Log in to your camera portal. When the live camera feed is visible, click 'Use Captured Stream' on the right.";
            _lblStatus.Font = new Font("Segoe UI", 9.5F, FontStyle.Regular);
            _lblStatus.ForeColor = Color.FromArgb(30, 41, 59);
            _lblStatus.Dock = DockStyle.Fill;
            _lblStatus.TextAlign = ContentAlignment.MiddleLeft;
            _lblStatus.AutoEllipsis = true;

            _panelTop.Controls.Add(_lblStatus);
            _panelTop.Controls.Add(panelTopButtons);

            // 2. Navigation toolbar
            _panelNav.Dock = DockStyle.Top;
            _panelNav.Height = 38;
            _panelNav.BackColor = Color.FromArgb(248, 250, 252);
            _panelNav.Padding = new Padding(8, 5, 8, 5);

            FlowLayoutPanel panelNavLeft = new FlowLayoutPanel
            {
                Dock = DockStyle.Left,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false,
                BackColor = Color.Transparent
            };

            _btnBack.Text = "◀";
            _btnBack.Size = new Size(32, 26);
            _btnBack.Margin = new Padding(0, 0, 4, 0);
            _btnBack.Click += (_, _) => { if (_webView.CoreWebView2?.CanGoBack == true) _webView.CoreWebView2.GoBack(); };

            _btnForward.Text = "▶";
            _btnForward.Size = new Size(32, 26);
            _btnForward.Margin = new Padding(0, 0, 4, 0);
            _btnForward.Click += (_, _) => { if (_webView.CoreWebView2?.CanGoForward == true) _webView.CoreWebView2.GoForward(); };

            _btnReload.Text = "⟳";
            _btnReload.Size = new Size(32, 26);
            _btnReload.Margin = new Padding(0, 0, 6, 0);
            _btnReload.Click += (_, _) => _webView.CoreWebView2?.Reload();

            panelNavLeft.Controls.Add(_btnBack);
            panelNavLeft.Controls.Add(_btnForward);
            panelNavLeft.Controls.Add(_btnReload);

            FlowLayoutPanel panelNavRight = new FlowLayoutPanel
            {
                Dock = DockStyle.Right,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false,
                BackColor = Color.Transparent
            };

            _btnGo.Text = "Go";
            _btnGo.Size = new Size(48, 26);
            _btnGo.Margin = new Padding(4, 0, 8, 0);
            _btnGo.Click += (_, _) => NavigateToNavUrl();

            _cmbQuickPortals.DropDownStyle = ComboBoxStyle.DropDownList;
            _cmbQuickPortals.Size = new Size(320, 26);
            _cmbQuickPortals.Margin = new Padding(0);
            PopulateQuickPortals();
            _cmbQuickPortals.SelectedIndexChanged += OnQuickPortalSelected;

            panelNavRight.Controls.Add(_btnGo);
            panelNavRight.Controls.Add(_cmbQuickPortals);

            Panel panelNavCenter = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.Transparent,
                Padding = new Padding(0, 1, 0, 1)
            };

            _txtNavUrl.Dock = DockStyle.Fill;
            _txtNavUrl.Text = _initialUrl;
            _txtNavUrl.KeyDown += (s, e) =>
            {
                if (e.KeyCode == Keys.Enter)
                {
                    NavigateToNavUrl();
                    e.Handled = true;
                    e.SuppressKeyPress = true;
                }
            };
            panelNavCenter.Controls.Add(_txtNavUrl);

            _panelNav.Controls.Add(panelNavCenter);
            _panelNav.Controls.Add(panelNavRight);
            _panelNav.Controls.Add(panelNavLeft);

            // 3. Main WebView2 Browser
            _webView.Dock = DockStyle.Fill;

            Controls.Add(_webView);
            Controls.Add(_panelNav);
            Controls.Add(_panelTop);
        }

        private void PopulateQuickPortals()
        {
            _cmbQuickPortals.Items.Clear();
            _cmbQuickPortals.Items.Add("🌐 Jump to Camera Portal or Recent Stream...");

            List<StreamHistoryItem> history = StreamHistoryManager.LoadHistory();
            if (history.Count > 0)
            {
                foreach (StreamHistoryItem? h in history.Take(10))
                {
                    _cmbQuickPortals.Items.Add($"🕒 {h.Title} - {h.Url}");
                }
            }

            List<ConfiguredCamera> configured = StreamHistoryManager.LoadConfiguredCameras();
            foreach (ConfiguredCamera? cam in configured.Where(p => p.Category == "My Cameras" || p.Category == "Camera Portals"))
            {
                _cmbQuickPortals.Items.Add($"📷 {cam.Name} ({cam.Url})");
            }

            _cmbQuickPortals.SelectedIndex = 0;
        }

        private void OnQuickPortalSelected(object? sender, EventArgs e)
        {
            if (_cmbQuickPortals.SelectedIndex <= 0) return;

            string selected = _cmbQuickPortals.SelectedItem?.ToString() ?? string.Empty;
            string targetUrl = string.Empty;

            int parenStart = selected.LastIndexOf('(');
            int parenEnd = selected.LastIndexOf(')');
            if (parenStart > 0 && parenEnd > parenStart)
            {
                targetUrl = selected.Substring(parenStart + 1, parenEnd - parenStart - 1).Trim();
            }
            else
            {
                int dashIdx = selected.LastIndexOf(" - ", StringComparison.Ordinal);
                if (dashIdx > 0)
                {
                    targetUrl = selected.Substring(dashIdx + 3).Trim();
                }
            }

            if (!string.IsNullOrWhiteSpace(targetUrl))
            {
                _txtNavUrl.Text = targetUrl;
                NavigateToNavUrl();
            }

            _cmbQuickPortals.SelectedIndex = 0;
        }

        private void NavigateToNavUrl()
        {
            string url = _txtNavUrl.Text.Trim();
            if (string.IsNullOrWhiteSpace(url)) return;

            if (!url.StartsWith("http://", StringComparison.OrdinalIgnoreCase) &&
                !url.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
            {
                url = "https://" + url;
                _txtNavUrl.Text = url;
            }

            try
            {
                _webView.CoreWebView2?.Navigate(url);
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, $"Failed to navigate to {url}: {ex.Message}", "Navigation Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async Task InitializeWebViewAsync()
        {
            try
            {
                string userDataFolder = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    "MotionHighlighter", "WebViewProfile");

                CoreWebView2Environment env = await CoreWebView2Environment.CreateAsync(userDataFolder: userDataFolder);
                await _webView.EnsureCoreWebView2Async(env);

                _webView.CoreWebView2.WebResourceResponseReceived += OnWebResourceResponseReceived;
                _webView.CoreWebView2.WebMessageReceived += OnWebMessageReceived;

                _webView.CoreWebView2.SourceChanged += (_, _) =>
                {
                    BeginInvoke(() =>
                    {
                        string current = _webView.Source?.ToString() ?? string.Empty;
                        if (!string.IsNullOrWhiteSpace(current) && current != "about:blank")
                        {
                            _txtNavUrl.Text = current;
                        }
                    });
                };

                _webView.CoreWebView2.NavigationCompleted += (_, _) =>
                {
                    BeginInvoke(() =>
                    {
                        if (string.IsNullOrEmpty(CapturedStreamUrl))
                        {
                            _lblStatus.Text = "Page loaded. When your camera video appears, click 'Use Captured Stream' to transfer it to Motion Highlighter.";
                        }
                    });
                };

                // Universal active video and canvas feed monitor (filters out thumbnail clips)
                await _webView.CoreWebView2.AddScriptToExecuteOnDocumentCreatedAsync(@"
                    (function() {
                        function findActiveFeed() {
                            const videos = Array.from(document.querySelectorAll('video'));
                            let best = null;
                            let maxScore = -1;

                            for (const v of videos) {
                                if (!v || v.videoWidth === 0 || v.videoHeight === 0) continue;

                                const w = v.videoWidth;
                                const h = v.videoHeight;

                                // Skip tiny thumbnail preview cards (like 240x136 cards on Google Home)
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
                                const isLive = (best.srcObject !== null && best.srcObject !== undefined) || !isFinite(best.duration);
                                return {
                                    src: best.currentSrc || best.src || '',
                                    width: best.videoWidth,
                                    height: best.videoHeight,
                                    isVideo: true,
                                    isLive: isLive
                                };
                            }

                            const canvases = Array.from(document.querySelectorAll('canvas'));
                            for (const c of canvases) {
                                if (c && c.width >= 320 && c.height >= 180) {
                                    return { src: '', width: c.width, height: c.height, isVideo: false, isLive: true };
                                }
                            }
                            return null;
                        }

                        setInterval(() => {
                            const feed = findActiveFeed();
                            if (feed) {
                                window.chrome.webview.postMessage(JSON.stringify({
                                    type: 'feed_active',
                                    src: feed.src,
                                    width: feed.width,
                                    height: feed.height,
                                    isVideo: feed.isVideo,
                                    isLive: feed.isLive
                                }));
                            }
                        }, 500);
                    })();
                ");

                _webView.CoreWebView2.Navigate(_initialUrl);
            }
            catch (Exception ex)
            {
                _lblStatus.Text = $"Failed to initialize browser: {ex.Message}";
                _lblStatus.ForeColor = Color.Red;
            }
        }

        private void OnWebMessageReceived(object? sender, CoreWebView2WebMessageReceivedEventArgs e)
        {
            try
            {
                string json = e.TryGetWebMessageAsString();
                if (string.IsNullOrWhiteSpace(json) || !json.Contains("feed_active"))
                {
                    return;
                }

                using JsonDocument doc = JsonDocument.Parse(json);
                JsonElement root = doc.RootElement;
                string src = root.TryGetProperty("src", out JsonElement srcProp) ? srcProp.GetString() ?? string.Empty : string.Empty;
                int width = root.TryGetProperty("width", out JsonElement wProp) ? wProp.GetInt32() : 0;
                int height = root.TryGetProperty("height", out JsonElement hProp) ? hProp.GetInt32() : 0;
                bool isLive = root.TryGetProperty("isLive", out JsonElement liveProp) && liveProp.GetBoolean();

                // Skip tiny thumbnail preview cards (< 320x180)
                if (width < 320 || height < 180)
                {
                    return;
                }

                BeginInvoke(() =>
                {
                    string pageUrl = _webView.Source?.ToString() ?? _initialUrl;
                    CapturedStreamUrl = pageUrl;
                    CapturedFormat = StreamFormat.WebLiveFeed;

                    _panelTop.BackColor = Color.FromArgb(220, 252, 231);
                    _lblStatus.Text = $"✓ Full camera live feed active ({width}×{height})!\nClick 'Use Captured Stream' on the right to start monitoring.";
                    _lblStatus.ForeColor = Color.FromArgb(21, 128, 61);
                });
            }
            catch { }
        }

        private async void OnWebResourceResponseReceived(object? sender, CoreWebView2WebResourceResponseReceivedEventArgs e)
        {
            try
            {
                string url = e.Request.Uri;
                string contentType = string.Empty;

                foreach (KeyValuePair<string, string> header in e.Response.Headers)
                {
                    if (string.Equals(header.Key, "Content-Type", StringComparison.OrdinalIgnoreCase))
                    {
                        contentType = header.Value;
                        break;
                    }
                }

                bool isStream = IsVideoStream(url, contentType);
                if (!isStream)
                {
                    return;
                }

                // If already tracking an active WebLiveFeed on page, don't let background sub-requests overwrite it
                if (CapturedFormat == StreamFormat.WebLiveFeed && !string.IsNullOrEmpty(CapturedStreamUrl))
                {
                    return;
                }

                StreamFormat format = WebpageStreamExtractor.DetectFormatFromContentType(contentType, url);
                if (format == StreamFormat.Unknown)
                {
                    format = WebpageStreamExtractor.DetectFormatFromUrl(url);
                }

                // Extract cookies
                List<CoreWebView2Cookie> cookies = await _webView.CoreWebView2.CookieManager.GetCookiesAsync(url);
                StreamAuthOptions auth = new StreamAuthOptions
                {
                    AuthType = AuthType.CustomHeaders,
                    UserAgent = _webView.CoreWebView2.Settings.UserAgent
                };

                if (cookies != null && cookies.Count > 0)
                {
                    string cookieHeader = string.Join("; ", cookies.Select(c => $"{c.Name}={c.Value}"));
                    auth.CustomHeaders["Cookie"] = cookieHeader;
                }

                foreach (KeyValuePair<string, string> reqHeader in e.Request.Headers)
                {
                    if (string.Equals(reqHeader.Key, "Authorization", StringComparison.OrdinalIgnoreCase))
                    {
                        auth.CustomHeaders["Authorization"] = reqHeader.Value;
                        if (reqHeader.Value.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
                        {
                            auth.AuthType = AuthType.BearerToken;
                            auth.BearerToken = reqHeader.Value.Substring(7).Trim();
                        }
                    }
                    else if (string.Equals(reqHeader.Key, "Referer", StringComparison.OrdinalIgnoreCase))
                    {
                        auth.CustomHeaders["Referer"] = reqHeader.Value;
                    }
                }

                BeginInvoke(() =>
                {
                    if (CapturedFormat != StreamFormat.WebLiveFeed)
                    {
                        CapturedStreamUrl = url;
                        CapturedFormat = format;
                        CapturedAuth = auth;

                        _panelTop.BackColor = Color.FromArgb(220, 252, 231);
                        _lblStatus.Text = $"✓ Intercepted camera stream packet ({format}):\n{url}";
                        _lblStatus.ForeColor = Color.FromArgb(21, 128, 61);
                    }
                });
            }
            catch { }
        }

        private static bool IsVideoStream(string url, string contentType)
        {
            if (string.IsNullOrWhiteSpace(url)) return false;

            // Never intercept static images, thumbnails, or preview clips!
            if (url.Contains("get_image", StringComparison.OrdinalIgnoreCase) ||
                url.Contains("nexusapi", StringComparison.OrdinalIgnoreCase) ||
                url.Contains("/thumbnail", StringComparison.OrdinalIgnoreCase) ||
                url.Contains("/preview", StringComparison.OrdinalIgnoreCase) ||
                url.Contains(".jpg", StringComparison.OrdinalIgnoreCase) ||
                url.Contains(".png", StringComparison.OrdinalIgnoreCase) ||
                url.Contains(".gif", StringComparison.OrdinalIgnoreCase) ||
                url.Contains(".webp", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            if (url.Contains(".mpd", StringComparison.OrdinalIgnoreCase) ||
                url.Contains(".m3u8", StringComparison.OrdinalIgnoreCase) ||
                url.Contains(".m4s", StringComparison.OrdinalIgnoreCase) ||
                url.Contains(".m4v", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            if (contentType.Contains("video/iso.segment", StringComparison.OrdinalIgnoreCase) ||
                contentType.Contains("dash+xml", StringComparison.OrdinalIgnoreCase) ||
                contentType.Contains("mpegurl", StringComparison.OrdinalIgnoreCase) ||
                contentType.Contains("x-m4v", StringComparison.OrdinalIgnoreCase) ||
                contentType.Contains("multipart/x-mixed-replace", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            return false;
        }

        private async Task OnUseStreamClickedAsync()
        {
            _btnUseStream.Enabled = false;

            try
            {
                // Prioritize active web page feed if on a camera portal or web page
                string currentUrl = _webView.Source?.ToString() ?? string.Empty;
                if (!string.IsNullOrWhiteSpace(currentUrl) && currentUrl != "about:blank")
                {
                    if (CapturedFormat == StreamFormat.WebLiveFeed ||
                        string.IsNullOrWhiteSpace(CapturedStreamUrl) ||
                        currentUrl.Contains("home.google.com", StringComparison.OrdinalIgnoreCase) ||
                        currentUrl.Contains("nest.com", StringComparison.OrdinalIgnoreCase) ||
                        currentUrl.Contains("ring.com", StringComparison.OrdinalIgnoreCase) ||
                        currentUrl.Contains("arlo.com", StringComparison.OrdinalIgnoreCase) ||
                        currentUrl.Contains("wyze.com", StringComparison.OrdinalIgnoreCase) ||
                        currentUrl.Contains("eufylife.com", StringComparison.OrdinalIgnoreCase))
                    {
                        CapturedStreamUrl = currentUrl;
                        CapturedFormat = StreamFormat.WebLiveFeed;
                    }
                }

                if (string.IsNullOrWhiteSpace(CapturedStreamUrl))
                {
                    currentUrl = _txtNavUrl.Text.Trim();
                    if (string.IsNullOrWhiteSpace(currentUrl))
                    {
                        currentUrl = _initialUrl;
                    }
                    CapturedStreamUrl = currentUrl;
                    CapturedFormat = StreamFormat.WebLiveFeed;
                }

                // Ensure cookies are captured from active domain
                if (_webView.CoreWebView2 != null)
                {
                    string targetDomainUrl = CapturedStreamUrl;
                    if (!targetDomainUrl.StartsWith("http://", StringComparison.OrdinalIgnoreCase) &&
                        !targetDomainUrl.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
                    {
                        targetDomainUrl = _webView.Source?.ToString() ?? _initialUrl;
                    }

                    List<CoreWebView2Cookie> cookies = await _webView.CoreWebView2.CookieManager.GetCookiesAsync(targetDomainUrl);
                    if (cookies != null && cookies.Count > 0)
                    {
                        string cookieHeader = string.Join("; ", cookies.Select(c => $"{c.Name}={c.Value}"));
                        CapturedAuth.CustomHeaders["Cookie"] = cookieHeader;
                    }

                    CapturedAuth.UserAgent = _webView.CoreWebView2.Settings.UserAgent;
                    if (CapturedAuth.AuthType == AuthType.None)
                    {
                        CapturedAuth.AuthType = AuthType.CustomHeaders;
                    }
                }

                // Save to stream history
                string title = _webView.CoreWebView2?.DocumentTitle ?? "Web Live Camera";
                StreamHistoryManager.SaveHistoryItem(CapturedStreamUrl, CapturedFormat, title);

                DialogResult = DialogResult.OK;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, $"Error finalizing stream capture: {ex.Message}", "Capture Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                _btnUseStream.Enabled = true;
            }
        }
    }
}
