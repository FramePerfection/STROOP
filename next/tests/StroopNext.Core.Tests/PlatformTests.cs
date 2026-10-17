using StroopNext.Memory;
using StroopNext.Platform;

namespace StroopNext.Core.Tests;

public class PlatformTests
{
    [Fact]
    public void Linux_maps_are_parsed_with_paths_containing_spaces_and_anonymous_mappings()
    {
        const string maps = """
            55d0c8a00000-55d0c8a2c000 r--p 00000000 08:02 1835123                    /usr/bin/mupen64plus
            7f3a10000000-7f3a10800000 rw-p 00000000 00:00 0
            7f3a20000000-7f3a20021000 ---p 00000000 00:00 0
            7f3a30000000-7f3a30100000 r-xp 00002000 08:02 1835200                    /home/me/My Emulators/libmupen64plus.so.2.0.0
            7f3a40000000-7f3a40001000 rw-s 00000000 00:05 42                         /dev/dri/card0
            7ffd5c3e0000-7ffd5c401000 rw-p 00000000 00:00 0                          [stack]
            ffffffffff600000-ffffffffff601000 --xp 00000000 00:00 0                  [vsyscall]
            garbage line
            """;
        var entries = LinuxProcessMemory.ParseMaps(maps);

        Assert.Equal(6, entries.Count);
        Assert.Equal(0x55d0c8a00000L, (long)entries[0].Start);
        Assert.Equal(0x55d0c8a2c000L, (long)entries[0].End);
        Assert.Equal("/usr/bin/mupen64plus", entries[0].Path);
        Assert.True(entries[0].Readable);

        Assert.Equal("", entries[1].Path);
        var anonymous = entries[1].ToRegion();
        Assert.Equal(0x7f3a10000000L, (long)anonymous.Base);
        Assert.Equal(0x800000, (long)anonymous.Size);
        Assert.True(anonymous.Readable);

        Assert.False(entries[2].Readable);
        Assert.Equal("/home/me/My Emulators/libmupen64plus.so.2.0.0", entries[3].Path);
        Assert.False(entries[4].ToRegion().Readable);
        Assert.Equal("[stack]", entries[5].Path);
    }

    [Fact]
    public void Scan_finds_rdram_at_a_non_zero_offset_and_sizes_it()
    {
        var memory = new FakeMemory();
        memory.AddRegion(0x10000, new byte[0x1000]);
        var region = new byte[N64.RamSize4MB + 0x3000];
        WriteSignature(region, 0x2A0C);
        memory.AddRegion(0x20000000, region);

        using var source = ProcessRamSource.Attach(memory, Candidate);

        Assert.Equal((nint)(0x20000000 + 0x2A0C), source.RamBase);
        Assert.Equal("memory scan", source.DetectionMethod);
        Assert.Equal(N64.RamSize4MB, source.Size);
        Assert.True(source.IsWritable);
        var probe = new byte[RdramSignature.Length];
        Assert.True(source.TryReadRange(0, probe));
        Assert.True(RdramSignature.Matches(probe));
    }

    [Fact]
    public void Scan_prefers_the_copy_whose_game_state_changes()
    {
        var memory = new FakeMemory();
        var stale = new byte[N64.RamSize4MB];
        var live = new byte[N64.RamSize4MB];
        WriteSignature(stale, 0);
        WriteSignature(live, 0);
        memory.AddRegion(0x10000000, stale);
        memory.AddRegion(0x30000000, live);
        memory.BeforeRead = () => live[0x32C000]++;

        using var source = ProcessRamSource.Attach(memory, Candidate);

        Assert.Equal((nint)0x30000000, source.RamBase);
    }

    [Fact]
    public void Symbol_pointer_wins_over_the_scan()
    {
        var memory = new FakeMemory { PointerSize = 4 };
        var ram = new byte[N64.RamSize8MB];
        WriteSignature(ram, 0);
        memory.AddRegion(0x40000000, ram);
        memory.AddRegion(0x1000, BitConverter.GetBytes(0x40000000u));
        memory.Symbols["CORE_RDRAM"] = 0x1000;

        using var source = ProcessRamSource.Attach(memory, Candidate);

        Assert.Equal("CORE_RDRAM symbol", source.DetectionMethod);
        Assert.Equal(N64.RamSize8MB, source.Size);
    }

    [Fact]
    public void Missing_rdram_is_a_user_facing_error_and_releases_the_process()
    {
        var memory = new FakeMemory();
        memory.AddRegion(0x20000000, new byte[N64.RamSize4MB]);
        var error = Assert.Throws<InvalidOperationException>(() => ProcessRamSource.Attach(memory, Candidate));
        Assert.Contains("no SM64 RAM", error.Message);
        Assert.True(memory.Disposed);
    }

    [Theory]
    [InlineData("mupen64plus", true)]
    [InlineData("mupen64plus-ui-console", true)]
    [InlineData("RMG", true)]
    [InlineData("retroarch", true)]
    [InlineData("RetroArch", true)]
    [InlineData("simple64-gui", true)]
    [InlineData("ares", true)]
    [InlineData("mupen64.exe", true)]
    [InlineData("Project64", true)]
    [InlineData("notepad", false)]
    [InlineData("shares", false)]
    [InlineData("rmgr", false)]
    public void Emulator_names_are_recognised(string processName, bool expected)
        => Assert.Equal(expected, EmulatorProfiles.LooksLikeEmulator(processName));

    [Fact]
    public void Timer_resolution_can_be_taken_and_released_on_any_os()
    {
        using var timer = TimerResolution.OneMillisecond();
    }

    private static readonly EmulatorCandidate Candidate = new(1234, "mupen64plus", "", null);

    private static void WriteSignature(byte[] ram, int offset)
    {
        byte[] vector = [0x00, 0x80, 0x1A, 0x3C, 0x80, 0x01, 0x5A, 0x27, 0x08, 0x00, 0x40, 0x03, 0x00, 0x00, 0x00, 0x00];
        for (int i = 0; i < 4; i++)
            vector.CopyTo(ram, offset + i * 0x80);
    }

    private sealed class FakeMemory : IProcessMemory
    {
        private readonly List<(nint Base, byte[] Bytes)> _regions = [];

        public Dictionary<string, nint> Symbols { get; } = [];
        public Action? BeforeRead { get; set; }
        public bool Disposed { get; private set; }
        public bool IsWritable => true;
        public bool IsAlive => !Disposed;
        public int PointerSize { get; init; } = 8;

        public void AddRegion(nint at, byte[] bytes)
        {
            _regions.Add((at, bytes));
            _regions.Sort((a, b) => a.Base.CompareTo(b.Base));
        }

        public bool Read(nint address, Span<byte> buffer)
        {
            BeforeRead?.Invoke();
            return Locate(address, buffer.Length) is { } span && span.Span.TryCopyTo(buffer);
        }

        public bool Write(nint address, ReadOnlySpan<byte> buffer)
            => Locate(address, buffer.Length) is { } span && buffer.TryCopyTo(span.Span);

        private Memory<byte>? Locate(nint address, int length)
        {
            foreach (var (start, bytes) in _regions)
                if (address >= start && address + length <= start + bytes.Length)
                    return bytes.AsMemory((int)(address - start), length);
            return null;
        }

        public IEnumerable<MemoryRegion> Regions() => _regions.Select(r => new MemoryRegion(r.Base, r.Bytes.Length, true));
        public nint? ModuleBase(string moduleName) => null;
        public nint? Symbol(string name) => Symbols.TryGetValue(name, out nint address) ? address : null;
        public void Dispose() => Disposed = true;
    }
}
