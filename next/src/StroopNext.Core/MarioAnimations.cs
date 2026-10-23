using System.Collections.Concurrent;
using StroopNext.Memory;

namespace StroopNext.Graphics;

public static class MarioAnimations
{
    public sealed record Captured(short Id, byte[] Blob, uint ValuesOffset, uint IndexOffset);

    private static readonly ConcurrentDictionary<short, Captured> Cache = new();

    public static int Count => Cache.Count;

    public static Captured? Get(short id) => Cache.GetValueOrDefault(id);

    public static void Clear() => Cache.Clear();

    private static string? _folder;

    public static void PersistIn(string folder)
    {
        _folder = folder;
        try
        {
            if (!Directory.Exists(folder))
                return;
            foreach (var file in Directory.EnumerateFiles(folder, "*.anim"))
                if (short.TryParse(Path.GetFileNameWithoutExtension(file), out short id) && Read(id, File.ReadAllBytes(file)) is { } captured)
                    Cache.TryAdd(id, captured);
        }
        catch (Exception e) when (e is IOException or UnauthorizedAccessException)
        {
        }
    }

    private static Captured? Read(short id, byte[] file)
    {
        if (file.Length < 8 + 0x18)
            return null;
        uint values = System.Buffers.Binary.BinaryPrimitives.ReadUInt32BigEndian(file);
        uint index = System.Buffers.Binary.BinaryPrimitives.ReadUInt32BigEndian(file.AsSpan(4));
        var blob = file[8..];
        return values < blob.Length && index < blob.Length ? new Captured(id, blob, values, index) : null;
    }

    private static void Save(Captured c)
    {
        if (_folder is null)
            return;
        try
        {
            Directory.CreateDirectory(_folder);
            var file = new byte[8 + c.Blob.Length];
            System.Buffers.Binary.BinaryPrimitives.WriteUInt32BigEndian(file, c.ValuesOffset);
            System.Buffers.Binary.BinaryPrimitives.WriteUInt32BigEndian(file.AsSpan(4), c.IndexOffset);
            c.Blob.CopyTo(file, 8);
            File.WriteAllBytes(Path.Combine(_folder, $"{c.Id}.anim"), file);
        }
        catch (Exception e) when (e is IOException or UnauthorizedAccessException)
        {
        }
    }

    public static string? RomLoaded { get; private set; }

    private const int RomTable = 0x4EC000;

    public static int LoadFromRom(string path)
    {
        try
        {
            var info = new FileInfo(path);
            if (!info.Exists || info.Length < RomTable + 8 || info.Length > 64 << 20)
                return 0;
            var rom = File.ReadAllBytes(path);
            if (rom[0] != 0x80 || rom[1] != 0x37 || System.Text.Encoding.ASCII.GetString(rom, 0x20, 14) != "SUPER MARIO 64" || rom[0x3E] != (byte)'E')
                return 0;
            uint count = System.Buffers.Binary.BinaryPrimitives.ReadUInt32BigEndian(rom.AsSpan(RomTable));
            if (count is 0 or > 1024)
                return 0;
            int added = 0;
            for (int i = 0; i < count; i++)
            {
                uint off = System.Buffers.Binary.BinaryPrimitives.ReadUInt32BigEndian(rom.AsSpan(RomTable + 8 + 8 * i));
                uint size = System.Buffers.Binary.BinaryPrimitives.ReadUInt32BigEndian(rom.AsSpan(RomTable + 12 + 8 * i));
                long at = RomTable + (long)off;
                if (size < 0x18 || size > 0x10000 || at + size > rom.Length)
                    continue;
                var blob = rom.AsSpan((int)at, (int)size).ToArray();
                uint values = System.Buffers.Binary.BinaryPrimitives.ReadUInt32BigEndian(blob.AsSpan(0x0C));
                uint index = System.Buffers.Binary.BinaryPrimitives.ReadUInt32BigEndian(blob.AsSpan(0x10));
                if (values >= size || index >= size)
                    continue;
                if (Cache.TryAdd((short)i, new Captured((short)i, blob, values, index)))
                    added++;
            }
            RomLoaded = path;
            return added;
        }
        catch (Exception e) when (e is IOException or UnauthorizedAccessException)
        {
            return 0;
        }
    }

    public static int LoadFromRomFolder(string folder)
    {
        if (RomLoaded is not null || !Directory.Exists(folder))
            return 0;
        try
        {
            foreach (var file in Directory.EnumerateFiles(folder, "*.z64"))
                if (LoadFromRom(file) is > 0 and var n)
                    return n;
        }
        catch (Exception e) when (e is IOException or UnauthorizedAccessException)
        {
        }
        return 0;
    }

    public static void Capture(RamImage ram, uint marioObject)
    {
        if (!ram.IsPointer(marioObject))
            return;
        short id = ram.S16(marioObject + 0x38);
        if (id < 0 || Cache.ContainsKey(id))
            return;
        uint header = ram.U32(marioObject + 0x3C);
        if (!ram.IsPointer(header))
            return;
        uint length = ram.U32(header + 0x14);
        if (length < 0x18 || length > 0x10000 || !ram.IsValid(header, (int)length))
            return;
        uint basePhys = header & 0x1FFFFFFF;
        uint values = (ram.U32(header + 0x0C) & 0x1FFFFFFF) - basePhys;
        uint index = (ram.U32(header + 0x10) & 0x1FFFFFFF) - basePhys;
        if (values >= length || index >= length)
            return;
        var blob = new byte[length];
        for (uint i = 0; i < length; i++)
            blob[i] = ram.U8(header + i);
        var captured = new Captured(id, blob, values, index);
        if (Cache.TryAdd(id, captured))
            Save(captured);
    }
}
