using System.Runtime.InteropServices;

namespace StroopNext.Platform;

public sealed partial class TimerResolution : IDisposable
{
    private bool _active;

    private TimerResolution() => _active = OperatingSystem.IsWindows() && timeBeginPeriod(1) == 0;

    public static TimerResolution OneMillisecond() => new();

    public void Dispose()
    {
        if (_active)
            timeEndPeriod(1);
        _active = false;
    }

    [LibraryImport("winmm.dll")]
    private static partial uint timeBeginPeriod(uint period);

    [LibraryImport("winmm.dll")]
    private static partial uint timeEndPeriod(uint period);
}
