using System.IO.Compression;
using K4os.Compression.LZ4.Streams;

namespace StroopNext.Memory;

public static class Savestate
{
    public const int RamOffset = 0x1B0;

    public static BufferRamSource Load(string path)
    {
        using var file = File.OpenRead(path);
        return new BufferRamSource(Path.GetFileName(path), ExtractRam(file));
    }

    public static byte[] ExtractRam(Stream stream)
    {
        byte[] data = Decompress(stream);
        if (data.Length <= RamOffset)
            throw new InvalidDataException("Savestate is too small to contain RDRAM.");
        int available = data.Length - RamOffset;
        if (available < N64.RamSize4MB)
            throw new InvalidDataException("Savestate RDRAM is truncated.");
        int size = available >= N64.RamSize8MB ? N64.RamSize8MB : N64.RamSize4MB;
        return data.AsSpan(RamOffset, size).ToArray();
    }

    private static byte[] Decompress(Stream stream)
    {
        using var buffered = new MemoryStream();
        stream.CopyTo(buffered);
        byte[] raw = buffered.ToArray();

        Stream? decoder = raw switch
        {
            [0x1F, 0x8B, ..] => new GZipStream(new MemoryStream(raw), CompressionMode.Decompress),
            [0x04, 0x22, 0x4D, 0x18, ..] => LZ4Stream.Decode(new MemoryStream(raw)),
            _ => null,
        };
        if (decoder is null)
            return raw;

        using (decoder)
        {
            using var output = new MemoryStream();
            decoder.CopyTo(output);
            return output.ToArray();
        }
    }
}
