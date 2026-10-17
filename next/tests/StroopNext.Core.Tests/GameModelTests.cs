using System.Numerics;
using StroopNext.Game;
using StroopNext.Memory;
using StroopNext.Session;
using O = StroopNext.Game.MarioOffsets;

namespace StroopNext.Core.Tests;

public class GameModelTests
{
    private const uint Pool = 0x8033D488, Lists = 0x8033CBE0, SurfacePool = 0x80200000;

    internal static void WriteSurface(RamPatch p, uint address, Vector3 v1, Vector3 v2, Vector3 v3, Vector3 normal, float originOffset, byte flags = 0)
    {
        p.WriteU8(address + 0x04, flags);
        uint o = 0x0A;
        foreach (var v in new[] { v1, v2, v3 })
        {
            p.WriteS16(address + o, (short)v.X).WriteS16(address + o + 2, (short)v.Y).WriteS16(address + o + 4, (short)v.Z);
            o += 6;
        }
        p.WriteF32(address + 0x1C, normal.X).WriteF32(address + 0x20, normal.Y).WriteF32(address + 0x24, normal.Z).WriteF32(address + 0x28, originOffset);
    }

    internal static TestRam WithSurfaces(TestRam ram, int staticCount, params (Vector3, Vector3, Vector3, Vector3, float)[] tris)
    {
        ram.Patch.WriteU32(0x8038EE9C, SurfacePool).WriteS32(0x80361178, staticCount).WriteS32(0x80361170, tris.Length);
        for (int i = 0; i < tris.Length; i++)
        {
            var (a, b, c, n, oo) = tris[i];
            WriteSurface(ram.Patch, SurfacePool + (uint)(i * 0x30), a, b, c, n, oo);
        }
        return ram;
    }

    [Fact]
    public void Mario_state_decodes_all_offsets()
    {
        var frame = TestRam.Us().Do(p => p
            .WriteF32(TestRam.UsMario + O.X, 1).WriteF32(TestRam.UsMario + O.Y, 2).WriteF32(TestRam.UsMario + O.Z, 3)
            .WriteF32(TestRam.UsMario + O.HSpeed, 48.5f)
            .WriteU16(TestRam.UsMario + O.FacingYaw, 0xC000)
            .WriteU32(TestRam.UsMario + O.Action, 0x0C400201)
            .WriteF32(TestRam.UsMario + O.SlideVelX, 3).WriteF32(TestRam.UsMario + O.SlideVelZ, 4)
            .WriteS16(TestRam.UsMario + O.Health, 0x880)
            .WriteU32(TestRam.UsMario + O.Flags, 0x10)).Frame();

        var m = frame.Mario!;
        Assert.Equal(new Vector3(1, 2, 3), m.Position);
        Assert.Equal(48.5f, m.HSpeed);
        Assert.Equal(0xC000, m.FacingYaw);
        Assert.Equal(5f, m.SlidingSpeed);
        Assert.Equal(8, m.HealthWedges);
        Assert.True(m.HasHatOnHead);
        Assert.Equal("standing", frame.Services.Actions.NameOf(m.Action));
    }

    [Fact]
    public void Unknown_rom_has_no_models()
    {
        var frame = new TestRam().Frame();
        Assert.False(frame.IsKnownGame);
        Assert.Null(frame.Mario);
        Assert.Empty(frame.Objects.All);
        Assert.Empty(frame.Surfaces.All);
    }

    [Fact]
    public void Objects_are_walked_in_processing_order_with_slots_and_names()
    {
        uint objA = Pool + 5 * 0x260, objB = Pool + 2 * 0x260, objC = Pool + 9 * 0x260;
        uint list0B = Lists + 0x0B * 0x68, list00 = Lists;
        const uint bank = 0x00400000;
        var frame = TestRam.Us().Do(p =>
        {
            for (int i = 0; i < 13; i++)
                p.WriteU32(Lists + (uint)i * 0x68 + 0x60, Lists + (uint)i * 0x68);
            p.WriteU32(0x8033B400 + 0x13 * 4, bank);
            p.WriteU32(list0B + 0x60, objA).WriteU32(objA + 0x60, objB).WriteU32(objB + 0x60, list0B);
            p.WriteU32(list00 + 0x60, objC).WriteU32(objC + 0x60, list00);
            p.WriteU32(objC + 0x20C, 0x80000000 | (bank + 0x2EC0));
            p.WriteF32(objA + 0xA0, 100).WriteF32(objA + 0x1F8, 70);
        }).Frame();

        var objects = frame.Objects.All;
        Assert.Equal([objA, objB, objC], objects.Select(o => o.Address));
        Assert.Equal([6, 3, 10], objects.Select(o => o.SlotNumber));
        Assert.Equal(0, objects[0].ProcessIndex);
        Assert.Equal(100f, objects[0].Position.X);
        Assert.Equal(70f, objects[0].HitboxRadius);
        Assert.Equal(0x13002EC0u, objects[2].SegmentedBehavior);
        Assert.Equal("Mario", objects[2].Name);
    }

    [Fact]
    public void Corrupted_object_lists_do_not_hang_or_throw()
    {
        uint obj = Pool;
        var frame = TestRam.Us().Do(p =>
        {
            for (int i = 0; i < 13; i++)
                p.WriteU32(Lists + (uint)i * 0x68 + 0x60, Lists + (uint)i * 0x68);
            p.WriteU32(Lists + 0x0B * 0x68 + 0x60, obj).WriteU32(obj + 0x60, obj);
            p.WriteU32(Lists + 0x09 * 0x68 + 0x60, 0x12345678);
        }).Frame();
        Assert.Single(frame.Objects.All);
    }

    [Fact]
    public void Surfaces_are_classified_and_static_geometry_is_cached()
    {
        var floor = (new Vector3(0, 0, 0), new Vector3(100, 0, 0), new Vector3(0, 0, 100), new Vector3(0, 1, 0), 0f);
        var wall = (new Vector3(0, 0, 0), new Vector3(100, 0, 0), new Vector3(0, 100, 0), new Vector3(0, 0, 1), 0f);
        var ceiling = (new Vector3(0, 300, 0), new Vector3(0, 300, 100), new Vector3(100, 300, 0), new Vector3(0, -1, 0), 300f);
        var services = new GameServices();
        var ram = WithSurfaces(TestRam.Us(), 2, floor, wall, ceiling);

        var first = ram.Frame(services: services).Surfaces;
        Assert.Equal([SurfaceClass.Floor, SurfaceClass.Wall, SurfaceClass.Ceiling], first.All.Select(s => s.Class));
        Assert.Equal(2, first.StaticCount);
        Assert.False(first.All[2].IsStatic);
        Assert.Equal(0f, first.All[0].HeightAt(10, 10));
        Assert.True(first.All[0].ContainsXZ(10, 10));
        Assert.False(first.All[0].ContainsXZ(90, 90));

        var second = ram.Frame(services: services).Surfaces;
        Assert.Same(first.All[0], second.All[0]);
        Assert.NotSame(first.All[2], second.All[2]);
        Assert.Same(first.All[1], second.Find(SurfacePool + 0x30));
    }

    [Fact]
    public void Implausible_surface_counts_yield_an_empty_table()
    {
        var frame = TestRam.Us().Do(p => p.WriteU32(0x8038EE9C, SurfacePool).WriteS32(0x80361178, 5).WriteS32(0x80361170, 2)).Frame();
        Assert.Empty(frame.Surfaces.All);
    }

    [Fact]
    public void Angle_helpers_follow_sm64_conventions()
    {
        var (x, z) = Angle.Direction(0x4000);
        Assert.Equal(1, x, 6);
        Assert.Equal(0, z, 6);
        Assert.Equal(0x4000, Angle.Towards(0, 0, 10, 0));
        Assert.Equal(0, Angle.Towards(0, 0, 0, 10));
        Assert.Equal(-2, Angle.Difference(0xFFFF, 1));
        Assert.Equal(2, Angle.Difference(1, 0xFFFF));
    }
}
