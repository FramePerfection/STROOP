namespace STROOP.Core.Emulators
{
    public class DolphinNotRunningGameException : Exception
    {
        public DolphinNotRunningGameException()
            : base("Dolphin running, but emulator hasn't started")
        {
        }
    }
}
