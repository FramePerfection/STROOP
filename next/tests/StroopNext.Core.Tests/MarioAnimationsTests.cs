using StroopNext.Graphics;
using StroopNext.Memory;

namespace StroopNext.Core.Tests;

[Collection("MarioAnimations")]
public class MarioAnimationsTests
{
    private const uint MarioObject = 0x8034B888, Buffer = 0x80060030;

    private static TestRam Playing(short id)
    {
        var ram = TestRam.Us().Do(p =>
        {
            p.WriteS16(MarioObject + 0x38, id).WriteU32(MarioObject + 0x3C, Buffer);
            p.WriteU16(Buffer, 0).WriteU32(Buffer + 0x0C, (Buffer & 0x1FFFFFFF) + 0x20).WriteU32(Buffer + 0x10, (Buffer & 0x1FFFFFFF) + 0x18).WriteU32(Buffer + 0x14, 0x40);
            p.WriteU16(Buffer + 0x18, 2).WriteU16(Buffer + 0x1A, 0);
            p.WriteS16(Buffer + 0x20, 1234).WriteS16(Buffer + 0x22, -5);
        });
        ram.Image();
        return ram;
    }

    [Fact]
    public void Captures_the_blob_with_pointers_made_relative()
    {
        MarioAnimations.Clear();
        var ram = Playing(42);
        MarioAnimations.Capture(new RamImage(ram.Bytes), MarioObject);
        var c = MarioAnimations.Get(42)!;
        Assert.Equal(0x40, c.Blob.Length);
        Assert.Equal(0x20u, c.ValuesOffset);
        Assert.Equal(0x18u, c.IndexOffset);
        Assert.Equal(1234, (short)(c.Blob[0x20] << 8 | c.Blob[0x21]));
    }

    [Fact]
    public void Ignores_data_that_is_not_a_loaded_animation()
    {
        MarioAnimations.Clear();
        var ram = Playing(7);
        new RamPatch().WriteU32(Buffer + 0x0C, 0x00700000).ApplyTo(ram.Bytes);
        MarioAnimations.Capture(new RamImage(ram.Bytes), MarioObject);
        Assert.Null(MarioAnimations.Get(7));
    }

    [Fact]
    public void Animations_are_kept_on_disk_and_loaded_again()
    {
        string folder = Path.Combine(Path.GetTempPath(), $"stroopnext-anims-{Guid.NewGuid():N}");
        try
        {
            MarioAnimations.Clear();
            MarioAnimations.PersistIn(folder);
            MarioAnimations.Capture(new RamImage(Playing(99).Bytes), MarioObject);
            Assert.True(File.Exists(Path.Combine(folder, "99.anim")));

            MarioAnimations.Clear();
            Assert.Null(MarioAnimations.Get(99));
            MarioAnimations.PersistIn(folder);
            Assert.Equal(0x40, MarioAnimations.Get(99)!.Blob.Length);
        }
        finally
        {
            MarioAnimations.Clear();
            Directory.Delete(folder, true);
        }
    }

    [Fact]
    public void The_overlay_is_read_like_memory_after_the_8_mb()
    {
        var memory = new GfxMemory(TestRam.Us().Image(), new uint[32]);
        Assert.False(memory.IsPointer(0x80800000));
        memory.SetOverlay([0x12, 0x34, 0x56, 0x78, 0x9A]);
        Assert.True(memory.IsPointer(0x80800000));
        Assert.Equal(0x12345678u, memory.U32(0x80800000));
        Assert.Equal(0x789A, memory.U16(0x80800003));
        Assert.Equal(0x56, memory.U8(0x00800002));
        memory.SetOverlay(null);
        Assert.Equal(0u, memory.U32(0x80800000));
    }
}
