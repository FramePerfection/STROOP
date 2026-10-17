using System.Diagnostics;
using StroopNext.Game;
using StroopNext.Map;
using StroopNext.Memory;
using StroopNext.Platform;
using StroopNext.Session;

namespace StroopNext.Desktop.Diagnostics;

internal static class GhostServe
{
    public static int Run(string controlFile, string? ghostOut)
    {
        var candidate = EmulatorDiscovery.Find().FirstOrDefault();
        if (candidate is null)
        {
            Console.Error.WriteLine("No running emulator.");
            return 1;
        }
        using var source = ProcessRamSource.Attach(candidate);
        var bytes = new byte[source.Size];
        var hack = new InGameGhosts();
        var recorder = new GhostRecorder();
        Ghost? ghost = null;
        string mode = "idle", lastLine = "";
        var clock = Stopwatch.StartNew();
        long lastReport = 0;
        Log($"attached to {candidate.ProcessName}, RAM {source.Size / 0x100000} MB, code cache {(source.CanInvalidateCode ? "found" : "not found")}");

        while (true)
        {
            if (!source.TryRead(bytes))
            {
                Log("read failed");
                return 1;
            }
            var frame = GameFrame.Create(new RamImage(bytes));
            string command = mode;
            try
            {
                if (File.Exists(controlFile))
                    command = File.ReadAllText(controlFile).Trim();
            }
            catch (IOException)
            {
            }
            if (command != mode)
            {
                Log($"command: {command}");
                if (command.StartsWith("poke16 ", StringComparison.Ordinal) && command.Split(' ') is [_, var a, var v])
                {
                    var poke = new RamPatch().WriteU16(Convert.ToUInt32(a, 16), Convert.ToUInt16(v, 16));
                    Log($"poke: {Apply(source, poke)}");
                    mode = command;
                    continue;
                }
                switch (command)
                {
                    case "install":
                        var patch = hack.Install(frame, out string? error);
                        if (patch is null)
                            Log($"install refused: {error}");
                        else
                            Log($"install written: {Apply(source, patch)}");
                        break;
                    case "remove":
                        Log($"disable requested: {Apply(source, hack.RequestDisable())}");
                        break;
                    case "record":
                        recorder.Clear();
                        break;
                    case "play":
                        ghost = recorder.ToGhost("test");
                        Log($"ghost: {ghost.Frames.Count} frames, timers {ghost.FirstTimer}…{ghost.FirstTimer + (uint)ghost.Frames.Count - 1}");
                        if (ghostOut is not null)
                            using (var f = File.Create(ghostOut))
                                ghost.Write(f);
                        break;
                    case "playnow":
                        ghost = recorder.ToGhost("test");
                        ghost.StartAt(frame.GlobalTimer + 1);
                        Log($"ghost replays now: {ghost.Frames.Count} frames from timer {ghost.FirstTimer}");
                        break;
                    case "quit":
                        return 0;
                }
                mode = command;
            }

            if (mode == "record")
                recorder.Record(frame);
            if (hack.ColoredCaps(frame) is { } caps)
                Log($"caps written: {Apply(source, caps)}");
            if (mode == "remove" && hack.State(frame) == InGameGhostState.Disabling && hack.ReadyToRestore(frame))
                Log(hack.Restore(frame) is { } restore ? $"restored: {Apply(source, restore)} ({restore.Writes.Count} writes)" : "nothing to restore");
            if (mode is "play" or "playnow" && ghost is not null)
            {
                var feed = hack.Feed(frame, [(ghost, MapColor.Rgb(0, 200, 0))], transparent: false);
                if (!feed.IsEmpty)
                    Apply(source, feed);
            }

            string line = Describe(frame, hack, ghost);
            if (line != lastLine || clock.ElapsedMilliseconds - lastReport > 2000)
            {
                Log(line);
                lastLine = line;
                lastReport = clock.ElapsedMilliseconds;
            }
            Thread.Sleep(5);
        }
    }

    private static bool Apply(IRamSource source, RamPatch patch)
    {
        bool ok = true;
        using var frozen = source.Freeze();
        foreach (var (offset, data) in patch.ToSwappedRuns())
        {
            ok &= source.TryWrite(offset, data);
            source.InvalidateCode(offset, data.Length);
        }
        return ok;
    }

    private static string Describe(GameFrame frame, InGameGhosts hack, Ghost? ghost)
    {
        var ram = frame.Ram;
        string mario = "";
        if (frame.Layout?.MarioObjectPointer is uint p && ram.IsPointer(ram.U32(p)))
        {
            uint m = ram.U32(p);
            mario = $"mario anim {ram.S16(m + 0x38)}/{ram.S16(m + 0x40)} flags {ram.U16(m + 2):X4} pos ({frame.Mario?.Position.X:0.###},{frame.Mario?.Position.Y:0.###},{frame.Mario?.Position.Z:0.###})";
        }
        string nodes = string.Join(" | ", InGameGhosts.GameGhostNodes(ram).Select(n =>
            $"node {n:X8} {(Linked(ram, n) ? "linked" : "ORPHAN")} pos ({ram.F32(n + 0x20):0},{ram.F32(n + 0x24):0},{ram.F32(n + 0x28):0}) anim {ram.S16(n + 0x38)}/{ram.S16(n + 0x40)} ptr {ram.U32(n + 0x3C):X8} flags {ram.U16(n + 2):X4}"));
        string expected = ghost?.FrameAt(frame.GlobalTimer) is { } g ? $"ghost frame anim {g.AnimationIndex}/{g.AnimationFrame} at ({g.Position.X:0},{g.Position.Y:0},{g.Position.Z:0})" : "no ghost frame";
        string model = "no model";
        if (frame.Layout?.MarioObjectPointer is uint mp && ram.IsPointer(ram.U32(mp)) && ram.U32(ram.U32(mp) + 0x14) is var root && ram.IsPointer(root))
            model = $"model {root:X8} {(ram.U32(root + 0x10) == 0x80407140 ? "switchers" : "plain")}";
        return $"timer {frame.GlobalTimer} · level {ram.S16(0x8032DDF8)} · {model} · {hack.State(frame)} · loop {(InGameGhosts.LoopHasRun(frame) ? "ran" : "not run")} · count {ram.U8(0x80407FFF)} · {mario} · {expected} · {(nodes.Length == 0 ? "no ghost node" : nodes)}";
    }

    private static bool Linked(RamImage ram, uint node)
    {
        uint parent = ram.U32(node + 0xC);
        if (!ram.IsPointer(parent))
            return false;
        uint first = ram.U32(parent + 0x10), at = first;
        for (int i = 0; i < 0x800 && ram.IsPointer(at); i++)
        {
            if (at == node)
                return true;
            at = ram.U32(at + 8);
            if (at == first)
                break;
        }
        return false;
    }

    private static void Log(string text) => Console.WriteLine($"{DateTime.Now:HH:mm:ss.fff} {text}");
}
