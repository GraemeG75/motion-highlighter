using System.Drawing.Imaging;
using OpenCvSharp;
using OpenCvSharp.Extensions;

namespace MotionHighlighter.Streaming
{
    public static class ScreenshotHelper
    {
        public static string GetDefaultScreenshotsDirectory()
        {
            string folder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyPictures), "MotionHighlighter_Screenshots");
            if (!Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder);
            }
            return folder;
        }

        public static string SaveScreenshot(Mat frameBgr, string? destinationFolder = null, string prefix = "StreamCapture", ImageFormat? format = null)
        {
            if (frameBgr == null || frameBgr.Empty())
            {
                throw new ArgumentException("Cannot save an empty frame as screenshot.", nameof(frameBgr));
            }

            destinationFolder ??= GetDefaultScreenshotsDirectory();
            if (!Directory.Exists(destinationFolder))
            {
                Directory.CreateDirectory(destinationFolder);
            }

            format ??= ImageFormat.Png;
            string ext = format.Equals(ImageFormat.Jpeg) ? ".jpg" : ".png";
            string filename = $"{prefix}_{DateTime.Now:yyyyMMdd_HHmmss_fff}{ext}";
            string fullPath = Path.Combine(destinationFolder, filename);

            using (Bitmap bmp = BitmapConverter.ToBitmap(frameBgr))
            {
                bmp.Save(fullPath, format);
            }

            return fullPath;
        }

        public static void CopyToClipboard(Mat frameBgr)
        {
            if (frameBgr == null || frameBgr.Empty())
            {
                return;
            }

            using Bitmap bmp = BitmapConverter.ToBitmap(frameBgr);
            Clipboard.SetImage(bmp);
        }
    }
}
