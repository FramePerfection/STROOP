using System.Numerics;
using StroopNext.Session;

namespace StroopNext.Game;

public readonly record struct GhostFrame(Vector3 Position, short AnimationIndex, short AnimationFrame, uint Pitch, uint Yaw, uint Roll)
{
    public ushort FacingYaw => (ushort)Yaw;
}

public sealed class Ghost
{
    private const uint ObjectExtraMagic = 0x4F424A54;
    private const uint BodyMagic = 0x424F4459;

    public string Name { get; set; } = "Ghost";
    public uint PlaybackBaseFrame { get; set; }
    public SortedDictionary<uint, GhostFrame> Frames { get; } = new();

    public uint NonMarioGraphics { get; set; }
    public Dictionary<uint, uint> AnimationSwitches { get; } = new();

    public Dictionary<uint, byte[]> Bodies { get; } = new();

    // Replays from the start once the last frame is passed (the engine's short attempts), instead of vanishing.
    public bool Loop { get; set; }

    // The frame index played at this timer (negative as int before the start).
    public uint IndexAt(uint globalTimer)
    {
        uint index = unchecked(globalTimer - 1 - PlaybackBaseFrame);
        if (Loop && Frames.Count > 0 && Order(index) > 0)
        {
            uint length = Frames.Keys.Max(k => (uint)Math.Max(0, Order(k))) + 1;
            if (index >= length)
                index %= length;
        }
        return index;
    }

    public GhostFrame? FrameAt(uint globalTimer)
        => Frames.TryGetValue(IndexAt(globalTimer), out var f) ? f : null;

    public uint TimerOf(uint index) => unchecked(PlaybackBaseFrame + 1 + index);

    public static int Order(uint index) => unchecked((int)index);

    public IEnumerable<GhostFrame> InPlaybackOrder => Frames.OrderBy(kv => Order(kv.Key)).Select(kv => kv.Value);

    public uint FirstTimer => Frames.Count == 0 ? PlaybackBaseFrame + 1 : TimerOf(Frames.Keys.MinBy(Order));

    public void StartAt(uint timer)
        => PlaybackBaseFrame = unchecked(timer - 1 - (Frames.Count == 0 ? 0 : Frames.Keys.MinBy(Order)));

    public static Ghost Read(Stream stream, string name)
    {
        using var reader = new BinaryReader(stream, System.Text.Encoding.UTF8, leaveOpen: true);
        var ghost = new Ghost { Name = name, PlaybackBaseFrame = reader.ReadUInt32() };
        int count = reader.ReadInt32();
        if (count < 0 || count > 10_000_000)
            throw new InvalidDataException($"Not a ghost file (frame count {count}).");
        for (int i = 0; i < count; i++)
        {
            uint index = reader.ReadUInt32();
            ghost.Frames[index] = new GhostFrame(
                new Vector3(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle()),
                AnimationIndex: reader.ReadInt16(), AnimationFrame: reader.ReadInt16(),
                Pitch: reader.ReadUInt32(), Yaw: reader.ReadUInt32(), Roll: reader.ReadUInt32());
        }
        if (stream.Length - stream.Position >= 12 && reader.ReadUInt32() == ObjectExtraMagic)
        {
            ghost.NonMarioGraphics = reader.ReadUInt32();
            uint switches = reader.ReadUInt32();
            for (uint i = 0; i < switches && stream.Length - stream.Position >= 8; i++)
                ghost.AnimationSwitches[reader.ReadUInt32()] = reader.ReadUInt32();
            if (stream.Length - stream.Position >= 8 && reader.ReadUInt32() == BodyMagic)
            {
                int bodies = reader.ReadInt32();
                for (int i = 0; i < bodies && stream.Length - stream.Position >= 4 + InGameGhosts.BodyBytes; i++)
                    ghost.Bodies[reader.ReadUInt32()] = reader.ReadBytes(InGameGhosts.BodyBytes);
            }
        }
        return ghost;
    }

    public void Write(Stream stream)
    {
        using var writer = new BinaryWriter(stream, System.Text.Encoding.UTF8, leaveOpen: true);
        writer.Write(PlaybackBaseFrame);
        writer.Write(Frames.Count);
        foreach (var (index, f) in Frames)
        {
            writer.Write(index);
            writer.Write(f.Position.X);
            writer.Write(f.Position.Y);
            writer.Write(f.Position.Z);
            writer.Write(f.AnimationIndex);
            writer.Write(f.AnimationFrame);
            writer.Write(f.Pitch);
            writer.Write(f.Yaw);
            writer.Write(f.Roll);
        }
        writer.Write(ObjectExtraMagic);
        writer.Write(NonMarioGraphics);
        writer.Write(AnimationSwitches.Count);
        foreach (var (key, value) in AnimationSwitches)
        {
            writer.Write(key);
            writer.Write(value);
        }
        if (Bodies.Count > 0)
        {
            writer.Write(BodyMagic);
            writer.Write(Bodies.Count);
            foreach (var (key, body) in Bodies)
            {
                writer.Write(key);
                writer.Write(body.AsSpan(0, InGameGhosts.BodyBytes));
            }
        }
    }
}

public sealed class GhostRecorder : ISampleTrack
{
    private readonly object _lock = new();
    private readonly SortedDictionary<uint, GhostFrame> _byTimer = new();
    private readonly Dictionary<uint, byte[]> _bodies = new();

    public string Id => "ghost.recorder";

    public int Count
    {
        get
        {
            lock (_lock)
                return _byTimer.Count;
        }
    }

    public void Record(GameFrame frame)
    {
        if (Sample(frame) is not { } sample)
            return;
        byte[]? body = SampleBody(frame);
        lock (_lock)
        {
            _byTimer[frame.GlobalTimer] = sample;
            if (body is not null)
                _bodies[frame.GlobalTimer] = body;
        }
    }

    public static byte[]? SampleBody(GameFrame frame)
    {
        if (frame.Mario is not { } m || !frame.Ram.IsValid(m.Address + 0x244, InGameGhosts.BodyBytes))
            return null;
        var body = new byte[InGameGhosts.BodyBytes];
        for (int i = 0; i < body.Length; i++)
            body[i] = frame.Ram.U8(m.Address + 0x244 + (uint)i);
        return body;
    }

    public void Clear()
    {
        lock (_lock)
        {
            _byTimer.Clear();
            _bodies.Clear();
        }
    }

    public static GhostFrame? Sample(GameFrame frame)
    {
        if (frame.Mario is not { } m)
            return null;
        uint obj = m.MarioObject;
        bool hasObject = frame.Ram.IsPointer(obj);
        Graphics.MarioAnimations.Capture(frame.Ram, obj);
        return new GhostFrame(m.Position,
            AnimationIndex: hasObject ? frame.Ram.S16(obj + 0x38) : (short)0,
            AnimationFrame: hasObject ? (short)(frame.Ram.S16(obj + 0x40) - 1) : (short)0,
            Pitch: hasObject ? frame.Ram.U32(obj + 0xD0) : 0,
            Yaw: hasObject ? frame.Ram.U32(obj + 0xD4) : m.FacingYaw,
            Roll: hasObject ? frame.Ram.U32(obj + 0xD8) : 0);
    }

    public Ghost ToGhost(string name)
    {
        lock (_lock)
        {
            var ghost = new Ghost { Name = name };
            if (_byTimer.Count == 0)
                return ghost;
            uint first = _byTimer.Keys.First();
            ghost.PlaybackBaseFrame = first - 1;
            foreach (var (timer, f) in _byTimer)
            {
                ghost.Frames[timer - first] = f;
                if (_bodies.TryGetValue(timer, out var body))
                    ghost.Bodies[timer - first] = body;
            }
            return ghost;
        }
    }
}
