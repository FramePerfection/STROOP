namespace StroopNext.Game;

public static class Sm64Addresses
{
    public static readonly Versioned<uint> MarioStruct = new(0x8033B170, 0x80339E00, 0x8031D9C0, 0x80309430);
    public static readonly Versioned<uint> MarioObjectPointer = new(0x80361158, 0x8035FDE8, 0x80343318, null);
    public static readonly Versioned<uint> StoodOnObjectPointer = new(0x80330E34, 0x8032FED4, 0x80310564, 0x802FCFF4);
    public static readonly Versioned<uint> ObjectPool = new(0x8033D488, 0x8033C118, 0x8031F648, 0x8030B0B8);
    public static readonly Versioned<uint> ObjectLists = new(0x8033CBE0, 0x8033B870, 0x80343428, 0x8032EE98);
    public static readonly Versioned<uint> GlobalTimer = new(0x8032D5D4, 0x8032C694, 0x8030CD04, 0x802F9784);
    public static readonly Versioned<uint> SurfacePool = new(0x8038EE9C, 0x8038EE9C, null, null);
    public static readonly Versioned<uint> StaticSurfaceCount = new(0x80361178, 0x8035FE08, 0x80343338, null);
    public static readonly Versioned<uint> TotalSurfaceCount = new(0x80361170, 0x8035FE00, 0x80343330, null);
    public static readonly Versioned<uint> Camera = new(0x8033C520, 0x8033B1B0, 0x8031EC78, 0x8030A7E0);
    public static readonly Versioned<uint> SegmentTable = new(0x8033B400, 0x8033A090, 0x8031DC58, 0x803096C8);
    public static readonly Versioned<uint> CurrentLevel = new(0x8032DDF8, 0x8032CE98, null, null);
    public static readonly Versioned<uint> CurrentAreaPointer = new(0x8032DDCC, 0x8032CE6C, null, 0x802F9F9C);
    public static readonly Versioned<uint> Areas = new(0x8033B8D0, 0x8033A560, null, 0x80309B90);
    public const uint AreaStructSize = 0x3C;

    public static readonly Versioned<uint> CurrentRoom = new(0x80361250, 0x8035FEE0, null, null);
    public static readonly Versioned<uint> SwimStrength = new(0x8032DD34, 0x8032CDD4, 0x8030D464, 0x802F9F04);
    public static readonly Versioned<uint> JustTeleported = new(0x8032DA94, 0x8032CB34, null, null);
    public static readonly Versioned<uint> BlinkState = new(0x800F0A5C, 0x800EDBDC, null, null);
    public static readonly Versioned<uint> BlinkTimer = new(0x8032DF08, 0x8032CFA8, null, null);
    public static readonly Versioned<uint> Spawn = new(0x8033B4B0, 0x8033A160, null, null);

    // level_update.c: what makes the game leave the level (see LevelWarp). JP has no name for sWarpCheckpointActive.
    public static readonly Versioned<uint> PlayMode = new(0x8033B238, 0x80339EC8, null, null);
    public static readonly Versioned<uint> TransitionTimer = new(0x8033B23C, 0x80339ECC, null, null);
    public static readonly Versioned<uint> TransitionUpdate = new(0x8033B240, 0x80339ED0, null, null);
    public static readonly Versioned<uint> WarpDestination = new(0x8033B248, 0x80339ED8, null, null);
    public static readonly Versioned<uint> CurrentAct = new(0x8033BAC8, 0x8033A758, null, null);
    public static readonly Versioned<uint> WarpCheckpointActive = new(0x8032D944, null, null, null);
    public static readonly Versioned<uint> LevelToCourseNum = new(0x8032DD98, 0x8032CE38, null, null);
    public static readonly Versioned<uint> WarpBhvSpawnTable = new(0x8032DDFC, 0x8032CE9C, null, null);
    public static readonly Versioned<uint> ArctanTable = new(0x8038B000, 0x8038B000, null, null);
}

public sealed record Sm64Layout(
    GameVersion Version,
    uint? MarioStruct,
    uint? MarioObjectPointer,
    uint? StoodOnObjectPointer,
    uint? ObjectPool,
    uint? ObjectLists,
    uint? GlobalTimer,
    uint? SurfacePool,
    uint? StaticSurfaceCount,
    uint? TotalSurfaceCount,
    uint? Camera,
    uint? SegmentTable,
    uint? CurrentLevel,
    uint? CurrentAreaPointer,
    uint? Areas)
{
    private static readonly Dictionary<GameVersion, Sm64Layout> Cache = Enum.GetValues<GameVersion>().ToDictionary(v => v, Build);

    public static Sm64Layout For(GameVersion version) => Cache[version];

    private static Sm64Layout Build(GameVersion v) => new(
        v,
        Sm64Addresses.MarioStruct.For(v),
        Sm64Addresses.MarioObjectPointer.For(v),
        Sm64Addresses.StoodOnObjectPointer.For(v),
        Sm64Addresses.ObjectPool.For(v),
        Sm64Addresses.ObjectLists.For(v),
        Sm64Addresses.GlobalTimer.For(v),
        Sm64Addresses.SurfacePool.For(v),
        Sm64Addresses.StaticSurfaceCount.For(v),
        Sm64Addresses.TotalSurfaceCount.For(v),
        Sm64Addresses.Camera.For(v),
        Sm64Addresses.SegmentTable.For(v),
        Sm64Addresses.CurrentLevel.For(v),
        Sm64Addresses.CurrentAreaPointer.For(v),
        Sm64Addresses.Areas.For(v));
}

public static class Angle
{
    public const double UnitsPerTurn = 65536.0;

    public static double ToDegrees(double units) => units * 360.0 / UnitsPerTurn;
    public static double ToRadians(double units) => units * Math.PI * 2 / UnitsPerTurn;
    public static ushort Normalize(double units) => (ushort)(long)Math.Round(units);

    public static (double X, double Z) Direction(double units)
    {
        double r = ToRadians(units);
        return (Math.Sin(r), Math.Cos(r));
    }

    public static ushort Towards(double x1, double z1, double x2, double z2)
        => Normalize(Math.Atan2(x2 - x1, z2 - z1) * UnitsPerTurn / (Math.PI * 2) + UnitsPerTurn);

    public static int Difference(int a, int b) => (short)(a - b);
}
