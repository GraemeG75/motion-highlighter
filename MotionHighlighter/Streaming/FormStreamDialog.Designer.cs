using System.ComponentModel;

namespace MotionHighlighter.Streaming
{
    partial class FormStreamDialog
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this._cmbPresetsAndHistory = new ComboBox();
            this._btnEditConfig = new Button();
            this._txtUrl = new TextBox();
            this._btnExtract = new Button();
            this._btnWebCapture = new Button();
            this._btnPasteCurl = new Button();
            this._cmbDiscoveredStreams = new ComboBox();
            this._lblDiscovered = new Label();
            this._cmbAuthType = new ComboBox();
            this._txtUsername = new TextBox();
            this._txtPassword = new TextBox();
            this._txtBearerToken = new TextBox();
            this._txtCustomHeaders = new TextBox();
            this._txtUserAgent = new TextBox();
            this._txtQueryParams = new TextBox();
            this._btnTestConnection = new Button();
            this._lblStatus = new Label();
            this._btnConnect = new Button();
            this._btnCancel = new Button();

            GroupBox grpSource = new GroupBox();
            GroupBox grpAuth = new GroupBox();
            Label lblPresets = new Label();
            Label lblUrl = new Label();
            Label lblAuthType = new Label();
            this._lblUsername = new Label();
            this._lblPassword = new Label();
            this._lblBearer = new Label();
            this._lblHeaders = new Label();
            Label lblUserAgent = new Label();
            Label lblQueryParams = new Label();
            FlowLayoutPanel panelButtons = new FlowLayoutPanel();

            grpSource.SuspendLayout();
            grpAuth.SuspendLayout();
            panelButtons.SuspendLayout();
            this.SuspendLayout();

            // 
            // grpSource
            // 
            grpSource.Controls.Add(lblPresets);
            grpSource.Controls.Add(this._cmbPresetsAndHistory);
            grpSource.Controls.Add(this._btnEditConfig);
            grpSource.Controls.Add(this._btnPasteCurl);
            grpSource.Controls.Add(lblUrl);
            grpSource.Controls.Add(this._txtUrl);
            grpSource.Controls.Add(this._btnExtract);
            grpSource.Controls.Add(this._btnWebCapture);
            grpSource.Controls.Add(this._lblDiscovered);
            grpSource.Controls.Add(this._cmbDiscoveredStreams);
            grpSource.Location = new Point(16, 12);
            grpSource.Name = "grpSource";
            grpSource.Size = new Size(652, 175);
            grpSource.TabIndex = 0;
            grpSource.TabStop = false;
            grpSource.Text = "Stream or Webpage Source";

            // 
            // lblPresets
            // 
            lblPresets.AutoSize = true;
            lblPresets.Location = new Point(16, 22);
            lblPresets.Name = "lblPresets";
            lblPresets.Size = new Size(185, 15);
            lblPresets.Text = "Camera Portals & Stream History:";

            // 
            // _cmbPresetsAndHistory
            // 
            this._cmbPresetsAndHistory.DropDownStyle = ComboBoxStyle.DropDownList;
            this._cmbPresetsAndHistory.FormattingEnabled = true;
            this._cmbPresetsAndHistory.Location = new Point(16, 40);
            this._cmbPresetsAndHistory.Name = "_cmbPresetsAndHistory";
            this._cmbPresetsAndHistory.Size = new Size(390, 23);
            this._cmbPresetsAndHistory.TabIndex = 0;

            // 
            // _btnEditConfig
            // 
            this._btnEditConfig.Location = new Point(412, 39);
            this._btnEditConfig.Name = "_btnEditConfig";
            this._btnEditConfig.Size = new Size(110, 25);
            this._btnEditConfig.TabIndex = 1;
            this._btnEditConfig.Text = "⚙ Config JSON";
            this._btnEditConfig.UseVisualStyleBackColor = true;

            // 
            // _btnPasteCurl
            // 
            this._btnPasteCurl.Location = new Point(528, 39);
            this._btnPasteCurl.Name = "_btnPasteCurl";
            this._btnPasteCurl.Size = new Size(108, 25);
            this._btnPasteCurl.TabIndex = 2;
            this._btnPasteCurl.Text = "📋 Paste cURL";
            this._btnPasteCurl.UseVisualStyleBackColor = true;

            // 
            // lblUrl
            // 
            lblUrl.AutoSize = true;
            lblUrl.Location = new Point(16, 72);
            lblUrl.Name = "lblUrl";
            lblUrl.Size = new Size(137, 15);
            lblUrl.Text = "Stream / Webpage URL:";

            // 
            // _txtUrl
            // 
            this._txtUrl.Location = new Point(16, 90);
            this._txtUrl.Name = "_txtUrl";
            this._txtUrl.PlaceholderText = "e.g. https://home.google.com/... or https://.../manifest.mpd or .m3u8, .m4v, .m4s";
            this._txtUrl.Size = new Size(320, 23);
            this._txtUrl.TabIndex = 2;

            // 
            // _btnExtract
            // 
            this._btnExtract.Location = new Point(342, 89);
            this._btnExtract.Name = "_btnExtract";
            this._btnExtract.Size = new Size(140, 25);
            this._btnExtract.TabIndex = 3;
            this._btnExtract.Text = "Extract from Page";
            this._btnExtract.UseVisualStyleBackColor = true;

            // 
            // _btnWebCapture
            // 
            this._btnWebCapture.Font = new Font("Segoe UI", 9F, FontStyle.Regular);
            this._btnWebCapture.Location = new Point(488, 89);
            this._btnWebCapture.Name = "_btnWebCapture";
            this._btnWebCapture.Size = new Size(148, 25);
            this._btnWebCapture.TabIndex = 4;
            this._btnWebCapture.Text = "🌐 Sign In & Capture...";
            this._btnWebCapture.UseVisualStyleBackColor = true;

            // 
            // _lblDiscovered
            // 
            this._lblDiscovered.AutoSize = true;
            this._lblDiscovered.Location = new Point(16, 122);
            this._lblDiscovered.Name = "_lblDiscovered";
            this._lblDiscovered.Size = new Size(148, 15);
            this._lblDiscovered.Text = "Detected Stream Candidates:";

            // 
            // _cmbDiscoveredStreams
            // 
            this._cmbDiscoveredStreams.DropDownStyle = ComboBoxStyle.DropDownList;
            this._cmbDiscoveredStreams.FormattingEnabled = true;
            this._cmbDiscoveredStreams.Location = new Point(16, 140);
            this._cmbDiscoveredStreams.Name = "_cmbDiscoveredStreams";
            this._cmbDiscoveredStreams.Size = new Size(620, 23);
            this._cmbDiscoveredStreams.TabIndex = 5;

            // 
            // grpAuth
            // 
            grpAuth.Controls.Add(lblAuthType);
            grpAuth.Controls.Add(this._cmbAuthType);
            grpAuth.Controls.Add(this._lblBearer);
            grpAuth.Controls.Add(this._txtBearerToken);
            grpAuth.Controls.Add(this._lblUsername);
            grpAuth.Controls.Add(this._txtUsername);
            grpAuth.Controls.Add(this._lblPassword);
            grpAuth.Controls.Add(this._txtPassword);
            grpAuth.Controls.Add(this._lblHeaders);
            grpAuth.Controls.Add(this._txtCustomHeaders);
            grpAuth.Controls.Add(lblQueryParams);
            grpAuth.Controls.Add(this._txtQueryParams);
            grpAuth.Controls.Add(lblUserAgent);
            grpAuth.Controls.Add(this._txtUserAgent);
            grpAuth.Location = new Point(16, 196);
            grpAuth.Name = "grpAuth";
            grpAuth.Size = new Size(652, 230);
            grpAuth.TabIndex = 1;
            grpAuth.TabStop = false;
            grpAuth.Text = "Authentication & Request Headers";

            // 
            // lblAuthType
            // 
            lblAuthType.AutoSize = true;
            lblAuthType.Location = new Point(16, 24);
            lblAuthType.Name = "lblAuthType";
            lblAuthType.Size = new Size(63, 15);
            lblAuthType.Text = "Auth Type:";

            // 
            // _cmbAuthType
            // 
            this._cmbAuthType.DropDownStyle = ComboBoxStyle.DropDownList;
            this._cmbAuthType.FormattingEnabled = true;
            this._cmbAuthType.Items.AddRange(new object[] {
                "None (Public)",
                "Bearer Token",
                "Basic Auth (Username / Password)",
                "Custom Headers Only"
            });
            this._cmbAuthType.Location = new Point(16, 42);
            this._cmbAuthType.Name = "_cmbAuthType";
            this._cmbAuthType.Size = new Size(200, 23);
            this._cmbAuthType.TabIndex = 0;

            // 
            // _lblBearer
            // 
            this._lblBearer.AutoSize = true;
            this._lblBearer.Location = new Point(232, 24);
            this._lblBearer.Name = "_lblBearer";
            this._lblBearer.Size = new Size(77, 15);
            this._lblBearer.Text = "Bearer Token:";

            // 
            // _txtBearerToken
            // 
            this._txtBearerToken.Location = new Point(232, 42);
            this._txtBearerToken.Name = "_txtBearerToken";
            this._txtBearerToken.PlaceholderText = "eyJhbGciOi...";
            this._txtBearerToken.Size = new Size(404, 23);
            this._txtBearerToken.TabIndex = 1;

            // 
            // _lblUsername
            // 
            this._lblUsername.AutoSize = true;
            this._lblUsername.Location = new Point(232, 24);
            this._lblUsername.Name = "_lblUsername";
            this._lblUsername.Size = new Size(63, 15);
            this._lblUsername.Text = "Username:";

            // 
            // _txtUsername
            // 
            this._txtUsername.Location = new Point(232, 42);
            this._txtUsername.Name = "_txtUsername";
            this._txtUsername.Size = new Size(195, 23);
            this._txtUsername.TabIndex = 2;

            // 
            // _lblPassword
            // 
            this._lblPassword.AutoSize = true;
            this._lblPassword.Location = new Point(441, 24);
            this._lblPassword.Name = "_lblPassword";
            this._lblPassword.Size = new Size(60, 15);
            this._lblPassword.Text = "Password:";

            // 
            // _txtPassword
            // 
            this._txtPassword.Location = new Point(441, 42);
            this._txtPassword.Name = "_txtPassword";
            this._txtPassword.Size = new Size(195, 23);
            this._txtPassword.TabIndex = 3;
            this._txtPassword.UseSystemPasswordChar = true;

            // 
            // _lblHeaders
            // 
            this._lblHeaders.AutoSize = true;
            this._lblHeaders.Location = new Point(16, 75);
            this._lblHeaders.Name = "_lblHeaders";
            this._lblHeaders.Size = new Size(207, 15);
            this._lblHeaders.Text = "Custom Headers (e.g. Cookie, Referer):";

            // 
            // _txtCustomHeaders
            // 
            this._txtCustomHeaders.Font = new Font("Consolas", 8.5F);
            this._txtCustomHeaders.Location = new Point(16, 93);
            this._txtCustomHeaders.Multiline = true;
            this._txtCustomHeaders.Name = "_txtCustomHeaders";
            this._txtCustomHeaders.PlaceholderText = "Cookie: session_id=xyz; auth=abc\r\nReferer: https://example.com/\r\nX-API-Key: secret123";
            this._txtCustomHeaders.ScrollBars = ScrollBars.Vertical;
            this._txtCustomHeaders.Size = new Size(620, 56);
            this._txtCustomHeaders.TabIndex = 4;

            // 
            // lblQueryParams
            // 
            lblQueryParams.AutoSize = true;
            lblQueryParams.Location = new Point(16, 157);
            lblQueryParams.Name = "lblQueryParams";
            lblQueryParams.Size = new Size(183, 15);
            lblQueryParams.Text = "URL Query Parameters (optional):";

            // 
            // _txtQueryParams
            // 
            this._txtQueryParams.Location = new Point(16, 175);
            this._txtQueryParams.Name = "_txtQueryParams";
            this._txtQueryParams.PlaceholderText = "token=xyz&key=123";
            this._txtQueryParams.Size = new Size(300, 23);
            this._txtQueryParams.TabIndex = 5;

            // 
            // lblUserAgent
            // 
            lblUserAgent.AutoSize = true;
            lblUserAgent.Location = new Point(332, 157);
            lblUserAgent.Name = "lblUserAgent";
            lblUserAgent.Size = new Size(67, 15);
            lblUserAgent.Text = "User-Agent:";

            // 
            // _txtUserAgent
            // 
            this._txtUserAgent.Location = new Point(332, 175);
            this._txtUserAgent.Name = "_txtUserAgent";
            this._txtUserAgent.Size = new Size(304, 23);
            this._txtUserAgent.TabIndex = 6;

            // 
            // _lblStatus
            // 
            this._lblStatus.AutoEllipsis = true;
            this._lblStatus.AutoSize = false;
            this._lblStatus.Location = new Point(16, 434);
            this._lblStatus.Name = "_lblStatus";
            this._lblStatus.Size = new Size(652, 28);
            this._lblStatus.Text = "Ready.";
            this._lblStatus.TextAlign = ContentAlignment.MiddleLeft;

            // 
            // _btnTestConnection
            // 
            this._btnTestConnection.Location = new Point(16, 468);
            this._btnTestConnection.Name = "_btnTestConnection";
            this._btnTestConnection.Size = new Size(120, 32);
            this._btnTestConnection.TabIndex = 2;
            this._btnTestConnection.Text = "Test Connection";
            this._btnTestConnection.UseVisualStyleBackColor = true;

            // 
            // panelButtons
            // 
            panelButtons.Controls.Add(this._btnCancel);
            panelButtons.Controls.Add(this._btnConnect);
            panelButtons.FlowDirection = FlowDirection.RightToLeft;
            panelButtons.Location = new Point(388, 466);
            panelButtons.Name = "panelButtons";
            panelButtons.Size = new Size(280, 36);
            panelButtons.TabIndex = 3;

            // 
            // _btnCancel
            // 
            this._btnCancel.DialogResult = DialogResult.Cancel;
            this._btnCancel.Location = new Point(187, 3);
            this._btnCancel.Name = "_btnCancel";
            this._btnCancel.Size = new Size(90, 30);
            this._btnCancel.TabIndex = 1;
            this._btnCancel.Text = "Cancel";
            this._btnCancel.UseVisualStyleBackColor = true;

            // 
            // _btnConnect
            // 
            this._btnConnect.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            this._btnConnect.Location = new Point(61, 3);
            this._btnConnect.Name = "_btnConnect";
            this._btnConnect.Size = new Size(120, 30);
            this._btnConnect.TabIndex = 0;
            this._btnConnect.Text = "Connect & Play";
            this._btnConnect.UseVisualStyleBackColor = true;

            // 
            // FormStreamDialog
            // 
            this.AcceptButton = this._btnConnect;
            this.CancelButton = this._btnCancel;
            this.ClientSize = new Size(684, 514);
            this.Controls.Add(this._btnTestConnection);
            this.Controls.Add(this._lblStatus);
            this.Controls.Add(panelButtons);
            this.Controls.Add(grpAuth);
            this.Controls.Add(grpSource);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FormStreamDialog";
            this.ShowInTaskbar = false;
            this.StartPosition = FormStartPosition.CenterParent;
            this.Text = "Open Streaming Video Source";

            // 
            // _toolTip
            // 
            this._toolTip = new ToolTip(this.components);
            this._toolTip.AutoPopDelay = 10000;
            this._toolTip.InitialDelay = 350;
            this._toolTip.ReshowDelay = 150;
            this._toolTip.ShowAlways = true;

            this._toolTip.SetToolTip(this._cmbPresetsAndHistory, "Choose from pre-configured camera portals (Google Home, Nest, Ring, Arlo, Wyze, UniFi) or recent stream history.");
            this._toolTip.SetToolTip(this._btnEditConfig, "Open streams.config.json in your editor to add camera URLs, RTSP links, usernames, and passwords.");
            this._toolTip.SetToolTip(this._txtUrl, "Webpage URL (e.g. camera portal) or direct video stream URL (.mpd, .m3u8, .m4s, .m4v, .mp4, rtsp://).");
            this._toolTip.SetToolTip(this._btnExtract, "Extract Streams: Automatically parses the webpage HTML to find video player configs and stream manifests.");
            this._toolTip.SetToolTip(this._btnWebCapture, "Sign In & Capture: Opens an in-app browser to log in to your camera and capture the live stream directly.");
            this._toolTip.SetToolTip(this._btnPasteCurl, "Paste cURL: Parses a cURL command copied from browser DevTools to import URL and all authentication headers.");
            this._toolTip.SetToolTip(this._cmbDiscoveredStreams, "Select one of the video stream candidates discovered on the webpage.");
            this._toolTip.SetToolTip(this._cmbAuthType, "Choose authentication method: None (Public), Bearer Token, Basic Auth, or Custom Headers.");
            this._toolTip.SetToolTip(this._txtCustomHeaders, "Custom HTTP request headers (Cookie, Referer, Authorization, API keys), one per line.");
            this._toolTip.SetToolTip(this._txtQueryParams, "Additional URL query parameters (e.g. token=xyz&key=123).");
            this._toolTip.SetToolTip(this._btnTestConnection, "Test whether the server responds to an HTTP request with the current URL and auth settings.");
            this._toolTip.SetToolTip(this._btnConnect, "Connect to the selected stream and begin live motion highlighting.");

            grpSource.ResumeLayout(false);
            grpSource.PerformLayout();
            grpAuth.ResumeLayout(false);
            grpAuth.PerformLayout();
            panelButtons.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        private ToolTip _toolTip;
        private ComboBox _cmbPresetsAndHistory;
        private Button _btnEditConfig;
        private TextBox _txtUrl;
        private Button _btnExtract;
        private Button _btnWebCapture;
        private Button _btnPasteCurl;
        private ComboBox _cmbDiscoveredStreams;
        private Label _lblDiscovered;
        private ComboBox _cmbAuthType;
        private TextBox _txtUsername;
        private TextBox _txtPassword;
        private TextBox _txtBearerToken;
        private TextBox _txtCustomHeaders;
        private TextBox _txtUserAgent;
        private TextBox _txtQueryParams;
        private Label _lblUsername;
        private Label _lblPassword;
        private Label _lblBearer;
        private Label _lblHeaders;
        private Button _btnTestConnection;
        private Label _lblStatus;
        private Button _btnConnect;
        private Button _btnCancel;
    }
}
