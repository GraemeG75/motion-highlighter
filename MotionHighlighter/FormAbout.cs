using System.Diagnostics;
using System.Reflection;

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

        /// <summary>
        /// Initializes a new instance of the <see cref="FormAbout"/> dialog.
        /// </summary>
        public FormAbout()
        {
            InitializeComponent();
            LoadMascotImage();
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
                using SolidBrush brush = new SolidBrush(prideColors[i]);
                e.Graphics.FillRectangle(brush, x, 0, stripeWidth + 1, h);
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
