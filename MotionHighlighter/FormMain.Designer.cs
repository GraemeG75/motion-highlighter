using System.ComponentModel;

namespace MotionHighlighter
{
    partial class FormMain
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            _picture = new PictureBox();
            _btnLoad = new Button();
            _btnPlay = new Button();
            _btnResetBg = new Button();
            _btnExport = new Button();
            _seek = new TrackBar();
            _lblInfo = new Label();
            _nudThreshold = new NumericUpDown();
            _nudMinArea = new NumericUpDown();
            _nudBlur = new NumericUpDown();
            _nudAlpha = new NumericUpDown();
            _nudLearnRate = new NumericUpDown();
            _chkBoxes = new CheckBox();
            _chkOverlay = new CheckBox();
            var topPanel = new FlowLayoutPanel();
            var lblThreshold = new Label();
            var lblMinArea = new Label();
            var lblBlur = new Label();
            var lblOverlay = new Label();
            var lblLearn = new Label();

            ((ISupportInitialize)_picture).BeginInit();
            ((ISupportInitialize)_seek).BeginInit();
            ((ISupportInitialize)_nudThreshold).BeginInit();
            ((ISupportInitialize)_nudMinArea).BeginInit();
            ((ISupportInitialize)_nudBlur).BeginInit();
            ((ISupportInitialize)_nudAlpha).BeginInit();
            ((ISupportInitialize)_nudLearnRate).BeginInit();
            topPanel.SuspendLayout();
            SuspendLayout();

            // 
            // _picture
            // 
            _picture.BackColor = Color.Black;
            _picture.Dock = DockStyle.Fill;
            _picture.Location = new Point(0, 88);
            _picture.Name = "_picture";
            _picture.Size = new Size(1200, 712);
            _picture.SizeMode = PictureBoxSizeMode.Zoom;
            _picture.TabIndex = 0;
            _picture.TabStop = false;
            // 
            // _btnLoad
            // 
            _btnLoad.Location = new Point(8, 8);
            _btnLoad.Name = "_btnLoad";
            _btnLoad.Size = new Size(100, 27);
            _btnLoad.TabIndex = 1;
            _btnLoad.Text = "Load MP4...";
            _btnLoad.UseVisualStyleBackColor = true;
            // 
            // _btnPlay
            // 
            _btnPlay.Location = new Point(114, 8);
            _btnPlay.Name = "_btnPlay";
            _btnPlay.Size = new Size(75, 27);
            _btnPlay.TabIndex = 2;
            _btnPlay.Text = "Play";
            _btnPlay.UseVisualStyleBackColor = true;
            // 
            // _btnResetBg
            // 
            _btnResetBg.Location = new Point(195, 8);
            _btnResetBg.Name = "_btnResetBg";
            _btnResetBg.Size = new Size(85, 27);
            _btnResetBg.TabIndex = 3;
            _btnResetBg.Text = "Reset BG";
            _btnResetBg.UseVisualStyleBackColor = true;
            // 
            // _btnExport
            // 
            _btnExport.Location = new Point(286, 8);
            _btnExport.Name = "_btnExport";
            _btnExport.Size = new Size(160, 27);
            _btnExport.TabIndex = 4;
            _btnExport.Text = "Export Annotated MP4...";
            _btnExport.UseVisualStyleBackColor = true;
            // 
            // _seek
            // 
            _seek.Dock = DockStyle.Top;
            _seek.Location = new Point(0, 44);
            _seek.Maximum = 0;
            _seek.Name = "_seek";
            _seek.Size = new Size(1200, 44);
            _seek.TabIndex = 5;
            _seek.TickStyle = TickStyle.None;
            // 
            // _lblInfo
            // 
            _lblInfo.AutoSize = true;
            _lblInfo.Location = new Point(832, 8);
            _lblInfo.Margin = new Padding(3, 8, 3, 0);
            _lblInfo.Name = "_lblInfo";
            _lblInfo.Size = new Size(0, 15);
            _lblInfo.TabIndex = 15;
            // 
            // _nudThreshold
            // 
            _nudThreshold.Location = new Point(530, 11);
            _nudThreshold.Maximum = new decimal(new int[] { 255, 0, 0, 0 });
            _nudThreshold.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            _nudThreshold.Name = "_nudThreshold";
            _nudThreshold.Size = new Size(70, 23);
            _nudThreshold.TabIndex = 7;
            _nudThreshold.Value = new decimal(new int[] { 25, 0, 0, 0 });
            // 
            // _nudMinArea
            // 
            _nudMinArea.Increment = new decimal(new int[] { 50, 0, 0, 0 });
            _nudMinArea.Location = new Point(625, 11);
            _nudMinArea.Maximum = new decimal(new int[] { 50000, 0, 0, 0 });
            _nudMinArea.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            _nudMinArea.Name = "_nudMinArea";
            _nudMinArea.Size = new Size(80, 23);
            _nudMinArea.TabIndex = 9;
            _nudMinArea.Value = new decimal(new int[] { 500, 0, 0, 0 });
            // 
            // _nudBlur
            // 
            _nudBlur.Increment = new decimal(new int[] { 2, 0, 0, 0 });
            _nudBlur.Location = new Point(711, 11);
            _nudBlur.Maximum = new decimal(new int[] { 41, 0, 0, 0 });
            _nudBlur.Name = "_nudBlur";
            _nudBlur.Size = new Size(70, 23);
            _nudBlur.TabIndex = 11;
            _nudBlur.Value = new decimal(new int[] { 7, 0, 0, 0 });
            // 
            // _nudAlpha
            // 
            _nudAlpha.Increment = new decimal(new int[] { 5, 0, 0, 0 });
            _nudAlpha.Location = new Point(787, 11);
            _nudAlpha.Name = "_nudAlpha";
            _nudAlpha.Size = new Size(70, 23);
            _nudAlpha.TabIndex = 13;
            _nudAlpha.Value = new decimal(new int[] { 55, 0, 0, 0 });
            // 
            // _nudLearnRate
            // 
            _nudLearnRate.Location = new Point(863, 11);
            _nudLearnRate.Name = "_nudLearnRate";
            _nudLearnRate.Size = new Size(70, 23);
            _nudLearnRate.TabIndex = 14;
            _nudLearnRate.Value = new decimal(new int[] { 2, 0, 0, 0 });
            // 
            // _chkBoxes
            // 
            _chkBoxes.AutoSize = true;
            _chkBoxes.Checked = true;
            _chkBoxes.CheckState = CheckState.Checked;
            _chkBoxes.Location = new Point(939, 11);
            _chkBoxes.Name = "_chkBoxes";
            _chkBoxes.Size = new Size(88, 19);
            _chkBoxes.TabIndex = 17;
            _chkBoxes.Text = "Draw boxes";
            _chkBoxes.UseVisualStyleBackColor = true;
            // 
            // _chkOverlay
            // 
            _chkOverlay.AutoSize = true;
            _chkOverlay.Checked = true;
            _chkOverlay.CheckState = CheckState.Checked;
            _chkOverlay.Location = new Point(1033, 11);
            _chkOverlay.Name = "_chkOverlay";
            _chkOverlay.Size = new Size(85, 19);
            _chkOverlay.TabIndex = 16;
            _chkOverlay.Text = "Red overlay";
            _chkOverlay.UseVisualStyleBackColor = true;
            // 
            // topPanel
            // 
            topPanel.AutoScroll = true;
            topPanel.Controls.Add(_btnLoad);
            topPanel.Controls.Add(_btnPlay);
            topPanel.Controls.Add(_btnResetBg);
            topPanel.Controls.Add(_btnExport);
            topPanel.Controls.Add(lblThreshold);
            topPanel.Controls.Add(_nudThreshold);
            topPanel.Controls.Add(lblMinArea);
            topPanel.Controls.Add(_nudMinArea);
            topPanel.Controls.Add(lblBlur);
            topPanel.Controls.Add(_nudBlur);
            topPanel.Controls.Add(lblOverlay);
            topPanel.Controls.Add(_nudAlpha);
            topPanel.Controls.Add(lblLearn);
            topPanel.Controls.Add(_nudLearnRate);
            topPanel.Controls.Add(_chkOverlay);
            topPanel.Controls.Add(_chkBoxes);
            topPanel.Controls.Add(_lblInfo);
            topPanel.Dock = DockStyle.Top;
            topPanel.Location = new Point(0, 0);
            topPanel.Name = "topPanel";
            topPanel.Padding = new Padding(8, 8, 8, 0);
            topPanel.Size = new Size(1200, 44);
            topPanel.TabIndex = 6;
            topPanel.WrapContents = false;
            // 
            // lblThreshold
            // 
            lblThreshold.AutoSize = true;
            lblThreshold.Location = new Point(452, 8);
            lblThreshold.Margin = new Padding(3, 8, 0, 0);
            lblThreshold.Name = "lblThreshold";
            lblThreshold.Padding = new Padding(12, 8, 0, 0);
            lblThreshold.Size = new Size(72, 23);
            lblThreshold.TabIndex = 6;
            lblThreshold.Text = "Threshold";
            // 
            // lblMinArea
            // 
            lblMinArea.AutoSize = true;
            lblMinArea.Location = new Point(606, 8);
            lblMinArea.Margin = new Padding(3, 8, 0, 0);
            lblMinArea.Name = "lblMinArea";
            lblMinArea.Padding = new Padding(12, 8, 0, 0);
            lblMinArea.Size = new Size(13, 23);
            lblMinArea.TabIndex = 8;
            lblMinArea.Text = "Min Area";
            // 
            // lblBlur
            // 
            lblBlur.AutoSize = true;
            lblBlur.Location = new Point(711, 8);
            lblBlur.Margin = new Padding(3, 8, 0, 0);
            lblBlur.Name = "lblBlur";
            lblBlur.Padding = new Padding(12, 8, 0, 0);
            lblBlur.Size = new Size(70, 23);
            lblBlur.TabIndex = 10;
            lblBlur.Text = "Blur (odd)";
            // 
            // lblOverlay
            // 
            lblOverlay.AutoSize = true;
            lblOverlay.Location = new Point(787, 8);
            lblOverlay.Margin = new Padding(3, 8, 0, 0);
            lblOverlay.Name = "lblOverlay";
            lblOverlay.Padding = new Padding(12, 8, 0, 0);
            lblOverlay.Size = new Size(70, 23);
            lblOverlay.TabIndex = 12;
            lblOverlay.Text = "Overlay %";
            // 
            // lblLearn
            // 
            lblLearn.AutoSize = true;
            lblLearn.Location = new Point(863, 8);
            lblLearn.Margin = new Padding(3, 8, 0, 0);
            lblLearn.Name = "lblLearn";
            lblLearn.Padding = new Padding(12, 8, 0, 0);
            lblLearn.Size = new Size(58, 23);
            lblLearn.TabIndex = 14;
            lblLearn.Text = "Learn %";
            // 
            // FormMain
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1200, 800);
            Controls.Add(_picture);
            Controls.Add(_seek);
            Controls.Add(topPanel);
            Name = "FormMain";
            Text = "Motion Highlighter (OpenCvSharp) – MP4 motion detection";
            ((ISupportInitialize)_picture).EndInit();
            ((ISupportInitialize)_seek).EndInit();
            ((ISupportInitialize)_nudThreshold).EndInit();
            ((ISupportInitialize)_nudMinArea).EndInit();
            ((ISupportInitialize)_nudBlur).EndInit();
            ((ISupportInitialize)_nudAlpha).EndInit();
            ((ISupportInitialize)_nudLearnRate).EndInit();
            topPanel.ResumeLayout(false);
            topPanel.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private PictureBox _picture;
        private Button _btnLoad;
        private Button _btnPlay;
        private Button _btnResetBg;
        private Button _btnExport;
        private TrackBar _seek;
        private Label _lblInfo;
        private NumericUpDown _nudThreshold;
        private NumericUpDown _nudMinArea;
        private NumericUpDown _nudBlur;
        private NumericUpDown _nudAlpha;
        private NumericUpDown _nudLearnRate;
        private CheckBox _chkBoxes;
        private CheckBox _chkOverlay;
    }
}
