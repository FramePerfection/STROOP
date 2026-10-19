using StroopNext.Game;
using StroopNext.Memory;
using StroopNext.Session;
using Xunit;

namespace StroopNext.Core.Tests;

public sealed class RomLevelsTests
{
    // Real ROMs and savestates of this machine (Downloads + STROOP's engine-path savestates); skipped elsewhere.
    private static readonly string Downloads = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads");
    private static readonly string EnginePath = Path.Combine(Path.GetTempPath(), "StroopNext", "engine-path");

    private static (byte[] Rom, GameFrame Frame)? Load(string rom, string savestate)
    {
        string romPath = Path.Combine(Downloads, rom), stPath = Path.Combine(EnginePath, savestate);
        if (!File.Exists(romPath) || !File.Exists(stPath))
        {
            Console.WriteLine($"RomLevelsTests skipped: {romPath} / {stPath} not found");
            return null;
        }
        Console.WriteLine($"RomLevelsTests on {rom}");
        using var st = File.OpenRead(stPath);
        return (File.ReadAllBytes(romPath), GameFrame.Create(new RamImage(Savestate.ExtractRam(st))));
    }

    [Fact]
    public void B3313_arrivals_are_the_nodes_the_engine_could_warp_to()
    {
        if (Load("b3313.z64", "start_20261019_192303.st") is not var (rom, frame))
            return;
        var levels = RomLevels.Scan(rom, frame)!;
        Assert.Equal(31, levels.Count);
        var level13area1 = levels[13].Single(a => a.Index == 1).Arrivals;
        Assert.Contains(0x42, level13area1);           // warping there worked in the engine
        Assert.Contains(0x30, level13area1);
        Assert.DoesNotContain(0x41, level13area1);     // these crashed the level script (no spawn object)
        Assert.DoesNotContain(0x20, level13area1);
        Assert.Contains(5, levels[9].Single(a => a.Index == 6).Arrivals);
    }

    [Fact]
    public void Power_star_madness_has_its_own_areas()
    {
        if (Load("SM64 POWER STAR MADNESS (v1.0 QoL).z64", "start_20261019_201641.st") is not var (rom, frame))
            return;
        var levels = RomLevels.Scan(rom, frame)!;
        Assert.Equal(93, levels.Values.Sum(a => a.Count));
        Assert.Equal([10, 20, 21, 22], levels[9].Single(a => a.Index == 1).Arrivals);
    }
}
