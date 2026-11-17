namespace MotionHighlighter.Updating
{
    partial class FormUpdate
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
            components = new System.ComponentModel.Container();
            panelPrideBanner = new Panel();
            panelContent = new Panel();
            picMascot = new PictureBox();
            lblTitle = new Label();
            lblCurrentVersion = new Label();
            lblLatestVersion = new Label();
            lblReleaseNotesHeader = new Label();
            txtReleaseNotes = new TextBox();
            progressBar = new ProgressBar();
            lblStatus = new Label();
            btnDownload = new Button();
            btnViewGitHub = new Button();
            btnLater = new Button();
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
            panelPrideBanner.Size = new Size(540, 10);
            panelPrideBanner.TabIndex = 0;
            panelPrideBanner.Paint += OnPrideBannerPaint;

            // 
            // panelContent
            // 
            panelContent.BackColor = Color.White;
            panelContent.Controls.Add(picMascot);
            panelContent.Controls.Add(lblTitle);
            panelContent.Controls.Add(lblCurrentVersion);
            panelContent.Controls.Add(lblLatestVersion);
            panelContent.Controls.Add(lblReleaseNotesHeader);
            panelContent.Controls.Add(txtReleaseNotes);
            panelContent.Controls.Add(progressBar);
            panelContent.Controls.Add(lblStatus);
            panelContent.Controls.Add(btnDownload);
            panelContent.Controls.Add(btnViewGitHub);
            panelContent.Controls.Add(btnLater);
            panelContent.Dock = DockStyle.Fill;
            panelContent.Location = new Point(0, 10);
            panelContent.Name = "panelContent";
            panelContent.Padding = new Padding(20);
            panelContent.Size = new Size(540, 430);
            panelContent.TabIndex = 1;

            // 
            // picMascot
            // 
            picMascot.AccessibleDescription = "Generated Pixel kitten mascot";
            picMascot.AccessibleName = "Generated Pixel kitten mascot";
            picMascot.Location = new Point(20, 20);
            picMascot.Name = "picMascot";
            picMascot.Size = new Size(72, 72);
            picMascot.SizeMode = PictureBoxSizeMode.Zoom;
            picMascot.TabIndex = 0;
            picMascot.TabStop = false;

            // 
            // lblTitle
            // 
            lblTitle.AutoSize = true;
            lblTitle.Font = new Font("Segoe UI", 13F, FontStyle.Bold);
            lblTitle.ForeColor = Color.FromArgb(28, 30, 33);
            lblTitle.Location = new Point(104, 20);
            lblTitle.Name = "lblTitle";
            lblTitle.Size = new Size(410, 25);
            lblTitle.TabIndex = 1;
            lblTitle.Text = "A new version of Motion Highlighter is available!";

            // 
            // lblCurrentVersion
            // 
            lblCurrentVersion.AutoSize = true;
            lblCurrentVersion.Font = new Font("Segoe UI", 9F);
            lblCurrentVersion.ForeColor = Color.FromArgb(100, 105, 115);
            lblCurrentVersion.Location = new Point(106, 50);
            lblCurrentVersion.Name = "lblCurrentVersion";
            lblCurrentVersion.Size = new Size(130, 15);
            lblCurrentVersion.TabIndex = 2;
            lblCurrentVersion.Text = "Current Version: 1.0.0";

            // 
            // lblLatestVersion
            // 
            lblLatestVersion.AutoSize = true;
            lblLatestVersion.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            lblLatestVersion.ForeColor = Color.FromArgb(0, 128, 38);
            lblLatestVersion.Location = new Point(106, 70);
            lblLatestVersion.Name = "lblLatestVersion";
            lblLatestVersion.Size = new Size(125, 15);
            lblLatestVersion.TabIndex = 3;
            lblLatestVersion.Text = "Latest Version: 1.1.0";

            // 
            // lblReleaseNotesHeader
            // 
            lblReleaseNotesHeader.AutoSize = true;
            lblReleaseNotesHeader.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            lblReleaseNotesHeader.Location = new Point(20, 105);
            lblReleaseNotesHeader.Name = "lblReleaseNotesHeader";
            lblReleaseNotesHeader.Size = new Size(88, 15);
            lblReleaseNotesHeader.TabIndex = 4;
            lblReleaseNotesHeader.Text = "Release Notes:";

            // 
            // txtReleaseNotes
            // 
            txtReleaseNotes.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            txtReleaseNotes.BackColor = Color.FromArgb(248, 249, 250);
            txtReleaseNotes.Font = new Font("Segoe UI", 9F);
            txtReleaseNotes.Location = new Point(20, 125);
            txtReleaseNotes.Multiline = true;
            txtReleaseNotes.Name = "txtReleaseNotes";
            txtReleaseNotes.ReadOnly = true;
            txtReleaseNotes.ScrollBars = ScrollBars.Vertical;
            txtReleaseNotes.Size = new Size(500, 200);
            txtReleaseNotes.TabIndex = 5;

            // 
            // progressBar
            // 
            progressBar.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            progressBar.Location = new Point(20, 335);
            progressBar.Name = "progressBar";
            progressBar.Size = new Size(500, 16);
            progressBar.TabIndex = 6;
            progressBar.Visible = false;

            // 
            // lblStatus
            // 
            lblStatus.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            lblStatus.Font = new Font("Segoe UI", 8.5F);
            lblStatus.ForeColor = Color.FromArgb(80, 85, 95);
            lblStatus.Location = new Point(20, 355);
            lblStatus.Name = "lblStatus";
            lblStatus.Size = new Size(500, 18);
            lblStatus.TabIndex = 7;
            lblStatus.Text = "Ready";

            // 
            // btnDownload
            // 
            btnDownload.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            btnDownload.BackColor = Color.FromArgb(0, 120, 215);
            btnDownload.FlatStyle = FlatStyle.System;
            btnDownload.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            btnDownload.Location = new Point(360, 385);
            btnDownload.Name = "btnDownload";
            btnDownload.Size = new Size(160, 32);
            btnDownload.TabIndex = 8;
            btnDownload.Text = "Download && Install";
            btnDownload.UseVisualStyleBackColor = true;
            btnDownload.Click += OnDownloadClicked;

            // 
            // btnViewGitHub
            // 
            btnViewGitHub.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            btnViewGitHub.Font = new Font("Segoe UI", 9F);
            btnViewGitHub.Location = new Point(20, 385);
            btnViewGitHub.Name = "btnViewGitHub";
            btnViewGitHub.Size = new Size(130, 32);
            btnViewGitHub.TabIndex = 9;
            btnViewGitHub.Text = "View on GitHub";
            btnViewGitHub.UseVisualStyleBackColor = true;
            btnViewGitHub.Click += OnViewGitHubClicked;

            // 
            // btnLater
            // 
            btnLater.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            btnLater.DialogResult = DialogResult.Cancel;
            btnLater.Font = new Font("Segoe UI", 9F);
            btnLater.Location = new Point(265, 385);
            btnLater.Name = "btnLater";
            btnLater.Size = new Size(85, 32);
            btnLater.TabIndex = 10;
            btnLater.Text = "Later";
            btnLater.UseVisualStyleBackColor = true;

            // 
            // FormUpdate
            // 
            AcceptButton = btnDownload;
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            CancelButton = btnLater;
            ClientSize = new Size(540, 440);
            Controls.Add(panelContent);
            Controls.Add(panelPrideBanner);
            Font = new Font("Segoe UI", 9F);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "FormUpdate";
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.CenterParent;
            Text = "Software Update";

            ((System.ComponentModel.ISupportInitialize)picMascot).EndInit();
            panelContent.ResumeLayout(false);
            panelContent.PerformLayout();
            ResumeLayout(false);
        }

        private Panel panelPrideBanner;
        private Panel panelContent;
        private PictureBox picMascot;
        private Label lblTitle;
        private Label lblCurrentVersion;
        private Label lblLatestVersion;
        private Label lblReleaseNotesHeader;
        private TextBox txtReleaseNotes;
        private ProgressBar progressBar;
        private Label lblStatus;
        private Button btnDownload;
        private Button btnViewGitHub;
        private Button btnLater;
        private ToolTip toolTip;
    }
}
