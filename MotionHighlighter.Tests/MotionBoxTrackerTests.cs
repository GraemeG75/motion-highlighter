using MotionHighlighter;
using OpenCvSharp;

namespace MotionHighlighter.Tests
{
    [TestClass]
    public class MotionBoxTrackerTests
    {
        [TestMethod]
        public void MotionBoxTracker_HoldsBoxWhenMotionTemporarilyStops_PreventsFlickering()
        {
            var tracker = new MotionBoxTracker();
            tracker.Configure(10); // 4-5 hold frames, 5-6 fade frames

            Rect initial = new Rect(100, 100, 50, 50);

            // Frame 1: Detection occurs
            var frame1 = tracker.Update(new[] { initial });
            Assert.AreEqual(1, frame1.Count);
            Assert.AreEqual(1.0, frame1[0].Opacity, 0.001);

            // Frame 2: Motion drops (0 detections) -> box should STAY with 100% opacity
            var frame2 = tracker.Update(Array.Empty<Rect>());
            Assert.AreEqual(1, frame2.Count);
            Assert.AreEqual(1.0, frame2[0].Opacity, 0.001, "Box should remain at 100% opacity during hold phase to prevent flicker.");

            // Frame 3: Motion still absent -> box continues to stay
            var frame3 = tracker.Update(Array.Empty<Rect>());
            Assert.AreEqual(1, frame3.Count);
            Assert.AreEqual(1.0, frame3[0].Opacity, 0.001);
        }

        [TestMethod]
        public void MotionBoxTracker_FadesOutSmoothlyAndEvictsAfterGracePeriod()
        {
            var tracker = new MotionBoxTracker();
            tracker.Configure(10); // Total 10 frames

            Rect initial = new Rect(50, 50, 40, 40);
            tracker.Update(new[] { initial });

            double previousOpacity = 1.0;
            bool sawFade = false;

            // Advance 15 frames with no detections
            for (int i = 0; i < 15; i++)
            {
                var boxes = tracker.Update(Array.Empty<Rect>());
                if (boxes.Count > 0)
                {
                    double currentOpacity = boxes[0].Opacity;
                    if (currentOpacity < previousOpacity)
                    {
                        sawFade = true;
                    }
                    Assert.IsTrue(currentOpacity <= previousOpacity, "Opacity should never increase without detection.");
                    previousOpacity = currentOpacity;
                }
            }

            Assert.IsTrue(sawFade, "Tracked box should smoothly reduce opacity after hold duration.");

            // After 20 frames, box should be completely evicted
            var finalBoxes = tracker.Update(Array.Empty<Rect>());
            Assert.AreEqual(0, finalBoxes.Count, "Box should be evicted once completely faded out.");
        }

        [TestMethod]
        public void MotionBoxTracker_ReenergizesAndSmoothsPositionWhenMotionContinues()
        {
            var tracker = new MotionBoxTracker();
            tracker.Configure(20);

            Rect start = new Rect(100, 100, 50, 50);
            tracker.Update(new[] { start });

            // Let it age slightly into fade
            for (int i = 0; i < 12; i++)
            {
                tracker.Update(Array.Empty<Rect>());
            }

            // Motion returns near the object (shifted by 4 pixels)
            Rect moved = new Rect(104, 102, 50, 50);
            var reenergized = tracker.Update(new[] { moved });

            Assert.AreEqual(1, reenergized.Count);
            Assert.AreEqual(1.0, reenergized[0].Opacity, "Re-detected box should immediately reset to 100% brightness.");
            Assert.IsTrue(reenergized[0].Box.X >= 100 && reenergized[0].Box.X <= 104, "Position should be smoothly interpolated.");
        }

        [TestMethod]
        public void MotionBoxTracker_DrawFadingBoundingBox_BlendsCorrectlyWithoutThrowing()
        {
            using Mat image = new Mat(480, 640, MatType.CV_8UC3, new Scalar(50, 50, 50));

            // Test normal box with full opacity
            MotionBoxTracker.DrawFadingBoundingBox(image, new Rect(50, 50, 100, 100), 1.0, 2);

            // Test translucent fading box
            MotionBoxTracker.DrawFadingBoundingBox(image, new Rect(200, 200, 80, 80), 0.5, 2);

            // Test box partially out of bounds
            MotionBoxTracker.DrawFadingBoundingBox(image, new Rect(600, 440, 100, 100), 0.7, 2);

            // Test tiny / zero-size box (edge case)
            MotionBoxTracker.DrawFadingBoundingBox(image, new Rect(10, 10, 0, 0), 0.5, 2);

            // Green channel at (50, 50) should be bright green
            Vec3b pixelFull = image.At<Vec3b>(50, 50);
            Assert.AreEqual(255, pixelFull.Item1); // Green in BGR is item1

            // Green channel at (200, 200) should be blended between 50 and 255
            Vec3b pixelBlended = image.At<Vec3b>(200, 200);
            Assert.IsTrue(pixelBlended.Item1 > 100 && pixelBlended.Item1 < 255);
        }

        [TestMethod]
        public void MotionBoxTracker_MergesOverlappingDetections_IntoSingleUnifiedBoundingBox()
        {
            var tracker = new MotionBoxTracker();
            tracker.Configure(10);

            // Two overlapping boxes (e.g. torso and arm of a person)
            Rect torso = new Rect(100, 100, 60, 80);
            Rect arm = new Rect(140, 120, 40, 50);

            var result = tracker.Update(new[] { torso, arm });

            Assert.AreEqual(1, result.Count, "Overlapping detections should merge into exactly 1 bounding box.");
            Assert.AreEqual(100, result[0].Box.X);
            Assert.AreEqual(100, result[0].Box.Y);
            Assert.AreEqual(80, result[0].Box.Width); // 100 to 180 = 80
            Assert.AreEqual(80, result[0].Box.Height); // 100 to 180 = 80
        }

        [TestMethod]
        public void MotionBoxTracker_MergesTrackedBoxesWhenTheyOverlap()
        {
            var tracker = new MotionBoxTracker();
            tracker.Configure(15);

            // Initially two separate boxes (separated by 20 pixels)
            Rect box1 = new Rect(50, 50, 30, 30);
            Rect box2 = new Rect(100, 50, 30, 30);
            var initial = tracker.Update(new[] { box1, box2 });
            Assert.AreEqual(2, initial.Count, "Separate boxes should initially remain separate.");

            // Next frame: both move toward each other and now overlap (65..95 and 80..110)
            Rect box1Moved = new Rect(65, 50, 30, 30);
            Rect box2Moved = new Rect(80, 50, 30, 30);
            var merged = tracker.Update(new[] { box1Moved, box2Moved });

            Assert.AreEqual(1, merged.Count, "When tracked boxes intersect, they should merge into one unified box.");
            Assert.IsTrue(merged[0].Box.X >= 50 && merged[0].Box.X <= 65, "Merged box should encompass smoothed starting coordinate.");
            Assert.AreEqual(50, merged[0].Box.Y);
        }

        [TestMethod]
        public void MergeOverlappingRects_DisjointBoxes_RemainSeparate()
        {
            Rect a = new Rect(10, 10, 30, 30);
            Rect b = new Rect(100, 100, 30, 30);
            Rect c = new Rect(200, 200, 30, 30);

            var merged = MotionBoxTracker.MergeOverlappingRects(new[] { a, b, c }, padding: 4);

            Assert.AreEqual(3, merged.Count, "Disjoint boxes separated by more than padding should not merge.");
        }
    }
}
