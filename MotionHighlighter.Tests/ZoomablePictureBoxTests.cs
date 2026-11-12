using System.Drawing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenCvSharp;

namespace MotionHighlighter.Tests
{
    [TestClass]
    public class ZoomablePictureBoxTests
    {
        [TestMethod]
        public void DefaultState_IsFitAndNotZoomed()
        {
            using ZoomablePictureBox pic = new();
            Assert.AreEqual(1.0, pic.ZoomLevel);
            Assert.IsFalse(pic.IsZoomed);
            Assert.IsFalse(pic.IsPanning);
            Assert.AreEqual(0.5f, pic.ZoomCenterNorm.X);
            Assert.AreEqual(0.5f, pic.ZoomCenterNorm.Y);
        }

        [TestMethod]
        public void ZoomIn_IncreasesZoomLevelAndRaisesEvent()
        {
            using ZoomablePictureBox pic = new();
            bool eventFired = false;
            pic.ZoomChanged += (_, _) => eventFired = true;

            pic.ZoomIn();

            Assert.IsTrue(pic.ZoomLevel > 1.0);
            Assert.IsTrue(pic.IsZoomed);
            Assert.IsTrue(eventFired);
        }

        [TestMethod]
        public void ResetZoom_RestoresTo1x()
        {
            using ZoomablePictureBox pic = new();
            pic.ZoomIn();
            pic.ZoomIn();
            Assert.IsTrue(pic.IsZoomed);

            pic.ResetZoom();
            Assert.AreEqual(1.0, pic.ZoomLevel);
            Assert.IsFalse(pic.IsZoomed);
            Assert.AreEqual(0.5f, pic.ZoomCenterNorm.X);
            Assert.AreEqual(0.5f, pic.ZoomCenterNorm.Y);
        }

        [TestMethod]
        public void ZoomClamping_RespectsMinAndMaxBounds()
        {
            using ZoomablePictureBox pic = new();

            pic.SetZoom(100.0);
            Assert.AreEqual(ZoomablePictureBox.MaxZoom, pic.ZoomLevel);

            pic.SetZoom(0.1);
            Assert.AreEqual(ZoomablePictureBox.MinZoom, pic.ZoomLevel);
        }

        [TestMethod]
        public void GetVisibleImageRoi_CalculatesAccurateSubRegion()
        {
            using ZoomablePictureBox pic = new();
            pic.ClientSize = new System.Drawing.Size(960, 540);

            // At 1.0x, ROI covers entire 1920x1080 frame
            Rect fullRoi = pic.GetVisibleImageRoi(1920, 1080);
            Assert.AreEqual(0, fullRoi.X);
            Assert.AreEqual(0, fullRoi.Y);
            Assert.AreEqual(1920, fullRoi.Width);
            Assert.AreEqual(1080, fullRoi.Height);

            // At 2.0x, ROI covers half frame
            pic.SetZoom(2.0);
            Rect halfRoi = pic.GetVisibleImageRoi(1920, 1080);
            Assert.IsTrue(halfRoi.Width <= 961 && halfRoi.Width >= 959, $"Expected ~960 width, got {halfRoi.Width}");
            Assert.IsTrue(halfRoi.Height <= 541 && halfRoi.Height >= 539, $"Expected ~540 height, got {halfRoi.Height}");
            Assert.IsTrue(halfRoi.X >= 475 && halfRoi.X <= 485, $"Expected ~480 X, got {halfRoi.X}");
            Assert.IsTrue(halfRoi.Y >= 265 && halfRoi.Y <= 275, $"Expected ~270 Y, got {halfRoi.Y}");
        }
    }
}
