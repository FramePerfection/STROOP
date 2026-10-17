using System.Numerics;
using StroopNext.Memory;
using StroopNext.Session;
using StroopNext.Settings;
using O = StroopNext.Game.MarioOffsets;

namespace StroopNext.Core.Tests;

public class SessionTests
{
    private const uint Timer = 0x8032D5D4;

    private static (GameSession Session, BufferRamSource Source) Attach(Action<RamPatch>? setup = null)
    {
        var ram = TestRam.Us();
        setup?.Invoke(ram.Patch);
        ram.Image();
        var source = new BufferRamSource("test", ram.Bytes);
        var session = new GameSession(autoPoll: false);
        session.Attach(source);
        return (session, source);
    }

    private static void SetTimer(GameSession session, uint value) => session.Apply(new RamPatch().WriteU32(Timer, value));

    [Fact]
    public void Writes_are_visible_after_the_next_poll()
    {
        var (session, _) = Attach();
        using (session)
        {
            session.Apply(new RamPatch().WriteF32(TestRam.UsMario + O.X, 42));
            session.PollOnce();
            var frame = session.AcquireLatest()!;
            Assert.Equal(42f, frame.Mario!.Position.X);
            frame.Release();
        }
    }

    [Fact]
    public void Previous_is_the_last_frame_with_a_different_global_timer()
    {
        var (session, _) = Attach(p => p.WriteU32(Timer, 10));
        using (session)
        {
            session.PollOnce();
            session.PollOnce();
            var f = session.AcquireLatest()!;
            Assert.Null(f.Previous);
            f.Release();

            SetTimer(session, 11);
            session.PollOnce();
            session.PollOnce();
            f = session.AcquireLatest()!;
            Assert.Equal(11u, f.GlobalTimer);
            Assert.Equal(10u, f.Previous!.GlobalTimer);
            Assert.Null(f.Previous.Previous);
            f.Release();
        }
    }

    [Fact]
    public void Detach_clears_the_published_frame_and_reattach_starts_a_fresh_history()
    {
        var (session, _) = Attach(p => p.WriteU32(Timer, 5));
        using (session)
        {
            session.PollOnce();
            session.Detach();
            Assert.False(session.PollOnce());
            Assert.Null(session.AcquireLatest());

            var other = TestRam.Us().Do(p => p.WriteU32(Timer, 6));
            other.Image();
            session.Attach(new BufferRamSource("other", other.Bytes));
            session.PollOnce();
            var f = session.AcquireLatest()!;
            Assert.Null(f.Previous);
            f.Release();
        }
    }

    [Fact]
    public void Long_session_with_ui_like_holding_leaks_no_buffer()
    {
        var (session, source) = Attach(p => p.WriteU32(Timer, 0));
        var random = new Random(1234);
        var held = new List<GameFrame>();
        using (session)
        {
            for (uint i = 1; i <= 2000; i++)
            {
                if (random.Next(3) > 0)
                    SetTimer(session, i);
                session.PollOnce();
                if (session.AcquireLatest() is { } f)
                    held.Add(f);
                while (held.Count > random.Next(1, 4))
                {
                    held[0].Release();
                    held.RemoveAt(0);
                }
                if (i % 500 == 0)
                {
                    session.Detach();
                    var copy = new byte[source.Size];
                    source.TryRead(copy);
                    session.Attach(new BufferRamSource("again", copy));
                    source = (BufferRamSource)session.Source!;
                }
                Assert.InRange(session.Diagnostics.PooledBuffers, 0, 8);
            }
            foreach (var f in held)
                f.Release();
            session.Detach();
            Assert.Equal(0, session.Diagnostics.OutstandingBuffers);
            Assert.Equal(0, session.Diagnostics.PooledBuffers);
        }
    }

    [Fact]
    public void Polls_only_copy_ram_when_the_game_advanced_or_was_written()
    {
        var (session, _) = Attach(p => p.WriteU32(Timer, 1));
        using (session)
        {
            session.IdleRefreshRate = 0;
            Assert.True(session.PollOnce());
            Assert.False(session.PollOnce());
            Assert.False(session.PollOnce());
            Assert.Equal(1, session.Diagnostics.FullReads);
            Assert.Equal(2, session.Diagnostics.SkippedPolls);

            session.Apply(new RamPatch().WriteF32(TestRam.UsMario + O.X, 5));
            Assert.True(session.PollOnce());
            SetTimer(session, 2);
            Assert.True(session.PollOnce());
            Assert.Equal(3, session.Diagnostics.FullReads);
        }
    }

    [Fact]
    public void Paused_game_is_still_refreshed_at_the_idle_rate()
    {
        var (session, source) = Attach(p => p.WriteU32(Timer, 1));
        using (session)
        {
            session.IdleRefreshRate = 1000;
            session.PollOnce();
            foreach (var (offset, bytes) in new RamPatch().WriteF32(TestRam.UsMario + O.X, 77).ToSwappedRuns())
                source.TryWrite(offset, bytes);
            Thread.Sleep(5);
            Assert.True(session.PollOnce());
            var f = session.AcquireLatest()!;
            Assert.Equal(77f, f.Mario!.Position.X);
            long sequence = f.Sequence;
            f.Release();

            Thread.Sleep(5);
            Assert.False(session.PollOnce());
            Assert.Equal(1, session.Diagnostics.UnchangedRefreshes);
            f = session.AcquireLatest()!;
            Assert.Equal(sequence, f.Sequence);
            f.Release();
        }
    }

    [Fact]
    public void Buffers_are_recycled_instead_of_reallocated()
    {
        var (session, _) = Attach();
        using (session)
        {
            for (uint i = 0; i < 50; i++)
            {
                SetTimer(session, i);
                session.PollOnce();
                var f = session.AcquireLatest()!;
                f.Release();
            }
            var pool = (FramePool)typeof(GameSession).GetField("_pool", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!.GetValue(session)!;
            Assert.InRange(pool.Allocated, 1, 4);
        }
    }

    [Fact]
    public void Held_frames_keep_their_data_while_polling_continues()
    {
        var (session, _) = Attach(p => p.WriteU32(Timer, 1).WriteF32(TestRam.UsMario + O.X, 1));
        using (session)
        {
            session.PollOnce();
            var held = session.AcquireLatest()!;
            for (uint i = 2; i < 20; i++)
            {
                session.Apply(new RamPatch().WriteU32(Timer, i).WriteF32(TestRam.UsMario + O.X, i));
                session.PollOnce();
            }
            Assert.Equal(1f, held.Ram.F32(TestRam.UsMario + O.X));
            held.Release();
        }
    }

    [Fact]
    public void Tracks_sample_once_per_game_frame_and_reset_on_rewind()
    {
        var (session, _) = Attach(p => p.WriteU32(Timer, 100));
        using (session)
        {
            var track = session.AddTrack(new SampleTrack<Vector3>("pos", f => f.Mario?.Position));
            session.PollOnce();
            session.PollOnce();
            SetTimer(session, 101);
            session.PollOnce();
            Assert.Equal([100u, 101u], track.Snapshot().Select(s => s.GlobalTimer));

            SetTimer(session, 50);
            session.PollOnce();
            Assert.Equal([50u], track.Snapshot().Select(s => s.GlobalTimer));
        }
    }

    [Fact]
    public void Track_is_a_bounded_ring()
    {
        var track = new SampleTrack<int>("t", f => (int)f.GlobalTimer, capacity: 3);
        for (uint i = 1; i <= 5; i++)
            track.Record(TestRam.Us().Do(p => p.WriteU32(Timer, i)).Frame());
        Assert.Equal([3, 4, 5], track.Snapshot().Select(s => s.Value));
    }

    [Fact]
    public void Settings_round_trip_through_json()
    {
        string path = Path.Combine(Path.GetTempPath(), $"stroop-next-{Guid.NewGuid():N}.json");
        try
        {
            var opacity = new Setting<double>("opacity", "Opacity", 0.5);
            var mode = new Setting<Map.MapOrientation>("orientation", "Orientation", Map.MapOrientation.North);
            var store = new SettingsStore(path);
            var scope = store.Scope("map.layer.floors");
            Assert.Equal(0.5, scope.Get(opacity));
            scope.Set(opacity, 0.8);
            scope.Set(mode, Map.MapOrientation.Camera);
            store.Save();

            var reloaded = new SettingsStore(path).Scope("map.layer.floors");
            Assert.Equal(0.8, reloaded.Get(opacity));
            Assert.Equal(Map.MapOrientation.Camera, reloaded.Get(mode));
            Assert.Equal(0.8, reloaded.GetBoxed(opacity));
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Fact]
    public void Generic_editors_may_pass_any_numeric_type()
    {
        var frames = new Setting<int>("frames", "Frames", 150);
        var opacity = new Setting<double>("opacity", "Opacity", 0.5);
        var scope = new SettingsStore().Scope("layer");
        scope.SetBoxed(frames, 200.0);
        scope.SetBoxed(opacity, 1m);
        Assert.Equal(200, scope.Get(frames));
        Assert.Equal(1.0, scope.Get(opacity));
    }

    [Fact]
    public void Corrupt_settings_file_falls_back_to_defaults()
    {
        string path = Path.Combine(Path.GetTempPath(), $"stroop-next-{Guid.NewGuid():N}.json");
        File.WriteAllText(path, "{ not json");
        try
        {
            Assert.True(new SettingsStore(path).Get("x", true));
        }
        finally
        {
            File.Delete(path);
        }
    }
}
