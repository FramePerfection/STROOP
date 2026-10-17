using StroopNext.Memory;

namespace StroopNext.Game;

public enum GameVersion { US, JP, SH, EU }

public static class GameVersionDetector
{
    public const uint TellAddress = 0x802F0000;

    public static GameVersion? Detect(RamImage ram) => ram.U32(TellAddress) switch
    {
        0xC58400A4 => GameVersion.US,
        0x27BD0020 => GameVersion.JP,
        0x8F250004 => GameVersion.SH,
        0x0C0BD4AC => GameVersion.EU,
        _ => null,
    };
}

public readonly record struct Versioned<T>(T? US, T? JP, T? SH, T? EU) where T : struct
{
    public static Versioned<T> All(T value) => new(value, value, value, value);

    public T? For(GameVersion version) => version switch
    {
        GameVersion.US => US,
        GameVersion.JP => JP,
        GameVersion.SH => SH,
        GameVersion.EU => EU,
        _ => null,
    };
}
