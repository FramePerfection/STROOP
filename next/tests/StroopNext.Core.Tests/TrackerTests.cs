using StroopNext.Graphics;
using System.Numerics;
using StroopNext.Map;
using StroopNext.Map3D;
using StroopNext.Session;
using StroopNext.Settings;
using O = StroopNext.Game.MarioOffsets;

namespace StroopNext.Core.Tests;

public class TrackerTests
{
    private static TestRam Mario(float x = 100, float z = 200, uint timer = 1) => TestRam.Us().Do(p => p
        .WriteF32(TestRam.UsMario + O.X, x).WriteF32(TestRam.UsMario + O.Z, z)
        .WriteF32(TestRam.UsMario + O.HSpeed, 40).WriteU16(TestRam.UsMario + O.FacingYaw, 0x4000)
        .WriteF32(TestRam.UsMario + O.HolpX, 300).WriteF32(TestRam.UsMario + O.HolpZ, 400)
        .WriteU32(0x8032D5D4, timer));

    private static MapScene Build2D(TrackersLayer layer, SettingsScope scope, GameFrame frame)
    {
        var scene = new MapScene();
        var view = new MapView { Width = 800, Height = 600, Scale = 0.5, Center = new Vector2(100, 200) };
        layer.Build(new MapContext(frame, view, scene, scope, new Selection()));
        return scene;
    }

    [Fact]
    public void Trackers_persist_in_settings_and_survive_a_new_layer()
    {
        var store = new SettingsStore();
        var scope = store.Scope("map.layer.trackers");
        var layer = new TrackersLayer();
        layer.Add(scope, TrackerSpec.Create(TrackerKind.FacingArrow, TrackerTarget.Mario));
        layer.Add(scope, TrackerSpec.Create(TrackerKind.Marker, TrackerTarget.Holp));

        var reloaded = new TrackersLayer().Trackers(scope);
        Assert.Equal([TrackerKind.FacingArrow, TrackerKind.Marker], reloaded.Select(t => t.Kind));
    }

    [Fact]
    public void Presets_save_load_and_delete_and_survive_the_settings_file()
    {
        string path = Path.Combine(Path.GetTempPath(), $"stroopnext-presets-{Guid.NewGuid():N}.json");
        try
        {
            var store = new SettingsStore(path);
            var scope = store.Scope("map.layer.trackers");
            var layer = new TrackersLayer();
            layer.Add(scope, TrackerSpec.Create(TrackerKind.Sector, TrackerTarget.Mario));
            layer.Add(scope, TrackerSpec.Create(TrackerKind.Marker, TrackerTarget.Holp));
            layer.SavePreset(scope, "BLJ setup");
            layer.Update(scope, _ => []);
            store.Save();

            var reloaded = new SettingsStore(path).Scope("map.layer.trackers");
            var fresh = new TrackersLayer();
            Assert.Equal(["BLJ setup"], fresh.PresetNames(reloaded));
            Assert.Empty(fresh.Trackers(reloaded));
            Assert.True(fresh.LoadPreset(reloaded, "BLJ setup"));
            Assert.Equal([TrackerKind.Sector, TrackerKind.Marker], fresh.Trackers(reloaded).Select(t => t.Kind));

            var loaded = fresh.Trackers(reloaded)[0];
            fresh.Replace(reloaded, loaded with { Size = 1 });
            Assert.True(fresh.LoadPreset(reloaded, "BLJ setup"));
            Assert.NotEqual(1, fresh.Trackers(reloaded)[0].Size);

            fresh.DeletePreset(reloaded, "BLJ setup");
            Assert.Empty(fresh.PresetNames(reloaded));
            Assert.False(fresh.LoadPreset(reloaded, "BLJ setup"));
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Fact]
    public void Move_remove_and_replace_edit_the_list()
    {
        var scope = new SettingsStore().Scope("t");
        var layer = new TrackersLayer();
        var a = TrackerSpec.Create(TrackerKind.Path, TrackerTarget.Mario);
        var b = TrackerSpec.Create(TrackerKind.Marker, TrackerTarget.Camera);
        layer.Add(scope, a);
        layer.Add(scope, b);
        layer.Move(scope, b.Id, -1);
        Assert.Equal([b.Id, a.Id], layer.Trackers(scope).Select(t => t.Id));
        layer.Replace(scope, a with { Size = 42 });
        Assert.Equal(42, layer.Trackers(scope).Single(t => t.Id == a.Id).Size);
        layer.Remove(scope, b.Id);
        Assert.Equal([a.Id], layer.Trackers(scope).Select(t => t.Id));
    }

    [Fact]
    public void Export_then_import_adds_copies_with_new_ids()
    {
        var scope = new SettingsStore().Scope("t");
        var layer = new TrackersLayer();
        layer.Add(scope, TrackerSpec.Create(TrackerKind.Sector, TrackerTarget.Mario) with { Color = 0xFF112233 });
        string json = layer.Export(scope);
        Assert.Equal(1, layer.Import(scope, json));
        var list = layer.Trackers(scope);
        Assert.Equal(2, list.Count);
        Assert.NotEqual(list[0].Id, list[1].Id);
        Assert.All(list, t => Assert.Equal(0xFF112233u, t.Color));
    }

    [Fact]
    public void Targets_resolve_mario_holp_and_fixed_points()
    {
        var frame = Mario().Frame();
        var selection = new Selection();
        Assert.Equal(new Vector3(100, 0, 200), TrackerTargets.Resolve(TrackerSpec.Create(TrackerKind.Marker, TrackerTarget.Mario), frame, selection).Single().Position);
        Assert.Equal(new Vector3(300, 0, 400), TrackerTargets.Resolve(TrackerSpec.Create(TrackerKind.Marker, TrackerTarget.Holp), frame, selection).Single().Position);
        var point = TrackerSpec.Create(TrackerKind.Marker, TrackerTarget.Point, point: new Vector3(1, 2, 3));
        Assert.Equal(new Vector3(1, 2, 3), TrackerTargets.Resolve(point, frame, selection).Single().Position);
        Assert.Empty(TrackerTargets.Resolve(TrackerSpec.Create(TrackerKind.Hitbox, TrackerTarget.SelectedObjects), frame, selection));
    }

    [Fact]
    public void Every_kind_draws_in_both_views_without_errors()
    {
        var frame = Mario().Frame();
        foreach (var target in new[] { TrackerTarget.Mario, TrackerTarget.Holp, TrackerTarget.Camera, TrackerTarget.Point })
            foreach (var kind in TrackerSpec.KindsFor(target))
            {
                var scope = new SettingsStore().Scope("t");
                var layer = new TrackersLayer();
                layer.Add(scope, TrackerSpec.Create(kind, target, point: new Vector3(5, 0, 5)));
                Build2D(layer, scope, frame);
                var scene3D = new Scene3D();
                layer.Build3D(new Map3DContext(frame, new Camera3D(), scene3D, scope, new Selection()));
            }
    }

    [Fact]
    public void Speed_arrows_cover_the_distance_per_frame()
    {
        var scope = new SettingsStore().Scope("t");
        var layer = new TrackersLayer();
        layer.Add(scope, TrackerSpec.Create(TrackerKind.FacingArrow, TrackerTarget.Mario));
        var arrow = Build2D(layer, scope, Mario().Frame()).Items.OfType<MapArrow>().Single();
        Assert.Equal(40 * 0.5f, arrow.LengthPx, 3);
        Assert.Equal(0x4000, arrow.Yaw);
    }

    [Fact]
    public void Path_samples_once_per_game_frame()
    {
        var scope = new SettingsStore().Scope("t");
        var layer = new TrackersLayer();
        layer.Add(scope, TrackerSpec.Create(TrackerKind.Path, TrackerTarget.Mario));
        Build2D(layer, scope, Mario(x: 0, timer: 1).Frame());
        Build2D(layer, scope, Mario(x: 0, timer: 1).Frame());
        var scene = Build2D(layer, scope, Mario(x: 50, timer: 2).Frame());
        Assert.Equal(2, scene.Items.OfType<MapPolyline>().Single().Points.Length);
    }

    [Fact]
    public void Nearby_floor_units_draw_a_grid_coloured_by_height()
    {
        var floor = (new Vector3(-100, 0, -100), new Vector3(100, 0, 200), new Vector3(200, 0, -100), new Vector3(0, 1, 0), 0f);
        var ram = GameModelTests.WithSurfaces(TestRam.Us(), 1, floor).Do(p => p
            .WriteF32(TestRam.UsMario + O.X, 10).WriteF32(TestRam.UsMario + O.Y, 20).WriteF32(TestRam.UsMario + O.Z, 10)
            .WriteU32(0x8032D5D4, 1));
        var scope = new SettingsStore().Scope("t");
        var layer = new TrackersLayer();
        layer.Add(scope, TrackerSpec.Create(TrackerKind.NearbyFloorUnits, TrackerTarget.Mario) with { Size = 2 });
        var polygons = Build2D(layer, scope, ram.Frame()).Items.OfType<MapPolygon>().ToList();
        Assert.Equal(25, polygons.Count);
        Assert.All(polygons, poly => Assert.True(poly.Fill.B >= poly.Fill.R));
    }

    [Fact]
    public void Hidden_trackers_draw_nothing_and_order_puts_on_top_last()
    {
        var scope = new SettingsStore().Scope("t");
        var layer = new TrackersLayer();
        var top = TrackerSpec.Create(TrackerKind.Marker, TrackerTarget.Mario) with { Order = TrackerOrder.OnTop, Color = 0xFF0000FF };
        layer.Add(scope, top);
        layer.Add(scope, TrackerSpec.Create(TrackerKind.Marker, TrackerTarget.Holp) with { Color = 0xFF00FF00 });
        layer.Add(scope, TrackerSpec.Create(TrackerKind.Marker, TrackerTarget.Camera) with { Visible = false });
        var markers = Build2D(layer, scope, Mario().Frame()).Items.OfType<MapMarker>().ToList();
        Assert.Equal(2, markers.Count);
        Assert.Equal(Argb8Color .FromHex(0xFF0000FF), markers[^1].Fill);
    }
}
