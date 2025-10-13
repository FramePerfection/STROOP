using System.Diagnostics;

namespace STROOP.Core;

public class CoreLoop
{
    private List<double> _fpsTimes = new List<double>();
    private byte[] _ram;
    private object _mStreamProcess = new object();

    public double FpsInPractice => _fpsTimes.Count == 0 ? 0 : 1 / _fpsTimes.Average();
    public double lastFrameTime => _fpsTimes.Count == 0 ? double.NaN : _fpsTimes.Last();

    public void Run(CancellationToken cancellationToken, Action handleEvents, Func<double> getTargetedFps)
    {
        Stopwatch frameStopwatch = Stopwatch.StartNew();

        while (!cancellationToken.IsCancellationRequested)
        {
            double timeToWait;
            lock (_mStreamProcess)
            {
                ProcessStream.Instance.RefreshRam();
                ProcessStream.Instance.OnUpdate?.Invoke();
            }

            handleEvents();

            // Calculate delay to match correct FPS
            frameStopwatch.Stop();
            double timePassed = frameStopwatch.ElapsedTicks / (double)Stopwatch.Frequency;
            timeToWait = getTargetedFps() - timePassed;
            timeToWait = Math.Max(timeToWait, 0);

            // Calculate Fps
            while (_fpsTimes.Count() >= 10)
                _fpsTimes.RemoveAt(0);
            _fpsTimes.Add(timePassed + timeToWait);

            frameStopwatch.Restart();

            if (timeToWait > 0)
                Thread.Sleep(new TimeSpan((long)(timeToWait * 10000000)));
            else
                Thread.Yield();
        }
    }
}
