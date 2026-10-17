namespace StroopNext.Game;

public static class GameCollision
{
    public static short Truncate(float coordinate) => (short)(int)coordinate;

    public static bool IsInside(Surface s, int x, int z)
    {
        int sign = s.Class == SurfaceClass.Ceiling ? -1 : 1;
        int x1 = (int)s.V1.X, z1 = (int)s.V1.Z, x2 = (int)s.V2.X, z2 = (int)s.V2.Z, x3 = (int)s.V3.X, z3 = (int)s.V3.Z;
        return sign * ((long)(z1 - z) * (x2 - x1) - (long)(x1 - x) * (z2 - z1)) >= 0
            && sign * ((long)(z2 - z) * (x3 - x2) - (long)(x2 - x) * (z3 - z2)) >= 0
            && sign * ((long)(z3 - z) * (x1 - x3) - (long)(x3 - x) * (z1 - z3)) >= 0;
    }

    public static (Surface Floor, float Height)? FindFloor(SurfaceTable surfaces, float x, float y, float z)
    {
        short ix = Truncate(x), iz = Truncate(z);
        (Surface, float)? best = null;
        foreach (var list in new[] { surfaces.Static, surfaces.Dynamic })
            for (int i = 0; i < list.Count; i++)
            {
                var s = list[i];
                if (s.Class != SurfaceClass.Floor || !InBounds(s, ix, iz) || !IsInside(s, ix, iz))
                    continue;
                float h = s.HeightAt(ix, iz);
                if (y - (h - 78) < 0)
                    continue;
                if (best is null || h > best.Value.Item2)
                    best = (s, h);
            }
        return best;
    }

    private static bool InBounds(Surface s, int x, int z)
        => x >= MathF.Min(s.V1.X, MathF.Min(s.V2.X, s.V3.X)) && x <= MathF.Max(s.V1.X, MathF.Max(s.V2.X, s.V3.X))
        && z >= MathF.Min(s.V1.Z, MathF.Min(s.V2.Z, s.V3.Z)) && z <= MathF.Max(s.V1.Z, MathF.Max(s.V2.Z, s.V3.Z));

    public static IEnumerable<(int Z, int XMin, int XMax)> UnitRows(Surface s, int zMin, int zMax)
    {
        int sign = s.Class == SurfaceClass.Ceiling ? -1 : 1;
        var v = new (long X, long Z)[] { ((long)s.V1.X, (long)s.V1.Z), ((long)s.V2.X, (long)s.V2.Z), ((long)s.V3.X, (long)s.V3.Z) };
        int top = (int)Math.Max(zMin, Math.Min(v[0].Z, Math.Min(v[1].Z, v[2].Z)));
        int bottom = (int)Math.Min(zMax, Math.Max(v[0].Z, Math.Max(v[1].Z, v[2].Z)));
        long left = (long)Math.Min(v[0].X, Math.Min(v[1].X, v[2].X)), right = (long)Math.Max(v[0].X, Math.Max(v[1].X, v[2].X));
        for (int z = top; z <= bottom; z++)
        {
            long lo = left, hi = right;
            bool empty = false;
            for (int e = 0; e < 3 && !empty; e++)
            {
                var (x1, z1) = v[e];
                var (x2, z2) = v[(e + 1) % 3];
                long dz = sign * (z2 - z1), c = sign * ((z1 - z) * (x2 - x1) - x1 * (z2 - z1));
                if (dz > 0)
                    lo = Math.Max(lo, CeilDiv(-c, dz));
                else if (dz < 0)
                    hi = Math.Min(hi, FloorDiv(-c, dz));
                else if (c < 0)
                    empty = true;
            }
            if (!empty && lo <= hi)
                yield return (z, (int)lo, (int)hi);
        }
    }

    public static (float Min, float Max) UnitSpan(int unit) => unit > 0 ? (unit, unit + 1) : unit < 0 ? (unit - 1, unit) : (-1, 1);

    private static long FloorDiv(long a, long b) => a / b - ((a % b != 0) && ((a < 0) != (b < 0)) ? 1 : 0);
    private static long CeilDiv(long a, long b) => -FloorDiv(-a, b);
}
