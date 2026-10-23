using StroopNext.Game;
using StroopNext.Ghosts;
using StroopNext.Memory;
using StroopNext.Platform;
using StroopNext.Session;

namespace StroopNext.Desktop.Diagnostics;

internal static class GhostProbe
{
    public static int Run()
    {
        var candidate = EmulatorDiscovery.Find().FirstOrDefault();
        if (candidate is null)
        {
            Console.Error.WriteLine("No running emulator.");
            return 1;
        }
        using var source = ProcessRamSource.Attach(candidate);
        var bytes = new byte[source.Size];
        if (!source.TryRead(bytes))
        {
            Console.Error.WriteLine("Cannot read the emulator's RAM.");
            return 1;
        }
        var ram = new RamImage(bytes);
        var frame = GameFrame.Create(ram);
        Console.WriteLine(source.DescribeCodeCache());
        Console.WriteLine($"{candidate.ProcessName} · RAM {ram.Length / 0x100000} MB · version {frame.Version} · timer {frame.GlobalTimer} · level {frame.LevelIndex}");
        if (InGameGhosts.Unavailable(frame) is { } why)
        {
            Console.WriteLine($"unavailable: {why}");
            return 0;
        }
        const uint B = 0x80400000;
        uint hook = frame.Version == GameVersion.JP ? 0x8027ABD8u : 0x8027B188u;
        Console.WriteLine($"state {new InGameGhosts().State(frame)} · hook {ram.U32(hook):X8} · count {ram.U8(B + 0x7FFF)} · disable flag {ram.U8(B + 0x7FFC):X2}");

        foreach (var e in InGameGhosts.HackFor(frame.Version!.Value).Entries)
        {
            int diff = Enumerable.Range(0, e.Bytes.Length).Count(i => ram.U8(e.Address + (uint)i) != e.Bytes[i]);
            Console.WriteLine($"  hack entry {e.Address:X8} ({e.Bytes.Length} B): {(diff == 0 ? "matches" : $"{diff} bytes differ")}");
        }

        uint marioObj = frame.Layout?.MarioObjectPointer is uint p ? ram.U32(p) : 0;
        if (ram.IsPointer(marioObj))
            Console.WriteLine($"mario object {marioObj:X8}: animID {ram.S16(marioObj + 0x38)} frame {ram.S16(marioObj + 0x40)} anim ptr {ram.U32(marioObj + 0x3C):X8} flags {ram.U16(marioObj + 0x02):X4} sharedChild {ram.U32(marioObj + 0x14):X8}");

        if (frame.Mario is { } ms)
            Console.WriteLine($"MarioState+0x88 marioObj {ms.MarioObject:X8} (gMarioObject {marioObj:X8}) · +0x38 {ram.S16(ms.MarioObject + 0x38)} +0x40 {ram.S16(ms.MarioObject + 0x40)} · GhostRecorder.Sample → {GhostRecorder.Sample(frame)}");
        for (int g = 0; g < 4; g++)
        {
            uint slotPtr = B + 0x7FF8 - (uint)g * 0x68;
            uint node = ram.U32(slotPtr);
            if (!ram.IsPointer(node))
            {
                Console.WriteLine($"ghost {g}: no node ({node:X8})");
                continue;
            }
            Console.WriteLine($"ghost {g}: node {node:X8} type {ram.U16(node):X4} flags {ram.U16(node + 2):X4} pos ({ram.F32(node + 0x20):0.#}, {ram.F32(node + 0x24):0.#}, {ram.F32(node + 0x28):0.#}) yaw {ram.U16(node + 0x1C)} animID {ram.S16(node + 0x38)} anim ptr {ram.U32(node + 0x3C):X8} frame {ram.S16(node + 0x40)} hat {ram.U8(node + 0x60)} transparent {ram.U8(node + 0x61)} sharedChild {ram.U32(node + 0x14):X8}");
            uint slot = B + 0x9B00 + (uint)g * 0x1000 + (frame.GlobalTimer & 0x7F) * 0x20;
            Console.WriteLine($"  buffer now: pos ({ram.F32(slot):0.#}, {ram.F32(slot + 4):0.#}, {ram.F32(slot + 8):0.#}) anim {ram.U32(slot + 0xC):X8} pitch {ram.U16(slot + 0x10)} yaw {ram.U16(slot + 0x12)} roll {ram.U16(slot + 0x14)} frame {ram.S16(slot + 0x16)} gfx {ram.U32(slot + 0x18):X8}");
        }

        if (frame.Layout?.SegmentTable is uint segments)
        {
            uint vanilla = frame.Version == GameVersion.JP ? 0x7BDC0u : 0x7EC20u;
            uint bank4 = (ram.U32(segments + 0x10) & 0x1FFFFFFF) | 0x80000000;
            Console.WriteLine($"segment 4 at {bank4:X8} (vanilla offset {vanilla:X})");
            for (uint a = bank4; a < bank4 + (0x100000 - vanilla); a += 4)
            {
                uint w = ram.U32(a), dl = ram.U32(a + 0x14);
                bool cap = dl is 0x040119A0 or 0x04011A90 or 0x04011B80 or 0x04012030 or (B + 0x8200);
                if (cap && ((w & 0xFFFF0000) == 0x001B0000 || (w & 0xFFFF0000) == 0x012A0000))
                    Console.WriteLine($"  cap node {a:X8}: {w:X8} → {dl:X8} (next words {ram.U32(a + 0x18):X8} {ram.U32(a + 0x1C):X8})");
            }
            uint head = bank4 - vanilla + (frame.Version == GameVersion.JP ? 0x8D720u : 0x90580u);
            Console.Write($"  head display list at {head:X8}:");
            for (uint i = 0; i < 0x40; i += 8)
                Console.Write($" {ram.U32(head + i):X8}{ram.U32(head + i + 4):X8}");
            Console.WriteLine();
        }
        return 0;
    }
}
