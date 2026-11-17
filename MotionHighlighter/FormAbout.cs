using System.Diagnostics;
using System.Reflection;
using MotionHighlighter.Localization;
using MotionHighlighter.Updating;

namespace MotionHighlighter
{
    /// <summary>
    /// Displays application metadata, branding, LGBTQ+ Pride theming, the kitten mascot,
    /// and website links to generatedpixel.dev.
    /// </summary>
    public partial class FormAbout : Form
    {
        public const string WebsiteUrl = "https://generatedpixel.dev/";
        public const string MascotAltText = "Generated Pixel kitten mascot";

        private readonly IUpdateService _updateService;

        /// <summary>
        /// Initializes a new instance of the <see cref="FormAbout"/> dialog.
        /// </summary>
        public FormAbout() : this(new GitHubUpdateService())
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FormAbout"/> dialog with a specified update service.
        /// </summary>
        /// <param name="updateService">Update service implementation.</param>
        public FormAbout(IUpdateService updateService)
        {
            _updateService = updateService ?? throw new ArgumentNullException(nameof(updateService));

            InitializeComponent();
            ApplyLocalization();
            LoadMascotImage();

            LocalizationManager.Instance.LanguageChanged += OnLanguageChanged;
        }

        /// <inheritdoc />
        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            LocalizationManager.Instance.LanguageChanged -= OnLanguageChanged;
            base.OnFormClosed(e);
        }

        private void OnLanguageChanged(object? sender, EventArgs e)
        {
            ApplyLocalization();
        }

        /// <summary>
        /// Translates all UI controls using the active localization provider.
        /// </summary>
        public void ApplyLocalization()
        {
            Text = LocalizationManager.T("about.title");
            lblTitle.Text = LocalizationManager.T("about.heading");
            lblPrideBadge.Text = LocalizationManager.T("about.pride_badge");
            lblVersion.Text = LocalizationManager.T("about.version", AppVersion.VersionString);
            lblDescription.Text = LocalizationManager.T("about.description");
            lblPrideStatement.Text = LocalizationManager.T("about.pride_statement");
            btnClose.Text = LocalizationManager.T("about.btn_close");
            btnCheckUpdates.Text = LocalizationManager.T("about.btn_check_updates");
            toolTip.SetToolTip(picMascot, LocalizationManager.T("about.mascot_alt"));
        }

        /// <summary>
        /// Loads the kitten mascot image from embedded resources or the local branding folder.
        /// </summary>
        private void LoadMascotImage()
        {
            try
            {
                // First try loading from local branding path
                string localPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "branding", "kitten.png");
                if (File.Exists(localPath))
                {
                    picMascot.Image = Image.FromFile(localPath);
                }
                else
                {
                    // Fall back to embedded resource
                    Assembly assembly = Assembly.GetExecutingAssembly();
                    string? resourceName = assembly.GetManifestResourceNames().FirstOrDefault(n => n.EndsWith("kitten.png", StringComparison.OrdinalIgnoreCase));
                    if (resourceName != null)
                    {
                        using Stream? stream = assembly.GetManifestResourceStream(resourceName);
                        if (stream != null)
                        {
                            picMascot.Image = Image.FromStream(stream);
                        }
                    }
                }

                toolTip.SetToolTip(picMascot, MascotAltText);
            }
            catch
            {
                // Non-fatal if image loading fails in headless test environments
            }
        }

        /// <summary>
        /// Renders a vivid Progress Pride Flag gradient/stripe ribbon across the top of the dialog.
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
        /// Checks GitHub Releases for a newer application version.
        /// </summary>
        /// <param name="sender">Event sender.</param>
        /// <param name="e">Event arguments.</param>
        private async void OnCheckUpdatesClicked(object? sender, EventArgs e)
        {
            btnCheckUpdates.Enabled = false;
            string originalText = btnCheckUpdates.Text;
            btnCheckUpdates.Text = LocalizationManager.T("update.status_checking");

            try
            {
                UpdateInfo? update = await _updateService.CheckForUpdatesAsync();
                if (update != null && update.IsNewer)
                {
                    using FormUpdate updateDialog = new(update, _updateService);
                    updateDialog.ShowDialog(this);
                }
                else if (update != null)
                {
                    MessageBox.Show(
                        this,
                        LocalizationManager.T("update.status_up_to_date", AppVersion.VersionString),
                        LocalizationManager.T("update.title"),
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                }
                else
                {
                    MessageBox.Show(
                        this,
                        LocalizationManager.T("update.status_failed", "GitHub Releases API was unreachable."),
                        LocalizationManager.T("common.warning"),
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    this,
                    LocalizationManager.T("update.status_failed", ex.Message),
                    LocalizationManager.T("common.error"),
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
            finally
            {
                btnCheckUpdates.Text = originalText;
                btnCheckUpdates.Enabled = true;
            }
        }

        /// <summary>
        /// Launches the official Generated Pixel website in the default system web browser.
        /// </summary>
        /// <param name="sender">Event sender.</param>
        /// <param name="e">Link label event arguments.</param>
        private void OnWebsiteLinkClicked(object? sender, LinkLabelLinkClickedEventArgs e)
        {
            try
            {
                Process.Start(new ProcessStartInfo(WebsiteUrl)
                {
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, $"Unable to open web browser:\n{ex.Message}", "Open Link", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }
    }
}
