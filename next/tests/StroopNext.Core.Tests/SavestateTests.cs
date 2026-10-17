using StroopNext.Game;
using StroopNext.Memory;
using StroopNext.Session;
using StroopNext.Variables;

namespace StroopNext.Core.Tests;

public class SavestateTests
{
    private static string? Find(string relative)
    {
        string root = Environment.GetEnvironmentVariable("STROOP_TEST_SAVESTATES") ?? @"D:\mupen";
        string path = Path.Combine(root, relative);
        return File.Exists(path) ? path : null;
    }

    [Fact]
    public void Lz4_savestate_decodes_mario_surfaces_and_objects()
    {
        if (Find(@"sm64-tas-cpu\MOVIE_0STAR\03_avant_pont.st") is not { } path)
            return;

        using var source = Savestate.Load(path);
        var bytes = new byte[source.Size];
        Assert.True(source.TryRead(bytes));
        var frame = GameFrame.Create(new RamImage(bytes));

        Assert.Equal(GameVersion.US, frame.Version);
        Assert.Equal((short)16, frame.LevelIndex);
        Assert.Equal(1, frame.AreaIndex);
        var m = frame.Mario!;
        Assert.Equal(-818.66, m.Position.X, 2);
        Assert.Equal(759, m.Position.Y, 2);
        Assert.Equal(1730.26, m.Position.Z, 2);
        Assert.Equal(49.2, m.HSpeed, 1);
        Assert.Equal(0x479u, m.Action);
        Assert.NotNull(frame.Services.Actions.NameOf(m.Action));

        Assert.Equal(885, frame.Surfaces.All.Count);
        Assert.Equal(879, frame.Surfaces.StaticCount);
        Assert.NotNull(frame.Surfaces.Find(m.FloorTri));
        Assert.Equal(SurfaceClass.Floor, frame.Surfaces.Find(m.FloorTri)!.Class);
        Assert.InRange(frame.Surfaces.Find(m.FloorTri)!.HeightAt(m.Position.X, m.Position.Z) - m.FloorY, -1f, 1f);

        Assert.NotEmpty(frame.Objects.All);
        Assert.Equal("Mario", frame.Objects.Find(m.MarioObject)!.Name);

        Assert.All(MarioVariables.All, v => Assert.NotEqual(Variable.NotAvailable, v.Format(frame, DisplayOptions.Default)));
    }

    [Fact]
    public void Rdram_signature_recognises_real_ram_and_is_found_inside_a_larger_buffer()
    {
        if (Find(@"sm64-tas-cpu\MOVIE_0STAR\00_reset.st") is not { } path)
            return;
        using var source = Savestate.Load(path);
        var ram = new byte[source.Size];
        source.TryRead(ram);
        Assert.True(RdramSignature.Matches(ram));
        Assert.False(RdramSignature.Matches(ram.AsSpan(4)));

        var haystack = new byte[0x1000];
        ram.AsSpan(0, 0x400).CopyTo(haystack.AsSpan(0x2A0));
        Assert.Equal([0x2A0], RdramSignature.FindAll(haystack, haystack.Length));
    }

    [Fact]
    public void Gzip_savestate_loads()
    {
        if (Find(@"Nouveau dossier\igloo.st") is not { } path)
            return;
        using var source = Savestate.Load(path);
        var bytes = new byte[source.Size];
        source.TryRead(bytes);
        Assert.Equal(GameVersion.US, GameVersionDetector.Detect(new RamImage(bytes)));
    }
}
