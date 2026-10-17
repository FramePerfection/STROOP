using System.Numerics;
using StroopNext.Game;
using StroopNext.Map3D;

namespace StroopNext.Core.Tests;

public class TeleportTargetTests
{
    private static Surface Tri(int index, Vector3 a, Vector3 b, Vector3 c)
    {
        var n = Vector3.Normalize(Vector3.Cross(b - a, c - a));
        return new Surface(index, 0x80100000 + (uint)index * 0x30, true, 0, 0, 0, a, b, c, n, -Vector3.Dot(n, a), 0);
    }

    private static readonly SurfaceTable Level = new(
    [
        Tri(0, new(-1000, 100, -1000), new(-1000, 100, 1000), new(1000, 100, -1000)),
        Tri(1, new(1000, 100, -1000), new(-1000, 100, 1000), new(1000, 100, 1000)),
        Tri(2, new(500, 0, -1000), new(500, 0, 1000), new(500, 900, 0)),
    ], 3);

    [Fact]
    public void A_click_on_the_floor_lands_there()
    {
        var p = Picking.TeleportTarget(Level, new Ray(new Vector3(200, 5000, 300), -Vector3.UnitY))!.Value;
        Assert.Equal(100, p.Y, 3);
        Assert.Equal(200, p.X, 1);
        Assert.Equal(300, p.Z, 1);
    }

    [Fact]
    public void A_click_on_a_wall_lands_on_the_floor_in_front_of_it()
    {
        var p = Picking.TeleportTarget(Level, new Ray(new Vector3(-2000, 300, 0), Vector3.UnitX))!.Value;
        Assert.True(p.X < 500, "on the camera's side of the wall, not inside it");
        Assert.Equal(100, p.Y, 3);
    }

    [Fact]
    public void Nothing_under_the_cursor_gives_no_target()
    {
        Assert.Null(Picking.TeleportTarget(Level, new Ray(new Vector3(5000, 5000, 5000), -Vector3.UnitY)));
        Assert.Null(Picking.TeleportTarget(Level, new Ray(new Vector3(0, 5000, 0), Vector3.UnitY)));
    }
}
