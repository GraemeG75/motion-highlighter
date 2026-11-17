#nullable enable

namespace MotionHighlighter
{
    partial class FormAbout
    {
        private System.ComponentModel.IContainer? components = null;

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
            components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormAbout));
            panelPrideBanner = new Panel();
            picMascot = new PictureBox();
            lblTitle = new Label();
            lblVersion = new Label();
            lblDescription = new Label();
            linkWebsite = new LinkLabel();
            lblPrideStatement = new Label();
            lblPrideBadge = new Label();
            btnCheckUpdates = new Button();
            btnClose = new Button();
            panelContent = new Panel();
            toolTip = new ToolTip(components);
            ((System.ComponentModel.ISupportInitialize)picMascot).BeginInit();
            panelContent.SuspendLayout();
            SuspendLayout();
            // 
            // panelPrideBanner
            // 
            panelPrideBanner.Dock = DockStyle.Top;
            panelPrideBanner.Location = new Point(0, 0);
            panelPrideBanner.Name = "panelPrideBanner";
            panelPrideBanner.Size = new Size(520, 10);
            panelPrideBanner.TabIndex = 0;
            panelPrideBanner.Paint += OnPrideBannerPaint;
            // 
            // picMascot
            // 
            picMascot.AccessibleDescription = "Generated Pixel kitten mascot";
            picMascot.AccessibleName = "Generated Pixel kitten mascot";
            picMascot.BackColor = Color.Transparent;
            picMascot.Location = new Point(24, 45);
            picMascot.Name = "picMascot";
            picMascot.Size = new Size(100, 100);
            picMascot.SizeMode = PictureBoxSizeMode.Zoom;
            picMascot.TabIndex = 1;
            picMascot.TabStop = false;
            // 
            // lblTitle
            // 
            lblTitle.AutoSize = true;
            lblTitle.Font = new Font("Segoe UI", 16F, FontStyle.Bold);
            lblTitle.ForeColor = Color.FromArgb(28, 30, 33);
            lblTitle.Location = new Point(145, 45);
            lblTitle.Name = "lblTitle";
            lblTitle.Size = new Size(213, 30);
            lblTitle.TabIndex = 2;
            lblTitle.Text = "Motion Highlighter";
            // 
            // lblVersion
            // 
            lblVersion.AutoSize = true;
            lblVersion.Font = new Font("Segoe UI", 9F);
            lblVersion.ForeColor = Color.FromArgb(100, 105, 115);
            lblVersion.Location = new Point(148, 78);
            lblVersion.Name = "lblVersion";
            lblVersion.Size = new Size(72, 15);
            lblVersion.TabIndex = 3;
            lblVersion.Text = "Version 1.0.0";
            // 
            // lblDescription
            // 
            lblDescription.Font = new Font("Segoe UI", 9F);
            lblDescription.ForeColor = Color.FromArgb(50, 55, 65);
            lblDescription.Location = new Point(24, 158);
            lblDescription.Name = "lblDescription";
            lblDescription.Size = new Size(472, 70);
            lblDescription.TabIndex = 5;
            lblDescription.Text = resources.GetString("lblDescription.Text");
            // 
            // linkWebsite
            // 
            linkWebsite.ActiveLinkColor = Color.FromArgb(102, 36, 131);
            linkWebsite.AutoSize = true;
            linkWebsite.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            linkWebsite.LinkBehavior = LinkBehavior.HoverUnderline;
            linkWebsite.LinkColor = Color.FromArgb(0, 102, 204);
            linkWebsite.Location = new Point(148, 102);
            linkWebsite.Name = "linkWebsite";
            linkWebsite.Size = new Size(195, 19);
            linkWebsite.TabIndex = 4;
            linkWebsite.TabStop = true;
            linkWebsite.Text = "https://generatedpixel.dev/";
            linkWebsite.LinkClicked += OnWebsiteLinkClicked;
            // 
            // lblPrideStatement
            // 
            lblPrideStatement.BackColor = Color.FromArgb(248, 249, 250);
            lblPrideStatement.Font = new Font("Segoe UI", 8.5F, FontStyle.Italic);
            lblPrideStatement.ForeColor = Color.FromArgb(70, 75, 85);
            lblPrideStatement.Location = new Point(24, 235);
            lblPrideStatement.Name = "lblPrideStatement";
            lblPrideStatement.Padding = new Padding(12, 10, 12, 10);
            lblPrideStatement.Size = new Size(472, 55);
            lblPrideStatement.TabIndex = 6;
            lblPrideStatement.Text = "💖 Celebrating diversity, equality, and LGBTQ+ pride in open source technology. All creators, developers, and users are welcome here.";
            // 
            // lblPrideBadge
            // 
            lblPrideBadge.AutoSize = true;
            lblPrideBadge.BackColor = Color.FromArgb(245, 238, 255);
            lblPrideBadge.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            lblPrideBadge.ForeColor = Color.FromArgb(102, 36, 131);
            lblPrideBadge.Location = new Point(148, 16);
            lblPrideBadge.Name = "lblPrideBadge";
            lblPrideBadge.Padding = new Padding(8, 4, 8, 4);
            lblPrideBadge.Size = new Size(223, 23);
            lblPrideBadge.TabIndex = 0;
            lblPrideBadge.Text = "🏳️‍🌈 Proudly Inclusive • LGBTQ+ Pride";
            lblPrideBadge.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // btnCheckUpdates
            // 
            btnCheckUpdates.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            btnCheckUpdates.Font = new Font("Segoe UI", 9F);
            btnCheckUpdates.Location = new Point(24, 318);
            btnCheckUpdates.Name = "btnCheckUpdates";
            btnCheckUpdates.Size = new Size(160, 30);
            btnCheckUpdates.TabIndex = 8;
            btnCheckUpdates.Text = "Check for Updates";
            btnCheckUpdates.UseVisualStyleBackColor = true;
            btnCheckUpdates.Click += OnCheckUpdatesClicked;
            // 
            // btnClose
            // 
            btnClose.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            btnClose.DialogResult = DialogResult.OK;
            btnClose.Font = new Font("Segoe UI", 9F);
            btnClose.Location = new Point(406, 318);
            btnClose.Name = "btnClose";
            btnClose.Size = new Size(90, 30);
            btnClose.TabIndex = 7;
            btnClose.Text = "Close";
            btnClose.UseVisualStyleBackColor = true;
            // 
            // panelContent
            // 
            panelContent.BackColor = Color.White;
            panelContent.Controls.Add(lblPrideBadge);
            panelContent.Controls.Add(picMascot);
            panelContent.Controls.Add(lblTitle);
            panelContent.Controls.Add(lblVersion);
            panelContent.Controls.Add(lblDescription);
            panelContent.Controls.Add(linkWebsite);
            panelContent.Controls.Add(lblPrideStatement);
            panelContent.Controls.Add(btnCheckUpdates);
            panelContent.Controls.Add(btnClose);
            panelContent.Dock = DockStyle.Fill;
            panelContent.Location = new Point(0, 10);
            panelContent.Name = "panelContent";
            panelContent.Padding = new Padding(24, 20, 24, 20);
            panelContent.Size = new Size(520, 360);
            panelContent.TabIndex = 1;
            // 
            // FormAbout
            // 
            AcceptButton = btnClose;
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            CancelButton = btnClose;
            ClientSize = new Size(520, 370);
            Controls.Add(panelContent);
            Controls.Add(panelPrideBanner);
            Font = new Font("Segoe UI", 9F);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "FormAbout";
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.CenterParent;
            Text = "About Motion Highlighter";
            ((System.ComponentModel.ISupportInitialize)picMascot).EndInit();
            panelContent.ResumeLayout(false);
            panelContent.PerformLayout();
            ResumeLayout(false);
        }

        private Panel panelPrideBanner = null!;
        private Panel panelContent = null!;
        private PictureBox picMascot = null!;
        private Label lblTitle = null!;
        private Label lblVersion = null!;
        private Label lblDescription = null!;
        private LinkLabel linkWebsite = null!;
        private Label lblPrideStatement = null!;
        private Label lblPrideBadge = null!;
        private Button btnCheckUpdates = null!;
        private Button btnClose = null!;
        private ToolTip toolTip = null!;
    }
}
