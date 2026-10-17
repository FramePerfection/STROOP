using System.Numerics;
using StroopNext.Memory;

namespace StroopNext.Game;

public static class ObjectOffsets
{
    public const uint StructSize = 0x260;
    public const int SlotCount = 240;
    public const int ListCount = 13;
    public const uint ListNodeSize = 0x68;

    public const uint Header = 0x00;
    public const uint ScaleX = 0x2C;
    public const uint Next = 0x60;
    public const uint Parent = 0x68;
    public const uint ActiveFlags = 0x74;
    public const uint X = 0xA0;
    public const uint Y = 0xA4;
    public const uint Z = 0xA8;
    public const uint YSpeed = 0xB0;
    public const uint HSpeed = 0xB8;
    public const uint MovingYaw = 0xCA;
    public const uint FacingYaw = 0xD6;
    public const uint Action = 0x14C;
    public const uint Timer = 0x154;
    public const uint HomeX = 0x164;
    public const uint HitboxRadius = 0x1F8;
    public const uint HitboxHeight = 0x1FC;
    public const uint HurtboxRadius = 0x200;
    public const uint HurtboxHeight = 0x204;
    public const uint HitboxDownOffset = 0x208;
    public const uint Behavior = 0x20C;

    public static readonly int[] ProcessingOrder = [0x0B, 0x09, 0x0A, 0x00, 0x05, 0x04, 0x02, 0x06, 0x08, 0x0C];
}

public sealed record GameObject(
    int Slot,
    int ProcessIndex,
    int List,
    uint Address,
    uint Behavior,
    uint SegmentedBehavior,
    string Name,
    Vector3 Position,
    Vector3 Home,
    float HSpeed,
    float YSpeed,
    ushort FacingYaw,
    ushort MovingYaw,
    float HitboxRadius,
    float HitboxHeight,
    float HitboxDownOffset,
    float HurtboxRadius,
    float HurtboxHeight,
    uint Action,
    int Timer,
    uint Parent)
{
    public int SlotNumber => Slot + 1;
}

public sealed class ObjectList
{
    public static readonly ObjectList Empty = new([]);

    private readonly Dictionary<uint, GameObject> _byAddress;

    public ObjectList(IReadOnlyList<GameObject> objects)
    {
        All = objects;
        _byAddress = objects.ToDictionary(o => o.Address);
    }

    public IReadOnlyList<GameObject> All { get; }

    public GameObject? Find(uint address) => _byAddress.GetValueOrDefault(address);

    public static ObjectList Read(RamImage ram, Sm64Layout layout, ObjectNames names)
    {
        if (layout.ObjectLists is not uint lists || layout.ObjectPool is not uint pool)
            return Empty;

        uint bankStart = 0;
        if (layout.SegmentTable is uint segments)
            bankStart = ram.U32(segments + 0x13 * 4) & 0x1FFFFFFF;

        var result = new List<GameObject>();
        var seen = new HashSet<uint>();
        foreach (int list in ObjectOffsets.ProcessingOrder)
        {
            uint head = lists + (uint)list * ObjectOffsets.ListNodeSize;
            uint current = ram.U32(head + ObjectOffsets.Next);
            while (current != head && result.Count < ObjectOffsets.SlotCount)
            {
                uint relative = current - pool;
                if (!ram.IsPointer(current) || current < pool || relative % ObjectOffsets.StructSize != 0 || relative / ObjectOffsets.StructSize >= ObjectOffsets.SlotCount || !seen.Add(current))
                    break;
                result.Add(ReadObject(ram, current, (int)(relative / ObjectOffsets.StructSize), result.Count, list, bankStart, layout.Version, names));
                current = ram.U32(current + ObjectOffsets.Next);
            }
        }
        return new ObjectList(result);
    }

    private static GameObject ReadObject(RamImage ram, uint a, int slot, int processIndex, int list, uint bankStart, GameVersion version, ObjectNames names)
    {
        uint behavior = ram.U32(a + ObjectOffsets.Behavior);
        uint physical = behavior & 0x1FFFFFFF;
        uint segmented = behavior != 0 && bankStart != 0 && physical >= bankStart ? 0x13000000 + (physical - bankStart) : 0;
        return new GameObject(
            slot,
            processIndex,
            list,
            a,
            behavior,
            segmented,
            names.Lookup(segmented, version),
            new Vector3(ram.F32(a + ObjectOffsets.X), ram.F32(a + ObjectOffsets.Y), ram.F32(a + ObjectOffsets.Z)),
            new Vector3(ram.F32(a + ObjectOffsets.HomeX), ram.F32(a + ObjectOffsets.HomeX + 4), ram.F32(a + ObjectOffsets.HomeX + 8)),
            ram.F32(a + ObjectOffsets.HSpeed),
            ram.F32(a + ObjectOffsets.YSpeed),
            ram.U16(a + ObjectOffsets.FacingYaw),
            ram.U16(a + ObjectOffsets.MovingYaw),
            ram.F32(a + ObjectOffsets.HitboxRadius),
            ram.F32(a + ObjectOffsets.HitboxHeight),
            ram.F32(a + ObjectOffsets.HitboxDownOffset),
            ram.F32(a + ObjectOffsets.HurtboxRadius),
            ram.F32(a + ObjectOffsets.HurtboxHeight),
            ram.U32(a + ObjectOffsets.Action),
            ram.S32(a + ObjectOffsets.Timer),
            ram.U32(a + ObjectOffsets.Parent));
    }
}

public sealed record CameraState(Vector3 Position, Vector3 Focus, ushort Yaw)
{
    public static CameraState? Read(RamImage ram, Sm64Layout layout)
    {
        if (layout.Camera is not uint c)
            return null;
        return new CameraState(
            new Vector3(ram.F32(c + 0x184), ram.F32(c + 0x188), ram.F32(c + 0x18C)),
            new Vector3(ram.F32(c + 0x178), ram.F32(c + 0x17C), ram.F32(c + 0x180)),
            ram.U16(c + 0x1C6));
    }
}
