using System.Diagnostics;
using System.Runtime.InteropServices;
using Windows.Win32;

namespace STROOP.Core;

public class CoreLoop
{
    private Queue<long> _frameTimes = new Queue<long>();
    private byte[] _ram;
    private object _mStreamProcess = new object();

    public double FpsInPractice => _frameTimes.Count == 0 ? 0 : Stopwatch.Frequency / _frameTimes.Average();
    public double lastFrameTime => _frameTimes.Count == 0 ? double.NaN : _frameTimes.Last() / (double)Stopwatch.Frequency;

    public void Run(CancellationToken cancellationToken, Action handleEvents, Func<double> getTargetedRefreshRate)
    {
        using var _ = new HighResTimer(1); // request ~1ms resolution

        // since computation of the time to wait for takes time itself, compensate with a few ticks
        const int BUFFER_TICKS = 1000;

        long ticksPerTwoMs = 2 * Stopwatch.Frequency / 1000;

        Stopwatch frameStopwatch = new Stopwatch();
        Queue<long> extraTime = new Queue<long>();
        extraTime.Enqueue(0);

        while (!cancellationToken.IsCancellationRequested)
        {
            long ticksPerFrame = (long)(Stopwatch.Frequency * getTargetedRefreshRate());

            frameStopwatch.Restart();
            lock (_mStreamProcess)
            {
                ProcessStream.Instance.RefreshRam();
                ProcessStream.Instance.OnUpdate?.Invoke();
            }

            handleEvents();

            while (frameStopwatch.ElapsedTicks < ticksPerFrame - ticksPerTwoMs)
                Thread.Sleep(1);
            do
                Thread.Yield();
            while (frameStopwatch.ElapsedTicks < ticksPerFrame - extraTime.Average());

            long frameTicks = frameStopwatch.ElapsedTicks;
            // Calculate Fps
            while (_frameTimes.Count() >= 10)
                _frameTimes.Dequeue();
            _frameTimes.Enqueue(frameStopwatch.ElapsedTicks);

            while (extraTime.Count() >= 10)
                extraTime.Dequeue();
            extraTime.Enqueue(frameStopwatch.ElapsedTicks - frameTicks + BUFFER_TICKS);
        }
    }
}
file class HighResTimer : IDisposable
{
    readonly uint _period;

    public HighResTimer(uint periodMs)
    {
        _period = periodMs;
        PInvoke.timeBeginPeriod(_period);
    }

    public void Dispose()
    {
        PInvoke.timeEndPeriod(_period);
    }
}
