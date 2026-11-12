using OpenCvSharp;

namespace MotionHighlighter
{
    public class TrackedMotionBox
    {
        public Rect Box { get; set; }
        public double Opacity { get; set; } = 1.0;
        public int FramesSinceDetected { get; set; }
        public int TotalFramesTracked { get; set; } = 1;
    }

    public class MotionBoxTracker
    {
        private readonly List<TrackedMotionBox> _trackedBoxes = new();
        private readonly object _lock = new();

        public int HoldFrames { get; set; } = 9;
        public int FadeFrames { get; set; } = 11;
        public double MinIoUForMatch { get; set; } = 0.20;
        public int MergePadding { get; set; } = 4;

        public void Configure(int totalFrames)
        {
            if (totalFrames <= 0)
            {
                HoldFrames = 0;
                FadeFrames = 0;
                return;
            }

            // Hold for ~45% of the total persistence time, then smoothly fade out over the remaining ~55%
            HoldFrames = Math.Max(1, (int)Math.Round(totalFrames * 0.45));
            FadeFrames = Math.Max(1, totalFrames - HoldFrames);
        }

        public void Reset()
        {
            lock (_lock)
            {
                _trackedBoxes.Clear();
            }
        }

        public List<(Rect Box, double Opacity)> Update(IEnumerable<Rect> detectedRects)
        {
            lock (_lock)
            {
                // First, merge any raw overlapping/touching detections (e.g. limbs/shadows of same subject)
                var detectedList = MergeOverlappingRects(detectedRects, MergePadding);

                // If persistence is disabled (totalFrames == 0), return merged detections with full opacity
                if (HoldFrames == 0 && FadeFrames == 0)
                {
                    _trackedBoxes.Clear();
                    return detectedList.Select(r => (r, 1.0)).ToList();
                }

                var matchedTracked = new HashSet<TrackedMotionBox>();
                var matchedDetected = new HashSet<Rect>();

                // Match detections with existing tracked boxes using IoU and centroid proximity
                foreach (var detected in detectedList)
                {
                    TrackedMotionBox? bestMatch = null;
                    double bestScore = 0;

                    foreach (var tracked in _trackedBoxes)
                    {
                        if (matchedTracked.Contains(tracked)) continue;

                        double score = CalculateMatchScore(detected, tracked.Box);
                        if (score > bestScore && score >= MinIoUForMatch)
                        {
                            bestScore = score;
                            bestMatch = tracked;
                        }
                    }

                    if (bestMatch != null)
                    {
                        // Smooth position to eliminate high-frequency jitter (70% new, 30% old)
                        bestMatch.Box = SmoothRect(bestMatch.Box, detected, 0.70);
                        bestMatch.Opacity = 1.0;
                        bestMatch.FramesSinceDetected = 0;
                        bestMatch.TotalFramesTracked++;

                        matchedTracked.Add(bestMatch);
                        matchedDetected.Add(detected);
                    }
                }

                // Add new detections as tracked boxes
                foreach (var detected in detectedList)
                {
                    if (!matchedDetected.Contains(detected))
                    {
                        _trackedBoxes.Add(new TrackedMotionBox
                        {
                            Box = detected,
                            Opacity = 1.0,
                            FramesSinceDetected = 0,
                            TotalFramesTracked = 1
                        });
                    }
                }

                // Fade out unmatched tracked boxes
                double fadeStep = FadeFrames > 0 ? (1.0 / FadeFrames) : 1.0;
                for (int i = _trackedBoxes.Count - 1; i >= 0; i--)
                {
                    var tracked = _trackedBoxes[i];
                    if (!matchedTracked.Contains(tracked))
                    {
                        tracked.FramesSinceDetected++;
                        if (tracked.FramesSinceDetected > HoldFrames)
                        {
                            tracked.Opacity -= fadeStep;
                        }

                        if (tracked.Opacity <= 0.02)
                        {
                            _trackedBoxes.RemoveAt(i);
                        }
                    }
                }

                // Merge any overlapping or touching tracked boxes so green borders NEVER overlap
                MergeTrackedBoxes();

                return _trackedBoxes.Select(t => (t.Box, Math.Clamp(t.Opacity, 0.0, 1.0))).ToList();
            }
        }

        private void MergeTrackedBoxes()
        {
            if (_trackedBoxes.Count <= 1) return;

            bool mergedAny;
            do
            {
                mergedAny = false;
                for (int i = 0; i < _trackedBoxes.Count; i++)
                {
                    for (int j = i + 1; j < _trackedBoxes.Count; j++)
                    {
                        if (AreOverlappingOrTouching(_trackedBoxes[i].Box, _trackedBoxes[j].Box, MergePadding))
                        {
                            var boxA = _trackedBoxes[i];
                            var boxB = _trackedBoxes[j];

                            boxA.Box = Union(boxA.Box, boxB.Box);
                            boxA.Opacity = Math.Max(boxA.Opacity, boxB.Opacity);
                            boxA.FramesSinceDetected = Math.Min(boxA.FramesSinceDetected, boxB.FramesSinceDetected);
                            boxA.TotalFramesTracked = Math.Max(boxA.TotalFramesTracked, boxB.TotalFramesTracked);

                            _trackedBoxes.RemoveAt(j);
                            mergedAny = true;
                            break;
                        }
                    }
                    if (mergedAny) break;
                }
            } while (mergedAny);
        }

        public static List<Rect> MergeOverlappingRects(IEnumerable<Rect> rects, int padding = 4)
        {
            var list = rects.ToList();
            if (list.Count <= 1) return list;

            if (list.Count > 100)
            {
                list = list.OrderByDescending(r => r.Width * r.Height).Take(100).ToList();
            }

            bool mergedAny;
            do
            {
                mergedAny = false;
                for (int i = 0; i < list.Count; i++)
                {
                    for (int j = i + 1; j < list.Count; j++)
                    {
                        if (AreOverlappingOrTouching(list[i], list[j], padding))
                        {
                            list[i] = Union(list[i], list[j]);
                            list.RemoveAt(j);
                            mergedAny = true;
                            break;
                        }
                    }
                    if (mergedAny) break;
                }
            } while (mergedAny);

            return list;
        }

        public static bool AreOverlappingOrTouching(Rect a, Rect b, int padding = 4)
        {
            int ax2 = a.X + a.Width;
            int ay2 = a.Y + a.Height;
            int bx2 = b.X + b.Width;
            int by2 = b.Y + b.Height;

            // Check if separated horizontally by more than padding
            if (a.X - padding > bx2 || bx2 + padding < a.X || b.X - padding > ax2 || ax2 + padding < b.X)
            {
                return false;
            }

            // Check if separated vertically by more than padding
            if (a.Y - padding > by2 || by2 + padding < a.Y || b.Y - padding > ay2 || ay2 + padding < b.Y)
            {
                return false;
            }

            return true;
        }

        public static Rect Union(Rect a, Rect b)
        {
            int minX = Math.Min(a.X, b.X);
            int minY = Math.Min(a.Y, b.Y);
            int maxX = Math.Max(a.X + a.Width, b.X + b.Width);
            int maxY = Math.Max(a.Y + a.Height, b.Y + b.Height);
            return new Rect(minX, minY, Math.Max(0, maxX - minX), Math.Max(0, maxY - minY));
        }

        private static Rect SmoothRect(Rect oldRect, Rect newRect, double weight)
        {
            int x = (int)Math.Round(oldRect.X * (1 - weight) + newRect.X * weight);
            int y = (int)Math.Round(oldRect.Y * (1 - weight) + newRect.Y * weight);
            int w = (int)Math.Round(oldRect.Width * (1 - weight) + newRect.Width * weight);
            int h = (int)Math.Round(oldRect.Height * (1 - weight) + newRect.Height * weight);
            return new Rect(x, y, w, h);
        }

        private static double CalculateMatchScore(Rect a, Rect b)
        {
            int interX1 = Math.Max(a.Left, b.Left);
            int interY1 = Math.Max(a.Top, b.Top);
            int interX2 = Math.Min(a.Right, b.Right);
            int interY2 = Math.Min(a.Bottom, b.Bottom);

            int interW = Math.Max(0, interX2 - interX1);
            int interH = Math.Max(0, interY2 - interY1);
            long interArea = (long)interW * interH;

            long areaA = (long)a.Width * a.Height;
            long areaB = (long)b.Width * b.Height;
            long unionArea = areaA + areaB - interArea;

            if (unionArea <= 0) return 0;
            double iou = (double)interArea / unionArea;

            // Centroid bonus for tracking moving objects with significant displacement
            double cxa = a.X + a.Width / 2.0;
            double cya = a.Y + a.Height / 2.0;
            double cxb = b.X + b.Width / 2.0;
            double cyb = b.Y + b.Height / 2.0;
            double distSq = (cxa - cxb) * (cxa - cxb) + (cya - cyb) * (cya - cyb);
            double maxDim = Math.Max(Math.Max(a.Width, a.Height), Math.Max(b.Width, b.Height));
            if (distSq < (maxDim * maxDim) && iou > 0.10)
            {
                iou += 0.20;
            }

            return Math.Min(1.0, iou);
        }

        public static void DrawFadingBoundingBox(Mat image, Rect rect, double opacity, int thickness = 2)
        {
            if (image == null || image.Empty() || opacity <= 0.02)
            {
                return;
            }

            int x1 = Math.Max(0, rect.X);
            int y1 = Math.Max(0, rect.Y);
            int x2 = Math.Min(image.Width, rect.X + rect.Width);
            int y2 = Math.Min(image.Height, rect.Y + rect.Height);

            if (x2 <= x1 || y2 <= y1)
            {
                return;
            }

            if (opacity >= 0.98)
            {
                Cv2.Rectangle(image, new Rect(x1, y1, x2 - x1, y2 - y1), new Scalar(0, 255, 0), thickness);
                return;
            }

            // High-performance alpha-blended border strips without full frame allocation
            int t = Math.Max(1, thickness);
            var greenColor = new Scalar(0, 255, 0);

            // Top strip
            int topH = Math.Min(t, y2 - y1);
            if (topH > 0)
            {
                BlendStrip(image, new Rect(x1, y1, x2 - x1, topH), greenColor, opacity);
            }

            // Bottom strip
            int bottomH = Math.Min(t, y2 - y1 - topH);
            if (bottomH > 0)
            {
                BlendStrip(image, new Rect(x1, y2 - bottomH, x2 - x1, bottomH), greenColor, opacity);
            }

            // Left strip (between top and bottom)
            int middleH = y2 - y1 - topH - bottomH;
            int leftW = Math.Min(t, x2 - x1);
            if (middleH > 0 && leftW > 0)
            {
                BlendStrip(image, new Rect(x1, y1 + topH, leftW, middleH), greenColor, opacity);
            }

            // Right strip (between top and bottom)
            int rightW = Math.Min(t, x2 - x1 - leftW);
            if (middleH > 0 && rightW > 0)
            {
                BlendStrip(image, new Rect(x2 - rightW, y1 + topH, rightW, middleH), greenColor, opacity);
            }
        }

        private static void BlendStrip(Mat image, Rect stripRect, Scalar color, double opacity)
        {
            if (stripRect.Width <= 0 || stripRect.Height <= 0) return;
            using Mat roi = new Mat(image, stripRect);
            using Mat colorMat = new Mat(roi.Size(), roi.Type(), color);
            Cv2.AddWeighted(roi, 1.0 - opacity, colorMat, opacity, 0, roi);
        }
    }
}
