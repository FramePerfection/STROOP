using StroopNext.Game;
using StroopNext.Memory;
using StroopNext.Session;

namespace StroopNext.Core.Tests;

internal sealed class TestRam
{
    public const uint UsMario = 0x8033B170;

    public byte[] Bytes { get; } = new byte[N64.RamSize8MB];
    public RamPatch Patch { get; } = new();

    public static TestRam Us()
    {
        var ram = new TestRam();
        ram.Patch.WriteU32(GameVersionDetector.TellAddress, 0xC58400A4);
        return ram;
    }

    public TestRam Do(Action<RamPatch> write)
    {
        write(Patch);
        return this;
    }

    public RamImage Image()
    {
        Patch.ApplyTo(Bytes);
        return new RamImage(Bytes);
    }

    public GameFrame Frame(GameFrame? previous = null, GameServices? services = null) => GameFrame.Create(Image(), previous, services);
}
