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
            this.components = new System.ComponentModel.Container();
            _picture = new ZoomablePictureBox();
            _btnLoad = new Button();
            _btnOpenStream = new Button();
            _btnAbout = new Button();
            _cmbLanguage = new ComboBox();
            _btnGrabScreenshot = new Button();
            _btnZoomOut = new Button();
            _lblZoomLevel = new Label();
            _btnZoomIn = new Button();
            _btnZoomReset = new Button();
            _btnPlay = new Button();
            _btnResetBg = new Button();
            _btnExport = new Button();
            _btnCancelExport = new Button();
            _progressBar = new ProgressBar();
            _seek = new TrackBar();
            _lblInfo = new Label();
            _nudThreshold = new NumericUpDown();
            _nudMinArea = new NumericUpDown();
            _nudBlur = new NumericUpDown();
            _nudAlpha = new NumericUpDown();
            _nudLearnRate = new NumericUpDown();
            _chkBoxes = new CheckBox();
            _chkOverlay = new CheckBox();
            _chkResetBgOnSeek = new CheckBox();
            _chkIgnoreTrails = new CheckBox();
            _chkTrailsInOverlay = new CheckBox();
            _nudBoxHold = new NumericUpDown();

            _btnStepBackSec = new Button();
            _btnStepBackFrame = new Button();
            _btnStepForwardFrame = new Button();
            _btnStepForwardSec = new Button();
            _btnStop = new Button();
            _lblCurrentTime = new Label();
            _lblTotalTime = new Label();
            _lblVideoTitle = new Label();

            _lblInfoResolution = new Label();
            _lblInfoFps = new Label();
            _lblInfoFrames = new Label();
            _lblInfoTime = new Label();
            _lblInfoRegions = new Label();

            Panel panelTop = new Panel();
            Panel panelBottom = new Panel();
            Panel panelSidebar = new Panel();
            TableLayoutPanel tableSeek = new TableLayoutPanel();
            FlowLayoutPanel panelControls = new FlowLayoutPanel();
            GroupBox grpDetection = new GroupBox();
            TableLayoutPanel tableDetection = new TableLayoutPanel();
            GroupBox grpDisplay = new GroupBox();
            TableLayoutPanel tableDisplay = new TableLayoutPanel();
            GroupBox grpInfo = new GroupBox();
            TableLayoutPanel tableInfo = new TableLayoutPanel();

            Label lblThreshold = new Label();
            Label lblMinArea = new Label();
            Label lblBlur = new Label();
            Label lblOverlay = new Label();
            Label lblLearn = new Label();
            Label lblBoxHold = new Label();
            Label lblHint = new Label();
            Label lblResHeader = new Label();
            Label lblFpsHeader = new Label();
            Label lblFramesHeader = new Label();
            Label lblTimeHeader = new Label();
            Label lblRegionsHeader = new Label();

            ((ISupportInitialize)_picture).BeginInit();
            ((ISupportInitialize)_seek).BeginInit();
            ((ISupportInitialize)_nudThreshold).BeginInit();
            ((ISupportInitialize)_nudMinArea).BeginInit();
            ((ISupportInitialize)_nudBlur).BeginInit();
            ((ISupportInitialize)_nudAlpha).BeginInit();
            ((ISupportInitialize)_nudLearnRate).BeginInit();
            ((ISupportInitialize)_nudBoxHold).BeginInit();
            panelTop.SuspendLayout();
            panelBottom.SuspendLayout();
            panelSidebar.SuspendLayout();
            tableSeek.SuspendLayout();
            panelControls.SuspendLayout();
            grpDetection.SuspendLayout();
            tableDetection.SuspendLayout();
            grpDisplay.SuspendLayout();
            tableDisplay.SuspendLayout();
            grpInfo.SuspendLayout();
            tableInfo.SuspendLayout();
            SuspendLayout();

            // 
            // panelTop
            // 
            panelTop.BackColor = SystemColors.Control;
            panelTop.Controls.Add(_btnLoad);
            panelTop.Controls.Add(_btnOpenStream);
            panelTop.Controls.Add(_btnResetBg);
            panelTop.Controls.Add(_btnExport);
            panelTop.Controls.Add(_btnCancelExport);
            panelTop.Controls.Add(_progressBar);
            panelTop.Controls.Add(_lblVideoTitle);
            panelTop.Controls.Add(_cmbLanguage);
            panelTop.Controls.Add(_btnAbout);
            panelTop.Dock = DockStyle.Top;
            panelTop.Location = new Point(0, 0);
            panelTop.Name = "panelTop";
            panelTop.Padding = new Padding(10, 8, 10, 8);
            panelTop.Size = new Size(1264, 48);
            panelTop.TabIndex = 0;

            // 
            // _btnLoad
            // 
            _btnLoad.Location = new Point(10, 9);
            _btnLoad.Name = "_btnLoad";
            _btnLoad.Size = new Size(105, 30);
            _btnLoad.TabIndex = 0;
            _btnLoad.Text = "Open Video...";
            _btnLoad.UseVisualStyleBackColor = true;

            // 
            // _btnOpenStream
            // 
            _btnOpenStream.Location = new Point(121, 9);
            _btnOpenStream.Name = "_btnOpenStream";
            _btnOpenStream.Size = new Size(110, 30);
            _btnOpenStream.TabIndex = 1;
            _btnOpenStream.Text = "Open Stream...";
            _btnOpenStream.UseVisualStyleBackColor = true;

            // 
            // _btnResetBg
            // 
            _btnResetBg.Location = new Point(237, 9);
            _btnResetBg.Name = "_btnResetBg";
            _btnResetBg.Size = new Size(130, 30);
            _btnResetBg.TabIndex = 2;
            _btnResetBg.Text = "Reset Background";
            _btnResetBg.UseVisualStyleBackColor = true;

            // 
            // _btnExport
            // 
            _btnExport.Location = new Point(373, 9);
            _btnExport.Name = "_btnExport";
            _btnExport.Size = new Size(155, 30);
            _btnExport.TabIndex = 3;
            _btnExport.Text = "Export Annotated MP4...";
            _btnExport.UseVisualStyleBackColor = true;

            // 
            // _btnCancelExport
            // 
            _btnCancelExport.Location = new Point(534, 9);
            _btnCancelExport.Name = "_btnCancelExport";
            _btnCancelExport.Size = new Size(70, 30);
            _btnCancelExport.TabIndex = 4;
            _btnCancelExport.Text = "Cancel";
            _btnCancelExport.UseVisualStyleBackColor = true;
            _btnCancelExport.Visible = false;

            // 
            // _progressBar
            // 
            _progressBar.Location = new Point(610, 13);
            _progressBar.Name = "_progressBar";
            _progressBar.Size = new Size(140, 22);
            _progressBar.TabIndex = 5;
            _progressBar.Visible = false;

            // 
            // _lblVideoTitle
            // 
            _lblVideoTitle.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            _lblVideoTitle.AutoEllipsis = true;
            _lblVideoTitle.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
            _lblVideoTitle.ForeColor = SystemColors.ControlDarkDark;
            _lblVideoTitle.Location = new Point(760, 14);
            _lblVideoTitle.Name = "_lblVideoTitle";
            _lblVideoTitle.Size = new Size(278, 20);
            _lblVideoTitle.TabIndex = 6;
            _lblVideoTitle.Text = "No video loaded. Click 'Open Video...' or 'Open Stream...' to begin.";

            // 
            // _cmbLanguage
            // 
            _cmbLanguage.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            _cmbLanguage.DropDownStyle = ComboBoxStyle.DropDownList;
            _cmbLanguage.Font = new Font("Segoe UI", 9F);
            _cmbLanguage.FormattingEnabled = true;
            _cmbLanguage.Location = new Point(1048, 12);
            _cmbLanguage.Name = "_cmbLanguage";
            _cmbLanguage.Size = new Size(128, 23);
            _cmbLanguage.TabIndex = 7;

            // 
            // _btnAbout
            // 
            _btnAbout.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            _btnAbout.Location = new Point(1184, 9);
            _btnAbout.Name = "_btnAbout";
            _btnAbout.Size = new Size(70, 30);
            _btnAbout.TabIndex = 8;
            _btnAbout.Text = "ℹ About";
            _btnAbout.UseVisualStyleBackColor = true;

            // 
            // panelBottom
            // 
            panelBottom.BackColor = SystemColors.Control;
            panelBottom.Controls.Add(panelControls);
            panelBottom.Controls.Add(tableSeek);
            panelBottom.Dock = DockStyle.Bottom;
            panelBottom.Location = new Point(0, 725);
            panelBottom.Name = "panelBottom";
            panelBottom.Padding = new Padding(10, 4, 10, 6);
            panelBottom.Size = new Size(1264, 86);
            panelBottom.TabIndex = 2;

            // 
            // tableSeek
            // 
            tableSeek.ColumnCount = 3;
            tableSeek.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 70F));
            tableSeek.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tableSeek.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 70F));
            tableSeek.Controls.Add(_lblCurrentTime, 0, 0);
            tableSeek.Controls.Add(_seek, 1, 0);
            tableSeek.Controls.Add(_lblTotalTime, 2, 0);
            tableSeek.Dock = DockStyle.Top;
            tableSeek.Location = new Point(10, 4);
            tableSeek.Name = "tableSeek";
            tableSeek.RowCount = 1;
            tableSeek.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            tableSeek.Size = new Size(1244, 38);
            tableSeek.TabIndex = 0;

            // 
            // _lblCurrentTime
            // 
            _lblCurrentTime.Anchor = AnchorStyles.Left | AnchorStyles.Right;
            _lblCurrentTime.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            _lblCurrentTime.Location = new Point(3, 9);
            _lblCurrentTime.Name = "_lblCurrentTime";
            _lblCurrentTime.Size = new Size(64, 20);
            _lblCurrentTime.TabIndex = 0;
            _lblCurrentTime.Text = "00:00.00";
            _lblCurrentTime.TextAlign = ContentAlignment.MiddleRight;

            // 
            // _seek
            // 
            _seek.Dock = DockStyle.Fill;
            _seek.Location = new Point(73, 3);
            _seek.Maximum = 0;
            _seek.Name = "_seek";
            _seek.Size = new Size(1098, 32);
            _seek.TabIndex = 1;
            _seek.TickStyle = TickStyle.None;

            // 
            // _lblTotalTime
            // 
            _lblTotalTime.Anchor = AnchorStyles.Left | AnchorStyles.Right;
            _lblTotalTime.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            _lblTotalTime.Location = new Point(1177, 9);
            _lblTotalTime.Name = "_lblTotalTime";
            _lblTotalTime.Size = new Size(64, 20);
            _lblTotalTime.TabIndex = 2;
            _lblTotalTime.Text = "00:00.00";
            _lblTotalTime.TextAlign = ContentAlignment.MiddleLeft;

            // 
            // panelControls
            // 
            panelControls.Controls.Add(_btnStepBackSec);
            panelControls.Controls.Add(_btnStepBackFrame);
            panelControls.Controls.Add(_btnPlay);
            panelControls.Controls.Add(_btnStepForwardFrame);
            panelControls.Controls.Add(_btnStepForwardSec);
            panelControls.Controls.Add(_btnStop);
            panelControls.Controls.Add(_btnGrabScreenshot);
            panelControls.Controls.Add(_btnZoomOut);
            panelControls.Controls.Add(_lblZoomLevel);
            panelControls.Controls.Add(_btnZoomIn);
            panelControls.Controls.Add(_btnZoomReset);
            panelControls.Controls.Add(lblHint);
            panelControls.Controls.Add(_lblInfo);
            panelControls.Dock = DockStyle.Bottom;
            panelControls.Location = new Point(10, 44);
            panelControls.Name = "panelControls";
            panelControls.Size = new Size(1244, 36);
            panelControls.TabIndex = 1;

            // 
            // _btnStepBackSec
            // 
            _btnStepBackSec.Location = new Point(3, 3);
            _btnStepBackSec.Name = "_btnStepBackSec";
            _btnStepBackSec.Size = new Size(62, 28);
            _btnStepBackSec.TabIndex = 0;
            _btnStepBackSec.Text = "⏮ -1s";
            _btnStepBackSec.UseVisualStyleBackColor = true;

            // 
            // _btnStepBackFrame
            // 
            _btnStepBackFrame.Location = new Point(71, 3);
            _btnStepBackFrame.Name = "_btnStepBackFrame";
            _btnStepBackFrame.Size = new Size(62, 28);
            _btnStepBackFrame.TabIndex = 1;
            _btnStepBackFrame.Text = "◀ -1f";
            _btnStepBackFrame.UseVisualStyleBackColor = true;

            // 
            // _btnPlay
            // 
            _btnPlay.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            _btnPlay.Location = new Point(139, 3);
            _btnPlay.Name = "_btnPlay";
            _btnPlay.Size = new Size(95, 28);
            _btnPlay.TabIndex = 2;
            _btnPlay.Text = "▶ Play";
            _btnPlay.UseVisualStyleBackColor = true;

            // 
            // _btnStepForwardFrame
            // 
            _btnStepForwardFrame.Location = new Point(240, 3);
            _btnStepForwardFrame.Name = "_btnStepForwardFrame";
            _btnStepForwardFrame.Size = new Size(62, 28);
            _btnStepForwardFrame.TabIndex = 3;
            _btnStepForwardFrame.Text = "+1f ▶";
            _btnStepForwardFrame.UseVisualStyleBackColor = true;

            // 
            // _btnStepForwardSec
            // 
            _btnStepForwardSec.Location = new Point(308, 3);
            _btnStepForwardSec.Name = "_btnStepForwardSec";
            _btnStepForwardSec.Size = new Size(62, 28);
            _btnStepForwardSec.TabIndex = 4;
            _btnStepForwardSec.Text = "+1s ⏭";
            _btnStepForwardSec.UseVisualStyleBackColor = true;

            // 
            // _btnStop
            // 
            _btnStop.Location = new Point(376, 3);
            _btnStop.Name = "_btnStop";
            _btnStop.Size = new Size(68, 28);
            _btnStop.TabIndex = 5;
            _btnStop.Text = "⏹ Stop";
            _btnStop.UseVisualStyleBackColor = true;

            // 
            // _btnGrabScreenshot
            // 
            _btnGrabScreenshot.Location = new Point(450, 3);
            _btnGrabScreenshot.Name = "_btnGrabScreenshot";
            _btnGrabScreenshot.Size = new Size(130, 28);
            _btnGrabScreenshot.TabIndex = 6;
            _btnGrabScreenshot.Text = "📸 Screenshot (S)";
            _btnGrabScreenshot.UseVisualStyleBackColor = true;

            // 
            // _btnZoomOut
            // 
            _btnZoomOut.Location = new Point(586, 3);
            _btnZoomOut.Name = "_btnZoomOut";
            _btnZoomOut.Size = new Size(34, 28);
            _btnZoomOut.TabIndex = 7;
            _btnZoomOut.Text = "🔍−";
            _btnZoomOut.UseVisualStyleBackColor = true;

            // 
            // _lblZoomLevel
            // 
            _lblZoomLevel.AutoSize = true;
            _lblZoomLevel.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            _lblZoomLevel.Location = new Point(626, 9);
            _lblZoomLevel.Margin = new Padding(3, 9, 3, 0);
            _lblZoomLevel.Name = "_lblZoomLevel";
            _lblZoomLevel.Size = new Size(38, 15);
            _lblZoomLevel.TabIndex = 8;
            _lblZoomLevel.Text = "100%";
            _lblZoomLevel.TextAlign = ContentAlignment.MiddleCenter;

            // 
            // _btnZoomIn
            // 
            _btnZoomIn.Location = new Point(670, 3);
            _btnZoomIn.Name = "_btnZoomIn";
            _btnZoomIn.Size = new Size(34, 28);
            _btnZoomIn.TabIndex = 9;
            _btnZoomIn.Text = "🔍+";
            _btnZoomIn.UseVisualStyleBackColor = true;

            // 
            // _btnZoomReset
            // 
            _btnZoomReset.Location = new Point(710, 3);
            _btnZoomReset.Name = "_btnZoomReset";
            _btnZoomReset.Size = new Size(44, 28);
            _btnZoomReset.TabIndex = 10;
            _btnZoomReset.Text = "Fit";
            _btnZoomReset.UseVisualStyleBackColor = true;

            // 
            // lblHint
            // 
            lblHint.AutoSize = true;
            lblHint.ForeColor = SystemColors.GrayText;
            lblHint.Location = new Point(760, 9);
            lblHint.Margin = new Padding(6, 9, 3, 0);
            lblHint.Name = "lblHint";
            lblHint.Size = new Size(420, 15);
            lblHint.TabIndex = 11;
            lblHint.Text = "Space: Play/Pause | S: Screenshot | Ctrl+Wheel: Zoom | Drag: Pan | Esc: Fit";

            // 
            // _lblInfo
            // 
            _lblInfo.AutoSize = true;
            _lblInfo.Location = new Point(913, 9);
            _lblInfo.Margin = new Padding(3, 9, 3, 0);
            _lblInfo.Name = "_lblInfo";
            _lblInfo.Size = new Size(0, 15);
            _lblInfo.TabIndex = 7;
            _lblInfo.Visible = false;

            // 
            // panelSidebar
            // 
            panelSidebar.AutoScroll = true;
            panelSidebar.BackColor = SystemColors.Control;
            panelSidebar.Controls.Add(grpInfo);
            panelSidebar.Controls.Add(grpDisplay);
            panelSidebar.Controls.Add(grpDetection);
            panelSidebar.Dock = DockStyle.Right;
            panelSidebar.Location = new Point(944, 48);
            panelSidebar.Name = "panelSidebar";
            panelSidebar.Padding = new Padding(8);
            panelSidebar.Size = new Size(320, 677);
            panelSidebar.TabIndex = 1;

            // 
            // grpDetection
            // 
            grpDetection.Controls.Add(tableDetection);
            grpDetection.Dock = DockStyle.Top;
            grpDetection.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            grpDetection.Location = new Point(8, 8);
            grpDetection.Name = "grpDetection";
            grpDetection.Padding = new Padding(10, 8, 10, 8);
            grpDetection.Size = new Size(304, 180);
            grpDetection.TabIndex = 0;
            grpDetection.TabStop = false;
            grpDetection.Text = "Motion Detection Settings";

            // 
            // tableDetection
            // 
            tableDetection.ColumnCount = 2;
            tableDetection.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 60F));
            tableDetection.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 40F));
            tableDetection.Controls.Add(lblThreshold, 0, 0);
            tableDetection.Controls.Add(_nudThreshold, 1, 0);
            tableDetection.Controls.Add(lblMinArea, 0, 1);
            tableDetection.Controls.Add(_nudMinArea, 1, 1);
            tableDetection.Controls.Add(lblBlur, 0, 2);
            tableDetection.Controls.Add(_nudBlur, 1, 2);
            tableDetection.Controls.Add(lblLearn, 0, 3);
            tableDetection.Controls.Add(_nudLearnRate, 1, 3);
            tableDetection.Dock = DockStyle.Fill;
            tableDetection.Font = new Font("Segoe UI", 9F);
            tableDetection.Location = new Point(10, 24);
            tableDetection.Name = "tableDetection";
            tableDetection.RowCount = 4;
            tableDetection.RowStyles.Add(new RowStyle(SizeType.Percent, 25F));
            tableDetection.RowStyles.Add(new RowStyle(SizeType.Percent, 25F));
            tableDetection.RowStyles.Add(new RowStyle(SizeType.Percent, 25F));
            tableDetection.RowStyles.Add(new RowStyle(SizeType.Percent, 25F));
            tableDetection.Size = new Size(284, 148);
            tableDetection.TabIndex = 0;

            // 
            // lblThreshold
            // 
            lblThreshold.Anchor = AnchorStyles.Left;
            lblThreshold.AutoSize = true;
            lblThreshold.Location = new Point(3, 11);
            lblThreshold.Name = "lblThreshold";
            lblThreshold.Size = new Size(95, 15);
            lblThreshold.TabIndex = 0;
            lblThreshold.Text = "Threshold (1-255)";

            // 
            // _nudThreshold
            // 
            _nudThreshold.Anchor = AnchorStyles.Left | AnchorStyles.Right;
            _nudThreshold.Location = new Point(173, 7);
            _nudThreshold.Maximum = new decimal(new int[] { 255, 0, 0, 0 });
            _nudThreshold.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            _nudThreshold.Name = "_nudThreshold";
            _nudThreshold.Size = new Size(108, 23);
            _nudThreshold.TabIndex = 1;
            _nudThreshold.Value = new decimal(new int[] { 25, 0, 0, 0 });

            // 
            // lblMinArea
            // 
            lblMinArea.Anchor = AnchorStyles.Left;
            lblMinArea.AutoSize = true;
            lblMinArea.Location = new Point(3, 48);
            lblMinArea.Name = "lblMinArea";
            lblMinArea.Size = new Size(104, 15);
            lblMinArea.TabIndex = 2;
            lblMinArea.Text = "Min Area (pixels²)";

            // 
            // _nudMinArea
            // 
            _nudMinArea.Anchor = AnchorStyles.Left | AnchorStyles.Right;
            _nudMinArea.Increment = new decimal(new int[] { 50, 0, 0, 0 });
            _nudMinArea.Location = new Point(173, 44);
            _nudMinArea.Maximum = new decimal(new int[] { 50000, 0, 0, 0 });
            _nudMinArea.Minimum = new decimal(new int[] { 50, 0, 0, 0 });
            _nudMinArea.Name = "_nudMinArea";
            _nudMinArea.Size = new Size(108, 23);
            _nudMinArea.TabIndex = 3;
            _nudMinArea.Value = new decimal(new int[] { 500, 0, 0, 0 });

            // 
            // lblBlur
            // 
            lblBlur.Anchor = AnchorStyles.Left;
            lblBlur.AutoSize = true;
            lblBlur.Location = new Point(3, 85);
            lblBlur.Name = "lblBlur";
            lblBlur.Size = new Size(106, 15);
            lblBlur.TabIndex = 4;
            lblBlur.Text = "Blur Kernel (odd)";

            // 
            // _nudBlur
            // 
            _nudBlur.Anchor = AnchorStyles.Left | AnchorStyles.Right;
            _nudBlur.Increment = new decimal(new int[] { 2, 0, 0, 0 });
            _nudBlur.Location = new Point(173, 81);
            _nudBlur.Maximum = new decimal(new int[] { 41, 0, 0, 0 });
            _nudBlur.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            _nudBlur.Name = "_nudBlur";
            _nudBlur.Size = new Size(108, 23);
            _nudBlur.TabIndex = 5;
            _nudBlur.Value = new decimal(new int[] { 7, 0, 0, 0 });

            // 
            // lblLearn
            // 
            lblLearn.Anchor = AnchorStyles.Left;
            lblLearn.AutoSize = true;
            lblLearn.Location = new Point(3, 122);
            lblLearn.Name = "lblLearn";
            lblLearn.Size = new Size(103, 15);
            lblLearn.TabIndex = 6;
            lblLearn.Text = "Learn Rate (0-100%)";

            // 
            // _nudLearnRate
            // 
            _nudLearnRate.Anchor = AnchorStyles.Left | AnchorStyles.Right;
            _nudLearnRate.Location = new Point(173, 118);
            _nudLearnRate.Name = "_nudLearnRate";
            _nudLearnRate.Size = new Size(108, 23);
            _nudLearnRate.TabIndex = 7;
            _nudLearnRate.Value = new decimal(new int[] { 2, 0, 0, 0 });

            // 
            // grpDisplay
            // 
            grpDisplay.Controls.Add(tableDisplay);
            grpDisplay.Dock = DockStyle.Top;
            grpDisplay.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            grpDisplay.Location = new Point(8, 188);
            grpDisplay.Name = "grpDisplay";
            grpDisplay.Padding = new Padding(10, 8, 10, 8);
            grpDisplay.Size = new Size(304, 275);
            grpDisplay.TabIndex = 1;
            grpDisplay.TabStop = false;
            grpDisplay.Text = "Display Options";

            // 
            // tableDisplay
            // 
            tableDisplay.ColumnCount = 2;
            tableDisplay.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 60F));
            tableDisplay.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 40F));
            tableDisplay.Controls.Add(_chkOverlay, 0, 0);
            tableDisplay.SetColumnSpan(_chkOverlay, 2);
            tableDisplay.Controls.Add(lblOverlay, 0, 1);
            tableDisplay.Controls.Add(_nudAlpha, 1, 1);
            tableDisplay.Controls.Add(_chkTrailsInOverlay, 0, 2);
            tableDisplay.SetColumnSpan(_chkTrailsInOverlay, 2);
            tableDisplay.Controls.Add(_chkBoxes, 0, 3);
            tableDisplay.SetColumnSpan(_chkBoxes, 2);
            tableDisplay.Controls.Add(_chkIgnoreTrails, 0, 4);
            tableDisplay.SetColumnSpan(_chkIgnoreTrails, 2);
            tableDisplay.Controls.Add(lblBoxHold, 0, 5);
            tableDisplay.Controls.Add(_nudBoxHold, 1, 5);
            tableDisplay.Controls.Add(_chkResetBgOnSeek, 0, 6);
            tableDisplay.SetColumnSpan(_chkResetBgOnSeek, 2);
            tableDisplay.Dock = DockStyle.Fill;
            tableDisplay.Font = new Font("Segoe UI", 9F);
            tableDisplay.Location = new Point(10, 24);
            tableDisplay.Name = "tableDisplay";
            tableDisplay.RowCount = 7;
            tableDisplay.RowStyles.Add(new RowStyle(SizeType.Percent, 14.28F));
            tableDisplay.RowStyles.Add(new RowStyle(SizeType.Percent, 14.28F));
            tableDisplay.RowStyles.Add(new RowStyle(SizeType.Percent, 14.28F));
            tableDisplay.RowStyles.Add(new RowStyle(SizeType.Percent, 14.28F));
            tableDisplay.RowStyles.Add(new RowStyle(SizeType.Percent, 14.28F));
            tableDisplay.RowStyles.Add(new RowStyle(SizeType.Percent, 14.28F));
            tableDisplay.RowStyles.Add(new RowStyle(SizeType.Percent, 14.32F));
            tableDisplay.Size = new Size(284, 243);
            tableDisplay.TabIndex = 0;

            // 
            // _chkOverlay
            // 
            _chkOverlay.Anchor = AnchorStyles.Left;
            _chkOverlay.AutoSize = true;
            _chkOverlay.Checked = true;
            _chkOverlay.CheckState = CheckState.Checked;
            _chkOverlay.Location = new Point(3, 7);
            _chkOverlay.Name = "_chkOverlay";
            _chkOverlay.Size = new Size(157, 19);
            _chkOverlay.TabIndex = 0;
            _chkOverlay.Text = "Highlight motion in red";
            _chkOverlay.UseVisualStyleBackColor = true;

            // 
            // lblOverlay
            // 
            lblOverlay.Anchor = AnchorStyles.Left;
            lblOverlay.AutoSize = true;
            lblOverlay.Location = new Point(3, 40);
            lblOverlay.Name = "lblOverlay";
            lblOverlay.Size = new Size(119, 15);
            lblOverlay.TabIndex = 1;
            lblOverlay.Text = "Red Opacity (0-100%)";

            // 
            // _nudAlpha
            // 
            _nudAlpha.Anchor = AnchorStyles.Left | AnchorStyles.Right;
            _nudAlpha.Increment = new decimal(new int[] { 5, 0, 0, 0 });
            _nudAlpha.Location = new Point(173, 37);
            _nudAlpha.Name = "_nudAlpha";
            _nudAlpha.Size = new Size(108, 23);
            _nudAlpha.TabIndex = 2;
            _nudAlpha.Value = new decimal(new int[] { 55, 0, 0, 0 });

            // 
            // _chkTrailsInOverlay
            // 
            _chkTrailsInOverlay.Anchor = AnchorStyles.Left;
            _chkTrailsInOverlay.AutoSize = true;
            _chkTrailsInOverlay.Checked = true;
            _chkTrailsInOverlay.CheckState = CheckState.Checked;
            _chkTrailsInOverlay.Location = new Point(3, 73);
            _chkTrailsInOverlay.Name = "_chkTrailsInOverlay";
            _chkTrailsInOverlay.Size = new Size(177, 19);
            _chkTrailsInOverlay.TabIndex = 3;
            _chkTrailsInOverlay.Text = "Show motion trails in red overlay";
            _chkTrailsInOverlay.UseVisualStyleBackColor = true;

            // 
            // _chkBoxes
            // 
            _chkBoxes.Anchor = AnchorStyles.Left;
            _chkBoxes.AutoSize = true;
            _chkBoxes.Checked = true;
            _chkBoxes.CheckState = CheckState.Checked;
            _chkBoxes.Location = new Point(3, 106);
            _chkBoxes.Name = "_chkBoxes";
            _chkBoxes.Size = new Size(174, 19);
            _chkBoxes.TabIndex = 4;
            _chkBoxes.Text = "Draw green bounding boxes";
            _chkBoxes.UseVisualStyleBackColor = true;

            // 
            // _chkIgnoreTrails
            // 
            _chkIgnoreTrails.Anchor = AnchorStyles.Left;
            _chkIgnoreTrails.AutoSize = true;
            _chkIgnoreTrails.Checked = true;
            _chkIgnoreTrails.CheckState = CheckState.Checked;
            _chkIgnoreTrails.Location = new Point(3, 139);
            _chkIgnoreTrails.Name = "_chkIgnoreTrails";
            _chkIgnoreTrails.Size = new Size(187, 19);
            _chkIgnoreTrails.TabIndex = 5;
            _chkIgnoreTrails.Text = "Filter trails from green boxes";
            _chkIgnoreTrails.UseVisualStyleBackColor = true;

            // 
            // lblBoxHold
            // 
            lblBoxHold.Anchor = AnchorStyles.Left;
            lblBoxHold.AutoSize = true;
            lblBoxHold.Location = new Point(3, 172);
            lblBoxHold.Name = "lblBoxHold";
            lblBoxHold.Size = new Size(130, 15);
            lblBoxHold.TabIndex = 6;
            lblBoxHold.Text = "Box Fade Time (frames)";

            // 
            // _nudBoxHold
            // 
            _nudBoxHold.Anchor = AnchorStyles.Left | AnchorStyles.Right;
            _nudBoxHold.Increment = new decimal(new int[] { 5, 0, 0, 0 });
            _nudBoxHold.Location = new Point(173, 168);
            _nudBoxHold.Maximum = new decimal(new int[] { 120, 0, 0, 0 });
            _nudBoxHold.Minimum = new decimal(new int[] { 0, 0, 0, 0 });
            _nudBoxHold.Name = "_nudBoxHold";
            _nudBoxHold.Size = new Size(108, 23);
            _nudBoxHold.TabIndex = 7;
            _nudBoxHold.Value = new decimal(new int[] { 20, 0, 0, 0 });

            // 
            // _chkResetBgOnSeek
            // 
            _chkResetBgOnSeek.Anchor = AnchorStyles.Left;
            _chkResetBgOnSeek.AutoSize = true;
            _chkResetBgOnSeek.Checked = true;
            _chkResetBgOnSeek.CheckState = CheckState.Checked;
            _chkResetBgOnSeek.Location = new Point(3, 208);
            _chkResetBgOnSeek.Name = "_chkResetBgOnSeek";
            _chkResetBgOnSeek.Size = new Size(189, 19);
            _chkResetBgOnSeek.TabIndex = 8;
            _chkResetBgOnSeek.Text = "Reset background when seeking";
            _chkResetBgOnSeek.UseVisualStyleBackColor = true;

            // 
            // grpInfo
            // 
            grpInfo.Controls.Add(tableInfo);
            grpInfo.Dock = DockStyle.Top;
            grpInfo.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            grpInfo.Location = new Point(8, 363);
            grpInfo.Name = "grpInfo";
            grpInfo.Padding = new Padding(10, 8, 10, 8);
            grpInfo.Size = new Size(304, 190);
            grpInfo.TabIndex = 2;
            grpInfo.TabStop = false;
            grpInfo.Text = "Video & Detection Info";

            // 
            // tableInfo
            // 
            tableInfo.ColumnCount = 2;
            tableInfo.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 42F));
            tableInfo.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 58F));
            tableInfo.Controls.Add(lblResHeader, 0, 0);
            tableInfo.Controls.Add(_lblInfoResolution, 1, 0);
            tableInfo.Controls.Add(lblFpsHeader, 0, 1);
            tableInfo.Controls.Add(_lblInfoFps, 1, 1);
            tableInfo.Controls.Add(lblFramesHeader, 0, 2);
            tableInfo.Controls.Add(_lblInfoFrames, 1, 2);
            tableInfo.Controls.Add(lblTimeHeader, 0, 3);
            tableInfo.Controls.Add(_lblInfoTime, 1, 3);
            tableInfo.Controls.Add(lblRegionsHeader, 0, 4);
            tableInfo.Controls.Add(_lblInfoRegions, 1, 4);
            tableInfo.Dock = DockStyle.Fill;
            tableInfo.Font = new Font("Segoe UI", 9F);
            tableInfo.Location = new Point(10, 24);
            tableInfo.Name = "tableInfo";
            tableInfo.RowCount = 5;
            tableInfo.RowStyles.Add(new RowStyle(SizeType.Percent, 20F));
            tableInfo.RowStyles.Add(new RowStyle(SizeType.Percent, 20F));
            tableInfo.RowStyles.Add(new RowStyle(SizeType.Percent, 20F));
            tableInfo.RowStyles.Add(new RowStyle(SizeType.Percent, 20F));
            tableInfo.RowStyles.Add(new RowStyle(SizeType.Percent, 20F));
            tableInfo.Size = new Size(284, 158);
            tableInfo.TabIndex = 0;

            // 
            // lblResHeader
            // 
            lblResHeader.Anchor = AnchorStyles.Left;
            lblResHeader.AutoSize = true;
            lblResHeader.ForeColor = SystemColors.GrayText;
            lblResHeader.Location = new Point(3, 8);
            lblResHeader.Name = "lblResHeader";
            lblResHeader.Size = new Size(66, 15);
            lblResHeader.TabIndex = 0;
            lblResHeader.Text = "Resolution:";

            // 
            // _lblInfoResolution
            // 
            _lblInfoResolution.Anchor = AnchorStyles.Left;
            _lblInfoResolution.AutoSize = true;
            _lblInfoResolution.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            _lblInfoResolution.Location = new Point(122, 8);
            _lblInfoResolution.Name = "_lblInfoResolution";
            _lblInfoResolution.Size = new Size(12, 15);
            _lblInfoResolution.TabIndex = 1;
            _lblInfoResolution.Text = "-";

            // 
            // lblFpsHeader
            // 
            lblFpsHeader.Anchor = AnchorStyles.Left;
            lblFpsHeader.AutoSize = true;
            lblFpsHeader.ForeColor = SystemColors.GrayText;
            lblFpsHeader.Location = new Point(3, 39);
            lblFpsHeader.Name = "lblFpsHeader";
            lblFpsHeader.Size = new Size(63, 15);
            lblFpsHeader.TabIndex = 2;
            lblFpsHeader.Text = "Framerate:";

            // 
            // _lblInfoFps
            // 
            _lblInfoFps.Anchor = AnchorStyles.Left;
            _lblInfoFps.AutoSize = true;
            _lblInfoFps.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            _lblInfoFps.Location = new Point(122, 39);
            _lblInfoFps.Name = "_lblInfoFps";
            _lblInfoFps.Size = new Size(12, 15);
            _lblInfoFps.TabIndex = 3;
            _lblInfoFps.Text = "-";

            // 
            // lblFramesHeader
            // 
            lblFramesHeader.Anchor = AnchorStyles.Left;
            lblFramesHeader.AutoSize = true;
            lblFramesHeader.ForeColor = SystemColors.GrayText;
            lblFramesHeader.Location = new Point(3, 70);
            lblFramesHeader.Name = "lblFramesHeader";
            lblFramesHeader.Size = new Size(43, 15);
            lblFramesHeader.TabIndex = 4;
            lblFramesHeader.Text = "Frame:";

            // 
            // _lblInfoFrames
            // 
            _lblInfoFrames.Anchor = AnchorStyles.Left;
            _lblInfoFrames.AutoSize = true;
            _lblInfoFrames.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            _lblInfoFrames.Location = new Point(122, 70);
            _lblInfoFrames.Name = "_lblInfoFrames";
            _lblInfoFrames.Size = new Size(33, 15);
            _lblInfoFrames.TabIndex = 5;
            _lblInfoFrames.Text = "0 / 0";

            // 
            // lblTimeHeader
            // 
            lblTimeHeader.Anchor = AnchorStyles.Left;
            lblTimeHeader.AutoSize = true;
            lblTimeHeader.ForeColor = SystemColors.GrayText;
            lblTimeHeader.Location = new Point(3, 101);
            lblTimeHeader.Name = "lblTimeHeader";
            lblTimeHeader.Size = new Size(36, 15);
            lblTimeHeader.TabIndex = 6;
            lblTimeHeader.Text = "Time:";

            // 
            // _lblInfoTime
            // 
            _lblInfoTime.Anchor = AnchorStyles.Left;
            _lblInfoTime.AutoSize = true;
            _lblInfoTime.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            _lblInfoTime.Location = new Point(122, 101);
            _lblInfoTime.Name = "_lblInfoTime";
            _lblInfoTime.Size = new Size(87, 15);
            _lblInfoTime.TabIndex = 7;
            _lblInfoTime.Text = "00:00 / 00:00";

            // 
            // lblRegionsHeader
            // 
            lblRegionsHeader.Anchor = AnchorStyles.Left;
            lblRegionsHeader.AutoSize = true;
            lblRegionsHeader.ForeColor = SystemColors.GrayText;
            lblRegionsHeader.Location = new Point(3, 133);
            lblRegionsHeader.Name = "lblRegionsHeader";
            lblRegionsHeader.Size = new Size(95, 15);
            lblRegionsHeader.TabIndex = 8;
            lblRegionsHeader.Text = "Motion Regions:";

            // 
            // _lblInfoRegions
            // 
            _lblInfoRegions.Anchor = AnchorStyles.Left;
            _lblInfoRegions.AutoSize = true;
            _lblInfoRegions.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            _lblInfoRegions.ForeColor = Color.DarkGreen;
            _lblInfoRegions.Location = new Point(122, 133);
            _lblInfoRegions.Name = "_lblInfoRegions";
            _lblInfoRegions.Size = new Size(58, 15);
            _lblInfoRegions.TabIndex = 9;
            _lblInfoRegions.Text = "0 regions";

            // 
            // _picture
            // 
            _picture.BackColor = Color.FromArgb(20, 20, 20);
            _picture.Dock = DockStyle.Fill;
            _picture.Location = new Point(0, 48);
            _picture.Name = "_picture";
            _picture.Size = new Size(944, 677);
            _picture.SizeMode = PictureBoxSizeMode.Zoom;
            _picture.TabIndex = 3;
            _picture.TabStop = false;

            // 
            // FormMain
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1264, 811);
            Controls.Add(_picture);
            Controls.Add(panelSidebar);
            Controls.Add(panelBottom);
            Controls.Add(panelTop);
            Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point);
            KeyPreview = true;
            MinimumSize = new Size(960, 640);
            Name = "FormMain";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Motion Highlighter – Video Motion Detection";

            // 
            // _toolTip
            // 
            _toolTip = new ToolTip(this.components);
            _toolTip.AutoPopDelay = 10000;
            _toolTip.InitialDelay = 350;
            _toolTip.ReshowDelay = 150;
            _toolTip.ShowAlways = true;
            _toolTip.IsBalloon = false;

            // Detection settings
            _toolTip.SetToolTip(_nudThreshold, "Threshold (1–255): Minimum difference in pixel intensity from the background model to trigger motion. Lower = more sensitive; Higher = ignores subtle changes.");
            _toolTip.SetToolTip(lblThreshold, "Threshold (1–255): Minimum difference in pixel intensity from the background model to trigger motion. Lower = more sensitive; Higher = ignores subtle changes.");

            _toolTip.SetToolTip(_nudMinArea, "Min Area (pixels²): Minimum bounding contour area required to register as motion (50–50000). Filters out noise, insects, dust, rain, or camera sensor grain to maintain smooth CPU performance.");
            _toolTip.SetToolTip(lblMinArea, "Min Area (pixels²): Minimum bounding contour area required to register as motion (50–50000). Filters out noise, insects, dust, rain, or camera sensor grain to maintain smooth CPU performance.");

            _toolTip.SetToolTip(_nudBlur, "Blur Kernel Size (odd number): Size of Gaussian blur filter applied before difference analysis. Higher values smooth out video noise, flickering, compression artifacts, and grass/leaves motion.");
            _toolTip.SetToolTip(lblBlur, "Blur Kernel Size (odd number): Size of Gaussian blur filter applied before difference analysis. Higher values smooth out video noise, flickering, compression artifacts, and grass/leaves motion.");

            _toolTip.SetToolTip(_nudLearnRate, "Background Learning Rate (0–100%): Speed at which the background model adapts to environmental changes (lighting, moving clouds, shadows). 0% freezes the background model.");
            _toolTip.SetToolTip(lblLearn, "Background Learning Rate (0–100%): Speed at which the background model adapts to environmental changes (lighting, moving clouds, shadows). 0% freezes the background model.");

            // Display settings
            _toolTip.SetToolTip(_chkOverlay, "Highlight motion in red: Toggles the semi-transparent red overlay over all detected moving pixels.");
            _toolTip.SetToolTip(_nudAlpha, "Red Opacity (0–100%): Adjusts the opacity / transparency of the red motion overlay.");
            _toolTip.SetToolTip(lblOverlay, "Red Opacity (0–100%): Adjusts the opacity / transparency of the red motion overlay.");
            _toolTip.SetToolTip(_chkTrailsInOverlay, "Show motion trails in red overlay: When checked, highlights both active motion and fading adaptation trails. Uncheck to highlight only current active moving pixels.");
            _toolTip.SetToolTip(_chkBoxes, "Draw green bounding boxes: Renders bounding rectangles around moving objects meeting the Min Area criteria.");
            _toolTip.SetToolTip(_chkIgnoreTrails, "Filter trails from green boxes: Rejects background-adaptation trails so green boxes track only active moving subjects and do not linger on empty trails.");
            _toolTip.SetToolTip(_nudBoxHold, "Box Fade Time (frames): How long green bounding borders stay visible and smoothly fade out after motion stops, eliminating flickering. Set to 0 for instant disappearance.");
            _toolTip.SetToolTip(lblBoxHold, "Box Fade Time (frames): How long green bounding borders stay visible and smoothly fade out after motion stops, eliminating flickering. Set to 0 for instant disappearance.");
            _toolTip.SetToolTip(_chkResetBgOnSeek, "Reset background when seeking: Automatically re-initializes the background model when scrubbing or jumping to a new frame to prevent false motion artifacts.");

            // Action buttons
            _toolTip.SetToolTip(_btnLoad, "Open Video File: Load a local video file (MP4, MKV, MOV, AVI) for motion analysis.");
            _toolTip.SetToolTip(_btnOpenStream, "Open Stream: Connect to a live streaming source (Google Home, Ring, Nest, Arlo, Wyze, CCTV, DASH, HLS, Fragmented MP4, MJPEG, or RTSP).");
            _toolTip.SetToolTip(_btnResetBg, "Reset Background: Immediately clears and recalibrates the background reference model using the current frame.");
            _toolTip.SetToolTip(_btnExport, "Export Annotated MP4: Renders the entire video with current motion detection settings, overlays, and bounding boxes into a new MP4 video file.");
            _toolTip.SetToolTip(_btnGrabScreenshot, "Grab Screenshot (Shortcut: S): Captures the current frame or zoomed detail view with all motion annotations to Pictures and copies it to the clipboard.");
            _toolTip.SetToolTip(_btnZoomOut, "Zoom Out (Ctrl + Minus / Wheel Down): Zoom out of the video.");
            _toolTip.SetToolTip(_lblZoomLevel, "Current Zoom Level (100% - 1000%): Click and drag on video to pan.");
            _toolTip.SetToolTip(_btnZoomIn, "Zoom In (Ctrl + Plus / Wheel Up): Zoom in to inspect motion highlights and bounding boxes in fine detail.");
            _toolTip.SetToolTip(_btnZoomReset, "Fit to Window (Shortcut: Escape / Ctrl + 0 / Double-Click): Resets zoom to fit entire video.");
            _toolTip.SetToolTip(_btnAbout, "About Motion Highlighter: Application details, Generated Pixel links, and LGBTQ+ Pride community info.");

            // Playback controls
            _toolTip.SetToolTip(_btnPlay, "Play / Pause (Shortcut: Spacebar): Toggle video playback or live stream monitoring.");
            _toolTip.SetToolTip(_btnStop, "Stop: Stops playback and rewinds to the beginning.");
            _toolTip.SetToolTip(_btnStepBackSec, "Step -1 Second (Shortcut: Shift + Left Arrow): Jump backward 1 second.");
            _toolTip.SetToolTip(_btnStepBackFrame, "Step -1 Frame (Shortcut: Left Arrow): Step backward exactly 1 frame.");
            _toolTip.SetToolTip(_btnStepForwardFrame, "Step +1 Frame (Shortcut: Right Arrow): Step forward exactly 1 frame.");
            _toolTip.SetToolTip(_btnStepForwardSec, "Step +1 Second (Shortcut: Shift + Right Arrow): Jump forward 1 second.");
            _toolTip.SetToolTip(_seek, "Timeline Scrubbing: Drag slider or use Mouse Wheel over the video or slider to scrub frame-by-frame.");

            ((ISupportInitialize)_picture).EndInit();
            ((ISupportInitialize)_seek).EndInit();
            ((ISupportInitialize)_nudThreshold).EndInit();
            ((ISupportInitialize)_nudMinArea).EndInit();
            ((ISupportInitialize)_nudBlur).EndInit();
            ((ISupportInitialize)_nudAlpha).EndInit();
            ((ISupportInitialize)_nudLearnRate).EndInit();
            ((ISupportInitialize)_nudBoxHold).EndInit();
            panelTop.ResumeLayout(false);
            panelBottom.ResumeLayout(false);
            panelSidebar.ResumeLayout(false);
            tableSeek.ResumeLayout(false);
            tableSeek.PerformLayout();
            panelControls.ResumeLayout(false);
            panelControls.PerformLayout();
            grpDetection.ResumeLayout(false);
            tableDetection.ResumeLayout(false);
            tableDetection.PerformLayout();
            grpDisplay.ResumeLayout(false);
            tableDisplay.ResumeLayout(false);
            tableDisplay.PerformLayout();
            grpInfo.ResumeLayout(false);
            tableInfo.ResumeLayout(false);
            tableInfo.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        private ZoomablePictureBox _picture;
        private Button _btnLoad;
        private Button _btnOpenStream;
        private ComboBox _cmbLanguage;
        private Button _btnAbout;
        private Button _btnGrabScreenshot;
        private Button _btnZoomOut;
        private Label _lblZoomLevel;
        private Button _btnZoomIn;
        private Button _btnZoomReset;
        private Button _btnPlay;
        private Button _btnResetBg;
        private Button _btnExport;
        private Button _btnCancelExport;
        private ProgressBar _progressBar;
        private TrackBar _seek;
        private Label _lblInfo;
        private NumericUpDown _nudThreshold;
        private NumericUpDown _nudMinArea;
        private NumericUpDown _nudBlur;
        private NumericUpDown _nudAlpha;
        private NumericUpDown _nudLearnRate;
        private CheckBox _chkBoxes;
        private CheckBox _chkOverlay;
        private CheckBox _chkResetBgOnSeek;
        private CheckBox _chkIgnoreTrails;
        private CheckBox _chkTrailsInOverlay;
        private NumericUpDown _nudBoxHold;

        private Button _btnStepBackSec;
        private Button _btnStepBackFrame;
        private Button _btnStepForwardFrame;
        private Button _btnStepForwardSec;
        private Button _btnStop;
        private Label _lblCurrentTime;
        private Label _lblTotalTime;
        private Label _lblVideoTitle;

        private Label _lblInfoResolution;
        private Label _lblInfoFps;
        private Label _lblInfoFrames;
        private Label _lblInfoTime;
        private Label _lblInfoRegions;
        private ToolTip _toolTip;
    }
}
