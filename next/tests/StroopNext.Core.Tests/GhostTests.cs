using System.Numerics;
using StroopNext.Ghosts;
using StroopNext.Map;
using StroopNext.Session;
using StroopNext.Settings;
using O = StroopNext.Game.MarioOffsets;

namespace StroopNext.Core.Tests;

public class GhostTests
{
    private const uint Timer = 0x8032D5D4;

    private static Ghost Sample()
    {
        var ghost = new Ghost { Name = "g", PlaybackBaseFrame = 99 };
        for (uint i = 0; i < 3; i++)
            ghost.Frames[i] = new GhostFrame(new Vector3(i * 10, 5, -i), (short)i, (short)(i + 1), 1, 0x4000 + i, 2);
        ghost.AnimationSwitches[7] = 8;
        return ghost;
    }

    [Fact]
    public void Many_ghosts_replaced_at_once_change_the_layer_once()
    {
        var layer = new GhostsLayer();
        int changes = 0;
        layer.Changed += () => changes++;
        using (layer.Batch())
        {
            for (int i = 0; i < 32; i++)
                layer.Add(Sample()).IsLight = true;
            layer.Remove(layer.Ghosts[0]);
        }
        Assert.Equal(1, changes);
        Assert.Equal(31, layer.Ghosts.Count);
        layer.Add(Sample());
        Assert.Equal(2, changes);
    }

    [Fact]
    public void Files_round_trip_in_the_legacy_format()
    {
        using var stream = new MemoryStream();
        Sample().Write(stream);
        stream.Position = 0;
        var read = Ghost.Read(stream, "copy");
        Assert.Equal(99u, read.PlaybackBaseFrame);
        Assert.Equal(Sample().Frames, read.Frames);
        Assert.Equal(8u, read.AnimationSwitches[7]);
    }

    [Fact]
    public void Files_without_a_trailer_read_too()
    {
        using var stream = new MemoryStream();
        using (var w = new BinaryWriter(stream, System.Text.Encoding.UTF8, leaveOpen: true))
        {
            w.Write(10u);
            w.Write(1);
            w.Write(0u);
            w.Write(1f); w.Write(2f); w.Write(3f);
            w.Write((short)4); w.Write((short)5);
            w.Write(6u); w.Write(7u); w.Write(8u);
        }
        stream.Position = 0;
        var ghost = Ghost.Read(stream, "old");
        Assert.Equal(new Vector3(1, 2, 3), ghost.Frames[0].Position);
        Assert.Equal(7u, ghost.Frames[0].Yaw);
    }

    [Fact]
    public void Frame_i_plays_at_base_plus_one_plus_i()
    {
        var ghost = Sample();
        Assert.Null(ghost.FrameAt(99));
        Assert.Equal(0, ghost.FrameAt(100)!.Value.Position.X);
        Assert.Equal(20, ghost.FrameAt(102)!.Value.Position.X);
        Assert.Null(ghost.FrameAt(103));
        Assert.Equal(102u, ghost.TimerOf(2));
    }

    [Fact]
    public void A_recording_replays_at_the_timers_it_was_recorded_at_and_rewinds_overwrite()
    {
        var recorder = new GhostRecorder();
        GameFrame At(uint timer, float x) => TestRam.Us().Do(p => p.WriteU32(Timer, timer).WriteF32(TestRam.UsMario + O.X, x)).Frame();
        recorder.Record(At(500, 1));
        recorder.Record(At(501, 2));
        recorder.Record(At(502, 3));
        recorder.Record(At(501, 20));
        var ghost = recorder.ToGhost("rec");
        Assert.Equal(3, ghost.Frames.Count);
        Assert.Equal(1, ghost.FrameAt(500)!.Value.Position.X);
        Assert.Equal(20, ghost.FrameAt(501)!.Value.Position.X);
        Assert.Equal(3, ghost.FrameAt(502)!.Value.Position.X);
    }

    [Fact]
    public void The_layer_draws_the_ghost_where_it_is_at_the_current_timer()
    {
        var layer = new GhostsLayer();
        layer.Add(Sample());
        var scene = new MapScene();
        var frame = TestRam.Us().Do(p => p.WriteU32(Timer, 101)).Frame();
        layer.Build(new MapContext(frame, new MapView { Width = 800, Height = 600 }, scene, new SettingsStore().Scope("t"), new Selection()));
        var marker = scene.Items.OfType<MapMarker>().Single();
        Assert.Equal(new Vector2(10, -1), marker.Center);
        Assert.Equal(0x4001, marker.Yaw);
        Assert.Single(scene.Items.OfType<MapPolyline>());
    }

    [Fact]
    public void Reads_a_real_legacy_ghost_file_when_present()
    {
        const string path = @"D:\mupen\STROOP\tmp.ghost";
        if (!File.Exists(path))
            return;
        using var stream = File.OpenRead(path);
        var ghost = Ghost.Read(stream, "tmp");
        Assert.NotEmpty(ghost.Frames);
        Assert.All(ghost.Frames.Values, f => Assert.True(MathF.Abs(f.Position.X) < 1e6 && MathF.Abs(f.Position.Z) < 1e6));
        if (ghost.Frames.ContainsKey(uint.MaxValue))
        {
            Assert.Equal(ghost.PlaybackBaseFrame, ghost.FirstTimer);
            Assert.Equal(ghost.Frames[uint.MaxValue], ghost.FrameAt(ghost.PlaybackBaseFrame));
            Assert.Equal(ghost.Frames[uint.MaxValue], ghost.InPlaybackOrder.First());
        }
    }

    [Fact]
    public void Moving_the_start_keeps_the_frames_and_shifts_the_timers()
    {
        var ghost = Sample();
        ghost.Frames[uint.MaxValue] = new GhostFrame(new Vector3(-1, 0, 0), 0, 0, 0, 0, 0);
        Assert.Equal(99u, ghost.FirstTimer);
        ghost.StartAt(1000);
        Assert.Equal(1000u, ghost.FirstTimer);
        Assert.Equal(-1, ghost.FrameAt(1000)!.Value.Position.X);
        Assert.Equal(0, ghost.FrameAt(1001)!.Value.Position.X);
    }
}
