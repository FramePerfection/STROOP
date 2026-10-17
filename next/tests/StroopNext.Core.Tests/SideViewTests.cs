using System.Numerics;
using StroopNext.Map;
using StroopNext.Map3D;

namespace StroopNext.Core.Tests;

public class SideViewTests
{
    private static Camera3D Side(SideAxis axis, int facing, float? cut)
    {
        var camera = new Camera3D { Width = 800, Height = 600 };
        SideView.Apply(camera, new Vector3(100, 50, 200), SideView.LookYaw(axis, facing), 2000, cut);
        return camera;
    }

    [Fact]
    public void Side_of_mario_puts_his_facing_direction_to_the_right_of_the_screen()
    {
        var camera = Side(SideAxis.SideOfMario, facing: 0, cut: null);
        var center = camera.Project(camera.Target)!.Value;
        var ahead = camera.Project(camera.Target + new Vector3(0, 0, 100))!.Value;
        Assert.True(ahead.X > center.X + 10);
        Assert.Equal(center.Y, ahead.Y, 1);
        Assert.Equal(0, camera.Pitch);
        Assert.NotNull(camera.OrthographicWidth);
    }

    [Fact]
    public void Along_z_looks_north_with_x_to_the_right_like_the_2d_map()
    {
        var camera = Side(SideAxis.AlongZ, facing: 1234, cut: null);
        var center = camera.Project(camera.Target)!.Value;
        Assert.True(camera.Project(camera.Target + new Vector3(100, 0, 0))!.Value.X > center.X + 10);
        Assert.True(camera.Project(camera.Target + new Vector3(0, 100, 0))!.Value.Y < center.Y - 10);
    }

    [Fact]
    public void A_cut_limits_near_and_far_to_a_slab_around_the_target()
    {
        var camera = Side(SideAxis.AlongZ, 0, cut: 150);
        float distance = Vector3.Distance(camera.Position, camera.Target);
        Assert.Equal(distance - 150, camera.Near, 2);
        Assert.Equal(distance + 150, camera.Far, 2);
        Assert.True(camera.IsInsideSlab(camera.Target + new Vector3(500, 300, -140)));
        Assert.False(camera.IsInsideSlab(camera.Target + new Vector3(0, 0, -400)));

        camera.Far = 1;
        Assert.Equal(distance + 150, camera.Far, 2);
    }

    [Fact]
    public void Without_a_cut_everything_is_inside_and_the_far_plane_is_settable()
    {
        var camera = Side(SideAxis.AlongX, 0, cut: 0);
        Assert.Null(camera.SlabHalfDepth);
        Assert.True(camera.IsInsideSlab(new Vector3(1e5f, 0, 0)));
        camera.Far = 1234;
        Assert.Equal(1234, camera.Far);
    }

    [Fact]
    public void Picking_ignores_what_the_cut_hides()
    {
        var camera = Side(SideAxis.AlongZ, 0, cut: 100);
        var scene = new Scene3D();
        var color = MapColor.Rgb(255, 255, 255);
        void Quad(float z) => scene.Add(new Triangle3D(new(-5000 + 100, -5000, z), new(5000 + 100, -5000, z), new(100, 5000, z), color));
        Quad(200 + 1000);
        Quad(200 - 50);
        var hit = Picking.Pick(scene, camera, new Vector2(400, 300));
        Assert.NotNull(hit);
        Assert.Equal(150, hit!.Value.Point.Z, 1);
    }
}
