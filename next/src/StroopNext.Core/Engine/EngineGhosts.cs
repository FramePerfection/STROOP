using System.Numerics;
using System.Text.Json.Nodes;
using StroopNext.Game;

namespace StroopNext.Engine;

public static class EngineGhosts
{
    public static Ghost? FromAnimation(JsonNode? anim, string name, uint stTimer)
    {
        if (anim?["frames"] is not JsonArray frames || frames.Count == 0)
            return null;
        var positions = new List<JsonArray?>(frames.Count);
        var poses = new List<JsonArray?>(frames.Count);
        foreach (var f in frames)
        {
            positions.Add(f?["m"] as JsonArray);
            poses.Add(f?["mo"] as JsonArray);
        }
        return Build(name, stTimer, positions, poses);
    }

    public static Ghost? FromReference(JsonNode? reference, string name, uint stTimer)
    {
        if (reference?["m"] is not JsonArray m || m.Count == 0)
            return null;
        var poses = reference["mo"] as JsonArray;
        return Build(name, stTimer, m.Select(x => x as JsonArray).ToList(), Enumerable.Range(0, m.Count).Select(i => poses is not null && i < poses.Count ? poses[i] as JsonArray : null).ToList());
    }

    public static IReadOnlyList<Ghost> FromTries(JsonNode? json, uint stTimer)
    {
        var list = new List<Ghost>();
        if (json?["tries"] is not JsonArray tries)
            return list;
        foreach (var t in tries)
        {
            string label = t?["label"]?.ToString() ?? "";
            string hit = EngineJson.IntOrNull(t?["hit"]) is int h ? $" ✓ {h} f" : "";
            if (FromReference(t, $"Engine try {list.Count + 1}: {label}{hit}", stTimer) is { } g)
                list.Add(g);
        }
        return list;
    }

    private static Ghost? Build(string name, uint stTimer, IReadOnlyList<JsonArray?> positions, IReadOnlyList<JsonArray?> poses)
    {
        var ghost = new Ghost { Name = name, PlaybackBaseFrame = unchecked(stTimer - 1) };
        for (int k = 0; k < positions.Count; k++)
        {
            if (positions[k] is not { Count: >= 4 } m)
                continue;
            var position = new Vector3(EngineJson.Float(m[0]), EngineJson.Float(m[1]), EngineJson.Float(m[2]));
            uint yaw = (ushort)EngineJson.Int(m[3]);
            uint pitch = 0, roll = 0;
            short animation = 0, animationFrame = 0;
            if (poses[k] is { Count: >= 9 } mo)
            {
                pitch = (ushort)EngineJson.Int(mo[0]);
                yaw = (ushort)EngineJson.Int(mo[1]);
                roll = (ushort)EngineJson.Int(mo[2]);
                animation = (short)(EngineJson.IntOrNull(mo[7]) ?? 0);
                animationFrame = (short)((EngineJson.IntOrNull(mo[8]) ?? 1) - 1);
            }
            ghost.Frames[(uint)k] = new GhostFrame(position, animation, animationFrame, pitch, yaw, roll);
        }
        return ghost.Frames.Count == 0 ? null : ghost;
    }
}
