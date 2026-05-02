namespace STROOP.Core.GameMemoryAccess;

public class DolphinNotRunningGameException : Exception
{
    public DolphinNotRunningGameException()
        : base("Dolphin running, but emulator hasn't started")
    {
    }
}
