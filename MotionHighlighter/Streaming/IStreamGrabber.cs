using OpenCvSharp;

namespace MotionHighlighter.Streaming
{
    public interface IStreamGrabber : IDisposable
    {
        string StreamUrl { get; }
        string StreamTitle { get; }
        StreamFormat Format { get; }
        bool IsRunning { get; }
        bool IsLive { get; }
        double Fps { get; }
        int Width { get; }
        int Height { get; }
        long FramesReceived { get; }
        TimeSpan ElapsedStreamTime { get; }

        Task StartAsync(CancellationToken cancellationToken);
        Task<Mat?> ReadNextFrameAsync(CancellationToken cancellationToken);
        void Stop();
    }
}
