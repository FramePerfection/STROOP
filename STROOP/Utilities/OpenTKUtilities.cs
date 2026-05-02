using OpenTK.Mathematics;
using System;
using System.Drawing;

namespace STROOP.Utilities;

public static class OpenTKUtilities
{
    public static bool TryParseVector3(string text, out Vector3 value)
    {
        value = default(Vector3);
        if (text == null) return false;
        string[] split = text.Split(';');
        return (split.Length == 3
                && float.TryParse(split[0].Trim(), out value.X)
                && float.TryParse(split[1].Trim(), out value.Y)
                && float.TryParse(split[2].Trim(), out value.Z));
    }

    public static Vector4 ColorToVec4(Color color, int alpha = -1) =>
        new Vector4(color.R / 255f, color.G / 255f, color.B / 255f, (alpha == -1 ? color.A : alpha) / 255f);

    public static Color Vec4ToColor(Vector4 color) =>
        Color.FromArgb((byte)(Math.Max(0, Math.Min(255, color.W * 255))),
            (byte)(Math.Max(0, Math.Min(255, color.X * 255))),
            (byte)(Math.Max(0, Math.Min(255, color.Y * 255))),
            (byte)(Math.Max(0, Math.Min(255, color.Z * 255))));

    public static Vector4 ColorFromHSV(float hue, float saturation, float value, float alpha = 1)
    {
        int hi = Convert.ToInt32(Math.Floor(hue / 60)) % 6;
        float f = hue / 60 - (float)Math.Floor(hue / 60);

        float v = value;
        float p = value * (1 - saturation);
        float q = value * (1 - f * saturation);
        float t = value * (1 - (1 - f) * saturation);

        if (hi == 0)
            return new Vector4(v, t, p, alpha);
        else if (hi == 1)
            return new Vector4(q, v, p, alpha);
        else if (hi == 2)
            return new Vector4(p, v, t, alpha);
        else if (hi == 3)
            return new Vector4(p, q, v, alpha);
        else if (hi == 4)
            return new Vector4(t, p, v, alpha);
        else
            return new Vector4(v, p, q, alpha);
    }

    public static Vector4 GetRandomColor(int seed)
    {
        Random rnd = new Random(seed);
        return ColorFromHSV((float)rnd.NextDouble() * 360, 0.5f, 0.8f);
    }
}
