using System.Diagnostics;
using System.Reflection;
using MotionHighlighter.Localization;

namespace MotionHighlighter.Updating
{
    /// <summary>
    /// Displays available application update information, release notes, and manages downloading and executing the installer.
    /// </summary>
    public partial class FormUpdate : Form
    {
        private readonly UpdateInfo _update;
        private readonly IUpdateService _updateService;
        private CancellationTokenSource? _downloadCts;

        /// <summary>
        /// Initializes a new instance of the <see cref="FormUpdate"/> dialog.
        /// </summary>
        /// <param name="update">Update details for the new version.</param>
        /// <param name="updateService">Service used to download and launch the installer.</param>
        public FormUpdate(UpdateInfo update, IUpdateService updateService)
        {
            _update = update ?? throw new ArgumentNullException(nameof(update));
            _updateService = updateService ?? throw new ArgumentNullException(nameof(updateService));

            InitializeComponent();
            ApplyLocalization();
            LoadMascotImage();
            PopulateReleaseData();
        }

        /// <summary>
        /// Applies current localized strings to all form controls.
        /// </summary>
        private void ApplyLocalization()
        {
            Text = LocalizationManager.T("update.title");
            lblTitle.Text = LocalizationManager.T("update.heading_available");
            lblCurrentVersion.Text = LocalizationManager.T("update.current_version", AppVersion.VersionString);
            lblLatestVersion.Text = LocalizationManager.T("update.latest_version", _update.Version.ToString());
            lblReleaseNotesHeader.Text = LocalizationManager.T("update.release_notes");
            btnDownload.Text = LocalizationManager.T("update.btn_download_install");
            btnLater.Text = LocalizationManager.T("update.btn_later");
            btnViewGitHub.Text = LocalizationManager.T("update.btn_view_github");
            lblStatus.Text = LocalizationManager.T("app.ready");
        }

        /// <summary>
        /// Loads the kitten mascot image for branding and visual identity.
        /// </summary>
        private void LoadMascotImage()
        {
            try
            {
                string localPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "branding", "kitten.png");
                if (File.Exists(localPath))
                {
                    picMascot.Image = Image.FromFile(localPath);
                }
                else
                {
                    Assembly assembly = Assembly.GetExecutingAssembly();
                    string? resourceName = assembly.GetManifestResourceNames()
                        .FirstOrDefault(n => n.EndsWith("kitten.png", StringComparison.OrdinalIgnoreCase));

                    if (resourceName != null)
                    {
                        using Stream? stream = assembly.GetManifestResourceStream(resourceName);
                        if (stream != null)
                        {
                            picMascot.Image = Image.FromStream(stream);
                        }
                    }
                }

                toolTip.SetToolTip(picMascot, LocalizationManager.T("about.mascot_alt"));
            }
            catch
            {
                // Non-fatal if mascot fails to load
            }
        }

        /// <summary>
        /// Populates dialog fields with details from the update payload.
        /// </summary>
        private void PopulateReleaseData()
        {
            if (!string.IsNullOrWhiteSpace(_update.ReleaseNotes))
            {
                txtReleaseNotes.Text = _update.ReleaseNotes.Replace("\r\n", "\n").Replace("\n", Environment.NewLine);
            }
            else
            {
                txtReleaseNotes.Text = LocalizationManager.T("update.heading_available");
            }

            if (_update.InstallerAsset == null)
            {
                btnDownload.Text = LocalizationManager.T("update.btn_view_github");
            }
        }

        /// <summary>
        /// Renders a vivid Progress Pride Flag gradient strip across the top of the dialog.
        /// </summary>
        /// <param name="sender">Event sender.</param>
        /// <param name="e">Paint event arguments.</param>
        private void OnPrideBannerPaint(object? sender, PaintEventArgs e)
        {
            Color[] prideColors = new Color[]
            {
                Color.FromArgb(255, 255, 255), // White (transgender pride)
                Color.FromArgb(255, 175, 200), // Pink (transgender pride)
                Color.FromArgb(116, 215, 238), // Light Blue (transgender pride)
                Color.FromArgb(97, 57, 21),    // Brown (marginalized communities)
                Color.FromArgb(0, 0, 0),       // Black (marginalized communities)
                Color.FromArgb(228, 3, 3),     // Red (life)
                Color.FromArgb(255, 140, 0),   // Orange (healing)
                Color.FromArgb(255, 237, 0),   // Yellow (sunlight)
                Color.FromArgb(0, 128, 38),    // Green (nature)
                Color.FromArgb(0, 76, 255),    // Blue (serenity)
                Color.FromArgb(115, 41, 130)   // Purple (spirit)
            };

            int w = panelPrideBanner.Width;
            int h = panelPrideBanner.Height;
            float stripeWidth = (float)w / prideColors.Length;

            for (int i = 0; i < prideColors.Length; i++)
            {
                float x = i * stripeWidth;
                using SolidBrush brush = new(prideColors[i]);
                e.Graphics.FillRectangle(brush, x, 0, stripeWidth + 1, h);
            }
        }

        /// <summary>
        /// Initiates downloading the installer asset and reports progress.
        /// </summary>
        /// <param name="sender">Event sender.</param>
        /// <param name="e">Event arguments.</param>
        private async void OnDownloadClicked(object? sender, EventArgs e)
        {
            if (_update.InstallerAsset == null)
            {
                OpenGitHubReleasePage();
                return;
            }

            btnDownload.Enabled = false;
            btnLater.Enabled = false;
            progressBar.Visible = true;
            progressBar.Value = 0;

            _downloadCts = new CancellationTokenSource();
            Progress<double> progress = new(fraction =>
            {
                int percentage = Math.Clamp((int)(fraction * 100), 0, 100);
                progressBar.Value = percentage;
                lblStatus.Text = LocalizationManager.T("update.status_downloading", percentage);
            });

            try
            {
                lblStatus.Text = LocalizationManager.T("update.status_downloading", 0);
                string installerPath = await _updateService.DownloadInstallerAsync(_update, progress, _downloadCts.Token);

                lblStatus.Text = LocalizationManager.T("update.status_download_complete");

                DialogResult result = MessageBox.Show(
                    this,
                    LocalizationManager.T("update.prompt_install"),
                    LocalizationManager.T("update.title"),
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question);

                if (result == DialogResult.Yes)
                {
                    bool launched = _updateService.LaunchInstaller(installerPath, silent: false, exitCurrentApp: true);
                    if (launched)
                    {
                        Close();
                    }
                }
                else
                {
                    btnDownload.Enabled = true;
                    btnLater.Enabled = true;
                }
            }
            catch (Exception ex)
            {
                lblStatus.Text = LocalizationManager.T("update.status_download_failed", ex.Message);
                MessageBox.Show(
                    this,
                    LocalizationManager.T("update.status_download_failed", ex.Message),
                    LocalizationManager.T("common.error"),
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);

                btnDownload.Enabled = true;
                btnLater.Enabled = true;
            }
        }

        /// <summary>
        /// Opens the GitHub release page in the default system browser.
        /// </summary>
        /// <param name="sender">Event sender.</param>
        /// <param name="e">Event arguments.</param>
        private void OnViewGitHubClicked(object? sender, EventArgs e)
        {
            OpenGitHubReleasePage();
        }

        /// <summary>
        /// Launches the web browser targeting the release page URL.
        /// </summary>
        private void OpenGitHubReleasePage()
        {
            string url = !string.IsNullOrWhiteSpace(_update.ReleasePageUrl)
                ? _update.ReleasePageUrl
                : $"https://github.com/GraemeG75/motion-highlighter/releases";

            try
            {
                Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    this,
                    LocalizationManager.T("about.msg_browser_error", ex.Message),
                    LocalizationManager.T("common.error"),
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            }
        }
    }
}
