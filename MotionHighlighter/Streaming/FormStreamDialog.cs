using System.Net;
using MotionHighlighter.Localization;

namespace MotionHighlighter.Streaming
{
    public partial class FormStreamDialog : Form
    {
        private readonly WebpageStreamExtractor _extractor = new();
        private readonly List<StreamCandidate> _discoveredCandidates = new();

        public string SelectedStreamUrl { get; private set; } = string.Empty;
        public StreamFormat SelectedFormat { get; private set; } = StreamFormat.Unknown;
        public StreamAuthOptions AuthOptions { get; private set; } = new();
        public string SelectedTitle { get; private set; } = string.Empty;

        public FormStreamDialog(string? initialUrl = null)
        {
            InitializeComponent();
            ApplyLocalization();

            _cmbAuthType.SelectedIndex = 0;
            _cmbAuthType.SelectedIndexChanged += (_, _) => UpdateAuthFieldsVisibility();
            UpdateAuthFieldsVisibility();

            PopulatePresetsAndHistory();
            _cmbPresetsAndHistory.SelectedIndexChanged += OnPresetOrHistorySelected;
            _cmbPresetsAndHistory.DropDown += (_, _) => PopulatePresetsAndHistory();
            _btnEditConfig.Click += (_, _) =>
            {
                StreamHistoryManager.OpenConfigFileInEditor();
                _lblStatus.Text = "Opened streams.config.json. Add your camera credentials, save the file, and re-select from dropdown.";
                _lblStatus.ForeColor = Color.Blue;
            };

            if (!string.IsNullOrWhiteSpace(initialUrl))
            {
                _txtUrl.Text = initialUrl;
            }

            _btnExtract.Click += async (_, _) => await ExtractStreamsAsync();
            _btnWebCapture.Click += (_, _) => OpenWebStreamCapture();
            _btnPasteCurl.Click += (_, _) => PasteCurlFromClipboard();
            _cmbDiscoveredStreams.SelectedIndexChanged += OnDiscoveredStreamSelected;
            _btnTestConnection.Click += async (_, _) => await TestConnectionAsync();
            _btnConnect.Click += (_, _) => OnConnectClicked();

            _txtUrl.TextChanged += OnUrlTextChanged;
        }

        private void ApplyLocalization()
        {
            Text = LocalizationManager.T("stream.title");
            _btnTestConnection.Text = LocalizationManager.T("stream.btn_test");
            _btnConnect.Text = LocalizationManager.T("stream.btn_connect");
            _btnCancel.Text = LocalizationManager.T("stream.btn_cancel");
            _btnWebCapture.Text = LocalizationManager.T("stream.btn_web_capture");
            _btnPasteCurl.Text = LocalizationManager.T("stream.btn_curl");
            _lblBearer.Text = LocalizationManager.T("stream.lbl_token");
            _lblUsername.Text = LocalizationManager.T("stream.lbl_username");
            _lblPassword.Text = LocalizationManager.T("stream.lbl_password");
            _lblHeaders.Text = LocalizationManager.T("stream.lbl_headers");
        }

        private void PopulatePresetsAndHistory()
        {
            _cmbPresetsAndHistory.Items.Clear();
            _cmbPresetsAndHistory.Items.Add("🌐 Select from Presets or Recent Stream History...");

            List<ConfiguredCamera> configured = StreamHistoryManager.LoadConfiguredCameras();

            List<ConfiguredCamera> myCameras = configured.Where(c => c.Category == "My Cameras").ToList();
            if (myCameras.Count > 0)
            {
                _cmbPresetsAndHistory.Items.Add("── My Cameras (from streams.config.json) ──");
                foreach (ConfiguredCamera cam in myCameras)
                {
                    _cmbPresetsAndHistory.Items.Add(cam);
                }
            }

            List<ConfiguredCamera> portals = configured.Where(c => c.Category == "Camera Portals").ToList();
            if (portals.Count > 0)
            {
                _cmbPresetsAndHistory.Items.Add("── Camera Web Portals (Require Login) ──");
                foreach (ConfiguredCamera cam in portals)
                {
                    _cmbPresetsAndHistory.Items.Add(cam);
                }
            }

            List<ConfiguredCamera> testStreams = configured.Where(c => c.Category == "Public Test Streams").ToList();
            if (testStreams.Count > 0)
            {
                _cmbPresetsAndHistory.Items.Add("── Public Demo & Test Streams ──");
                foreach (ConfiguredCamera cam in testStreams)
                {
                    _cmbPresetsAndHistory.Items.Add(cam);
                }
            }

            List<StreamHistoryItem> history = StreamHistoryManager.LoadHistory();
            if (history.Count > 0)
            {
                _cmbPresetsAndHistory.Items.Add("── Recent Stream History ──");
                foreach (StreamHistoryItem item in history)
                {
                    _cmbPresetsAndHistory.Items.Add(item);
                }
            }

            _cmbPresetsAndHistory.SelectedIndex = 0;
        }

        private void OnPresetOrHistorySelected(object? sender, EventArgs e)
        {
            if (_cmbPresetsAndHistory.SelectedIndex <= 0) return;

            object? selected = _cmbPresetsAndHistory.SelectedItem;
            if (selected == null) return;

            string selectedStr = selected.ToString() ?? string.Empty;
            if (selectedStr.StartsWith("──"))
            {
                _cmbPresetsAndHistory.SelectedIndex = 0;
                return;
            }

            if (selected is ConfiguredCamera cam)
            {
                _txtUrl.Text = cam.Url;
                SelectedStreamUrl = cam.Url;
                SelectedFormat = cam.Format != StreamFormat.Unknown ? cam.Format : WebpageStreamExtractor.DetectFormatFromUrl(cam.Url);
                SelectedTitle = cam.Name;

                // Apply pre-configured credentials
                switch (cam.AuthType)
                {
                    case AuthType.BearerToken:
                        _cmbAuthType.SelectedIndex = 1;
                        _txtBearerToken.Text = cam.BearerToken;
                        break;
                    case AuthType.BasicAuth:
                        _cmbAuthType.SelectedIndex = 2;
                        _txtUsername.Text = cam.Username;
                        _txtPassword.Text = cam.Password;
                        break;
                    case AuthType.CustomHeaders:
                        _cmbAuthType.SelectedIndex = 3;
                        break;
                    default:
                        _cmbAuthType.SelectedIndex = 0;
                        break;
                }

                if (!string.IsNullOrWhiteSpace(cam.UserAgent))
                {
                    _txtUserAgent.Text = cam.UserAgent;
                }

                if (cam.CustomHeaders != null && cam.CustomHeaders.Count > 0)
                {
                    if (_cmbAuthType.SelectedIndex == 0) _cmbAuthType.SelectedIndex = 3;
                    _txtCustomHeaders.Text = string.Join(Environment.NewLine, cam.CustomHeaders.Select(kv => $"{kv.Key}: {kv.Value}"));
                }
                else if (cam.AuthType != AuthType.CustomHeaders)
                {
                    _txtCustomHeaders.Text = string.Empty;
                }

                if (cam.QueryParameters != null && cam.QueryParameters.Count > 0)
                {
                    _txtQueryParams.Text = string.Join("&", cam.QueryParameters.Select(kv => $"{kv.Key}={kv.Value}"));
                }
                else
                {
                    _txtQueryParams.Text = string.Empty;
                }

                if (SelectedFormat == StreamFormat.WebLiveFeed)
                {
                    _lblStatus.Text = $"✓ Loaded '{cam.Name}'! If login is needed, click '🌐 Sign In & Capture...', or 'Connect & Play'.";
                    _lblStatus.ForeColor = Color.Blue;
                }
                else
                {
                    _lblStatus.Text = $"✓ Loaded '{cam.Name}' ({SelectedFormat}, {cam.AuthType}) with credentials! Click 'Connect & Play'.";
                    _lblStatus.ForeColor = Color.DarkGreen;
                }
                return;
            }

            if (selected is StreamHistoryItem historyItem)
            {
                _txtUrl.Text = historyItem.Url;
                SelectedStreamUrl = historyItem.Url;
                SelectedFormat = historyItem.Format;
                SelectedTitle = historyItem.Title;

                _lblStatus.Text = $"Selected from history: {historyItem.Title} ({historyItem.Format})";
                _lblStatus.ForeColor = Color.DarkGreen;
                return;
            }

            string targetUrl = string.Empty;
            int parenStart = selectedStr.LastIndexOf('(');
            int parenEnd = selectedStr.LastIndexOf(')');
            if (parenStart > 0 && parenEnd > parenStart)
            {
                targetUrl = selectedStr.Substring(parenStart + 1, parenEnd - parenStart - 1).Trim();
            }

            if (!string.IsNullOrWhiteSpace(targetUrl))
            {
                _txtUrl.Text = targetUrl;
                SelectedStreamUrl = targetUrl;
                SelectedFormat = WebpageStreamExtractor.DetectFormatFromUrl(targetUrl);

                if (SelectedFormat == StreamFormat.WebLiveFeed)
                {
                    _lblStatus.Text = "Camera portal selected. Click '🌐 Sign In & Capture...' to log in and capture, or 'Connect & Play'.";
                    _lblStatus.ForeColor = Color.Blue;
                }
                else
                {
                    _lblStatus.Text = $"Selected stream preset ({SelectedFormat}). Click 'Connect & Play'.";
                    _lblStatus.ForeColor = Color.DarkGreen;
                }
            }
        }

        private void OnUrlTextChanged(object? sender, EventArgs e)
        {
            string text = _txtUrl.Text.Trim();
            if (text.StartsWith("curl ", StringComparison.OrdinalIgnoreCase) || text.StartsWith("curl.exe ", StringComparison.OrdinalIgnoreCase))
            {
                ParseAndApplyCurl(text);
            }
        }

        private void PasteCurlFromClipboard()
        {
            try
            {
                string text = Clipboard.GetText();
                if (string.IsNullOrWhiteSpace(text))
                {
                    MessageBox.Show(this, LocalizationManager.T("stream.msg_clipboard_empty"), LocalizationManager.T("common.info"), MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                if (!ParseAndApplyCurl(text))
                {
                    MessageBox.Show(this, LocalizationManager.T("stream.msg_curl_parse_error"), LocalizationManager.T("common.warning"), MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, LocalizationManager.T("stream.msg_curl_read_error", ex.Message), LocalizationManager.T("common.error"), MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private bool ParseAndApplyCurl(string curlCommand)
        {
            if (!CurlParser.TryParse(curlCommand, out string url, out StreamAuthOptions auth))
            {
                return false;
            }

            _txtUrl.Text = url;
            if (!string.IsNullOrWhiteSpace(auth.UserAgent))
            {
                _txtUserAgent.Text = auth.UserAgent;
            }

            if (auth.AuthType == AuthType.BearerToken)
            {
                _cmbAuthType.SelectedIndex = 1;
                _txtBearerToken.Text = auth.BearerToken;
            }
            else if (auth.AuthType == AuthType.BasicAuth)
            {
                _cmbAuthType.SelectedIndex = 2;
                _txtUsername.Text = auth.Username;
                _txtPassword.Text = auth.Password;
            }
            else if (auth.CustomHeaders.Count > 0)
            {
                _cmbAuthType.SelectedIndex = 3;
            }

            if (auth.CustomHeaders.Count > 0)
            {
                _txtCustomHeaders.Text = string.Join(Environment.NewLine, auth.CustomHeaders.Select(kv => $"{kv.Key}: {kv.Value}"));
            }

            _lblStatus.Text = "✓ Imported stream URL & authentication headers from cURL!";
            _lblStatus.ForeColor = Color.DarkGreen;
            return true;
        }

        private void OpenWebStreamCapture()
        {
            try
            {
                string targetUrl = _txtUrl.Text.Trim();
                using FormWebStreamCapture webForm = new FormWebStreamCapture(targetUrl);
                if (webForm.ShowDialog(this) == DialogResult.OK && !string.IsNullOrEmpty(webForm.CapturedStreamUrl))
                {
                    _txtUrl.Text = webForm.CapturedStreamUrl;
                    SelectedStreamUrl = webForm.CapturedStreamUrl;
                    SelectedFormat = webForm.CapturedFormat;

                    string portalHost = Uri.TryCreate(webForm.CapturedStreamUrl, UriKind.Absolute, out Uri? pUri) ? pUri.Host : "Web Camera";
                    string candidateTitle = webForm.CapturedFormat == StreamFormat.WebLiveFeed ? $"{portalHost} Live Camera" : "Captured Video Stream";

                    StreamCandidate candidate = new StreamCandidate
                    {
                        Url = webForm.CapturedStreamUrl,
                        Format = webForm.CapturedFormat,
                        Title = candidateTitle,
                        Details = $"Captured from browser ({webForm.CapturedFormat})",
                        SourceElement = "WebView2 Auto-Capture"
                    };

                    _discoveredCandidates.Clear();
                    _cmbDiscoveredStreams.Items.Clear();
                    _discoveredCandidates.Add(candidate);
                    _cmbDiscoveredStreams.Items.Add($"[{candidate.Format}] {candidate.Title} - {candidate.Url}");
                    _cmbDiscoveredStreams.SelectedIndex = 0;

                    if (!string.IsNullOrWhiteSpace(webForm.CapturedAuth.UserAgent))
                    {
                        _txtUserAgent.Text = webForm.CapturedAuth.UserAgent;
                    }

                    if (webForm.CapturedAuth.AuthType == AuthType.BearerToken)
                    {
                        _cmbAuthType.SelectedIndex = 1;
                        _txtBearerToken.Text = webForm.CapturedAuth.BearerToken;
                    }
                    else
                    {
                        _cmbAuthType.SelectedIndex = 3; // Custom Headers
                    }

                    if (webForm.CapturedAuth.CustomHeaders.Count > 0)
                    {
                        _txtCustomHeaders.Text = string.Join(Environment.NewLine, webForm.CapturedAuth.CustomHeaders.Select(kv => $"{kv.Key}: {kv.Value}"));
                    }

                    // Save to history & refresh dropdown
                    StreamHistoryManager.SaveHistoryItem(webForm.CapturedStreamUrl, webForm.CapturedFormat, candidateTitle);
                    PopulatePresetsAndHistory();

                    _lblStatus.Text = $"✓ Captured stream ({webForm.CapturedFormat}) with session authentication! Ready to connect.";
                    _lblStatus.ForeColor = Color.DarkGreen;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, LocalizationManager.T("stream.msg_web_capture_error", ex.Message), LocalizationManager.T("common.error"), MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void UpdateAuthFieldsVisibility()
        {
            int index = _cmbAuthType.SelectedIndex;
            // 0: None, 1: Bearer Token, 2: Basic Auth, 3: Custom Headers Only
            bool isBearer = index == 1;
            bool isBasic = index == 2;

            _lblBearer.Visible = isBearer;
            _txtBearerToken.Visible = isBearer;

            _lblUsername.Visible = isBasic;
            _txtUsername.Visible = isBasic;
            _lblPassword.Visible = isBasic;
            _txtPassword.Visible = isBasic;
        }

        private StreamAuthOptions BuildAuthOptionsFromUi()
        {
            StreamAuthOptions auth = new StreamAuthOptions();

            switch (_cmbAuthType.SelectedIndex)
            {
                case 1:
                    auth.AuthType = AuthType.BearerToken;
                    auth.BearerToken = _txtBearerToken.Text.Trim();
                    break;
                case 2:
                    auth.AuthType = AuthType.BasicAuth;
                    auth.Username = _txtUsername.Text.Trim();
                    auth.Password = _txtPassword.Text;
                    break;
                case 3:
                    auth.AuthType = AuthType.CustomHeaders;
                    break;
                default:
                    auth.AuthType = AuthType.None;
                    break;
            }

            if (!string.IsNullOrWhiteSpace(_txtUserAgent.Text))
            {
                auth.UserAgent = _txtUserAgent.Text.Trim();
            }

            // Parse custom headers
            if (!string.IsNullOrWhiteSpace(_txtCustomHeaders.Text))
            {
                string[] lines = _txtCustomHeaders.Text.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries);
                foreach (string line in lines)
                {
                    int colon = line.IndexOf(':');
                    if (colon > 0)
                    {
                        string headerName = line.Substring(0, colon).Trim();
                        string headerVal = line.Substring(colon + 1).Trim();
                        auth.CustomHeaders[headerName] = headerVal;
                    }
                }
            }

            // Parse query parameters
            if (!string.IsNullOrWhiteSpace(_txtQueryParams.Text))
            {
                string raw = _txtQueryParams.Text.TrimStart('?');
                string[] pairs = raw.Split(new[] { '&', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (string pair in pairs)
                {
                    int eq = pair.IndexOf('=');
                    if (eq > 0)
                    {
                        string key = pair.Substring(0, eq).Trim();
                        string val = pair.Substring(eq + 1).Trim();
                        auth.QueryParameters[key] = val;
                    }
                }
            }

            return auth;
        }

        private async Task ExtractStreamsAsync()
        {
            string url = _txtUrl.Text.Trim();
            if (string.IsNullOrWhiteSpace(url))
            {
                MessageBox.Show(this, "Please enter a webpage URL to extract video streams from.", "URL Required", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (!url.StartsWith("http://", StringComparison.OrdinalIgnoreCase) &&
                !url.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
            {
                url = "https://" + url;
                _txtUrl.Text = url;
            }

            _btnExtract.Enabled = false;
            _lblStatus.Text = "Scanning webpage for stream manifests and video sources...";
            _lblStatus.ForeColor = Color.Blue;

            try
            {
                StreamAuthOptions auth = BuildAuthOptionsFromUi();
                List<StreamCandidate> candidates = await _extractor.ExtractStreamsFromUrlAsync(url, auth);

                _discoveredCandidates.Clear();
                _cmbDiscoveredStreams.Items.Clear();

                if (candidates.Count == 0)
                {
                    _lblStatus.Text = "No video streams detected on this webpage.";
                    _lblStatus.ForeColor = Color.DarkOrange;
                    MessageBox.Show(this, LocalizationManager.T("stream.msg_no_streams_found"), LocalizationManager.T("common.info"), MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                _discoveredCandidates.AddRange(candidates);
                foreach (StreamCandidate c in candidates)
                {
                    _cmbDiscoveredStreams.Items.Add($"[{c.Format}] {c.Title} ({c.SourceElement})");
                }

                _cmbDiscoveredStreams.SelectedIndex = 0;
                _lblStatus.Text = $"Found {candidates.Count} stream {(candidates.Count == 1 ? "candidate" : "candidates")}!";
                _lblStatus.ForeColor = Color.DarkGreen;
            }
            catch (Exception ex)
            {
                _lblStatus.Text = "Failed to extract streams from webpage.";
                _lblStatus.ForeColor = Color.Red;
                MessageBox.Show(this, LocalizationManager.T("stream.msg_extract_error", ex.Message), LocalizationManager.T("common.error"), MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                _btnExtract.Enabled = true;
            }
        }

        private void OnDiscoveredStreamSelected(object? sender, EventArgs e)
        {
            int idx = _cmbDiscoveredStreams.SelectedIndex;
            if (idx >= 0 && idx < _discoveredCandidates.Count)
            {
                StreamCandidate candidate = _discoveredCandidates[idx];
                _txtUrl.Text = candidate.Url;
            }
        }

        private async Task TestConnectionAsync()
        {
            string url = _txtUrl.Text.Trim();
            if (string.IsNullOrWhiteSpace(url))
            {
                MessageBox.Show(this, "Please enter a URL to test.", "URL Required", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            _btnTestConnection.Enabled = false;
            _lblStatus.Text = "Testing connection...";
            _lblStatus.ForeColor = Color.Blue;

            try
            {
                if (!Uri.TryCreate(url, UriKind.Absolute, out Uri? uri))
                {
                    throw new InvalidOperationException("Invalid URL format.");
                }

                StreamAuthOptions auth = BuildAuthOptionsFromUi();
                uri = auth.ApplyToUri(uri);

                using HttpClientHandler handler = new HttpClientHandler
                {
                    AllowAutoRedirect = true,
                    AutomaticDecompression = DecompressionMethods.All
                };
                using HttpClient client = new HttpClient(handler)
                {
                    Timeout = TimeSpan.FromSeconds(10)
                };

                using HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, uri);
                auth.ApplyToRequest(request, uri);

                using HttpResponseMessage response = await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);
                string contentType = response.Content.Headers.ContentType?.MediaType ?? "unknown";
                long? length = response.Content.Headers.ContentLength;

                if (response.IsSuccessStatusCode)
                {
                    _lblStatus.Text = $"✓ Success! HTTP {(int)response.StatusCode} OK ({contentType})";
                    _lblStatus.ForeColor = Color.DarkGreen;
                }
                else
                {
                    _lblStatus.Text = $"✗ Server responded with HTTP {(int)response.StatusCode} {response.ReasonPhrase}";
                    _lblStatus.ForeColor = Color.Red;
                }
            }
            catch (Exception ex)
            {
                _lblStatus.Text = $"✗ Connection failed: {ex.Message}";
                _lblStatus.ForeColor = Color.Red;
            }
            finally
            {
                _btnTestConnection.Enabled = true;
            }
        }

        private void OnConnectClicked()
        {
            string url = _txtUrl.Text.Trim();
            if (string.IsNullOrWhiteSpace(url))
            {
                MessageBox.Show(this, LocalizationManager.T("stream.msg_url_required"), LocalizationManager.T("common.warning"), MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (!url.StartsWith("http://", StringComparison.OrdinalIgnoreCase) &&
                !url.StartsWith("https://", StringComparison.OrdinalIgnoreCase) &&
                !url.StartsWith("rtsp://", StringComparison.OrdinalIgnoreCase))
            {
                url = "https://" + url;
                _txtUrl.Text = url;
            }

            SelectedStreamUrl = url;
            SelectedFormat = WebpageStreamExtractor.DetectFormatFromUrl(url);

            int selectedCandidateIdx = _cmbDiscoveredStreams.SelectedIndex;
            if (selectedCandidateIdx >= 0 && selectedCandidateIdx < _discoveredCandidates.Count &&
                string.Equals(_discoveredCandidates[selectedCandidateIdx].Url, url, StringComparison.OrdinalIgnoreCase))
            {
                StreamCandidate candidate = _discoveredCandidates[selectedCandidateIdx];
                SelectedFormat = candidate.Format;
                SelectedTitle = candidate.Title;
            }
            else
            {
                if (Uri.TryCreate(url, UriKind.Absolute, out Uri? parsedUri))
                {
                    string fn = Path.GetFileName(parsedUri.AbsolutePath);
                    SelectedTitle = !string.IsNullOrWhiteSpace(fn) && fn.Length > 2 ? $"{parsedUri.Host}/{fn}" : parsedUri.Host;
                }
                else
                {
                    SelectedTitle = url;
                }
            }

            AuthOptions = BuildAuthOptionsFromUi();

            // Save to history for easy selection in future sessions
            StreamHistoryManager.SaveHistoryItem(SelectedStreamUrl, SelectedFormat, SelectedTitle);

            DialogResult = DialogResult.OK;
            Close();
        }
    }
}
