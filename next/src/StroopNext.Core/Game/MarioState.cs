using System.Numerics;
using StroopNext.Memory;

namespace StroopNext.Game;

public static class MarioOffsets
{
    public const uint Flags = 0x04;
    public const uint FloorFootstep = 0x14;
    public const uint Action = 0x0C;
    public const uint PrevAction = 0x10;
    public const uint ActionState = 0x18;
    public const uint ActionTimer = 0x1A;
    public const uint ActionArg = 0x1C;
    public const uint ActionArgLow = 0x1E;
    public const uint IntendedMag = 0x20;
    public const uint IntendedYaw = 0x24;
    public const uint InvincTimer = 0x26;
    public const uint Pitch = 0x2C;
    public const uint FacingYaw = 0x2E;
    public const uint Roll = 0x30;
    public const uint PitchVelocity = 0x32;
    public const uint YawVelocity = 0x34;
    public const uint RollVelocity = 0x36;
    public const uint SlideYaw = 0x38;
    public const uint TwirlYaw = 0x3A;
    public const uint X = 0x3C;
    public const uint Y = 0x40;
    public const uint Z = 0x44;
    public const uint VelX = 0x48;
    public const uint VelY = 0x4C;
    public const uint VelZ = 0x50;
    public const uint HSpeed = 0x54;
    public const uint SlideVelX = 0x58;
    public const uint SlideVelZ = 0x5C;
    public const uint WallTri = 0x60;
    public const uint CeilingTri = 0x64;
    public const uint FloorTri = 0x68;
    public const uint CeilingY = 0x6C;
    public const uint FloorY = 0x70;
    public const uint FloorYaw = 0x74;
    public const uint WaterLevel = 0x76;
    public const uint InteractObject = 0x78;
    public const uint HeldObject = 0x7C;
    public const uint UsedObject = 0x80;
    public const uint RiddenObject = 0x84;
    public const uint MarioObject = 0x88;
    public const uint Health = 0xAE;
    public const uint SquishTimer = 0xB4;
    public const uint CapTimer = 0xB6;
    public const uint PeakY = 0xBC;
    public const uint QuicksandDepth = 0xC0;

    public const uint HolpType = 0x24A;
    public const uint TorsoPitch = 0x24C;
    public const uint TorsoYaw = 0x24E;
    public const uint TorsoRoll = 0x250;
    public const uint HolpX = 0x258;
    public const uint HolpY = 0x25C;
    public const uint HolpZ = 0x260;

    public const uint FirstPersonPitch2 = 0x13C6;
    public const uint FirstPersonYaw2 = 0x13C8;
    public const uint FirstPersonPitch = 0x1600;
    public const uint FirstPersonYaw = 0x1602;

    public const uint FlagNormalCap = 0x01, FlagVanishCap = 0x02, FlagMetalCap = 0x04, FlagWingCap = 0x08, FlagCapOnHead = 0x10, FlagCapInHand = 0x20;
}

public sealed record MarioState(
    uint Address,
    uint Flags,
    uint Action,
    uint PrevAction,
    ushort ActionState,
    ushort ActionTimer,
    uint ActionArg,
    float IntendedMag,
    ushort IntendedYaw,
    short Pitch,
    ushort FacingYaw,
    short Roll,
    ushort SlideYaw,
    ushort TwirlYaw,
    Vector3 Position,
    Vector3 Velocity,
    float HSpeed,
    float SlideVelX,
    float SlideVelZ,
    uint WallTri,
    uint CeilingTri,
    uint FloorTri,
    float CeilingY,
    float FloorY,
    short FloorYaw,
    short WaterLevel,
    uint HeldObject,
    uint RiddenObject,
    uint MarioObject,
    short Health,
    float PeakY)
{
    public const float WallHitboxRadius = 50f;
    public const float HitboxRadius = 37f;
    public const float HitboxHeight = 160f;

    public static MarioState Read(RamImage ram, uint b) => new(
        b,
        ram.U32(b + MarioOffsets.Flags),
        ram.U32(b + MarioOffsets.Action),
        ram.U32(b + MarioOffsets.PrevAction),
        ram.U16(b + MarioOffsets.ActionState),
        ram.U16(b + MarioOffsets.ActionTimer),
        ram.U32(b + MarioOffsets.ActionArg),
        ram.F32(b + MarioOffsets.IntendedMag),
        ram.U16(b + MarioOffsets.IntendedYaw),
        ram.S16(b + MarioOffsets.Pitch),
        ram.U16(b + MarioOffsets.FacingYaw),
        ram.S16(b + MarioOffsets.Roll),
        ram.U16(b + MarioOffsets.SlideYaw),
        ram.U16(b + MarioOffsets.TwirlYaw),
        new Vector3(ram.F32(b + MarioOffsets.X), ram.F32(b + MarioOffsets.Y), ram.F32(b + MarioOffsets.Z)),
        new Vector3(ram.F32(b + MarioOffsets.VelX), ram.F32(b + MarioOffsets.VelY), ram.F32(b + MarioOffsets.VelZ)),
        ram.F32(b + MarioOffsets.HSpeed),
        ram.F32(b + MarioOffsets.SlideVelX),
        ram.F32(b + MarioOffsets.SlideVelZ),
        ram.U32(b + MarioOffsets.WallTri),
        ram.U32(b + MarioOffsets.CeilingTri),
        ram.U32(b + MarioOffsets.FloorTri),
        ram.F32(b + MarioOffsets.CeilingY),
        ram.F32(b + MarioOffsets.FloorY),
        ram.S16(b + MarioOffsets.FloorYaw),
        ram.S16(b + MarioOffsets.WaterLevel),
        ram.U32(b + MarioOffsets.HeldObject),
        ram.U32(b + MarioOffsets.RiddenObject),
        ram.U32(b + MarioOffsets.MarioObject),
        ram.S16(b + MarioOffsets.Health),
        ram.F32(b + MarioOffsets.PeakY));

    public bool HasHatOnHead => (Flags & 0x10) != 0;
    public float SlidingSpeed => MathF.Sqrt(SlideVelX * SlideVelX + SlideVelZ * SlideVelZ);
    public ushort SlidingYaw => Angle.Towards(0, 0, SlideVelX, SlideVelZ);
    public float HeightAboveFloor => Position.Y - FloorY;
    public float FallHeight => PeakY - FloorY;
    public int HealthWedges => Health >> 8;
}
