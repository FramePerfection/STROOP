using System.IO.Compression;

namespace STROOP.Core.GameMemoryAccess;

public static class SavestateDecompression
{
    static readonly byte[] Lz4Magic = { 0x04, 0x22, 0x4D, 0x18 };

    public static bool IsLz4(byte[] data) => data.Length >= 4 && data.AsSpan(0, 4).SequenceEqual(Lz4Magic);

    public static byte[] Decompress(byte[] data)
    {
        if (IsLz4(data))
            return DecompressLz4Frame(data);

        using MemoryStream input = new MemoryStream(data);
        using GZipStream gzipStream = new GZipStream(input, CompressionMode.Decompress);
        using MemoryStream unzip = new MemoryStream();
        gzipStream.CopyTo(unzip);
        return unzip.ToArray();
    }

    static byte[] DecompressLz4Frame(byte[] data)
    {
        int p = 4;
        byte flg = data[p];
        p += 2;
        bool blockChecksum = (flg & 0x10) != 0;
        bool contentSize = (flg & 0x08) != 0;
        bool dictId = (flg & 0x01) != 0;
        if (contentSize) p += 8;
        if (dictId) p += 4;
        p += 1;

        MemoryStream output = new MemoryStream(data.Length * 5);
        while (p + 4 <= data.Length)
        {
            uint blockSize = BitConverter.ToUInt32(data, p);
            p += 4;
            if (blockSize == 0)
                break;
            bool uncompressed = (blockSize & 0x80000000) != 0;
            int size = (int)(blockSize & 0x7FFFFFFF);
            if (uncompressed)
                output.Write(data, p, size);
            else
                DecodeBlock(data, p, size, output);
            p += size;
            if (blockChecksum) p += 4;
        }

        return output.ToArray();
    }

    static void DecodeBlock(byte[] src, int start, int length, MemoryStream output)
    {
        int p = start, end = start + length;
        while (p < end)
        {
            int token = src[p++];
            int literals = token >> 4;
            if (literals == 15)
            {
                int b;
                do literals += b = src[p++];
                while (b == 255);
            }

            output.Write(src, p, literals);
            p += literals;
            if (p >= end)
                break;

            int offset = src[p] | src[p + 1] << 8;
            p += 2;
            int matchLength = token & 15;
            if (matchLength == 15)
            {
                int b;
                do matchLength += b = src[p++];
                while (b == 255);
            }

            matchLength += 4;

            byte[] buffer = output.GetBuffer();
            int matchStart = (int)output.Length - offset;
            if (offset >= matchLength)
                output.Write(buffer, matchStart, matchLength);
            else
                for (int i = 0; i < matchLength; i++)
                {
                    buffer = output.GetBuffer();
                    output.WriteByte(buffer[matchStart + i]);
                }
        }
    }
}
