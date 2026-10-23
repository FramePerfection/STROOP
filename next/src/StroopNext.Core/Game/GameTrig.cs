using StroopNext.Session;

namespace StroopNext.Game;

public static class GameTrig
{
    public static ushort Truncate(int angle) => (ushort)(angle & 0xFFF0);

    public static float InGameSine(int angle)
    {
        ushort a = Truncate(angle);
        return a == 32768 ? 0 : (float)Math.Sin(Angle.ToRadians(a));
    }

    public static float InGameCosine(int angle)
    {
        ushort a = Truncate(angle);
        return a is 16384 or 49152 ? 0 : (float)Math.Cos(Angle.ToRadians(a));
    }

    public static ushort InGameAtan(GameFrame frame, float xComp, float yComp)
    {
        ushort Lookup(float y, float x) => InGameAtanLookup(frame, y, x);
        int result;
        if (0 <= yComp)
            if (0 <= xComp)
                result = yComp <= xComp ? Lookup(yComp, xComp) : 0x4000 - Lookup(xComp, yComp);
            else if (-xComp < yComp)
                result = 0x4000 + Lookup(-xComp, yComp);
            else
                result = 0x8000 - Lookup(yComp, -xComp);
        else if (xComp < 0)
            if (-yComp < -xComp)
                result = 0x8000 + Lookup(-yComp, -xComp);
            else
                result = 0xC000 - Lookup(-xComp, -yComp);
        else if (xComp < -yComp)
            result = 0xC000 + Lookup(xComp, -yComp);
        else
            result = 0x10000 - Lookup(-yComp, xComp);
        return (ushort)result;
    }

    public static ushort? InGameAngleTo(GameFrame frame, float xFrom, float zFrom, float xTo, float zTo)
        => ArctanTable(frame) is null ? null : InGameAtan(frame, zTo - zFrom, xTo - xFrom);

    private static uint? ArctanTable(GameFrame frame)
        => frame.Layout is { } l && Sm64Addresses.ArctanTable.For(l.Version) is uint a && frame.Ram.IsValid(a, 2) ? a : null;

    private static ushort InGameAtanLookup(GameFrame frame, float y, float x)
    {
        uint offset = x == 0 ? 0 : 2 * (uint)(y / x * 1024f + 0.5f);
        return ArctanTable(frame) is uint table && frame.Ram.IsValid(table + offset, 2) ? frame.Ram.U16(table + offset) : (ushort)0;
    }

    public static (float X, float Y, float Z) RotationDisplacement(
        (float X, float Y, float Z) offset, (short Pitch, short Yaw, short Roll) facing, (short Pitch, short Yaw, short Roll) angularVelocity)
    {
        var before = ((short)(facing.Pitch - angularVelocity.Pitch), (short)(facing.Yaw - angularVelocity.Yaw), (short)(facing.Roll - angularVelocity.Roll));
        var relative = TransposeMul(RotateZxy(before), offset);
        var moved = TransposeMul(RotateZxy((facing.Pitch, facing.Yaw, facing.Roll)), relative);
        return (moved.X - offset.X, moved.Y - offset.Y, moved.Z - offset.Z);
    }

    private static float[,] RotateZxy((short Pitch, short Yaw, short Roll) r)
    {
        float sx = InGameSine(r.Pitch), cx = InGameCosine(r.Pitch);
        float sy = InGameSine(r.Yaw), cy = InGameCosine(r.Yaw);
        float sz = InGameSine(r.Roll), cz = InGameCosine(r.Roll);
        return new float[3, 3]
        {
            { cy * cz + sx * sy * sz, cx * sz, -sy * cz + sx * cy * sz },
            { -cy * sz + sx * sy * cz, cx * cz, sy * sz + sx * cy * cz },
            { cx * sy, -sx, cx * cy },
        };
    }

    private static (float X, float Y, float Z) TransposeMul(float[,] m, (float X, float Y, float Z) v)
        => (m[0, 0] * v.X + m[0, 1] * v.Y + m[0, 2] * v.Z,
            m[1, 0] * v.X + m[1, 1] * v.Y + m[1, 2] * v.Z,
            m[2, 0] * v.X + m[2, 1] * v.Y + m[2, 2] * v.Z);
}
