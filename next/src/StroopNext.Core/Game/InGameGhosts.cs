using System.Buffers.Binary;
using StroopNext.Map;
using StroopNext.Memory;
using StroopNext.Session;

namespace StroopNext.Game;

public sealed class PatchSnapshot
{
    private const uint Magic = 0x534E5047;

    private readonly record struct Entry(uint Address, byte[] Original, byte[] Written, ushort Group);

    private readonly List<Entry> _entries = new();
    private readonly HashSet<uint> _saved = new();
    private ushort _lastGroup;

    public bool IsEmpty => _entries.Count == 0;

    public ushort NewGroup() => ++_lastGroup;

    public void Save(RamImage ram, uint address, byte[] written, ushort group = 0)
    {
        if (!_saved.Add(address))
            return;
        var original = new byte[written.Length];
        for (int i = 0; i < original.Length; i++)
            original[i] = ram.U8(address + (uint)i);
        _entries.Add(new Entry(address, original, (byte[])written.Clone(), group));
        _lastGroup = Math.Max(_lastGroup, group);
    }

    public RamPatch Restore(RamImage ram)
    {
        var changed = new HashSet<ushort>();
        foreach (var e in _entries)
            if (e.Group != 0 && !Still(ram, e.Address, e.Written))
                changed.Add(e.Group);
        var patch = new RamPatch();
        for (int i = _entries.Count - 1; i >= 0; i--)
            if (!changed.Contains(_entries[i].Group))
                patch.WriteBytes(_entries[i].Address, _entries[i].Original);
        return patch;
    }

    private static bool Still(RamImage ram, uint address, byte[] bytes)
    {
        if (!ram.IsValid(address, bytes.Length))
            return false;
        for (int i = 0; i < bytes.Length; i++)
            if (ram.U8(address + (uint)i) != bytes[i])
                return false;
        return true;
    }

    public byte[] Serialize()
    {
        using var stream = new MemoryStream();
        var word = new byte[4];
        void U32(uint v) { BinaryPrimitives.WriteUInt32BigEndian(word, v); stream.Write(word); }
        U32(Magic);
        U32((uint)_entries.Count);
        foreach (var e in _entries)
        {
            U32(e.Address);
            U32((uint)e.Group << 16 | (uint)e.Original.Length);
            stream.Write(e.Original);
            stream.Write(e.Written);
            while (stream.Length % 4 != 0)
                stream.WriteByte(0);
        }
        return stream.ToArray();
    }

    public static PatchSnapshot? Read(RamImage ram, uint address, int maxLength)
    {
        if (!ram.IsValid(address, maxLength) || ram.U32(address) != Magic)
            return null;
        uint count = ram.U32(address + 4);
        var snapshot = new PatchSnapshot();
        uint at = address + 8, end = address + (uint)maxLength;
        for (uint i = 0; i < count; i++)
        {
            if (at + 8 > end)
                return null;
            uint target = ram.U32(at);
            uint info = ram.U32(at + 4);
            int length = (int)(info & 0xFFFF);
            ushort group = (ushort)(info >> 16);
            if (at + 8 + 2 * (uint)length > end)
                return null;
            var original = new byte[length];
            var written = new byte[length];
            for (int k = 0; k < length; k++)
            {
                original[k] = ram.U8(at + 8 + (uint)k);
                written[k] = ram.U8(at + 8 + (uint)(length + k));
            }
            if (!snapshot._saved.Add(target))
                return null;
            snapshot._entries.Add(new Entry(target, original, written, group));
            snapshot._lastGroup = Math.Max(snapshot._lastGroup, group);
            at += 8 + ((2 * (uint)length + 3) & ~3u);
        }
        return snapshot;
    }
}

public enum InGameGhostState
{
    Unavailable,
    Disabled,
    Installed,
    Running,
    Disabling,
}

public sealed class InGameGhosts
{
    public const ushort DefaultBaseHi = 0x8040;
    public const int MaxGhosts = 16;

    private const uint GuardOffset = 0x8000, LoopOffset = 0x8800, HatsOffset = 0x8200, LightsOffset = 0x8300, BufferOffset = 0x9B00;
    private const uint SnapshotOffset = 0x4000;
    private const int SnapshotMax = 0x3000;
    private const uint ExpansionRam = 0x80400000;
    private const uint FileLoop = 0x80408000, FileHats = 0x80408200;
    private const uint SlotSize = 0x20, BufferSize = 0x1000, NodeStride = 0x68;

    private const uint BodyBuffers = 0x60000, BodySave = 0x7104, BodyFlag = 0x7130, InNode = 0x7140, OutNode = 0x7160;
    private const uint InCode = 0x8600, OutCode = 0x8700;
    public const int BodyBytes = 0x14;

    private readonly record struct GameSymbols(uint CurGraphNodeObject, uint GlobalTimer, uint BodyStates);

    private static GameSymbols Symbols(GameVersion version) => version == GameVersion.JP
        ? new GameSymbols(0x8032CFA0, 0x8032C694, 0x8033A040)
        : new GameSymbols(0x8032DF00, 0x8032D5D4, 0x8033B3B0);
    private const float Far = 30_000;

    private static readonly uint[] HatDisplayLists = [0x040119A0, 0x04011A90, 0x04011B80, 0x04012030];

    private readonly Dictionary<Ghost, (GhostFrame Frame, uint Animation)> _lastValid = new();

    public InGameGhosts(ushort baseHi = DefaultBaseHi) => BaseHi = baseHi;

    public ushort BaseHi { get; }

    private uint Base => (uint)BaseHi << 16;
    private uint CountAddress => Base + 0x7FFF;
    private uint DisableAddress => Base + 0x7FFC;
    private uint FirstNodePointer => Base + 0x7FF8;

    public static IEnumerable<uint> GameGhostNodes(RamImage ram, ushort baseHi = DefaultBaseHi)
    {
        uint first = ((uint)baseHi << 16) + 0x7FF8;
        if (!ram.IsValid(first, 4))
            yield break;
        for (uint i = 0; i < 40; i++)
        {
            uint node = ram.U32(first - i * NodeStride);
            if (!ram.IsPointer(node))
                yield break;
            yield return node;
        }
    }

    public bool CanRestore(GameFrame frame) => StoredSnapshot(frame) is not null;

    private PatchSnapshot? StoredSnapshot(GameFrame frame) => PatchSnapshot.Read(frame.Ram, Base + SnapshotOffset, SnapshotMax);

    private static bool InExpansionRam(uint address) => address >= ExpansionRam;

    public static RomHack HackFor(GameVersion version)
        => RomHack.Parse(EmbeddedData.LoadText(version == GameVersion.JP ? "GhostHackJP.hck" : "GhostHackUS.hck"));

    private static uint? HookAddress(GameVersion? version) => version switch
    {
        GameVersion.US => 0x8027B188,
        GameVersion.JP => 0x8027ABD8,
        _ => null,
    };

    public static string? Unavailable(GameFrame frame)
    {
        if (!frame.IsKnownGame)
            return "No SM64 game.";
        if (HookAddress(frame.Version) is null)
            return $"In-game ghosts exist for SM64 US and JP only (this is {frame.Version}).";
        if (frame.Ram.Length < N64.RamSize8MB)
            return "In-game ghosts need the Expansion Pak (8 MB of RAM): enable it in the emulator's settings.";
        if (frame.Ram.U32(0x80000000) == 0)
            return "The game is not booted yet.";
        if (BaseFor(frame) is null)
            return "This game uses the whole Expansion Pak RAM: no room for the in-game ghosts.";
        return null;
    }

    // The ghosts take Footprint bytes from their base: code and buffers at base..base+0xAB00, the body buffers at
    // base+0x60000. ROM hacks can use the Expansion Pak themselves (Power Star Madness and B3313 fill 0x80400000..):
    // the ghosts then go to the first free 64 KB-aligned area instead of overwriting the game.
    private const uint Footprint = BodyBuffers + MaxGhosts * BufferSize;
    private static ushort? _relocated;

    /// <summary>The base of installed ghosts (from the hook), else where they can be installed; null when no room.</summary>
    public static ushort? BaseFor(GameFrame frame)
    {
        var ram = frame.Ram;
        if (InstalledBaseHi(ram, frame.Version) is ushort installed)
            return installed;
        if (Ours(ram, frame.Version, DefaultBaseHi) || IsFree(ram, (uint)DefaultBaseHi << 16, Footprint, 0x100))
            return DefaultBaseHi;
        if (_relocated is ushort r && IsFree(ram, (uint)r << 16, Footprint, 0x100))
            return r;
        for (uint b = ExpansionRam + 0x10000; b + Footprint <= ExpansionRam + 0x400000; b += 0x10000)
            if (IsFree(ram, b, Footprint, 4))
            {
                _relocated = (ushort)(b >> 16);
                return _relocated;
            }
        return null;
    }

    /// <summary>The base hi of ghosts whose hook is in the game (US or JP hook site), else null.</summary>
    public static ushort? InstalledBaseHi(RamImage ram, GameVersion? version = null)
        => InstalledBaseHi(a => ram.IsValid(a, 4) ? ram.U32(a) : 0, version);

    public static ushort? InstalledBaseHi(Func<uint, uint> u32, GameVersion? version = null)
    {
        foreach (var v in version is { } known ? [known] : new[] { GameVersion.US, GameVersion.JP })
        {
            if (HookAddress(v) is not uint hook)
                continue;
            uint word = u32(hook);
            uint b = (0x80000000u | ((word & 0x03FFFFFF) << 2)) - GuardOffset;
            if (word >> 26 == 3 && (b & 0xFFFF) == 0 && b >= ExpansionRam && b + Footprint <= ExpansionRam + 0x400000)
                return (ushort)(b >> 16);
        }
        return null;
    }

    // Ghosts installed then removed leave their snapshot or code at the base: the area is still theirs.
    private static bool Ours(RamImage ram, GameVersion? version, ushort baseHi)
    {
        uint b = (uint)baseHi << 16;
        if (PatchSnapshot.Read(ram, b + SnapshotOffset, SnapshotMax) is not null)
            return true;
        byte[]? loop = HackFor(version!.Value).Entries.FirstOrDefault(e => e.Address == FileLoop).Bytes;
        return loop is { Length: >= 0x40 }
            && Enumerable.Range(0, 0x10).All(i => ram.U32(b + LoopOffset + 4 * (uint)i) == BinaryPrimitives.ReadUInt32BigEndian(loop.AsSpan(4 * i)));
    }

    // Free: no more than 64 non-zero words among those read every `stride` bytes (vanilla SM64 leaves the Expansion
    // Pak empty; PSM and B3313 have thousands of non-zero words in their areas).
    private static bool IsFree(RamImage ram, uint start, uint size, uint stride)
    {
        if (!ram.IsValid(start, (int)size))
            return false;
        int used = 0;
        for (uint a = start; a < start + size; a += stride)
            if (ram.U32(a) != 0 && ++used > (stride == 4 ? 64 : 0))
                return false;
        return true;
    }

    public InGameGhostState State(GameFrame frame)
    {
        if (Unavailable(frame) is not null)
            return InGameGhostState.Unavailable;
        uint hook = HookAddress(frame.Version)!.Value;
        bool installed = frame.Ram.U32(hook) == Jal(Base + GuardOffset);
        if (!installed)
            return InGameGhostState.Disabled;
        if (frame.Ram.U8(DisableAddress) == 0xFF)
            return InGameGhostState.Disabling;
        return frame.Ram.IsPointer(frame.Ram.U32(FirstNodePointer)) ? InGameGhostState.Running : InGameGhostState.Installed;
    }

    private static uint Jal(uint target) => (0x0Cu << 24) | ((target & 0x00FFFFFF) >> 2);

    public RamPatch? Install(GameFrame frame, out string? error)
    {
        error = Unavailable(frame);
        if (error is not null)
            return null;
        var version = frame.Version!.Value;
        var ram = frame.Ram;
        var snapshot = new PatchSnapshot();
        var patch = new RamPatch();
        void Write(uint address, byte[] bytes)
        {
            if (!InExpansionRam(address))
                snapshot.Save(ram, address, bytes);
            patch.WriteBytes(address, bytes);
        }

        Write(Base + 0x7000, new byte[0x1000]);

        foreach (var entry in HackFor(version).Entries)
        {
            uint address = entry.Address == FileLoop ? Base + LoopOffset : entry.Address == FileHats ? Base + HatsOffset : entry.Address;
            byte[] bytes = entry.Address == FileLoop ? LoopCode(version) : (byte[])entry.Bytes.Clone();
            if (entry.Address == FileHats)
                SetLui(bytes, 0x70, BaseHi);
            if (address == HookAddress(version))
                continue;
            Write(address, bytes);
        }

        Write(Base + LightsOffset, Lights(MapColor.Rgb(255, 0, 0)));

        var (inCode, outCode) = BodySwitchers(version);
        Write(Base + InCode, inCode);
        Write(Base + OutCode, outCode);
        Write(Base + GuardOffset, Guard());

        Write(HookAddress(version)!.Value, U32(Jal(Base + GuardOffset)));

        patch.WriteBytes(Base + SnapshotOffset, snapshot.Serialize());
        _lastValid.Clear();
        return patch;
    }

    public static bool LoopHasRun(GameFrame frame)
    {
        if (frame.Layout?.MarioObjectPointer is not uint pointer)
            return false;
        uint mario = frame.Ram.U32(pointer);
        return frame.Ram.IsPointer(mario) && (frame.Ram.U16(mario + 2) & 0x40) != 0;
    }

    private static (uint Root, uint First, uint Last)? MarioModel(GameFrame frame)
    {
        var ram = frame.Ram;
        if (frame.Layout?.MarioObjectPointer is not uint pointer || !ram.IsPointer(ram.U32(pointer)))
            return null;
        uint root = ram.U32(ram.U32(pointer) + 0x14);
        uint first = ram.IsPointer(root) ? ram.U32(root + 0x10) : 0;
        if (!ram.IsPointer(first))
            return null;
        uint last = ram.U32(first + 4);
        return ram.IsPointer(last) ? (root, first, last) : null;
    }

    public bool StepTwoPending(GameFrame frame)
        => State(frame) is InGameGhostState.Installed or InGameGhostState.Running
           && LoopHasRun(frame)
           && MarioModel(frame) is { } model && model.First != Base + InNode;

    public RamPatch? ColoredCaps(GameFrame frame)
    {
        if (!StepTwoPending(frame) || frame.Version is not { } version)
            return null;
        if (StoredSnapshot(frame) is not { } snapshot)
            return null;
        var patch = new RamPatch();
        void Write(uint address, byte[] bytes, ushort group)
        {
            snapshot.Save(frame.Ram, address, bytes, group);
            patch.WriteBytes(address, bytes);
        }
        WriteColoredCaps(frame, version, Write, snapshot);
        LinkBodySwitchers(frame, Write, snapshot.NewGroup());
        patch.WriteBytes(Base + SnapshotOffset, snapshot.Serialize());
        return patch;
    }

    private byte[] LoopCode(GameVersion version)
    {
        byte[] bytes = (byte[])HackFor(version).Entries.First(e => e.Address == FileLoop).Bytes.Clone();
        SetLui(bytes, 0xF8, (ushort)(BaseHi + 1));
        foreach (int lui in new[] { 0x44, 0x78, 0x190 })
            SetLui(bytes, lui, BaseHi);
        SetLui(bytes, 0x50, (ushort)(BaseHi + 0x10));
        FixCleanUp(bytes);
        return bytes;
    }

    // While removing: ghosts written with the legacy clean-up can leave nodes after an empty slot, which nothing
    // removes. Puts the fixed clean-up in and packs those nodes from the first slot so it removes them all.
    public RamPatch? Unstick(GameFrame frame)
    {
        if (frame.Version is not { } version)
            return null;
        var ram = frame.Ram;
        var nodes = new List<uint>();
        bool gap = false, packed = true;
        for (uint i = 0; i < MaxGhosts; i++)
        {
            uint node = ram.U32(FirstNodePointer - i * NodeStride);
            if (node - (Base + 0x7000) < 0x1000)
            {
                nodes.Add(node);
                packed &= !gap;
            }
            else
                gap = true;
        }
        byte[] code = LoopCode(version);
        bool fixedCode = Enumerable.Range(0, code.Length / 4).All(i => ram.U32(Base + LoopOffset + 4 * (uint)i) == BinaryPrimitives.ReadUInt32BigEndian(code.AsSpan(4 * i)));
        if (nodes.Count == 0 || packed && fixedCode)
            return null;
        var patch = new RamPatch();
        if (!fixedCode)
            patch.WriteBytes(Base + LoopOffset, code);
        for (int i = 0; i < MaxGhosts; i++)
            patch.WriteU32(FirstNodePointer - (uint)i * NodeStride, i < nodes.Count ? nodes[i] : 0);
        return patch;
    }

    // Legacy ghost_loop.asm's clean-up reloads the slot it just cleared before stepping to the next one, so it
    // removes one node per frame and stops: when the count drops by more than one the other nodes stay in the game,
    // frozen, and removing never completes. Step first, then load; the two entries start one slot early.
    internal static void FixCleanUp(byte[] loop)
    {
        (int Offset, uint Old, uint New)[] fix =
        [
            (0x4C, 0x36317FF8, 0x36318060), // ori s1, s1, PointerToFirstGhost + ghost size
            (0x84, 0x1001004C, 0x5001004C), // beq -> beql r0, at, @CLEAN_UP_EARLY
            (0x88, 0x00000000, 0x26310068), //   addiu s1, s1, ghost size (only when branching)
            (0x1B8, 0x8E240000, 0x2631FF98), // @CLEAN_UP_EARLY: addiu s1, s1, -ghost size
            (0x1BC, 0x1480FFFC, 0x8E240000), //   lw a0, 0 (s1)
            (0x1C0, 0x2631FF98, 0x1480FFFB), //   bnez a0, @CLEAN_UP_LOOP
        ];
        if (loop.Length < 0x1C4 || fix.Any(f => BinaryPrimitives.ReadUInt32BigEndian(loop.AsSpan(f.Offset)) != f.Old))
            return;
        foreach (var f in fix)
            BinaryPrimitives.WriteUInt32BigEndian(loop.AsSpan(f.Offset), f.New);
    }

    internal byte[] Guard()
    {
        const int T0 = Mips.T0, T1 = Mips.T1, T2 = Mips.T2, T3 = Mips.T3, T4 = Mips.T4, T5 = Mips.T5, T6 = Mips.T6, T7 = Mips.T7, R0 = Mips.R0, AT = Mips.AT;
        return new Mips(Base + GuardOffset)
            .Lui(T7, BaseHi)
            .Ori(T6, T7, 0x7FF8)
            .Addiu(T5, R0, MaxGhosts)
            .Label("slot")
            .Lw(T0, 0, T6)
            .Beq(T0, R0, "next").Nop()
            .Subu(T1, T0, T7)
            .Addiu(T1, T1, -0x7000)
            .Sltiu(T1, T1, 0x1000)
            .Beq(T1, R0, "orphan").Nop()
            .Lw(T1, 0xC, T0)
            .Lui(AT, 0x8000).Subu(T2, T1, AT)
            .Lui(AT, 0x0080).Sltu(T2, T2, AT)
            .Beq(T2, R0, "orphan").Nop()
            .Lw(T2, 0x10, T1)
            .Beq(T2, R0, "orphan")
            .Or(T3, T2, R0)
            .Addiu(T4, R0, 0x800)
            .Label("walk")
            .Beq(T3, T0, "next").Nop()
            .Lw(T3, 8, T3)
            .Beq(T3, R0, "done")
            .Addiu(T4, T4, -1)
            .Beq(T4, R0, "done").Nop()
            .Bne(T3, T2, "walk").Nop()
            .Label("orphan")
            .Sw(R0, 0, T6)
            .Label("next")
            .Addiu(T5, T5, -1)
            .Bne(T5, R0, "slot")
            .Addiu(T6, T6, -(short)NodeStride)
            .Label("done")
            .J(Base + LoopOffset).Nop()
            .Assemble();
    }

    internal (byte[] In, byte[] Out) BodySwitchers(GameVersion version)
    {
        var s = Symbols(version);
        uint body = s.BodyStates + 4;
        const int T0 = Mips.T0, T3 = Mips.T3, T4 = Mips.T4, T5 = Mips.T5, T6 = Mips.T6, T7 = Mips.T7, T8 = Mips.T8, T9 = Mips.T9;

        Mips Prologue(Mips m, string ret) => m
            .Addiu(Mips.AT, Mips.R0, 1)
            .Bne(Mips.A0, Mips.AT, ret)
            .Lui(T0, Mips.Hi(s.CurGraphNodeObject))
            .Lw(T0, Mips.Lo(s.CurGraphNodeObject), T0)
            .Lui(T7, BaseHi)
            .Ori(T7, T7, 0x7000)
            .Subu(T8, T0, T7)
            .Sltiu(T8, T8, 0x1000)
            .Beq(T8, Mips.R0, ret)
            .Lui(T3, Mips.Hi(body));

        var inCode = Prologue(new Mips(Base + InCode), "ret");
        for (uint k = 0; k < BodyBytes; k += 4)
            inCode.Lw(T4, Mips.Lo(body + k), T3).Sw(T4, (short)(BodySave - 0x7000 + k), T7);
        inCode.Addiu(T9, Mips.R0, 1).Sb(T9, (short)(BodyFlag - 0x7000), T7)
            .Lbu(T5, 0x60, T0)
            .Addiu(T5, T5, -1)
            .Sll(T5, T5, 12)
            .Lui(T6, Mips.Hi(s.GlobalTimer))
            .Lw(T6, Mips.Lo(s.GlobalTimer), T6)
            .Andi(T6, T6, 0x7F)
            .Sll(T6, T6, 5)
            .Addu(T5, T5, T6)
            .Lui(T6, (ushort)(BaseHi + (BodyBuffers >> 16)))
            .Addu(T5, T5, T6);
        for (uint k = 0; k < BodyBytes; k += 4)
            inCode.Lw(T4, (short)k, T5).Sw(T4, Mips.Lo(body + k), T3);
        inCode.Label("ret").Jr(Mips.RA).Or(Mips.V0, Mips.R0, Mips.R0);

        var outCode = Prologue(new Mips(Base + OutCode), "ret")
            .Lbu(T9, (short)(BodyFlag - 0x7000), T7)
            .Beq(T9, Mips.R0, "ret")
            .Lui(T3, Mips.Hi(body));
        for (uint k = 0; k < BodyBytes; k += 4)
            outCode.Lw(T4, (short)(BodySave - 0x7000 + k), T7).Sw(T4, Mips.Lo(body + k), T3);
        outCode.Sb(Mips.R0, (short)(BodyFlag - 0x7000), T7)
            .Label("ret").Jr(Mips.RA).Or(Mips.V0, Mips.R0, Mips.R0);
        return (inCode.Assemble(), outCode.Assemble());
    }

    private static byte[] GeneratedNode(uint prev, uint next, uint parent, uint function)
    {
        var node = new byte[0x1C];
        BinaryPrimitives.WriteUInt16BigEndian(node, 0x012A);
        BinaryPrimitives.WriteUInt16BigEndian(node.AsSpan(2), 0x0001);
        BinaryPrimitives.WriteUInt32BigEndian(node.AsSpan(4), prev);
        BinaryPrimitives.WriteUInt32BigEndian(node.AsSpan(8), next);
        BinaryPrimitives.WriteUInt32BigEndian(node.AsSpan(0xC), parent);
        BinaryPrimitives.WriteUInt32BigEndian(node.AsSpan(0x14), function);
        return node;
    }

    private static void SetLui(byte[] code, int wordOffset, ushort immediate)
        => BinaryPrimitives.WriteUInt16BigEndian(code.AsSpan(wordOffset + 2, 2), immediate);

    private void LinkBodySwitchers(GameFrame frame, Action<uint, byte[], ushort> write, ushort group)
    {
        if (MarioModel(frame) is not var (root, first, last))
            return;
        uint inNode = Base + InNode, outNode = Base + OutNode;
        write(inNode, GeneratedNode(prev: outNode, next: first, parent: root, function: Base + InCode), 0);
        write(outNode, GeneratedNode(prev: last, next: inNode, parent: root, function: Base + OutCode), 0);
        write(first + 4, U32(inNode), group);
        write(last + 8, U32(outNode), group);
        write(root + 0x10, U32(inNode), group);
    }

    private void WriteColoredCaps(GameFrame frame, GameVersion version, Action<uint, byte[], ushort> write, PatchSnapshot groups)
    {
        var ram = frame.Ram;
        if (frame.Layout?.SegmentTable is not uint segments)
            return;
        uint vanillaOffset = version == GameVersion.JP ? 0x0007BDC0u : 0x0007EC20u;
        uint bank4 = (ram.U32(segments + 0x10) & 0x1FFFFFFF) | 0x80000000;
        uint end = bank4 + (0x100000 - vanillaOffset);
        if (!ram.IsValid(bank4, 4) || !ram.IsValid(end - 4, 4))
            return;
        for (uint a = bank4; a < end; a += 4)
        {
            uint word = ram.U32(a);
            if ((word & 0xFFFF0000) == 0x001B0000 && Array.IndexOf(HatDisplayLists, ram.U32(a + 0x14)) >= 0)
            {
                ushort node = groups.NewGroup();
                write(a, [0x01, 0x2A], node);
                write(a + 0x14, U32(Base + HatsOffset), node);
            }
            else if (word == 0x02580640)
                write(a, U32(0x02587FFF), groups.NewGroup());
            else if (word == 0x06407FFF)
                write(a, U32(0x7FFF7FFF), groups.NewGroup());
        }
        uint headList = bank4 - vanillaOffset + (version == GameVersion.JP ? 0x8D720u : 0x90580u) + 8;
        if (ram.IsValid(headList, 8))
            write(headList, [0xB8, 0, 0, 0, 0, 0, 0, 0], groups.NewGroup());
    }

    private static byte[] U32(uint value)
    {
        var bytes = new byte[4];
        BinaryPrimitives.WriteUInt32BigEndian(bytes, value);
        return bytes;
    }

    public static byte[] Lights(MapColor color)
    {
        byte r = color.R, g = color.G, b = color.B, r2 = (byte)(r / 2), g2 = (byte)(g / 2), b2 = (byte)(b / 2);
        return [r2, g2, b2, 0, r2, g2, b2, 0, r, g, b, 0, r, g, b, 0, 0x28, 0x28, 0x28, 0, 0, 0, 0, 0];
    }

    public RamPatch Feed(GameFrame frame, IReadOnlyList<(Ghost Ghost, MapColor Color)> ghosts, bool transparent)
    {
        var patch = new RamPatch();
        var state = State(frame);
        if (state is InGameGhostState.Unavailable or InGameGhostState.Disabled or InGameGhostState.Disabling)
            return patch;
        int count = Math.Min(ghosts.Count, MaxGhosts);
        patch.WriteU8(CountAddress, (byte)count);
        uint timer = frame.GlobalTimer;
        var buffer = new byte[BufferSize];
        var bodies = new byte[BufferSize];
        for (int g = 0; g < count; g++)
        {
            var (ghost, color) = ghosts[g];
            Array.Clear(buffer);
            bool present = ghost.FrameAt(timer) is not null;
            for (uint tm = 0; tm < 0x80; tm++)
            {
                int slot = (int)((timer + tm) & 0x7F) * (int)SlotSize;
                long index = Ghost.Order(ghost.IndexAt(unchecked(timer + tm + 1)));
                if (index >= 0 && ghost.Frames.TryGetValue((uint)index, out var newFrame))
                {
                    uint previous = _lastValid.TryGetValue(ghost, out var last) ? last.Animation : 0;
                    uint animation = ghost.AnimationSwitches.TryGetValue((uint)index, out var sw) ? sw
                        : previous == 0 && ghost.AnimationSwitches.Count > 0 ? ghost.AnimationSwitches.MinBy(kv => kv.Key).Value
                        : previous;
                    _lastValid[ghost] = (newFrame, animation);
                }
                var (f, anim) = _lastValid.GetValueOrDefault(ghost);
                var span = buffer.AsSpan(slot, (int)SlotSize);
                var p = present ? f.Position : new System.Numerics.Vector3(Far, Far, Far);
                BinaryPrimitives.WriteSingleBigEndian(span[0x00..], p.X);
                BinaryPrimitives.WriteSingleBigEndian(span[0x04..], p.Y);
                BinaryPrimitives.WriteSingleBigEndian(span[0x08..], p.Z);
                uint animationWord = ghost.NonMarioGraphics != 0 ? (anim == 0 ? (uint)(ushort)f.AnimationIndex : anim) : (uint)(ushort)f.AnimationIndex;
                BinaryPrimitives.WriteUInt32BigEndian(span[0x0C..], animationWord);
                BinaryPrimitives.WriteUInt16BigEndian(span[0x10..], (ushort)f.Pitch);
                BinaryPrimitives.WriteUInt16BigEndian(span[0x12..], (ushort)f.Yaw);
                BinaryPrimitives.WriteUInt16BigEndian(span[0x14..], (ushort)f.Roll);
                BinaryPrimitives.WriteUInt16BigEndian(span[0x16..], (ushort)f.AnimationFrame);
                BinaryPrimitives.WriteUInt32BigEndian(span[0x18..], ghost.NonMarioGraphics);
            }
            patch.WriteBytes(Base + BufferOffset + (uint)g * BufferSize, buffer);

            Array.Clear(bodies);
            for (uint tm = 0; tm < 0x80; tm++)
            {
                long index = Ghost.Order(ghost.IndexAt(unchecked(timer + tm + 1)));
                if (index >= 0 && ghost.Bodies.TryGetValue((uint)index, out var bodyState))
                    bodyState.AsSpan(0, Math.Min(BodyBytes, bodyState.Length)).CopyTo(bodies.AsSpan((int)(((timer + tm) & 0x7F) * SlotSize)));
            }
            patch.WriteBytes(Base + BodyBuffers + (uint)g * BufferSize, bodies);
            patch.WriteBytes(Base + LightsOffset + (uint)(g + 1) * 0x20, Lights(color));
            uint node = frame.Ram.U32(FirstNodePointer - (uint)g * NodeStride);
            if (frame.Ram.IsPointer(node))
                patch.WriteU8(node + 0x61, (byte)(transparent ? 1 : 0));
        }
        return patch;
    }

    public RamPatch RequestDisable()
        => new RamPatch().WriteU8(CountAddress, 0).WriteU8(DisableAddress, 0xFF);

    public bool ReadyToRestore(GameFrame frame)
    {
        for (uint i = 0; i < MaxGhosts; i++)
            if (frame.Ram.IsPointer(frame.Ram.U32(FirstNodePointer - i * NodeStride)))
                return false;
        return true;
    }

    public RamPatch? Restore(GameFrame frame)
    {
        if (StoredSnapshot(frame) is not { } snapshot)
            return null;
        var patch = snapshot.Restore(frame.Ram);
        patch.WriteU32(Base + SnapshotOffset, 0);
        _lastValid.Clear();
        return patch;
    }
}
