using System.Drawing;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MotionHighlighter.Tests
{
    [TestClass]
    public class ZoomMathTests
    {
        [TestMethod]
        public void ComputeFitScale_ValidAspectRatios_ComputesExactScale()
        {
            // Exact 16:9 fit in 16:9 window
            float s1 = ZoomMath.ComputeFitScale(960, 540, 1920, 1080);
            Assert.AreEqual(0.5f, s1, 0.0001f);

            // Wide video in narrower window (width-constrained)
            float s2 = ZoomMath.ComputeFitScale(1000, 800, 2000, 1000);
            Assert.AreEqual(0.5f, s2, 0.0001f);

            // Tall portrait video in landscape window (height-constrained)
            float s3 = ZoomMath.ComputeFitScale(1280, 720, 1080, 1920);
            Assert.AreEqual(720f / 1920f, s3, 0.0001f);

            // Edge cases
            Assert.AreEqual(1.0f, ZoomMath.ComputeFitScale(0, 500, 1920, 1080), 0.0001f);
            Assert.AreEqual(1.0f, ZoomMath.ComputeFitScale(500, 0, 1920, 1080), 0.0001f);
            Assert.AreEqual(1.0f, ZoomMath.ComputeFitScale(500, 500, 0, 1080), 0.0001f);
        }

        [TestMethod]
        public void ClampCenter_At1xZoom_AlwaysReturnsCentered()
        {
            PointF c1 = ZoomMath.ClampCenter(960, 540, 1920, 1080, 1.0, new PointF(0.1f, 0.9f));
            Assert.AreEqual(0.5f, c1.X, 0.0001f);
            Assert.AreEqual(0.5f, c1.Y, 0.0001f);
        }

        [TestMethod]
        public void ClampCenter_At2xZoom_ClampsToPreventOverscroll()
        {
            // At 2x zoom in a matching 16:9 aspect ratio, screenW = 2 * clientW, so halfU = 0.25
            PointF cMin = ZoomMath.ClampCenter(960, 540, 1920, 1080, 2.0, new PointF(0.05f, 0.05f));
            Assert.AreEqual(0.25f, cMin.X, 0.0001f);
            Assert.AreEqual(0.25f, cMin.Y, 0.0001f);

            PointF cMax = ZoomMath.ClampCenter(960, 540, 1920, 1080, 2.0, new PointF(0.95f, 0.95f));
            Assert.AreEqual(0.75f, cMax.X, 0.0001f);
            Assert.AreEqual(0.75f, cMax.Y, 0.0001f);

            PointF cMid = ZoomMath.ClampCenter(960, 540, 1920, 1080, 2.0, new PointF(0.5f, 0.5f));
            Assert.AreEqual(0.5f, cMid.X, 0.0001f);
            Assert.AreEqual(0.5f, cMid.Y, 0.0001f);
        }

        [TestMethod]
        public void ComputeViewport_At1xZoom_DisplaysEntireSourceImage()
        {
            RectangleF destRect = ZoomMath.ComputeViewport(960, 540, 1920, 1080, 1.0, new PointF(0.5f, 0.5f), out RectangleF srcRect);

            // Source rectangle must be the full 1920x1080 image
            Assert.AreEqual(0f, srcRect.X, 0.01f);
            Assert.AreEqual(0f, srcRect.Y, 0.01f);
            Assert.AreEqual(1920f, srcRect.Width, 0.01f);
            Assert.AreEqual(1080f, srcRect.Height, 0.01f);

            // Destination rectangle matches client area
            Assert.AreEqual(0f, destRect.X, 0.01f);
            Assert.AreEqual(0f, destRect.Y, 0.01f);
            Assert.AreEqual(960f, destRect.Width, 0.01f);
            Assert.AreEqual(540f, destRect.Height, 0.01f);
        }

        [TestMethod]
        public void ComputeViewport_At2xZoom_ScalesSourceAndFillsClient()
        {
            RectangleF destRect = ZoomMath.ComputeViewport(960, 540, 1920, 1080, 2.0, new PointF(0.5f, 0.5f), out RectangleF srcRect);

            // At 2x zoom, source viewport should be half width (960) and half height (540) centered
            Assert.AreEqual(480f, srcRect.X, 0.01f);
            Assert.AreEqual(270f, srcRect.Y, 0.01f);
            Assert.AreEqual(960f, srcRect.Width, 0.01f);
            Assert.AreEqual(540f, srcRect.Height, 0.01f);

            // Dest rect fills the client area
            Assert.AreEqual(0f, destRect.X, 0.01f);
            Assert.AreEqual(0f, destRect.Y, 0.01f);
            Assert.AreEqual(960f, destRect.Width, 0.01f);
            Assert.AreEqual(540f, destRect.Height, 0.01f);
        }

        [TestMethod]
        public void ComputeNewCenterAfterPan_DragsViewportCorrectly()
        {
            // At 2x zoom: screenW = 1920. Dragging right by 192 pixels should decrease centerNorm.X by 0.10
            PointF startCenter = new PointF(0.5f, 0.5f);
            PointF newCenter = ZoomMath.ComputeNewCenterAfterPan(960, 540, 1920, 1080, 2.0, startCenter, 192, 0);

            Assert.AreEqual(0.40f, newCenter.X, 0.001f);
            Assert.AreEqual(0.50f, newCenter.Y, 0.001f);
        }

        [TestMethod]
        public void ComputeNewCenterAfterZoom_MaintainsAnchorUnderCursor()
        {
            // Center anchor: zooming from 1x to 2x at (480, 270) keeps center at (0.5, 0.5)
            Point anchorCenter = new Point(480, 270);
            PointF newCenter = ZoomMath.ComputeNewCenterAfterZoom(960, 540, 1920, 1080, 1.0, 2.0, new PointF(0.5f, 0.5f), anchorCenter);
            Assert.AreEqual(0.5f, newCenter.X, 0.001f);
            Assert.AreEqual(0.5f, newCenter.Y, 0.001f);

            // Left anchor: zooming in near left edge should shift center to the left
            Point anchorLeft = new Point(100, 270);
            PointF newCenterLeft = ZoomMath.ComputeNewCenterAfterZoom(960, 540, 1920, 1080, 1.0, 2.0, new PointF(0.5f, 0.5f), anchorLeft);
            Assert.IsTrue(newCenterLeft.X < 0.5f, "Center X should shift left towards anchor");
            Assert.IsTrue(newCenterLeft.X >= 0.25f, "Center X must stay clamped within visible range");
        }
    }
}
