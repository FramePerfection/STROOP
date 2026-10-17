using System.Numerics;
using StroopNext.Map;
using StroopNext.Session;
using StroopNext.Settings;
using O = StroopNext.Game.MarioOffsets;

namespace StroopNext.Core.Tests;

public class MapToolsTests
{
    private static GameFrame MarioAt(float x, float y, float z) => TestRam.Us().Do(p => p
        .WriteF32(TestRam.UsMario + O.X, x).WriteF32(TestRam.UsMario + O.Y, y).WriteF32(TestRam.UsMario + O.Z, z)).Frame();

    private static MapScene Build(MapToolsLayer tools, GameFrame frame)
    {
        var scene = new MapScene();
        tools.Build(new MapContext(frame, new MapView { Width = 800, Height = 600, Scale = 0.5 }, scene, new SettingsStore().Scope("t"), new Selection()));
        return scene;
    }

    [Fact]
    public void Measurement_decomposes_the_distance_and_gives_the_game_yaw()
    {
        var m = new Measurement(new Vector3(0, 10, 0), new Vector3(30, 50, 40));
        Assert.Equal(30, m.DX);
        Assert.Equal(40, m.DY);
        Assert.Equal(40, m.DZ);
        Assert.Equal(50, m.HorizontalDistance, 4);
        Assert.Equal(MathF.Sqrt(30 * 30 + 40 * 40 + 40 * 40), m.Distance, 4);
        Assert.Equal(Game.Angle.Towards(0, 0, 30, 40), m.Yaw);
    }

    [Fact]
    public void An_end_that_follows_mario_moves_with_him()
    {
        var tools = new MapToolsLayer();
        tools.SetStart(MeasureEnd.Mario);
        tools.SetEnd(new MeasureEnd(new Vector3(100, 0, 0)));
        Assert.Equal(100, tools.Measure(MarioAt(0, 0, 0))!.Value.HorizontalDistance, 4);
        Assert.Equal(60, tools.Measure(MarioAt(40, 0, 0))!.Value.HorizontalDistance, 4);
    }

    [Fact]
    public void Measure_draws_only_when_both_ends_exist_and_clears()
    {
        var tools = new MapToolsLayer();
        var frame = MarioAt(0, 0, 0);
        tools.SetStart(new MeasureEnd(new Vector3(0, 0, 0)));
        Assert.Null(tools.Measure(frame));
        Assert.Single(Build(tools, frame).Items.OfType<MapMarker>());
        tools.SetEnd(new MeasureEnd(new Vector3(10, 0, 0)));
        var scene = Build(tools, frame);
        Assert.Single(scene.Items.OfType<MapLabel>());
        Assert.Contains(scene.Items, i => i.Hit is MeasureHit);
        tools.ClearMeasure();
        Assert.Empty(Build(tools, frame).Items);
    }

    [Fact]
    public void Strokes_skip_points_closer_than_the_spacing()
    {
        var tools = new MapToolsLayer();
        int changes = 0;
        tools.Changed += () => changes++;
        tools.BeginStroke(new Vector2(0, 0));
        tools.ExtendStroke(new Vector2(0.5f, 0), 1);
        tools.ExtendStroke(new Vector2(5, 0), 1);
        tools.ExtendStroke(new Vector2(10, 0), 1);
        Assert.Equal(3, tools.Strokes[0].Count);
        Assert.Equal(3, changes);
        Assert.Single(Build(tools, MarioAt(0, 0, 0)).Items.OfType<MapPolyline>());
        tools.ClearDrawings();
        Assert.Empty(tools.Strokes);
    }
}
