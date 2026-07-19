using OpenCvSharp;
using OpenCvSharp.Extensions;

namespace MotionHighlighter
{
    public partial class FormMain : Form
    {
        // Video
        private VideoCapture? _cap;
        private int _totalFrames;
        private double _fps;
        private int _currentFrameIndex;
        private bool _isPlaying;
        private bool _isSeeking;
        private CancellationTokenSource? _playCts;

        // Background model (float grayscale for running average)
        private Mat? _bgFloatGray; // CV_32F 1-channel
        private readonly object _capLock = new();

        public FormMain()
        {
            InitializeComponent();
        
            _btnLoad.Click += (_, _) => LoadVideo();
            _btnPlay.Click += (_, _) => TogglePlay();
            _btnResetBg.Click += (_, _) => ResetBackgroundToCurrentFrame();
            _btnExport.Click += async (_, _) => await ExportAnnotatedAsync();

            _seek.MouseDown += (_, _) => _isSeeking = true;
            _seek.MouseUp += (_, _) =>
            {
                _isSeeking = false;
                SeekTo(_seek.Value);
            };
            _seek.Scroll += (_, _) =>
            {
                if (_isSeeking && !_isPlaying)
                {
                    ShowFrameAt(_seek.Value);
                }
            };

            FormClosing += (_, _) => Cleanup();
            UpdateUiEnabled(false);
        }

        private void UpdateUiEnabled(bool enabled)
        {
            _btnPlay.Enabled = enabled;
            _btnResetBg.Enabled = enabled;
            _btnExport.Enabled = enabled;
            _seek.Enabled = enabled;
        }

        private void LoadVideo()
        {
            using OpenFileDialog ofd = new()
            {
                Filter = "Video Files (*.mp4;*.mov;*.mkv;*.avi)|*.mp4;*.mov;*.mkv;*.avi|All Files (*.*)|*.*",
                Title = "Choose a video file"
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
                    throw new Exception("Failed to open video.");
                }
                _cap.Set(VideoCaptureProperties.BufferSize, 1);

                _totalFrames = (int)_cap.Get(VideoCaptureProperties.FrameCount);
                _fps = _cap.Get(VideoCaptureProperties.Fps);
                if (_fps <= 0)
                {
                    _fps = 30;
                }

                _seek.Minimum = 0;
                _seek.Maximum = Math.Max(_totalFrames - 1, 0);
                _seek.Value = 0;
                _currentFrameIndex = 0;

                // Initialize background model
                _bgFloatGray?.Dispose();
                _bgFloatGray = null;

                SeekTo(0);
                UpdateUiEnabled(true);
                _btnPlay.Text = "Play";
                _isPlaying = false;
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, $"Error loading video:\n{ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void CleanupVideoOnly()
        {
            StopPlaying();
            lock (_capLock)
            {
                _cap?.Dispose();
                _cap = null;
            }
            _bgFloatGray?.Dispose();
            _bgFloatGray = null;
            _picture.Image?.Dispose();
            _picture.Image = null;
        }

        private void Cleanup()
        {
            CleanupVideoOnly();
        }

        private void TogglePlay()
        {
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

            _isPlaying = true;
            _btnPlay.Text = "Pause";
            _playCts?.Cancel();
            _playCts = new CancellationTokenSource();
            var token = _playCts.Token;

            Task.Run(async () =>
            {
                try
                {
                    var delayMs = (int)(1000.0 / _fps);
                    while (!token.IsCancellationRequested && _isPlaying)
                    {
                        var nextIdx = _currentFrameIndex + 1;
                        if (nextIdx >= _totalFrames)
                        {
                            BeginInvoke(StopPlaying);
                            break;
                        }

                        BeginInvoke(() => SeekTo(nextIdx));
                        await Task.Delay(delayMs, token);
                    }
                }
                catch (OperationCanceledException)
                {
                    // expected
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
            _btnPlay.Text = "Play";
            _playCts?.Cancel();
        }

        private void SeekTo(int frameIndex)
        {
            if (_cap == null)
            {
                return;
            }

            lock (_capLock)
            {
                if (!_cap.IsOpened())
                {
                    return;
                }

                _cap.Set(VideoCaptureProperties.PosFrames, frameIndex);
                _currentFrameIndex = frameIndex;
            }

            ShowFrameAt(frameIndex);
        }

        private void ShowFrameAt(int frameIndex)
        {
            if (_cap == null)
            {
                return;
            }

            using var frameBgr = new Mat();
            lock (_capLock)
            {
                if (!_cap.IsOpened())
                {
                    return;
                }

                if (!_cap.Read(frameBgr) || frameBgr.Empty())
                {
                    return;
                }
            }

            _currentFrameIndex = frameIndex;
            if (_seek.Value != frameIndex)
            {
                _seek.Value = frameIndex;
            }

            // Convert frame to grayscale
            using var frameGray = new Mat();
            Cv2.CvtColor(frameBgr, frameGray, ColorConversionCodes.BGR2GRAY);

            // Initialize or update background
            if (_bgFloatGray == null)
            {
                _bgFloatGray = new Mat();
                frameGray.ConvertTo(_bgFloatGray, MatType.CV_32F);
            }
            else
            {
                var learnRate = (double)_nudLearnRate.Value / 100.0;
                if (learnRate > 0)
                {
                    using var frameFloat = new Mat();
                    frameGray.ConvertTo(frameFloat, MatType.CV_32F);
                    Cv2.AccumulateWeighted(frameFloat, _bgFloatGray, learnRate, null);
                }
            }

            // Convert background model to byte
            using var bgGray = new Mat();
            _bgFloatGray.ConvertTo(bgGray, MatType.CV_8U);

            // Difference
            using var diff = new Mat();
            Cv2.Absdiff(frameGray, bgGray, diff);

            // Threshold
            var thresh = (int)_nudThreshold.Value;
            using var mask = new Mat();
            Cv2.Threshold(diff, mask, thresh, 255, ThresholdTypes.Binary);

            // Blur
            var blurKernel = (int)_nudBlur.Value;
            if (blurKernel % 2 == 0)
            {
                blurKernel++;
            }
            if (blurKernel < 1)
            {
                blurKernel = 1;
            }

            using var blurred = new Mat();
            if (blurKernel > 1)
            {
                Cv2.GaussianBlur(mask, blurred, new OpenCvSharp.Size(blurKernel, blurKernel), 0);
            }
            else
            {
                mask.CopyTo(blurred);
            }

            // Re-threshold
            using var binary = new Mat();
            Cv2.Threshold(blurred, binary, 127, 255, ThresholdTypes.Binary);

            // Find contours
            Cv2.FindContours(binary, out var contours, out _, RetrievalModes.External, ContourApproximationModes.ApproxSimple);

            var minArea = (double)_nudMinArea.Value;
            var motionRects = new System.Collections.Generic.List<Rect>();

            foreach (var cnt in contours)
            {
                var area = Cv2.ContourArea(cnt);
                if (area >= minArea)
                {
                    var rect = Cv2.BoundingRect(cnt);
                    motionRects.Add(rect);
                }
            }

            // Draw on frame
            using var displayFrame = frameBgr.Clone();

            // Red overlay
            if (_chkOverlay.Checked)
            {
                using var redMask = new Mat();
                binary.CopyTo(redMask);

                var alpha = (double)_nudAlpha.Value / 100.0;
                using var colorMask = new Mat(redMask.Size(), MatType.CV_8UC3, new Scalar(0, 0, 255));
                using var colorMaskFinal = new Mat();
                colorMask.CopyTo(colorMaskFinal, redMask);

                Cv2.AddWeighted(displayFrame, 1.0, colorMaskFinal, alpha, 0, displayFrame);
            }

            // Bounding boxes
            if (_chkBoxes.Checked)
            {
                foreach (var r in motionRects)
                {
                    Cv2.Rectangle(displayFrame, r, new Scalar(0, 255, 0), 2);
                }
            }

            var bmp = BitmapConverter.ToBitmap(displayFrame);
            var old = _picture.Image;
            _picture.Image = bmp;
            old?.Dispose();

            _lblInfo.Text = $"Frame {_currentFrameIndex + 1}/{_totalFrames} | Regions: {motionRects.Count}";
        }

        private void ResetBackgroundToCurrentFrame()
        {
            if (_cap == null)
            {
                return;
            }

            using var frameBgr = new Mat();
            lock (_capLock)
            {
                if (!_cap.IsOpened())
                {
                    return;
                }

                var savedPos = (int)_cap.Get(VideoCaptureProperties.PosFrames);
                _cap.Set(VideoCaptureProperties.PosFrames, _currentFrameIndex);
                if (!_cap.Read(frameBgr) || frameBgr.Empty())
                {
                    _cap.Set(VideoCaptureProperties.PosFrames, savedPos);
                    return;
                }
                _cap.Set(VideoCaptureProperties.PosFrames, savedPos);
            }

            using var frameGray = new Mat();
            Cv2.CvtColor(frameBgr, frameGray, ColorConversionCodes.BGR2GRAY);

            _bgFloatGray?.Dispose();
            _bgFloatGray = new Mat();
            frameGray.ConvertTo(_bgFloatGray, MatType.CV_32F);

            ShowFrameAt(_currentFrameIndex);
        }

        private async Task ExportAnnotatedAsync()
        {
            if (_cap == null || _totalFrames == 0)
            {
                return;
            }

            using SaveFileDialog sfd = new()
            {
                Filter = "MP4 Video|*.mp4",
                Title = "Save Annotated Video",
                FileName = "annotated_output.mp4"
            };

            if (sfd.ShowDialog(this) != DialogResult.OK)
            {
                return;
            }

            var wasPlaying = _isPlaying;
            if (wasPlaying)
            {
                StopPlaying();
            }

            var savedFrameIdx = _currentFrameIndex;

            try
            {
                var w = (int)_cap.Get(VideoCaptureProperties.FrameWidth);
                var h = (int)_cap.Get(VideoCaptureProperties.FrameHeight);
                var codec = VideoWriter.FourCC('m', 'p', '4', 'v');

                using var writer = new VideoWriter(sfd.FileName, codec, _fps, new OpenCvSharp.Size(w, h));
                if (!writer.IsOpened())
                {
                    throw new Exception("Failed to open VideoWriter.");
                }

                // Reset background model
                _bgFloatGray?.Dispose();
                _bgFloatGray = null;

                for (var i = 0; i < _totalFrames; i++)
                {
                    using var frameBgr = new Mat();
                    lock (_capLock)
                    {
                        _cap.Set(VideoCaptureProperties.PosFrames, i);
                        if (!_cap.Read(frameBgr) || frameBgr.Empty())
                        {
                            break;
                        }
                    }

                    _currentFrameIndex = i;

                    // Convert frame to grayscale
                    using var frameGray = new Mat();
                    Cv2.CvtColor(frameBgr, frameGray, ColorConversionCodes.BGR2GRAY);

                    // Initialize or update background
                    if (_bgFloatGray == null)
                    {
                        _bgFloatGray = new Mat();
                        frameGray.ConvertTo(_bgFloatGray, MatType.CV_32F);
                    }
                    else
                    {
                        var learnRate = (double)_nudLearnRate.Value / 100.0;
                        if (learnRate > 0)
                        {
                            using var frameFloat = new Mat();
                            frameGray.ConvertTo(frameFloat, MatType.CV_32F);
                            Cv2.AccumulateWeighted(frameFloat, _bgFloatGray, learnRate, null);
                        }
                    }

                    // Convert background model to byte
                    using var bgGray = new Mat();
                    _bgFloatGray.ConvertTo(bgGray, MatType.CV_8U);

                    // Difference
                    using var diff = new Mat();
                    Cv2.Absdiff(frameGray, bgGray, diff);

                    // Threshold
                    var thresh = (int)_nudThreshold.Value;
                    using var mask = new Mat();
                    Cv2.Threshold(diff, mask, thresh, 255, ThresholdTypes.Binary);

                    // Blur
                    var blurKernel = (int)_nudBlur.Value;
                    if (blurKernel % 2 == 0)
                    {
                        blurKernel++;
                    }
                    if (blurKernel < 1)
                    {
                        blurKernel = 1;
                    }

                    using var blurred = new Mat();
                    if (blurKernel > 1)
                    {
                        Cv2.GaussianBlur(mask, blurred, new OpenCvSharp.Size(blurKernel, blurKernel), 0);
                    }
                    else
                    {
                        mask.CopyTo(blurred);
                    }

                    // Re-threshold
                    using var binary = new Mat();
                    Cv2.Threshold(blurred, binary, 127, 255, ThresholdTypes.Binary);

                    // Find contours
                    Cv2.FindContours(binary, out var contours, out _, RetrievalModes.External, ContourApproximationModes.ApproxSimple);

                    var minArea = (double)_nudMinArea.Value;
                    var motionRects = new System.Collections.Generic.List<Rect>();

                    foreach (var cnt in contours)
                    {
                        var area = Cv2.ContourArea(cnt);
                        if (area >= minArea)
                        {
                            var rect = Cv2.BoundingRect(cnt);
                            motionRects.Add(rect);
                        }
                    }

                    // Draw on frame
                    using var displayFrame = frameBgr.Clone();

                    // Red overlay
                    if (_chkOverlay.Checked)
                    {
                        using var redMask = new Mat();
                        binary.CopyTo(redMask);

                        var alpha = (double)_nudAlpha.Value / 100.0;
                        using var colorMask = new Mat(redMask.Size(), MatType.CV_8UC3, new Scalar(0, 0, 255));
                        using var colorMaskFinal = new Mat();
                        colorMask.CopyTo(colorMaskFinal, redMask);

                        Cv2.AddWeighted(displayFrame, 1.0, colorMaskFinal, alpha, 0, displayFrame);
                    }

                    // Bounding boxes
                    if (_chkBoxes.Checked)
                    {
                        foreach (var r in motionRects)
                        {
                            Cv2.Rectangle(displayFrame, r, new Scalar(0, 255, 0), 2);
                        }
                    }

                    writer.Write(displayFrame);

                    if (i % 10 == 0)
                    {
                        BeginInvoke(() => _lblInfo.Text = $"Exporting... {i + 1}/{_totalFrames}");
                        await Task.Delay(1);
                    }
                }

                MessageBox.Show(this, "Export complete!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, $"Export error:\n{ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                SeekTo(savedFrameIdx);
            }
        }
    }
}
