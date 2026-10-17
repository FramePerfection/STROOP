using System.Numerics;
using StroopNext.Game;
using StroopNext.Map;
using StroopNext.Session;
using StroopNext.Settings;
using O = StroopNext.Game.MarioOffsets;

namespace StroopNext.Core.Tests;

public class MapTests
{
    private static MapView View(double rotation = 0) => new() { Width = 800, Height = 600, Center = new Vector2(100, -50), Scale = 0.5, Rotation = rotation };

    [Theory]
    [InlineData(0)]
    [InlineData(1.234)]
    [InlineData(Math.PI)]
    public void World_screen_transforms_are_inverse(double rotation)
    {
        var view = View(rotation);
        var world = new Vector2(-1234.5f, 678.25f);
        var back = view.ScreenToWorld(view.WorldToScreen(world));
        Assert.Equal(world.X, back.X, 2);
        Assert.Equal(world.Y, back.Y, 2);
        Assert.Equal(new Vector2(400, 300), view.WorldToScreen(view.Center));
    }

    [Fact]
    public void Unrotated_map_has_x_right_and_z_down()
    {
        var view = View();
        var origin = view.WorldToScreen(view.Center);
        Assert.True(view.WorldToScreen(view.Center + new Vector2(10, 0)).X > origin.X);
        Assert.True(view.WorldToScreen(view.Center + new Vector2(0, 10)).Y > origin.Y);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(0x4000)]
    [InlineData(0x9123)]
    public void Pointing_a_yaw_up_puts_that_direction_at_the_top(int yaw)
    {
        var view = View();
        view.PointYawUp(yaw);
        var (x, z) = Angle.Direction(yaw);
        var ahead = view.WorldToScreen(view.Center + new Vector2((float)x, (float)z) * 100);
        Assert.Equal(400, ahead.X, 2);
        Assert.True(ahead.Y < 300);
    }

    [Fact]
    public void Screen_angles_stay_exact_far_from_the_origin_at_max_zoom()
    {
        var view = new MapView { Width = 800, Height = 600, Center = new Vector2(7999.123f, -7777.77f), Scale = MapView.MaxScale, Rotation = 0.3 };
        foreach (int yaw in new[] { 0, 0x1234, 0x4000, 0xA000 })
        {
            var (x, z) = Angle.Direction(yaw);
            double expected = Math.Atan2(x * Math.Sin(0.3) + z * Math.Cos(0.3), x * Math.Cos(0.3) - z * Math.Sin(0.3));
            Assert.Equal(expected, view.ScreenAngleOfYaw(yaw), 9);
        }
        Assert.Equal(Math.PI / 2, View().ScreenAngleOfYaw(0), 9);
    }

    [Fact]
    public void Zoom_keeps_the_anchor_fixed()
    {
        var view = View(0.7);
        var anchor = new Vector2(123, 456);
        var before = view.ScreenToWorld(anchor);
        view.ZoomAt(anchor, 3);
        var after = view.ScreenToWorld(anchor);
        Assert.Equal(before.X, after.X, 2);
        Assert.Equal(before.Y, after.Y, 2);
        Assert.Equal(1.5, view.Scale, 6);
    }

    [Fact]
    public void Wall_hitbox_band_extends_along_its_projection_axis()
    {
        var zWall = new Surface(0, 0, true, 0, 0, 0, new(0, 0, 0), new(100, 0, 0), new(0, 100, 0), new(0, 0, 1), 0, 0);
        var band = WallsLayer.HitboxBand(zWall, 50)!;
        Assert.Contains(new Vector2(0, 50), band);
        Assert.Contains(new Vector2(100, -50), band);

        var n = Vector3.Normalize(new Vector3(1, 0, 0.5f));
        var xWall = new Surface(0, 0, true, 0, 0x08, 0, new(0, 0, 0), new(-50, 0, 100), new(0, 100, 0), n, 0, 0);
        var diagonal = WallsLayer.HitboxBand(xWall, 50)!;
        Assert.Equal(50 / n.X, diagonal[0].X - WallsLayer.Segment(xWall).A.X, 3);
    }

    [Fact]
    public void Hit_testing_returns_the_topmost_item_with_a_payload()
    {
        var view = View();
        var scene = new MapScene();
        var bottom = new MapCircle(view.Center, 100, MapColor.Transparent, MapColor.Transparent) { Hit = new CameraHit(new CameraState(default, default, 0)) };
        var top = new MapMarker(view.Center, 10, MarkerShape.Square, MapColor.Transparent, MapColor.Transparent) { Hit = new CameraHit(new CameraState(default, default, 1)) };
        scene.Add(bottom);
        scene.Add(top);
        scene.Add(new MapCircle(view.Center, 1000, MapColor.Transparent, MapColor.Transparent));

        Assert.Same(top, scene.HitTest(view, new Vector2(400, 300)));
        Assert.Same(bottom, scene.HitTest(view, new Vector2(400 + 40, 300)));
        Assert.Null(scene.HitTest(view, new Vector2(400 + 70, 300)));
    }

    [Fact]
    public void Standard_layers_build_from_a_synthetic_frame_without_ui()
    {
        var floor = (new Vector3(-500, 0, -500), new Vector3(500, 0, -500), new Vector3(0, 0, 500), new Vector3(0, 1, 0), 0f);
        var wall = (new Vector3(-500, -100, 200), new Vector3(500, -100, 200), new Vector3(0, 400, 200), new Vector3(0, 0, -1), 200f);
        var ram = GameModelTests.WithSurfaces(TestRam.Us(), 2, floor, wall);
        ram.Patch.WriteU32(TestRam.UsMario + O.FloorTri, 0x80200000).WriteF32(TestRam.UsMario + O.IntendedMag, 32);
        var frame = ram.Frame();

        var track = StandardLayers.CreateMarioPathTrack();
        var store = new SettingsStore();
        var scene = new MapScene();
        var view = new MapView { Width = 800, Height = 600, Scale = 0.3 };
        foreach (var layer in StandardLayers.Create(track))
            layer.Build(new MapContext(frame, view, scene, store.Scope($"map.layer.{layer.Id}"), new Selection()));

        Assert.Single(scene.Background.OfType<MapPolygon>(), p => p.Hit is SurfaceHit { Surface.Class: SurfaceClass.Floor });
        Assert.Single(scene.Items.OfType<MapPolygon>(), p => p.Hit is SurfaceHit { Surface.Class: SurfaceClass.Floor } && p.Stroke == MapPalette.Highlight);
        Assert.Contains(scene.Background, p => p.Hit is SurfaceHit { Surface.Class: SurfaceClass.Wall });
        Assert.Contains(scene.Items, p => p is MapMarker { Hit: MarioHit });
        Assert.Contains(scene.Items, p => p is MapArrow a && a.Color == MapPalette.Intended);
    }

    private static SceneKey BackgroundKeyOf(GameFrame frame, SettingsStore store)
    {
        var scene = new MapScene();
        var view = new MapView { Width = 800, Height = 600, Scale = 0.3 };
        foreach (var layer in StandardLayers.Create(StandardLayers.CreateMarioPathTrack()))
            layer.Build(new MapContext(frame, view, scene, store.Scope($"map.layer.{layer.Id}"), new Selection()));
        return scene.BackgroundKey;
    }

    [Fact]
    public void Background_key_changes_exactly_when_cached_content_would()
    {
        var floor = (new Vector3(-500, 0, -500), new Vector3(500, 0, -500), new Vector3(0, 0, 500), new Vector3(0, 1, 0), 0f);
        var services = new GameServices();
        var ram = GameModelTests.WithSurfaces(TestRam.Us(), 1, floor);
        var store = new SettingsStore();

        var key1 = BackgroundKeyOf(ram.Frame(services: services), store);
        ram.Patch.WriteF32(TestRam.UsMario + O.X, 300).WriteF32(TestRam.UsMario + O.HSpeed, 40);
        Assert.Equal(key1, BackgroundKeyOf(ram.Frame(services: services), store));

        ram.Patch.WriteF32(TestRam.UsMario + O.Y, 120);
        var key2 = BackgroundKeyOf(ram.Frame(services: services), store);
        Assert.NotEqual(key1, key2);

        store.Scope("map.layer.floors").Set(new FloorsLayer().HeightShading, false);
        store.Scope("map.layer.walls").Set(new WallsLayer().AtMarioHeightOnly, false);
        var key3 = BackgroundKeyOf(ram.Frame(services: services), store);
        ram.Patch.WriteF32(TestRam.UsMario + O.Y, 500);
        Assert.Equal(key3, BackgroundKeyOf(ram.Frame(services: services), store));

        store.Scope("map.layer.floors").Set(new FloorsLayer().Opacity, 0.9);
        Assert.NotEqual(key3, BackgroundKeyOf(ram.Frame(services: services), store));
    }

    [Fact]
    public void Layer_settings_are_declared_and_respected()
    {
        var layer = new FloorsLayer();
        Assert.Contains(layer.Settings, s => s.Key == "opacity");
        var keys = layer.Settings.Select(s => s.Key).ToList();
        Assert.Equal(keys.Count, keys.Distinct().Count());

        var floor = (new Vector3(-500, 5000, -500), new Vector3(500, 5000, -500), new Vector3(0, 5000, 500), new Vector3(0, 1, 0), -5000f);
        var frame = GameModelTests.WithSurfaces(TestRam.Us(), 1, floor).Frame();
        var store = new SettingsStore();
        var scope = store.Scope("floors");
        scope.Set(layer.HeightRange, 1000);
        var scene = new MapScene();
        layer.Build(new MapContext(frame, new MapView { Width = 100, Height = 100 }, scene, scope, new Selection()));
        Assert.Empty(scene.All);
    }
}
