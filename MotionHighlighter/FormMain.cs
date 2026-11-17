using OpenCvSharp;
using OpenCvSharp.Extensions;
using MotionHighlighter.Localization;
using MotionHighlighter.Streaming;
using MotionHighlighter.Updating;

namespace MotionHighlighter
{
    public partial class FormMain : Form
    {
        // Video
        private VideoCapture? _cap;
        private string? _videoFilePath;
        private int _totalFrames;
        private double _fps;
        private int _currentFrameIndex;
        private bool _isPlaying;
        private bool _updatingTrackBar;
        private CancellationTokenSource? _playCts;
        private CancellationTokenSource? _exportCts;

        // Streaming
        private bool _isStreaming;
        private IStreamGrabber? _streamGrabber;
        private string? _streamSourceUrl;
        private StreamFormat _streamFormat = StreamFormat.Unknown;
        private StreamAuthOptions? _streamAuth;
        private string? _streamTitle;
        private Mat? _latestAnnotatedBgr;
        private volatile bool _isRenderingStreamFrame;
        private volatile bool _isRenderingVideoFrame;
        private Task? _playTask;
        private long _streamRenderedCount;

        // Current frame and background model
        private Mat? _currentFrameBgr;
        private Mat? _prevFrameGray;
        private Mat? _bgFloatGray; // CV_32F 1-channel
        private readonly object _capLock = new();
        private readonly MotionBoxTracker _boxTracker = new();

        public FormMain()
        {
            InitializeComponent();

            PopulateLanguages();
            _cmbLanguage.SelectedIndexChanged += OnLanguageSelectionChanged;
            LocalizationManager.Instance.LanguageChanged += (_, _) => ApplyLocalization();
            ApplyLocalization();

            _btnLoad.Click += (_, _) => LoadVideo();
            _btnOpenStream.Click += (_, _) => OpenStreamDialog();
            _btnAbout.Click += (_, _) => ShowAboutDialog();
            _btnGrabScreenshot.Click += (_, _) => GrabScreenshot();
            _btnZoomOut.Click += (_, _) => _picture.ZoomOut();
            _btnZoomIn.Click += (_, _) => _picture.ZoomIn();
            _btnZoomReset.Click += (_, _) => _picture.ResetZoom();
            _picture.ZoomChanged += (_, _) => OnZoomChanged();
            _btnPlay.Click += (_, _) => TogglePlay();
            _btnStop.Click += (_, _) => StopAndRewind();
            _btnResetBg.Click += (_, _) => ResetBackgroundToCurrentFrame();
            _btnExport.Click += async (_, _) => await ExportAnnotatedAsync();
            _btnCancelExport.Click += (_, _) => CancelExport();

            _btnStepBackSec.Click += (_, _) => StepFrames(-(int)Math.Max(1.0, _fps));
            _btnStepBackFrame.Click += (_, _) => StepFrames(-1);
            _btnStepForwardFrame.Click += (_, _) => StepFrames(1);
            _btnStepForwardSec.Click += (_, _) => StepFrames((int)Math.Max(1.0, _fps));

            _seek.Scroll += (_, _) => OnSeekTrackBarScrolled();
            _seek.ValueChanged += (_, _) => OnSeekTrackBarValueChanged();

            _picture.MouseEnter += (_, _) => _picture.Focus();
            _picture.MouseWheel += OnPictureMouseWheel;

            _nudThreshold.ValueChanged += (_, _) => OnParameterChanged();
            _nudMinArea.ValueChanged += (_, _) => OnParameterChanged();
            _nudBlur.ValueChanged += (_, _) => OnParameterChanged();
            _nudAlpha.ValueChanged += (_, _) => OnParameterChanged();
            _nudLearnRate.ValueChanged += (_, _) => OnParameterChanged();
            _chkOverlay.CheckedChanged += (_, _) => OnParameterChanged();
            _chkTrailsInOverlay.CheckedChanged += (_, _) => OnParameterChanged();
            _chkBoxes.CheckedChanged += (_, _) => OnParameterChanged();
            _chkIgnoreTrails.CheckedChanged += (_, _) => OnParameterChanged();
            _nudBoxHold.ValueChanged += (_, _) => OnParameterChanged();

            KeyDown += OnFormKeyDown;
            FormClosing += (_, _) => Cleanup();

            UpdateUiEnabled(false);
        }

        private void OnZoomChanged()
        {
            _lblZoomLevel.Text = $"{Math.Round(_picture.ZoomLevel * 100)}%";
            _btnZoomReset.Font = _picture.IsZoomed ? new Font("Segoe UI", 9F, FontStyle.Bold) : new Font("Segoe UI", 9F, FontStyle.Regular);
        }

        private void UpdateUiEnabled(bool enabled)
        {
            _btnZoomIn.Enabled = enabled;
            _btnZoomOut.Enabled = enabled;
            _btnZoomReset.Enabled = enabled;
            _lblZoomLevel.Enabled = enabled;

            if (!enabled)
            {
                _picture.ResetZoom();
            }

            if (_isStreaming)
            {
                _btnPlay.Enabled = enabled;
                _btnStop.Enabled = enabled;
                _btnGrabScreenshot.Enabled = enabled;
                _btnResetBg.Enabled = enabled;
                _btnExport.Enabled = false;
                _seek.Enabled = false;
                _btnStepBackSec.Enabled = false;
                _btnStepBackFrame.Enabled = false;
                _btnStepForwardFrame.Enabled = false;
                _btnStepForwardSec.Enabled = false;
                _chkResetBgOnSeek.Enabled = false;

                _nudThreshold.Enabled = enabled;
                _nudMinArea.Enabled = enabled;
                _nudBlur.Enabled = enabled;
                _nudAlpha.Enabled = enabled;
                _nudLearnRate.Enabled = enabled;
                _chkOverlay.Enabled = enabled;
                _chkTrailsInOverlay.Enabled = enabled;
                _chkBoxes.Enabled = enabled;
                _chkIgnoreTrails.Enabled = enabled;
                _nudBoxHold.Enabled = enabled;
                return;
            }

            _btnPlay.Enabled = enabled;
            _btnStop.Enabled = enabled;
            _btnGrabScreenshot.Enabled = enabled;
            _btnStepBackSec.Enabled = enabled;
            _btnStepBackFrame.Enabled = enabled;
            _btnStepForwardFrame.Enabled = enabled;
            _btnStepForwardSec.Enabled = enabled;
            _btnResetBg.Enabled = enabled;
            _btnExport.Enabled = enabled;
            _seek.Enabled = enabled;
            _nudThreshold.Enabled = enabled;
            _nudMinArea.Enabled = enabled;
            _nudBlur.Enabled = enabled;
            _nudAlpha.Enabled = enabled;
            _nudLearnRate.Enabled = enabled;
            _chkOverlay.Enabled = enabled;
            _chkTrailsInOverlay.Enabled = enabled;
            _chkBoxes.Enabled = enabled;
            _chkIgnoreTrails.Enabled = enabled;
            _chkResetBgOnSeek.Enabled = enabled;
            _nudBoxHold.Enabled = enabled;
        }

        private void PopulateLanguages()
        {
            _cmbLanguage.Items.Clear();
            foreach (LocaleInfo loc in LocalizationManager.Instance.AvailableLocales)
            {
                _cmbLanguage.Items.Add(loc);
            }

            LocaleInfo current = LocalizationManager.Instance.CurrentLocale;
            for (int i = 0; i < _cmbLanguage.Items.Count; i++)
            {
                if (_cmbLanguage.Items[i] is LocaleInfo info && info.Code.Equals(current.Code, StringComparison.OrdinalIgnoreCase))
                {
                    _cmbLanguage.SelectedIndex = i;
                    break;
                }
            }
        }

        private void OnLanguageSelectionChanged(object? sender, EventArgs e)
        {
            if (_cmbLanguage.SelectedItem is LocaleInfo locale)
            {
                LocalizationManager.Instance.SetLanguage(locale.Code);
            }
        }

        /// <summary>
        /// Translates all user-visible text throughout the main application form and controls.
        /// </summary>
        public void ApplyLocalization()
        {
            Text = LocalizationManager.T("main.title");
            _btnLoad.Text = LocalizationManager.T("main.btn_open_video");
            _btnOpenStream.Text = LocalizationManager.T("main.btn_open_stream");
            _btnAbout.Text = LocalizationManager.T("main.btn_about");
            _btnExport.Text = LocalizationManager.T("main.btn_export");
            _btnCancelExport.Text = LocalizationManager.T("main.btn_cancel_export");
            _btnGrabScreenshot.Text = LocalizationManager.T("main.btn_screenshot");
            _btnZoomOut.Text = LocalizationManager.T("main.btn_zoom_out");
            _btnZoomIn.Text = LocalizationManager.T("main.btn_zoom_in");
            _btnZoomReset.Text = LocalizationManager.T("main.btn_zoom_reset");
            _btnPlay.Text = _isPlaying ? LocalizationManager.T("main.btn_pause") : LocalizationManager.T("main.btn_play");
            _chkOverlay.Text = LocalizationManager.T("main.chk_overlay");
            _chkTrailsInOverlay.Text = LocalizationManager.T("main.chk_trails");
            _chkBoxes.Text = LocalizationManager.T("main.chk_boxes");

            if (string.IsNullOrWhiteSpace(_videoFilePath) && _streamGrabber == null)
            {
                _lblVideoTitle.Text = LocalizationManager.T("main.no_video");
            }

            UpdateSidebarLocalization();

            // Tooltips
            _toolTip.SetToolTip(_btnLoad, LocalizationManager.T("main.tooltip_open_video"));
            _toolTip.SetToolTip(_btnOpenStream, LocalizationManager.T("main.tooltip_open_stream"));
            _toolTip.SetToolTip(_btnAbout, LocalizationManager.T("main.tooltip_about"));
            _toolTip.SetToolTip(_btnGrabScreenshot, LocalizationManager.T("main.tooltip_screenshot"));
            _toolTip.SetToolTip(_btnZoomIn, LocalizationManager.T("main.tooltip_zoom_in"));
            _toolTip.SetToolTip(_btnZoomOut, LocalizationManager.T("main.tooltip_zoom_out"));
            _toolTip.SetToolTip(_btnZoomReset, LocalizationManager.T("main.tooltip_zoom_reset"));
            _toolTip.SetToolTip(_lblZoomLevel, LocalizationManager.T("main.tooltip_zoom_level"));
            _toolTip.SetToolTip(_btnPlay, LocalizationManager.T("main.tooltip_play"));
            _toolTip.SetToolTip(_btnStepBackFrame, LocalizationManager.T("main.tooltip_step_back"));
            _toolTip.SetToolTip(_btnStepForwardFrame, LocalizationManager.T("main.tooltip_step_forward"));
            _toolTip.SetToolTip(_chkOverlay, LocalizationManager.T("main.tooltip_chk_overlay"));
            _toolTip.SetToolTip(_chkTrailsInOverlay, LocalizationManager.T("main.tooltip_chk_trails"));
            _toolTip.SetToolTip(_chkBoxes, LocalizationManager.T("main.tooltip_chk_boxes"));
            _toolTip.SetToolTip(_btnExport, LocalizationManager.T("main.tooltip_export"));
            _toolTip.SetToolTip(_btnCancelExport, LocalizationManager.T("main.tooltip_cancel_export"));
        }

        private void UpdateSidebarLocalization()
        {
            UpdateControlsRecursive(this);
        }

        private static void UpdateControlsRecursive(Control parent)
        {
            foreach (Control c in parent.Controls)
            {
                if (c is GroupBox gb)
                {
                    if (gb.Name == "grpDetection")
                    {
                        gb.Text = LocalizationManager.T("main.grp_detection");
                    }
                    else if (gb.Name == "grpDisplay")
                    {
                        gb.Text = LocalizationManager.T("main.grp_overlay");
                    }
                    else if (gb.Name == "grpInfo")
                    {
                        gb.Text = LocalizationManager.T("main.grp_metrics");
                    }
                }
                else if (c is Label lbl)
                {
                    if (lbl.Name == "lblThreshold")
                    {
                        lbl.Text = LocalizationManager.T("main.lbl_threshold");
                    }
                    else if (lbl.Name == "lblMinArea")
                    {
                        lbl.Text = LocalizationManager.T("main.lbl_min_area");
                    }
                    else if (lbl.Name == "lblBlur")
                    {
                        lbl.Text = LocalizationManager.T("main.lbl_blur");
                    }
                    else if (lbl.Name == "lblLearn")
                    {
                        lbl.Text = LocalizationManager.T("main.lbl_learn_rate");
                    }
                    else if (lbl.Name == "lblOverlay")
                    {
                        lbl.Text = LocalizationManager.T("main.lbl_alpha");
                    }
                    else if (lbl.Name == "lblBoxHold")
                    {
                        lbl.Text = LocalizationManager.T("main.lbl_box_hold");
                    }
                }

                if (c.HasChildren)
                {
                    UpdateControlsRecursive(c);
                }
            }
        }

        private void ShowAboutDialog()
        {
            using FormAbout dlg = new();
            dlg.ShowDialog(this);
        }

        private void OpenStreamDialog()
        {
            StopPlaying();
            using FormStreamDialog dlg = new FormStreamDialog(_streamSourceUrl);
            if (dlg.ShowDialog(this) != DialogResult.OK)
            {
                return;
            }

            StartStreaming(dlg.SelectedStreamUrl, dlg.SelectedFormat, dlg.AuthOptions, dlg.SelectedTitle);
        }

        private void StartStreaming(string url, StreamFormat format, StreamAuthOptions auth, string title)
        {
            CleanupVideoOnly();

            try
            {
                _isStreaming = true;
                _streamSourceUrl = url;
                _streamFormat = format;
                _streamAuth = auth;
                _streamTitle = title;

                _streamGrabber = new StreamGrabber(url, format, auth, title);

                _lblVideoTitle.Text = $"[STREAM] {title} ({format})";
                _lblVideoTitle.ForeColor = Color.DarkBlue;

                _lblCurrentTime.Text = "LIVE";
                _lblTotalTime.Text = "STREAM";
                _seek.Minimum = 0;
                _seek.Maximum = 100;
                _seek.Value = 0;
                _seek.Enabled = false;

                _btnPlay.Text = LocalizationManager.T("main.btn_pause");
                _isPlaying = true;
                UpdateUiEnabled(true);

                StartStreamWorker();
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, $"Error starting stream:\n{ex.Message}", "Stream Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                CleanupVideoOnly();
                UpdateUiEnabled(false);
            }
        }

        private void StartStreamWorker()
        {
            if (_streamGrabber == null) return;

            _playCts?.Cancel();
            _playCts?.Dispose();
            _playCts = new CancellationTokenSource();
            CancellationToken token = _playCts.Token;

            Task.Run(async () =>
            {
                try
                {
                    await _streamGrabber.StartAsync(token);

                    while (!token.IsCancellationRequested && _isStreaming)
                    {
                        if (!_isPlaying)
                        {
                            await Task.Delay(100, token);
                            continue;
                        }

                        Mat? frame = await _streamGrabber.ReadNextFrameAsync(token);
                        if (frame == null)
                        {
                            if (!_streamGrabber.IsRunning)
                            {
                                break;
                            }
                            await Task.Delay(15, token);
                            continue;
                        }

                        if (_isRenderingStreamFrame)
                        {
                            frame.Dispose();
                            continue;
                        }

                        _isRenderingStreamFrame = true;
                        BeginInvoke(() =>
                        {
                            try
                            {
                                if (!_isStreaming)
                                {
                                    frame.Dispose();
                                    return;
                                }

                                _currentFrameBgr?.Dispose();
                                _currentFrameBgr = frame;

                                ProcessAndDisplayCurrentFrame();

                                _streamRenderedCount++;
                                if (_streamRenderedCount % 120 == 0)
                                {
                                    GC.Collect(1, GCCollectionMode.Optimized, false);
                                }
                            }
                            finally
                            {
                                _isRenderingStreamFrame = false;
                            }
                        });

                        int frameDelay = (int)Math.Max(5.0, 1000.0 / Math.Max(1.0, _streamGrabber.Fps));
                        await Task.Delay(frameDelay, token);
                    }
                }
                catch (OperationCanceledException) { }
                catch (Exception ex)
                {
                    BeginInvoke(() =>
                    {
                        MessageBox.Show(this, $"Streaming error:\n{ex.Message}", "Stream Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        StopPlaying();
                    });
                }
                finally
                {
                    BeginInvoke(() =>
                    {
                        if (_isStreaming && _streamGrabber != null && !_streamGrabber.IsRunning)
                        {
                            _isPlaying = false;
                            _btnPlay.Text = LocalizationManager.T("main.btn_play");
                            _lblVideoTitle.Text = $"[STREAM ENDED] {_streamTitle}";
                            _lblVideoTitle.ForeColor = Color.DarkOrange;
                        }
                    });
                }
            }, token);
        }

        private void GrabScreenshot()
        {
            Mat? frameToCapture = _latestAnnotatedBgr ?? _currentFrameBgr;
            if (frameToCapture == null || frameToCapture.IsDisposed || frameToCapture.Empty())
            {
                MessageBox.Show(this, LocalizationManager.T("main.msg_no_frame_screenshot"), LocalizationManager.T("main.btn_screenshot"), MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            try
            {
                if (_picture.IsZoomed)
                {
                    OpenCvSharp.Rect roi = _picture.GetVisibleImageRoi(frameToCapture.Width, frameToCapture.Height);
                    if (roi.Width > 0 && roi.Height > 0)
                    {
                        using Mat zoomedRoi = new Mat(frameToCapture, roi);
                        string zoomPrefix = _isStreaming ? "StreamCapture_ZoomDetail" : "VideoCapture_ZoomDetail";
                        string savedZoomPath = ScreenshotHelper.SaveScreenshot(zoomedRoi, prefix: zoomPrefix);
                        ScreenshotHelper.CopyToClipboard(zoomedRoi);

                        _lblInfo.Text = $"📸 Saved Zoom Detail: {Path.GetFileName(savedZoomPath)} ({roi.Width}x{roi.Height}) (Copied to Clipboard)";
                        _lblInfo.ForeColor = Color.DarkGreen;
                        _lblInfo.Visible = true;
                        return;
                    }
                }

                string savedPath = ScreenshotHelper.SaveScreenshot(frameToCapture, prefix: _isStreaming ? "StreamCapture" : "VideoCapture");
                ScreenshotHelper.CopyToClipboard(frameToCapture);

                _lblInfo.Text = $"📸 Saved: {Path.GetFileName(savedPath)} (Copied to Clipboard)";
                _lblInfo.ForeColor = Color.DarkGreen;
                _lblInfo.Visible = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, LocalizationManager.T("main.msg_screenshot_error", ex.Message), LocalizationManager.T("common.error"), MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadVideo()
        {
            StopPlaying();

            using OpenFileDialog ofd = new()
            {
                Filter = LocalizationManager.T("main.dlg_open_video_filter"),
                Title = LocalizationManager.T("main.dlg_open_video_title")
            };

            if (ofd.ShowDialog(this) != DialogResult.OK)
            {
                return;
            }

            CleanupVideoOnly();

            try
            {
                _cap = new VideoCapture(ofd.FileName);
                if (!_cap.IsOpened())
                {
                    throw new InvalidOperationException("Failed to open video file with OpenCV.");
                }

                _cap.Set(VideoCaptureProperties.BufferSize, 1);
                _videoFilePath = ofd.FileName;
                _totalFrames = (int)_cap.Get(VideoCaptureProperties.FrameCount);
                _fps = _cap.Get(VideoCaptureProperties.Fps);
                if (_fps <= 0 || double.IsNaN(_fps))
                {
                    _fps = 30.0;
                }

                _lblVideoTitle.Text = Path.GetFileName(ofd.FileName);
                _lblVideoTitle.ForeColor = SystemColors.ControlText;

                _updatingTrackBar = true;
                _seek.Minimum = 0;
                _seek.Maximum = Math.Max(_totalFrames - 1, 0);
                _seek.Value = 0;
                _updatingTrackBar = false;

                _currentFrameIndex = 0;
                _isPlaying = false;
                _btnPlay.Text = LocalizationManager.T("main.btn_play");

                SeekTo(0, resetBackground: true);
                UpdateUiEnabled(true);
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, LocalizationManager.T("main.msg_load_error", ex.Message), LocalizationManager.T("common.error"), MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void CleanupVideoOnly()
        {
            StopPlaying();

            if (_isStreaming)
            {
                _isRenderingStreamFrame = false;
                _streamGrabber?.Stop();
                _streamGrabber?.Dispose();
                _streamGrabber = null;
                _isStreaming = false;
                _streamSourceUrl = null;
                _streamRenderedCount = 0;
                GC.Collect(2, GCCollectionMode.Forced, true);
            }

            _isRenderingVideoFrame = false;

            _latestAnnotatedBgr?.Dispose();
            _latestAnnotatedBgr = null;
            _boxTracker.Reset();

            lock (_capLock)
            {
                _cap?.Dispose();
                _cap = null;
            }

            _currentFrameBgr?.Dispose();
            _currentFrameBgr = null;

            _prevFrameGray?.Dispose();
            _prevFrameGray = null;

            _bgFloatGray?.Dispose();
            _bgFloatGray = null;

            Image? oldImage = _picture.Image;
            _picture.Image = null;
            oldImage?.Dispose();

            _videoFilePath = null;
            _totalFrames = 0;
            _fps = 30.0;
            _currentFrameIndex = 0;

            _lblCurrentTime.Text = "00:00.00";
            _lblTotalTime.Text = "00:00.00";
            _lblInfoResolution.Text = "-";
            _lblInfoFps.Text = "-";
            _lblInfoFrames.Text = "0 / 0";
            _lblInfoTime.Text = "00:00.00 / 00:00.00";
            _lblInfoRegions.Text = "0 regions";
            _lblVideoTitle.Text = "No video loaded. Click 'Open Video...' or 'Open Stream...' to begin.";
            _lblVideoTitle.ForeColor = SystemColors.ControlDarkDark;
        }

        private void Cleanup()
        {
            StopPlaying();

            _playCts?.Cancel();
            try { _playTask?.Wait(200); } catch { }
            _playCts?.Dispose();
            _playCts = null;

            _exportCts?.Cancel();
            _exportCts?.Dispose();
            _exportCts = null;

            CleanupVideoOnly();
        }

        private void TogglePlay()
        {
            if (_isStreaming)
            {
                if (_isPlaying)
                {
                    _isPlaying = false;
                    _btnPlay.Text = LocalizationManager.T("main.btn_play");
                }
                else
                {
                    if (_streamGrabber == null || !_streamGrabber.IsRunning)
                    {
                        if (!string.IsNullOrEmpty(_streamSourceUrl))
                        {
                            StartStreaming(_streamSourceUrl, _streamFormat, _streamAuth ?? new StreamAuthOptions(), _streamTitle ?? _streamSourceUrl);
                            return;
                        }
                    }

                    _isPlaying = true;
                    _btnPlay.Text = LocalizationManager.T("main.btn_pause");
                }
                return;
            }

            if (_isPlaying)
            {
                StopPlaying();
            }
            else
            {
                StartPlaying();
            }
        }

        private void StartPlaying()
        {
            if (_cap == null || _isPlaying)
            {
                return;
            }

            if (_currentFrameIndex >= _totalFrames - 1)
            {
                SeekTo(0, resetBackground: true);
            }

            _isPlaying = true;
            _btnPlay.Text = LocalizationManager.T("main.btn_pause");

            _playCts?.Cancel();
            try { _playTask?.Wait(200); } catch { }
            _playCts?.Dispose();
            _playCts = new CancellationTokenSource();
            CancellationToken token = _playCts.Token;

            _playTask = Task.Run(async () =>
            {
                int delayMs = (int)Math.Max(1.0, 1000.0 / _fps);
                while (!token.IsCancellationRequested && _isPlaying)
                {
                    if (_isRenderingVideoFrame)
                    {
                        await Task.Delay(5, token);
                        continue;
                    }

                    Mat nextFrame = new();
                    int frameIndex;

                    lock (_capLock)
                    {
                        if (_cap == null || !_cap.IsOpened() || !_isPlaying || token.IsCancellationRequested)
                        {
                            nextFrame.Dispose();
                            BeginInvoke(StopPlaying);
                            break;
                        }

                        if (!_cap.Read(nextFrame) || nextFrame.Empty())
                        {
                            nextFrame.Dispose();
                            BeginInvoke(StopPlaying);
                            break;
                        }

                        _currentFrameIndex++;
                        frameIndex = _currentFrameIndex;
                    }

                    if (frameIndex >= _totalFrames)
                    {
                        nextFrame.Dispose();
                        BeginInvoke(StopPlaying);
                        break;
                    }

                    _isRenderingVideoFrame = true;
                    BeginInvoke(() =>
                    {
                        try
                        {
                            if (!_isPlaying || _cap == null || token.IsCancellationRequested)
                            {
                                nextFrame.Dispose();
                                return;
                            }

                            _updatingTrackBar = true;
                            if (frameIndex <= _seek.Maximum)
                            {
                                _seek.Value = frameIndex;
                            }
                            _updatingTrackBar = false;

                            _currentFrameBgr?.Dispose();
                            _currentFrameBgr = nextFrame;

                            ProcessAndDisplayCurrentFrame();
                        }
                        catch (ObjectDisposedException)
                        {
                            nextFrame.Dispose();
                        }
                        finally
                        {
                            _isRenderingVideoFrame = false;
                        }
                    });

                    await Task.Delay(delayMs, token);
                }
            }, token);
        }

        private void StopPlaying()
        {
            if (!_isPlaying)
            {
                return;
            }

            _isPlaying = false;
            _btnPlay.Text = LocalizationManager.T("main.btn_play");
            if (!_isStreaming)
            {
                _playCts?.Cancel();
                try
                {
                    _playTask?.Wait(200);
                }
                catch
                {
                }
                _playTask = null;
            }
        }

        private void StopAndRewind()
        {
            if (_isStreaming)
            {
                _isPlaying = false;
                _btnPlay.Text = LocalizationManager.T("main.btn_play");
                _playCts?.Cancel();
                _streamGrabber?.Stop();
                _lblVideoTitle.Text = $"[STREAM STOPPED] {_streamTitle}";
                _lblVideoTitle.ForeColor = Color.DarkSlateGray;
                return;
            }

            StopPlaying();
            if (_cap != null)
            {
                SeekTo(0, resetBackground: true);
            }
        }

        private void StepFrames(int delta)
        {
            if (_isStreaming || _cap == null || _totalFrames == 0)
            {
                return;
            }

            if (_isPlaying)
            {
                StopPlaying();
            }

            int targetFrame = Math.Clamp(_currentFrameIndex + delta, 0, _totalFrames - 1);
            SeekTo(targetFrame, resetBackground: _chkResetBgOnSeek.Checked);
        }

        private void OnSeekTrackBarScrolled()
        {
            if (_isStreaming || _updatingTrackBar || _cap == null)
            {
                return;
            }

            if (_isPlaying)
            {
                StopPlaying();
            }

            SeekTo(_seek.Value, resetBackground: _chkResetBgOnSeek.Checked);
        }

        private void OnSeekTrackBarValueChanged()
        {
            if (_isStreaming || _updatingTrackBar || _cap == null)
            {
                return;
            }

            if (_isPlaying)
            {
                StopPlaying();
            }

            SeekTo(_seek.Value, resetBackground: _chkResetBgOnSeek.Checked);
        }

        private void OnPictureMouseWheel(object? sender, MouseEventArgs e)
        {
            if (ModifierKeys.HasFlag(Keys.Control))
            {
                return;
            }

            if (_isStreaming)
            {
                if (e.Delta > 0)
                {
                    _picture.ZoomIn(e.Location);
                }
                else if (e.Delta < 0)
                {
                    _picture.ZoomOut(e.Location);
                }
                return;
            }

            if (_cap == null || _totalFrames == 0)
            {
                return;
            }

            int step = 1;
            if (ModifierKeys.HasFlag(Keys.Shift))
            {
                step = (int)Math.Max(1.0, _fps);
            }

            int delta = e.Delta > 0 ? step : -step;
            StepFrames(delta);
        }

        private void OnFormKeyDown(object? sender, KeyEventArgs e)
        {
            if (e.Control && (e.KeyCode == Keys.Oemplus || e.KeyCode == Keys.Add))
            {
                _picture.ZoomIn();
                e.Handled = true;
                e.SuppressKeyPress = true;
                return;
            }
            if (e.Control && (e.KeyCode == Keys.OemMinus || e.KeyCode == Keys.Subtract))
            {
                _picture.ZoomOut();
                e.Handled = true;
                e.SuppressKeyPress = true;
                return;
            }
            if ((e.Control && (e.KeyCode == Keys.D0 || e.KeyCode == Keys.NumPad0)) || (e.KeyCode == Keys.Escape && _picture.IsZoomed))
            {
                _picture.ResetZoom();
                e.Handled = true;
                e.SuppressKeyPress = true;
                return;
            }

            if (_isStreaming)
            {
                if (e.KeyCode == Keys.Space)
                {
                    TogglePlay();
                    e.Handled = true;
                    e.SuppressKeyPress = true;
                }
                else if (e.KeyCode == Keys.S)
                {
                    GrabScreenshot();
                    e.Handled = true;
                    e.SuppressKeyPress = true;
                }
                return;
            }

            if (_cap == null || _totalFrames == 0)
            {
                return;
            }

            if (e.KeyCode == Keys.Space)
            {
                TogglePlay();
                e.Handled = true;
                e.SuppressKeyPress = true;
            }
            else if (e.KeyCode == Keys.S)
            {
                GrabScreenshot();
                e.Handled = true;
                e.SuppressKeyPress = true;
            }
            else if (e.KeyCode == Keys.Left)
            {
                int delta = e.Shift ? -(int)Math.Max(1.0, _fps) : -1;
                StepFrames(delta);
                e.Handled = true;
            }
            else if (e.KeyCode == Keys.Right)
            {
                int delta = e.Shift ? (int)Math.Max(1.0, _fps) : 1;
                StepFrames(delta);
                e.Handled = true;
            }
            else if (e.KeyCode == Keys.Home)
            {
                SeekTo(0, resetBackground: true);
                e.Handled = true;
            }
            else if (e.KeyCode == Keys.End)
            {
                SeekTo(_totalFrames - 1, resetBackground: true);
                e.Handled = true;
            }
        }

        private void OnParameterChanged()
        {
            if ((_cap != null || _isStreaming) && !_isPlaying && _currentFrameBgr != null)
            {
                ProcessAndDisplayCurrentFrame();
            }
        }

        private void SeekTo(int frameIndex, bool resetBackground)
        {
            if (_cap == null || _totalFrames == 0)
            {
                return;
            }

            frameIndex = Math.Clamp(frameIndex, 0, _totalFrames - 1);

            lock (_capLock)
            {
                if (!_cap.IsOpened())
                {
                    return;
                }

                if (frameIndex > 0)
                {
                    _cap.Set(VideoCaptureProperties.PosFrames, frameIndex - 1);
                    using Mat prev = new Mat();
                    if (_cap.Read(prev) && !prev.Empty())
                    {
                        _prevFrameGray?.Dispose();
                        _prevFrameGray = new Mat();
                        Cv2.CvtColor(prev, _prevFrameGray, ColorConversionCodes.BGR2GRAY);
                    }
                }
                else
                {
                    _prevFrameGray?.Dispose();
                    _prevFrameGray = null;
                }

                _cap.Set(VideoCaptureProperties.PosFrames, frameIndex);
                _currentFrameIndex = frameIndex;

                Mat frame = new Mat();
                if (!_cap.Read(frame) || frame.Empty())
                {
                    frame.Dispose();
                    return;
                }

                _currentFrameBgr?.Dispose();
                _currentFrameBgr = frame;
            }

            _updatingTrackBar = true;
            if (_seek.Value != frameIndex)
            {
                _seek.Value = frameIndex;
            }
            _updatingTrackBar = false;

            if (resetBackground)
            {
                ResetBackgroundModelToFrame(_currentFrameBgr);
            }

            ProcessAndDisplayCurrentFrame();
        }

        private void ResetBackgroundModelToFrame(Mat frameBgr)
        {
            if (frameBgr == null || frameBgr.IsDisposed || frameBgr.Empty())
            {
                return;
            }

            _bgFloatGray?.Dispose();
            _bgFloatGray = new Mat();
            _boxTracker.Reset();

            using Mat gray = new();
            Cv2.CvtColor(frameBgr, gray, ColorConversionCodes.BGR2GRAY);
            gray.ConvertTo(_bgFloatGray, MatType.CV_32F);
        }

        private void ResetBackgroundToCurrentFrame()
        {
            if (_currentFrameBgr == null || _currentFrameBgr.IsDisposed || _currentFrameBgr.Empty())
            {
                return;
            }

            ResetBackgroundModelToFrame(_currentFrameBgr);
            ProcessAndDisplayCurrentFrame();
        }

        private void ProcessAndDisplayCurrentFrame()
        {
            try
            {
                if (_currentFrameBgr == null || _currentFrameBgr.IsDisposed || _currentFrameBgr.Empty())
                {
                    return;
                }

                // Convert frame to grayscale
                using Mat frameGray = new();
                Cv2.CvtColor(_currentFrameBgr, frameGray, ColorConversionCodes.BGR2GRAY);

                // Initialize or update background
                if (_bgFloatGray == null)
                {
                    _bgFloatGray = new Mat();
                    frameGray.ConvertTo(_bgFloatGray, MatType.CV_32F);
                }
                else
                {
                    double learnRate = (double)_nudLearnRate.Value / 100.0;
                    if (learnRate > 0)
                    {
                        using Mat frameFloat = new();
                        frameGray.ConvertTo(frameFloat, MatType.CV_32F);
                        Cv2.AccumulateWeighted(frameFloat, _bgFloatGray, learnRate, null!);
                    }
                }

                // Convert background model to byte
                using Mat bgGray = new();
                _bgFloatGray.ConvertTo(bgGray, MatType.CV_8U);

                // Difference
                using Mat diff = new();
                Cv2.Absdiff(frameGray, bgGray, diff);

                // Threshold
                int thresh = (int)_nudThreshold.Value;
                using Mat mask = new();
                Cv2.Threshold(diff, mask, thresh, 255, ThresholdTypes.Binary);

                // Blur
                int blurKernel = (int)_nudBlur.Value;
                if (blurKernel % 2 == 0)
                {
                    blurKernel++;
                }
                if (blurKernel < 1)
                {
                    blurKernel = 1;
                }

                using Mat blurred = new();
                if (blurKernel > 1)
                {
                    Cv2.GaussianBlur(mask, blurred, new OpenCvSharp.Size(blurKernel, blurKernel), 0);
                }
                else
                {
                    mask.CopyTo(blurred);
                }

                // Re-threshold
                using Mat binary = new Mat();
                Cv2.Threshold(blurred, binary, 127, 255, ThresholdTypes.Binary);

                // Active motion mask to eliminate ghost trails from bounding boxes
                using Mat boxBinary = new Mat();
                if (_chkIgnoreTrails.Checked && _prevFrameGray != null && !_prevFrameGray.Empty())
                {
                    using Mat frameDiff = new Mat();
                    Cv2.Absdiff(frameGray, _prevFrameGray, frameDiff);

                    using Mat diffMask = new Mat();
                    Cv2.Threshold(frameDiff, diffMask, Math.Max(5, thresh / 3), 255, ThresholdTypes.Binary);

                    using Mat kernel = Cv2.GetStructuringElement(MorphShapes.Rect, new OpenCvSharp.Size(15, 15));
                    using Mat dilatedDiff = new Mat();
                    Cv2.Dilate(diffMask, dilatedDiff, kernel);
                    Cv2.MorphologyEx(dilatedDiff, dilatedDiff, MorphTypes.Close, kernel);

                    Cv2.BitwiseAnd(binary, dilatedDiff, boxBinary);
                }
                else
                {
                    binary.CopyTo(boxBinary);
                }

                // Find contours
                Cv2.FindContours(boxBinary, out OpenCvSharp.Point[][] contours, out HierarchyIndex[] _, RetrievalModes.External, ContourApproximationModes.ApproxSimple);

                double minArea = Math.Max(50.0, (double)_nudMinArea.Value);
                List<Rect> motionRects = new List<Rect>();

                foreach (OpenCvSharp.Point[] cnt in contours)
                {
                    double area = Cv2.ContourArea(cnt);
                    if (area >= minArea)
                    {
                        Rect rect = Cv2.BoundingRect(cnt);
                        motionRects.Add(rect);
                    }
                }

                if (motionRects.Count > 100)
                {
                    motionRects = motionRects.OrderByDescending(r => r.Width * r.Height).Take(100).ToList();
                }

                // Update tracked bounding boxes for smooth persistence and fading
                _boxTracker.Configure((int)_nudBoxHold.Value);
                var trackedBoxes = _boxTracker.Update(motionRects);

                // Draw on frame
                if (_currentFrameBgr == null || _currentFrameBgr.IsDisposed || _currentFrameBgr.Empty())
                {
                    return;
                }

                using Mat displayFrame = _currentFrameBgr.Clone();

                // Red overlay
                if (_chkOverlay.Checked)
                {
                    using Mat redMask = new Mat();
                    if (_chkTrailsInOverlay.Checked)
                    {
                        binary.CopyTo(redMask);
                    }
                    else
                    {
                        boxBinary.CopyTo(redMask);
                    }

                    double alpha = (double)_nudAlpha.Value / 100.0;
                    using Mat colorMask = new Mat(redMask.Size(), MatType.CV_8UC3, new Scalar(0, 0, 255));
                    using Mat colorMaskFinal = new Mat();
                    colorMask.CopyTo(colorMaskFinal, redMask);

                    Cv2.AddWeighted(displayFrame, 1.0, colorMaskFinal, alpha, 0, displayFrame);
                }

                // Bounding boxes with smooth persistence and fading border
                if (_chkBoxes.Checked)
                {
                    foreach (var (box, opacity) in trackedBoxes)
                    {
                        MotionBoxTracker.DrawFadingBoundingBox(displayFrame, box, opacity, 2);
                    }
                }

                _latestAnnotatedBgr?.Dispose();
                _latestAnnotatedBgr = displayFrame.Clone();

                Bitmap bmp = BitmapConverter.ToBitmap(displayFrame);
                Image? oldImage = _picture.Image;
                _picture.Image = bmp;
                oldImage?.Dispose();

                _prevFrameGray?.Dispose();
                _prevFrameGray = frameGray.Clone();

                UpdateMetricsDisplay(trackedBoxes.Count);
            }
            catch (ObjectDisposedException)
            {
                // Frame or image was disposed during video switch or teardown
            }
        }

        private void UpdateMetricsDisplay(int motionCount)
        {
            if (_isStreaming)
            {
                double streamSeconds = _streamGrabber?.ElapsedStreamTime.TotalSeconds ?? 0;
                long frames = _streamGrabber?.FramesReceived ?? 0;
                double streamFps = _streamGrabber?.Fps ?? _fps;

                _lblCurrentTime.Text = "LIVE";
                _lblTotalTime.Text = FormatTime(streamSeconds);

                if (_currentFrameBgr != null)
                {
                    _lblInfoResolution.Text = $"{_currentFrameBgr.Width} × {_currentFrameBgr.Height}";
                }
                _lblInfoFps.Text = $"{streamFps:F2} fps (live)";
                _lblInfoFrames.Text = $"{frames} frames";
                _lblInfoTime.Text = $"{FormatTime(streamSeconds)} (LIVE)";

                _lblInfoRegions.Text = LocalizationManager.T("main.metric_motion_count", motionCount);
                _lblInfoRegions.ForeColor = motionCount > 0 ? Color.DarkRed : Color.DarkGreen;
                return;
            }

            double currentSeconds = _currentFrameIndex / _fps;
            double totalSeconds = _totalFrames / _fps;

            _lblCurrentTime.Text = FormatTime(currentSeconds);
            _lblTotalTime.Text = FormatTime(totalSeconds);

            if (_currentFrameBgr != null)
            {
                _lblInfoResolution.Text = $"{_currentFrameBgr.Width} × {_currentFrameBgr.Height}";
            }
            _lblInfoFps.Text = $"{_fps:F2} fps";
            _lblInfoFrames.Text = $"{_currentFrameIndex + 1} / {_totalFrames}";
            _lblInfoTime.Text = $"{FormatTime(currentSeconds)} / {FormatTime(totalSeconds)}";

            _lblInfoRegions.Text = LocalizationManager.T("main.metric_motion_count", motionCount);
            _lblInfoRegions.ForeColor = motionCount > 0 ? Color.DarkRed : Color.DarkGreen;
        }

        private static string FormatTime(double seconds)
        {
            if (seconds < 0 || double.IsNaN(seconds) || double.IsInfinity(seconds))
            {
                seconds = 0;
            }

            TimeSpan ts = TimeSpan.FromSeconds(seconds);
            return $"{(int)ts.TotalMinutes:D2}:{ts.Seconds:D2}.{ts.Milliseconds / 10:D2}";
        }

        private void CancelExport()
        {
            _exportCts?.Cancel();
        }

        private async Task ExportAnnotatedAsync()
        {
            if (string.IsNullOrEmpty(_videoFilePath) || _totalFrames == 0)
            {
                return;
            }

            using SaveFileDialog sfd = new()
            {
                Filter = LocalizationManager.T("main.dlg_save_export_filter"),
                Title = LocalizationManager.T("main.dlg_save_export_title"),
                FileName = Path.GetFileNameWithoutExtension(_videoFilePath) + "_annotated.mp4"
            };

            if (sfd.ShowDialog(this) != DialogResult.OK)
            {
                return;
            }

            StopPlaying();

            int savedFrameIndex = _currentFrameIndex;
            string exportFilePath = sfd.FileName;
            string sourceFilePath = _videoFilePath;
            double exportFps = _fps;

            // Read settings before going to background task
            double learnRate = (double)_nudLearnRate.Value / 100.0;
            int thresh = (int)_nudThreshold.Value;
            int blurKernel = (int)_nudBlur.Value;
            if (blurKernel % 2 == 0)
            {
                blurKernel++;
            }
            if (blurKernel < 1)
            {
                blurKernel = 1;
            }
            double minArea = Math.Max(50.0, (double)_nudMinArea.Value);
            bool drawOverlay = _chkOverlay.Checked;
            bool showTrails = _chkTrailsInOverlay.Checked;
            double alpha = (double)_nudAlpha.Value / 100.0;
            bool drawBoxes = _chkBoxes.Checked;
            bool filterTrails = _chkIgnoreTrails.Checked;
            int boxHold = (int)_nudBoxHold.Value;

            // UI state for exporting
            _btnExport.Enabled = false;
            _btnCancelExport.Visible = true;
            _progressBar.Visible = true;
            _progressBar.Value = 0;
            _progressBar.Maximum = _totalFrames;
            UpdateUiEnabled(false);

            _exportCts?.Cancel();
            _exportCts?.Dispose();
            _exportCts = new CancellationTokenSource();
            CancellationToken token = _exportCts.Token;

            bool success = false;
            string? errorMessage = null;

            try
            {
                await Task.Run(() =>
                {
                    using VideoCapture reader = new(sourceFilePath);
                    if (!reader.IsOpened())
                    {
                        throw new InvalidOperationException("Failed to open source video for export.");
                    }

                    int width = (int)reader.Get(VideoCaptureProperties.FrameWidth);
                    int height = (int)reader.Get(VideoCaptureProperties.FrameHeight);
                    int totalFrames = (int)reader.Get(VideoCaptureProperties.FrameCount);
                    FourCC codec = FourCC.FromString("mp4v");

                    using VideoWriter writer = new(exportFilePath, codec, exportFps, new OpenCvSharp.Size(width, height));
                    if (!writer.IsOpened())
                    {
                        throw new InvalidOperationException("Failed to initialize VideoWriter for target file.");
                    }

                    Mat? bgModel = null;
                    Mat? prevFrameGray = null;
                    MotionBoxTracker exportTracker = new();
                    exportTracker.Configure(boxHold);
                    try
                    {
                        for (int i = 0; i < totalFrames; i++)
                        {
                            if (token.IsCancellationRequested)
                            {
                                token.ThrowIfCancellationRequested();
                            }

                            using Mat frameBgr = new Mat();
                            if (!reader.Read(frameBgr) || frameBgr.Empty())
                            {
                                break;
                            }

                            using Mat frameGray = new Mat();
                            Cv2.CvtColor(frameBgr, frameGray, ColorConversionCodes.BGR2GRAY);

                            if (bgModel == null)
                            {
                                bgModel = new Mat();
                                frameGray.ConvertTo(bgModel, MatType.CV_32F);
                            }
                            else if (learnRate > 0)
                            {
                                using Mat frameFloat = new Mat();
                                frameGray.ConvertTo(frameFloat, MatType.CV_32F);
                                Cv2.AccumulateWeighted(frameFloat, bgModel, learnRate, null!);
                            }

                            using Mat bgGray = new Mat();
                            bgModel.ConvertTo(bgGray, MatType.CV_8U);

                            using Mat diff = new Mat();
                            Cv2.Absdiff(frameGray, bgGray, diff);

                            using Mat mask = new Mat();
                            Cv2.Threshold(diff, mask, thresh, 255, ThresholdTypes.Binary);

                            using Mat blurred = new Mat();
                            if (blurKernel > 1)
                            {
                                Cv2.GaussianBlur(mask, blurred, new OpenCvSharp.Size(blurKernel, blurKernel), 0);
                            }
                            else
                            {
                                mask.CopyTo(blurred);
                            }

                            using Mat binary = new Mat();
                            Cv2.Threshold(blurred, binary, 127, 255, ThresholdTypes.Binary);

                            using Mat boxBinary = new Mat();
                            if (filterTrails && prevFrameGray != null && !prevFrameGray.Empty())
                            {
                                using Mat frameDiff = new Mat();
                                Cv2.Absdiff(frameGray, prevFrameGray, frameDiff);

                                using Mat diffMask = new Mat();
                                Cv2.Threshold(frameDiff, diffMask, Math.Max(5, thresh / 3), 255, ThresholdTypes.Binary);

                                using Mat kernel = Cv2.GetStructuringElement(MorphShapes.Rect, new OpenCvSharp.Size(15, 15));
                                using Mat dilatedDiff = new Mat();
                                Cv2.Dilate(diffMask, dilatedDiff, kernel);
                                Cv2.MorphologyEx(dilatedDiff, dilatedDiff, MorphTypes.Close, kernel);

                                Cv2.BitwiseAnd(binary, dilatedDiff, boxBinary);
                            }
                            else
                            {
                                binary.CopyTo(boxBinary);
                            }

                            Cv2.FindContours(boxBinary, out OpenCvSharp.Point[][] contours, out HierarchyIndex[] _, RetrievalModes.External, ContourApproximationModes.ApproxSimple);

                            List<Rect> motionRects = new List<Rect>();
                            foreach (OpenCvSharp.Point[] cnt in contours)
                            {
                                double area = Cv2.ContourArea(cnt);
                                if (area >= minArea)
                                {
                                    Rect rect = Cv2.BoundingRect(cnt);
                                    motionRects.Add(rect);
                                }
                            }

                            if (motionRects.Count > 100)
                            {
                                motionRects = motionRects.OrderByDescending(r => r.Width * r.Height).Take(100).ToList();
                            }

                            using Mat outputFrame = frameBgr.Clone();

                            if (drawOverlay)
                            {
                                using Mat redMask = new Mat();
                                if (showTrails)
                                {
                                    binary.CopyTo(redMask);
                                }
                                else
                                {
                                    boxBinary.CopyTo(redMask);
                                }

                                using Mat colorMask = new Mat(redMask.Size(), MatType.CV_8UC3, new Scalar(0, 0, 255));
                                using Mat colorMaskFinal = new Mat();
                                colorMask.CopyTo(colorMaskFinal, redMask);

                                Cv2.AddWeighted(outputFrame, 1.0, colorMaskFinal, alpha, 0, outputFrame);
                            }

                            var trackedBoxes = exportTracker.Update(motionRects);

                            if (drawBoxes)
                            {
                                foreach (var (box, opacity) in trackedBoxes)
                                {
                                    MotionBoxTracker.DrawFadingBoundingBox(outputFrame, box, opacity, 2);
                                }
                            }

                            writer.Write(outputFrame);

                            prevFrameGray?.Dispose();
                            prevFrameGray = frameGray.Clone();

                            if (i % 15 == 0 || i == totalFrames - 1)
                            {
                                int processed = i + 1;
                                BeginInvoke(() =>
                                {
                                    if (_progressBar.Visible && processed <= _progressBar.Maximum)
                                    {
                                        _progressBar.Value = processed;
                                    }
                                    _lblVideoTitle.Text = $"Exporting: {processed} / {totalFrames} frames ({(processed * 100 / totalFrames)}%)...";
                                });
                            }
                        }
                    }
                    finally
                    {
                        prevFrameGray?.Dispose();
                        bgModel?.Dispose();
                    }

                    success = true;
                }, token);
            }
            catch (OperationCanceledException)
            {
                errorMessage = LocalizationManager.T("main.msg_export_cancelled");
            }
            catch (Exception ex)
            {
                errorMessage = $"Export error:\n{ex.Message}";
            }
            finally
            {
                _progressBar.Visible = false;
                _btnCancelExport.Visible = false;
                _btnExport.Enabled = true;
                UpdateUiEnabled(true);

                if (!string.IsNullOrEmpty(_videoFilePath))
                {
                    _lblVideoTitle.Text = Path.GetFileName(_videoFilePath);
                }

                SeekTo(savedFrameIndex, resetBackground: true);

                if (success)
                {
                    MessageBox.Show(this, LocalizationManager.T("main.msg_export_complete"), LocalizationManager.T("main.grp_export"), MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else if (errorMessage != null)
                {
                    MessageBox.Show(this, errorMessage, "Export", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
        }
    }
}
