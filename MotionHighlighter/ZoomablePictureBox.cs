using System.ComponentModel;
using System.Drawing.Drawing2D;
using OpenCvSharp;

namespace MotionHighlighter
{
    /// <summary>
    /// Double-buffered, hardware-accelerated picture box supporting anchored zooming (1x - 10x),
    /// interactive mouse panning, interpolation switching (Bicubic at low zoom, Nearest Neighbor at high zoom),
    /// and a mini-map HUD navigator.
    /// </summary>
    public class ZoomablePictureBox : PictureBox
    {
        private double _zoomLevel = 1.0;
        private PointF _zoomCenterNorm = new PointF(0.5f, 0.5f);
        private bool _isPanning = false;
        private System.Drawing.Point _panStartPoint;
        private PointF _panStartCenterNorm;

        public const double MinZoom = 1.0;
        public const double MaxZoom = 10.0;
        public const double ZoomStepFactor = 1.25;

        public event EventHandler? ZoomChanged;

        public ZoomablePictureBox()
        {
            // Enable double buffering and custom paint to eliminate flicker completely during 30+ FPS video
            SetStyle(ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.UserPaint |
                     ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.ResizeRedraw, true);
            UpdateStyles();
            SizeMode = PictureBoxSizeMode.Zoom;
        }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public double ZoomLevel
        {
            get => _zoomLevel;
            set => SetZoom(value, null);
        }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public PointF ZoomCenterNorm
        {
            get => _zoomCenterNorm;
            set
            {
                if (Image == null)
                {
                    _zoomCenterNorm = new PointF(0.5f, 0.5f);
                    return;
                }

                _zoomCenterNorm = ZoomMath.ClampCenter(ClientSize.Width, ClientSize.Height, Image.Width, Image.Height, _zoomLevel, value);
                Invalidate();
            }
        }

        [Browsable(false)]
        public bool IsZoomed => _zoomLevel > 1.01;

        [Browsable(false)]
        public bool IsPanning => _isPanning;

        public void ZoomIn(System.Drawing.Point? screenAnchor = null)
        {
            SetZoom(_zoomLevel * ZoomStepFactor, screenAnchor);
        }

        public void ZoomOut(System.Drawing.Point? screenAnchor = null)
        {
            SetZoom(_zoomLevel / ZoomStepFactor, screenAnchor);
        }

        public void ResetZoom()
        {
            _zoomLevel = 1.0;
            _zoomCenterNorm = new PointF(0.5f, 0.5f);
            Cursor = Cursors.Default;
            Invalidate();
            ZoomChanged?.Invoke(this, EventArgs.Empty);
        }

        public void SetZoom(double targetZoom, System.Drawing.Point? screenAnchor = null)
        {
            double newZoom = Math.Clamp(targetZoom, MinZoom, MaxZoom);
            // Snap to 1.0 if very close
            if (Math.Abs(newZoom - 1.0) < 0.03)
            {
                newZoom = 1.0;
            }

            if (Math.Abs(newZoom - _zoomLevel) < 0.001)
            {
                return;
            }

            if (Image == null)
            {
                _zoomLevel = newZoom;
                _zoomCenterNorm = new PointF(0.5f, 0.5f);
                ZoomChanged?.Invoke(this, EventArgs.Empty);
                Invalidate();
                return;
            }

            System.Drawing.Point anchor = screenAnchor ?? new System.Drawing.Point(ClientSize.Width / 2, ClientSize.Height / 2);
            _zoomCenterNorm = ZoomMath.ComputeNewCenterAfterZoom(ClientSize.Width, ClientSize.Height, Image.Width, Image.Height, _zoomLevel, newZoom, _zoomCenterNorm, anchor);
            _zoomLevel = newZoom;

            Cursor = IsZoomed ? Cursors.Hand : Cursors.Default;
            ZoomChanged?.Invoke(this, EventArgs.Empty);
            Invalidate();
        }

        /// <summary>
        /// Gets the source rectangle of the image currently visible on the control.
        /// </summary>
        public RectangleF GetVisibleImageRect()
        {
            if (Image == null)
            {
                return RectangleF.Empty;
            }

            ZoomMath.ComputeViewport(ClientSize.Width, ClientSize.Height, Image.Width, Image.Height, _zoomLevel, _zoomCenterNorm, out RectangleF srcRect);
            return srcRect;
        }

        /// <summary>
        /// Gets an OpenCvSharp.Rect representing the visible region in the specified frame dimensions.
        /// </summary>
        public Rect GetVisibleImageRoi(int frameWidth, int frameHeight)
        {
            if (frameWidth <= 0 || frameHeight <= 0)
            {
                return new Rect(0, 0, 0, 0);
            }

            ZoomMath.ComputeViewport(ClientSize.Width, ClientSize.Height, frameWidth, frameHeight, _zoomLevel, _zoomCenterNorm, out RectangleF srcRect);
            int x = (int)Math.Max(0, Math.Floor(srcRect.X));
            int y = (int)Math.Max(0, Math.Floor(srcRect.Y));
            int w = (int)Math.Min(frameWidth - x, Math.Ceiling(srcRect.Width));
            int h = (int)Math.Min(frameHeight - y, Math.Ceiling(srcRect.Height));
            return new Rect(x, y, Math.Max(1, w), Math.Max(1, h));
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Image? img = Image;
            if (img == null || img.Width <= 0 || img.Height <= 0 || ClientSize.Width <= 0 || ClientSize.Height <= 0)
            {
                e.Graphics.Clear(BackColor);
                return;
            }

            RectangleF destRect = ZoomMath.ComputeViewport(ClientSize.Width, ClientSize.Height, img.Width, img.Height, _zoomLevel, _zoomCenterNorm, out RectangleF srcRect);

            // Clear background
            e.Graphics.Clear(BackColor);

            // Setup high performance interpolation: bicubic for smooth scaling <= 3x, nearest-neighbor > 3x for crisp pixel examination
            if (_zoomLevel > 3.0)
            {
                e.Graphics.InterpolationMode = InterpolationMode.NearestNeighbor;
                e.Graphics.PixelOffsetMode = PixelOffsetMode.Half;
            }
            else
            {
                e.Graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                e.Graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
            }

            e.Graphics.DrawImage(img, destRect, srcRect, GraphicsUnit.Pixel);

            // Draw HUD (Zoom badge and Mini-map Navigator) when zoomed
            if (IsZoomed)
            {
                DrawHud(e.Graphics, img, srcRect);
            }
        }

        private void DrawHud(Graphics g, Image img, RectangleF visibleSrcRect)
        {
            // 1. Top-left Zoom Badge
            string zoomText = $"🔍 {Math.Round(_zoomLevel * 100)}% (Drag to Pan | Esc to Fit)";
            using (Font font = new Font("Segoe UI", 9f, FontStyle.Bold))
            {
                SizeF textSize = g.MeasureString(zoomText, font);
                RectangleF badgeRect = new RectangleF(10, 10, textSize.Width + 16, textSize.Height + 8);

                using (SolidBrush bgBrush = new SolidBrush(Color.FromArgb(190, 20, 20, 20)))
                using (Pen borderPen = new Pen(Color.FromArgb(120, 255, 255, 255), 1f))
                using (SolidBrush textBrush = new SolidBrush(Color.White))
                {
                    g.FillRectangle(bgBrush, badgeRect);
                    g.DrawRectangle(borderPen, badgeRect.X, badgeRect.Y, badgeRect.Width, badgeRect.Height);
                    g.DrawString(zoomText, font, textBrush, badgeRect.X + 8, badgeRect.Y + 4);
                }
            }

            // 2. Bottom-right Mini-map Navigator
            int maxMiniW = 140;
            int maxMiniH = 90;
            float imgAspect = (float)img.Width / img.Height;

            int miniW, miniH;
            if (imgAspect >= 1.0f)
            {
                miniW = maxMiniW;
                miniH = (int)Math.Max(30, maxMiniW / imgAspect);
            }
            else
            {
                miniH = maxMiniH;
                miniW = (int)Math.Max(30, maxMiniH * imgAspect);
            }

            int miniX = ClientSize.Width - miniW - 12;
            int miniY = ClientSize.Height - miniH - 12;

            if (miniX > 100 && miniY > 100)
            {
                Rectangle miniRect = new Rectangle(miniX, miniY, miniW, miniH);

                // Background & border
                using (SolidBrush miniBg = new SolidBrush(Color.FromArgb(180, 15, 15, 15)))
                using (Pen miniBorder = new Pen(Color.FromArgb(140, 255, 255, 255), 1f))
                {
                    g.FillRectangle(miniBg, miniRect);

                    // Draw image thumbnail
                    g.InterpolationMode = InterpolationMode.Low;
                    g.DrawImage(img, miniRect, 0, 0, img.Width, img.Height, GraphicsUnit.Pixel);

                    // Overlay darkening on whole mini-map so viewport stands out
                    using (SolidBrush shadowBrush = new SolidBrush(Color.FromArgb(90, 0, 0, 0)))
                    {
                        g.FillRectangle(shadowBrush, miniRect);
                    }

                    // Viewport rectangle inside mini-map
                    float vpX = miniX + (visibleSrcRect.X / img.Width) * miniW;
                    float vpY = miniY + (visibleSrcRect.Y / img.Height) * miniH;
                    float vpW = (visibleSrcRect.Width / img.Width) * miniW;
                    float vpH = (visibleSrcRect.Height / img.Height) * miniH;
                    RectangleF vpRect = new RectangleF(vpX, vpY, vpW, vpH);

                    using (SolidBrush vpFill = new SolidBrush(Color.FromArgb(70, 0, 255, 120)))
                    using (Pen vpPen = new Pen(Color.FromArgb(240, 0, 255, 120), 2f))
                    {
                        g.FillRectangle(vpFill, vpRect);
                        g.DrawRectangle(vpPen, vpRect.X, vpRect.Y, vpRect.Width, vpRect.Height);
                    }

                    g.DrawRectangle(miniBorder, miniRect);
                }
            }
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);

            if ((e.Button == MouseButtons.Left || e.Button == MouseButtons.Middle) && IsZoomed)
            {
                _isPanning = true;
                _panStartPoint = e.Location;
                _panStartCenterNorm = _zoomCenterNorm;
                Cursor = Cursors.SizeAll;
                Capture = true;
            }
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            if (_isPanning && Image != null)
            {
                int dx = e.Location.X - _panStartPoint.X;
                int dy = e.Location.Y - _panStartPoint.Y;

                PointF newCenter = ZoomMath.ComputeNewCenterAfterPan(ClientSize.Width, ClientSize.Height, Image.Width, Image.Height, _zoomLevel, _panStartCenterNorm, dx, dy);
                _zoomCenterNorm = newCenter;
                Invalidate();
            }
            else if (IsZoomed)
            {
                Cursor = Cursors.Hand;
            }
            else
            {
                Cursor = Cursors.Default;
            }
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);

            if (_isPanning)
            {
                _isPanning = false;
                Capture = false;
                Cursor = IsZoomed ? Cursors.Hand : Cursors.Default;
            }
        }

        protected override void OnMouseDoubleClick(MouseEventArgs e)
        {
            base.OnMouseDoubleClick(e);

            if (e.Button == MouseButtons.Left)
            {
                if (IsZoomed)
                {
                    ResetZoom();
                }
                else
                {
                    SetZoom(2.0, e.Location);
                }
            }
        }

        protected override void OnMouseWheel(MouseEventArgs e)
        {
            if (ModifierKeys.HasFlag(Keys.Control))
            {
                if (e.Delta > 0)
                {
                    ZoomIn(e.Location);
                }
                else if (e.Delta < 0)
                {
                    ZoomOut(e.Location);
                }
                return;
            }

            base.OnMouseWheel(e);
        }
    }

    /// <summary>
    /// Pure mathematical algorithms for aspect-ratio fit, coordinate mapping, anchor zooming, and viewport clamping.
    /// Completely decoupled from UI handles for 100% automated testability.
    /// </summary>
    public static class ZoomMath
    {
        public static float ComputeFitScale(int clientW, int clientH, int imgW, int imgH)
        {
            if (clientW <= 0 || clientH <= 0 || imgW <= 0 || imgH <= 0)
            {
                return 1.0f;
            }

            return Math.Min((float)clientW / imgW, (float)clientH / imgH);
        }

        public static PointF ClampCenter(int clientW, int clientH, int imgW, int imgH, double zoomLevel, PointF centerNorm)
        {
            if (clientW <= 0 || clientH <= 0 || imgW <= 0 || imgH <= 0 || zoomLevel <= 1.001)
            {
                return new PointF(0.5f, 0.5f);
            }

            float sFit = ComputeFitScale(clientW, clientH, imgW, imgH);
            float scale = (float)(sFit * zoomLevel);

            float screenW = imgW * scale;
            float screenH = imgH * scale;

            float u = centerNorm.X;
            float v = centerNorm.Y;

            if (screenW > clientW)
            {
                float halfU = clientW / (2f * screenW);
                u = Math.Clamp(u, halfU, 1f - halfU);
            }
            else
            {
                u = 0.5f;
            }

            if (screenH > clientH)
            {
                float halfV = clientH / (2f * screenH);
                v = Math.Clamp(v, halfV, 1f - halfV);
            }
            else
            {
                v = 0.5f;
            }

            return new PointF(u, v);
        }

        public static RectangleF ComputeViewport(int clientW, int clientH, int imgW, int imgH, double zoomLevel, PointF centerNorm, out RectangleF srcRect)
        {
            if (clientW <= 0 || clientH <= 0 || imgW <= 0 || imgH <= 0)
            {
                srcRect = RectangleF.Empty;
                return RectangleF.Empty;
            }

            PointF clampedCenter = ClampCenter(clientW, clientH, imgW, imgH, zoomLevel, centerNorm);
            float sFit = ComputeFitScale(clientW, clientH, imgW, imgH);
            float scale = (float)(sFit * zoomLevel);

            float screenW = imgW * scale;
            float screenH = imgH * scale;

            float halfU = Math.Min(0.5f, clientW / (2f * screenW));
            float halfV = Math.Min(0.5f, clientH / (2f * screenH));

            float u0 = clampedCenter.X - halfU;
            float u1 = clampedCenter.X + halfU;
            float v0 = clampedCenter.Y - halfV;
            float v1 = clampedCenter.Y + halfV;

            float srcX = Math.Max(0f, u0 * imgW);
            float srcY = Math.Max(0f, v0 * imgH);
            float srcW = Math.Min(imgW - srcX, (u1 - u0) * imgW);
            float srcH = Math.Min(imgH - srcY, (v1 - v0) * imgH);

            srcRect = new RectangleF(srcX, srcY, srcW, srcH);

            float destW = srcW * scale;
            float destH = srcH * scale;
            float destX = (clientW - destW) / 2f;
            float destY = (clientH - destH) / 2f;

            return new RectangleF(destX, destY, destW, destH);
        }

        public static PointF ComputeNewCenterAfterZoom(int clientW, int clientH, int imgW, int imgH, double oldZoom, double newZoom, PointF oldCenterNorm, System.Drawing.Point anchorScreenPt)
        {
            if (clientW <= 0 || clientH <= 0 || imgW <= 0 || imgH <= 0)
            {
                return new PointF(0.5f, 0.5f);
            }

            float sFit = ComputeFitScale(clientW, clientH, imgW, imgH);
            float oldScale = (float)(sFit * oldZoom);
            float newScale = (float)(sFit * newZoom);

            PointF clampedOldCenter = ClampCenter(clientW, clientH, imgW, imgH, oldZoom, oldCenterNorm);

            // Normalized coordinates under anchor before zoom
            float oldScreenW = imgW * oldScale;
            float oldScreenH = imgH * oldScale;

            float anchorImgNormX = clampedOldCenter.X + (anchorScreenPt.X - clientW / 2f) / oldScreenW;
            float anchorImgNormY = clampedOldCenter.Y + (anchorScreenPt.Y - clientH / 2f) / oldScreenH;

            // Solve for new center such that anchorImgNorm remains under anchorScreenPt
            float newScreenW = imgW * newScale;
            float newScreenH = imgH * newScale;

            float newCenterNormX = anchorImgNormX - (anchorScreenPt.X - clientW / 2f) / newScreenW;
            float newCenterNormY = anchorImgNormY - (anchorScreenPt.Y - clientH / 2f) / newScreenH;

            return ClampCenter(clientW, clientH, imgW, imgH, newZoom, new PointF(newCenterNormX, newCenterNormY));
        }

        public static PointF ComputeNewCenterAfterPan(int clientW, int clientH, int imgW, int imgH, double zoomLevel, PointF startCenterNorm, int deltaX, int deltaY)
        {
            if (clientW <= 0 || clientH <= 0 || imgW <= 0 || imgH <= 0 || zoomLevel <= 1.0)
            {
                return new PointF(0.5f, 0.5f);
            }

            float sFit = ComputeFitScale(clientW, clientH, imgW, imgH);
            float scale = (float)(sFit * zoomLevel);

            float screenW = imgW * scale;
            float screenH = imgH * scale;

            float deltaNormX = -deltaX / screenW;
            float deltaNormY = -deltaY / screenH;

            PointF unclamped = new PointF(startCenterNorm.X + deltaNormX, startCenterNorm.Y + deltaNormY);
            return ClampCenter(clientW, clientH, imgW, imgH, zoomLevel, unclamped);
        }
    }
}
