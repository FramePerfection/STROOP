using StroopNext.Game;
using StroopNext.Memory;
using StroopNext.Session;
using O = StroopNext.Game.MarioOffsets;

namespace StroopNext.Variables;

public static class MarioVariables
{
    public const string Position = "Position";
    public const string Speed = "Speed";
    public const string Angles = "Angles";
    public const string Action = "Action";
    public const string Trajectory = "Trajectory";
    public const string Holp = "HOLP";
    public const string Objects = "Objects";
    public const string Collision = "Collision";
    public const string Status = "Status";
    public const string Advanced = "Advanced";

    public static readonly IReadOnlyList<string> Groups = [Position, Speed, Angles, Action, Trajectory, Holp, Objects, Collision, Status, Advanced];

    public static readonly IReadOnlySet<string> CollapsedByDefault = new HashSet<string> { Trajectory, Advanced };

    private static MemoryField<T> Field<T>(uint offset, Primitive<T> primitive) where T : struct => new(Bases.Mario, offset, primitive);
    private static MemoryField<T> Global<T>(Versioned<uint> address, Primitive<T> primitive, uint offset = 0) where T : struct => new(Bases.Versioned(address), offset, primitive);

    public static readonly MemoryField<float> X = Field(O.X, Primitives.F32);
    public static readonly MemoryField<float> Y = Field(O.Y, Primitives.F32);
    public static readonly MemoryField<float> Z = Field(O.Z, Primitives.F32);
    public static readonly MemoryField<float> VSpeed = Field(O.VelY, Primitives.F32);
    public static readonly MemoryField<float> HSpeed = Field(O.HSpeed, Primitives.F32);
    public static readonly MemoryField<int> FacingYaw = Field(O.FacingYaw, Primitives.U16);
    public static readonly MemoryField<uint> Flags = Field(O.Flags, Primitives.U32);
    public static readonly MemoryField<float> HolpX = Field(O.HolpX, Primitives.F32);
    public static readonly MemoryField<float> HolpY = Field(O.HolpY, Primitives.F32);
    public static readonly MemoryField<float> HolpZ = Field(O.HolpZ, Primitives.F32);

    private static readonly BaseResolver FloorTri = Bases.Pointer(Bases.Mario, O.FloorTri);

    public static float DeFactoMultiplier(GameFrame f)
        => f.Mario is { } m && m.Position.Y == m.FloorY && FloorTri(f) is uint tri ? f.Ram.F32(tri + 0x20) : 1f;

    public static float? DeFactoSpeed(GameFrame f) => f.Mario is { } m ? m.HSpeed * DeFactoMultiplier(f) : null;

    public static System.Numerics.Vector3? HolpPosition(GameFrame f)
        => HolpX.Read(f) is float x && HolpY.Read(f) is float y && HolpZ.Read(f) is float z ? new(x, y, z) : null;

    public static float? SpeedMultiplier(GameFrame f)
    {
        if (f.Mario is not { } m || FloorTri(f) is not uint tri)
            return null;
        double a = TriangleInfo.Default.FrictionMultiplier(f.Ram.S16(tri));
        int dyaw = m.IntendedYaw - m.SlideYaw;
        double cos = GameTrig.InGameCosine(dyaw);
        double k = cos < 0 && m.HSpeed >= 0 ? 0.5 + 0.5 * m.HSpeed / 100 : 1;
        return (float)(m.IntendedMag / 32 * cos * k * 0.02 + a);
    }

    public static int? InGameDeltaAngle(GameFrame f, int angle)
    {
        var (sin, cos) = Angle.Direction(angle);
        return GameTrig.InGameAngleTo(f, 0, 0, (float)sin, (float)cos) is ushort ig ? angle - ig : null;
    }

    public static (float X, float Y, float Z)? RotationDisplacement(GameFrame f)
    {
        if (f.Mario is not { } m || f.Layout?.StoodOnObjectPointer is not uint pointer)
            return null;
        uint obj = f.Ram.U32(pointer);
        if (!f.Ram.IsPointer(obj))
            return (0, 0, 0);
        var offset = (m.Position.X - f.Ram.F32(obj + 0xA0), m.Position.Y - f.Ram.F32(obj + 0xA4), m.Position.Z - f.Ram.F32(obj + 0xA8));
        var facing = (f.Ram.S16(obj + 0xD2), f.Ram.S16(obj + 0xD6), f.Ram.S16(obj + 0xDA));
        var angVel = ((short)f.Ram.S32(obj + 0x114), (short)f.Ram.S32(obj + 0x118), (short)f.Ram.S32(obj + 0x11C));
        return GameTrig.RotationDisplacement(offset, ((short)facing.Item1, (short)facing.Item2, (short)facing.Item3), angVel);
    }

    public static IReadOnlyList<Variable> All { get; } = Build();

    private static List<Variable> Build()
    {
        var actions = new ActionFormat(ActionTable.Default);
        return
        [
            X.ToVariable("mario.x", "X", Position, Formats.Float),
            Y.ToVariable("mario.y", "Y", Position, Formats.Float),
            Z.ToVariable("mario.z", "Z", Position, Formats.Float),
            Field(O.FloorY, Primitives.F32).ToVariable("mario.floorY", "Floor Y", Position, Formats.Float),
            new Variable<float>("mario.heightAboveFloor", "Height Above Floor", Position, Formats.Float,
                f => f.Mario?.HeightAboveFloor,
                (f, v, p) => Y.Write(f, f.Mario!.FloorY + v, p)),
            Field(O.CeilingY, Primitives.F32).ToVariable("mario.ceilingY", "Ceiling Y", Position, Formats.Float),
            Field(O.WaterLevel, Primitives.S16).ToVariable("mario.waterLevel", "Water Level", Position, Formats.Integer),
            Field(O.PeakY, Primitives.F32).ToVariable("mario.peakY", "Peak Y", Position, Formats.Float),
            new Variable<float>("mario.fallHeight", "Fall Height", Position, Formats.Float, f => f.Mario?.FallHeight,
                (f, v, p) => p.WriteF32(f.Mario!.Address + O.PeakY, f.Mario.FloorY + v),
                description: "Peak Y − floor Y: fall damage starts above 1150. Writing it moves the peak."),
            Field(O.QuicksandDepth, Primitives.F32).ToVariable("mario.quicksand", "Quicksand Depth", Position, Formats.Float),
            Global(Sm64Addresses.CurrentRoom, Primitives.S16).ToVariable("game.currentRoom", "Current Room", Position, Formats.Integer),

            HSpeed.ToVariable("mario.hspeed", "H Speed", Speed, Formats.Float),
            new Variable<float>("mario.defactoSpeed", "De Facto Speed", Speed, Formats.Float, DeFactoSpeed,
                (f, v, p) => HSpeed.Write(f, v / DeFactoMultiplier(f), p),
                description: "H speed × floor normal Y while on the ground: the distance actually covered per frame."),
            VSpeed.ToVariable("mario.vspeed", "Y Speed", Speed, Formats.Float),
            Field(O.VelX, Primitives.F32).ToVariable("mario.velX", "X Speed", Speed, Formats.Float),
            Field(O.VelZ, Primitives.F32).ToVariable("mario.velZ", "Z Speed", Speed, Formats.Float),
            new Variable<float>("mario.slidingSpeed", "Sliding Speed", Speed, Formats.Float, f => f.Mario?.SlidingSpeed,
                (f, v, p) =>
                {
                    var m = f.Mario!;
                    if (m.SlidingSpeed == 0)
                    {
                        p.WriteF32(m.Address + O.SlideVelX, v);
                        p.WriteF32(m.Address + O.SlideVelZ, 0);
                        return;
                    }
                    float scale = v / m.SlidingSpeed;
                    p.WriteF32(m.Address + O.SlideVelX, m.SlideVelX * scale);
                    p.WriteF32(m.Address + O.SlideVelZ, m.SlideVelZ * scale);
                }),
            Field(O.SlideVelX, Primitives.F32).ToVariable("mario.slideVelX", "Sliding X Speed", Speed, Formats.Float),
            Field(O.SlideVelZ, Primitives.F32).ToVariable("mario.slideVelZ", "Sliding Z Speed", Speed, Formats.Float),
            Field(O.IntendedMag, Primitives.F32).ToVariable("mario.intendedMag", "Intended Magnitude", Speed, Formats.Float),
            new Variable<float>("mario.flyingEnergy", "Flying Energy", Speed, Formats.Float,
                f => f.Mario is { } m ? (float)MarioMath.FlyingEnergy(m.HSpeed, m.Position.Y) : null,
                (f, v, p) =>
                {
                    var m = f.Mario!;
                    double h = Math.Sqrt(Math.Max(0, v - MarioMath.FlyingEnergy(0, m.Position.Y)));
                    HSpeed.Write(f, (float)(m.HSpeed < 0 ? -h : h), p);
                },
                description: "H speed² + (4/π)·Y: roughly conserved while flying. Writing it sets the H speed."),

            FacingYaw.ToVariable("mario.facingYaw", "Facing Yaw", Angles, Formats.Angle),
            Field(O.IntendedYaw, Primitives.U16).ToVariable("mario.intendedYaw", "Intended Yaw", Angles, Formats.Angle),
            new Variable<int>("mario.dyawIntendedFacing", "Δ Intended − Facing", Angles, Formats.SignedAngle,
                f => f.Mario is { } m ? Angle.Difference(m.IntendedYaw, m.FacingYaw) : null,
                (f, v, p) => FacingYaw.Write(f, (f.Mario!.IntendedYaw - v) & 0xFFFF, p),
                description: "Writing it turns Mario (the facing yaw), the intended yaw staying."),
            new Variable<int>("mario.dyawIntendedBack", "Δ Intended − Back", Angles, Formats.SignedAngle,
                f => f.Mario is { } m ? Angle.Difference(m.IntendedYaw, m.FacingYaw + 0x8000) : null,
                (f, v, p) => FacingYaw.Write(f, (f.Mario!.IntendedYaw - v - 0x8000) & 0xFFFF, p),
                description: "Δ between the intended yaw and Mario's back: what backwards movement compares. Writing it turns Mario."),
            Field(O.SlideYaw, Primitives.U16).ToVariable("mario.slideYaw", "Moving (Slide) Yaw", Angles, Formats.Angle),
            new Variable<int>("mario.slidingYaw", "Sliding Velocity Yaw", Angles, Formats.Angle, f => f.Mario?.SlidingYaw,
                (f, v, p) =>
                {
                    var m = f.Mario!;
                    var (dx, dz) = Angle.Direction(v);
                    p.WriteF32(m.Address + O.SlideVelX, (float)(dx * m.SlidingSpeed));
                    p.WriteF32(m.Address + O.SlideVelZ, (float)(dz * m.SlidingSpeed));
                },
                description: "Direction of the sliding velocity. Writing it turns the slide, keeping its speed."),
            Field(O.TwirlYaw, Primitives.U16).ToVariable("mario.twirlYaw", "Twirl Yaw", Angles, Formats.Angle),
            new Variable<int>("mario.twirlYawMod2048", "Twirl Yaw mod 2048", Angles, Formats.Integer,
                f => Field(O.TwirlYaw, Primitives.U16).Read(f) is int t ? t % 2048 : null,
                (f, v, p) =>
                {
                    var twirl = Field(O.TwirlYaw, Primitives.U16);
                    if (twirl.Read(f) is int t)
                        twirl.Write(f, (t - t % 2048 + ((v % 2048) + 2048) % 2048) & 0xFFFF, p);
                }),
            Field(O.FloorYaw, Primitives.U16).ToVariable("mario.floorYaw", "Floor Yaw", Angles, Formats.Angle),
            Field(O.Pitch, Primitives.S16).ToVariable("mario.pitch", "Pitch", Angles, Formats.SignedAngle),
            Field(O.Roll, Primitives.S16).ToVariable("mario.roll", "Roll", Angles, Formats.SignedAngle),
            Field(O.YawVelocity, Primitives.S16).ToVariable("mario.yawVelocity", "Yaw Velocity", Angles, Formats.SignedAngle),
            Field(O.PitchVelocity, Primitives.S16).ToVariable("mario.pitchVelocity", "Pitch Velocity", Angles, Formats.SignedAngle),
            Field(O.RollVelocity, Primitives.S16).ToVariable("mario.rollVelocity", "Roll Velocity", Angles, Formats.SignedAngle),

            Field(O.Action, Primitives.U32).ToVariable("mario.action", "Action", Action, actions),
            Field(O.PrevAction, Primitives.U32).ToVariable("mario.prevAction", "Previous Action", Action, actions),
            Field(O.ActionState, Primitives.U16).ToVariable("mario.actionState", "Action State", Action, Formats.Integer),
            Field(O.ActionTimer, Primitives.U16).ToVariable("mario.actionTimer", "Action Timer", Action, Formats.Integer,
                description: "Also the water shell timer while riding a shell."),
            Field(O.ActionArg, Primitives.U32).ToVariable("mario.actionArg", "Action Arg", Action, Formats.Hex),

            new Variable<float>("mario.trajRemainingHeight", "Remaining Height", Trajectory, Formats.Float,
                f => f.Mario is { } m ? (float)MarioMath.HeightFromVSpeed(m.Velocity.Y) : null,
                (f, v, p) => VSpeed.Write(f, (float)MarioMath.VSpeedForHeight(v), p),
                description: "Height still gained with the current Y speed (gravity 4/frame). Writing it sets the Y speed."),
            new Variable<float>("mario.trajPeakHeight", "Peak Height", Trajectory, Formats.Float,
                f => f.Mario is { } m ? (float)(m.Position.Y + MarioMath.HeightFromVSpeed(m.Velocity.Y)) : null,
                (f, v, p) => VSpeed.Write(f, (float)MarioMath.VSpeedForHeight(v - f.Mario!.Position.Y), p),
                description: "Y at the top of the current jump. Writing it sets the Y speed."),
            new Variable<float>("mario.djVSpeed", "Double Jump Y Speed", Trajectory, Formats.Float,
                f => f.Mario is { } m ? (float)MarioMath.DoubleJumpVSpeed(m.HSpeed) : null,
                (f, v, p) => HSpeed.Write(f, (float)MarioMath.HSpeedForDoubleJumpVSpeed(v), p),
                description: "52 + H speed / 4. Writing it sets the H speed."),
            new Variable<float>("mario.djHeight", "Double Jump Height", Trajectory, Formats.Float,
                f => f.Mario is { } m ? (float)MarioMath.HeightFromVSpeed(MarioMath.DoubleJumpVSpeed(m.HSpeed)) : null,
                (f, v, p) => HSpeed.Write(f, (float)MarioMath.HSpeedForDoubleJumpVSpeed(MarioMath.VSpeedForHeight(v)), p)),
            new Variable<float>("mario.djPeakHeight", "Double Jump Peak", Trajectory, Formats.Float,
                f => f.Mario is { } m ? (float)(m.Position.Y + MarioMath.HeightFromVSpeed(MarioMath.DoubleJumpVSpeed(m.HSpeed))) : null,
                (f, v, p) => HSpeed.Write(f, (float)MarioMath.HSpeedForDoubleJumpVSpeed(MarioMath.VSpeedForHeight(v - f.Mario!.Position.Y)), p)),

            HolpX.ToVariable("mario.holpX", "HOLP X", Holp, Formats.Float),
            HolpY.ToVariable("mario.holpY", "HOLP Y", Holp, Formats.Float),
            HolpZ.ToVariable("mario.holpZ", "HOLP Z", Holp, Formats.Float),
            Field(O.HolpType, Primitives.U8).ToVariable("mario.holpType", "HOLP Type", Holp, Formats.Integer),
            new Variable<float>("mario.holpDistance", "Distance to HOLP", Holp, Formats.Float,
                f => f.Mario is { } m && HolpPosition(f) is { } h ? System.Numerics.Vector3.Distance(m.Position, h) : null,
                (f, v, p) =>
                {
                    if (f.Mario is not { } m || HolpPosition(f) is not { } h)
                        return;
                    var d = h - m.Position;
                    var to = d.Length() > 0 ? m.Position + d * (v / d.Length()) : m.Position + new System.Numerics.Vector3(0, v, 0);
                    HolpX.Write(f, to.X, p);
                    HolpY.Write(f, to.Y, p);
                    HolpZ.Write(f, to.Z, p);
                },
                description: "Writing it moves the HOLP along the line from Mario."),
            new Variable<float>("mario.holpHDistance", "H Distance to HOLP", Holp, Formats.Float,
                f => f.Mario is { } m && HolpPosition(f) is { } h ? MathF.Sqrt((h.X - m.Position.X) * (h.X - m.Position.X) + (h.Z - m.Position.Z) * (h.Z - m.Position.Z)) : null,
                (f, v, p) => PlaceMarioAroundHolp(f, v, null, p),
                description: "Writing it moves Mario along the line to the HOLP."),
            new Variable<float>("mario.holpDY", "Y Distance to HOLP", Holp, Formats.Float,
                f => f.Mario is { } m && HolpPosition(f) is { } h ? h.Y - m.Position.Y : null,
                (f, v, p) => HolpY.Write(f, f.Mario!.Position.Y + v, p),
                description: "Writing it moves the HOLP vertically."),
            new Variable<int>("mario.holpAngle", "Angle Mario → HOLP", Holp, Formats.Angle,
                f => f.Mario is { } m && HolpPosition(f) is { } h ? Angle.Towards(m.Position.X, m.Position.Z, h.X, h.Z) : null,
                (f, v, p) => PlaceMarioAroundHolp(f, null, v, p),
                description: "Writing it moves Mario around the HOLP, keeping the distance."),
            new Variable<int>("mario.holpDAngle", "Δ Angle to HOLP − Facing", Holp, Formats.SignedAngle,
                f => f.Mario is { } m && HolpPosition(f) is { } h ? Angle.Difference(Angle.Towards(m.Position.X, m.Position.Z, h.X, h.Z), m.FacingYaw) : null,
                (f, v, p) => PlaceMarioAroundHolp(f, null, (f.Mario!.FacingYaw + v) & 0xFFFF, p),
                description: "Writing it moves Mario around the HOLP, keeping the distance and his facing."),

            ObjectReference.Variable("mario.stoodOnObject", "Stood-On Object", Objects, Bases.Global(l => l.StoodOnObjectPointer), 0),
            ObjectReference.Variable("mario.heldObject", "Held Object", Objects, Bases.Mario, O.HeldObject),
            ObjectReference.Variable("mario.riddenObject", "Ridden Object", Objects, Bases.Mario, O.RiddenObject),
            ObjectReference.Variable("mario.interactObject", "Interaction Object", Objects, Bases.Mario, O.InteractObject),
            ObjectReference.Variable("mario.usedObject", "Used Object", Objects, Bases.Mario, O.UsedObject),
            ObjectReference.Variable("mario.visualObject", "Visual Mario Object", Objects, Bases.Mario, O.MarioObject),
            ObjectReference.Variable("mario.effectiveObject", "Effective Mario Object", Objects, Bases.Global(l => l.MarioObjectPointer), 0),

            Field(O.FloorTri, Primitives.U32).ToVariable("mario.floorTri", "Floor Triangle", Collision, Formats.Pointer),
            new Variable<int>("mario.floorType", "Floor Type", Collision, Formats.Integer, f => FloorTri(f) is uint t ? f.Ram.S16(t) : null,
                (f, v, p) => { if (FloorTri(f) is uint t) p.WriteS16(t, (short)v); },
                description: "Writing it changes the floor triangle itself (for the whole level until it reloads)."),
            new Variable<float>("mario.floorNormalY", "Floor Normal Y", Collision, Formats.Float, f => FloorTri(f) is uint t ? f.Ram.F32(t + 0x20) : null,
                (f, v, p) => { if (FloorTri(f) is uint t) p.WriteF32(t + 0x20, v); },
                description: "Writing it changes the floor triangle itself (for the whole level until it reloads)."),
            Field(O.WallTri, Primitives.U32).ToVariable("mario.wallTri", "Wall Triangle", Collision, Formats.Pointer),
            Field(O.CeilingTri, Primitives.U32).ToVariable("mario.ceilingTri", "Ceiling Triangle", Collision, Formats.Pointer),
            Field(O.FloorFootstep, Primitives.S16).ToVariable("mario.floorFootstep", "Floor Footstep Type", Collision, Formats.Integer),

            Field(O.Health, Primitives.S16).ToVariable("mario.health", "Health (raw)", Status, Formats.Integer),
            new Variable<int>("mario.healthWedges", "Health Wedges", Status, Formats.Integer, f => f.Mario?.HealthWedges,
                (f, v, p) => p.WriteS16(f.Mario!.Address + O.Health, (short)((v << 8) | 0x80)),
                description: "Full health is 8 wedges (raw 0x880)."),
            Flags.ToVariable("mario.flags", "Flags", Status, Formats.Hex),
            FlagBit("mario.hatOnHead", "Cap On Head", O.FlagCapOnHead),
            FlagBit("mario.hatInHand", "Cap In Hand", O.FlagCapInHand),
            FlagBit("mario.wingCap", "Wing Cap", O.FlagWingCap),
            FlagBit("mario.metalCap", "Metal Cap", O.FlagMetalCap),
            FlagBit("mario.vanishCap", "Vanish Cap", O.FlagVanishCap),
            FlagBit("mario.normalCap", "Should Have Cap", O.FlagNormalCap),
            Field(O.CapTimer, Primitives.U16).ToVariable("mario.capTimer", "Cap Timer", Status, Formats.Integer),
            Field(O.InvincTimer, Primitives.S16).ToVariable("mario.hitstunTimer", "Hitstun Timer", Status, Formats.Integer,
                description: "invincTimer: frames of invincibility after a hit."),
            Field(O.SquishTimer, Primitives.S8).ToVariable("mario.unsquishTimer", "Unsquish Timer", Status, Formats.Integer),
            Field(O.ActionArgLow, Primitives.U16).ToVariable("mario.squishedTimer", "Squished Timer", Status, Formats.Integer,
                description: "Low half of the action argument while squished."),
            Global(Sm64Addresses.SwimStrength, Primitives.S16).ToVariable("game.swimStrength", "Swim Strength", Status, Formats.Integer),
            new Variable<bool>("game.justTeleported", "Just Teleported", Status, Formats.Boolean,
                f => Global(Sm64Addresses.JustTeleported, Primitives.U8).Read(f) is int b ? (b & 1) != 0 : null,
                (f, v, p) =>
                {
                    var field = Global(Sm64Addresses.JustTeleported, Primitives.U8);
                    if (field.Read(f) is int b)
                        field.Write(f, v ? b | 1 : b & ~1, p);
                }),
            Global(Sm64Addresses.BlinkState, Primitives.S32).ToVariable("game.blinkState", "Blinking State", Status, Formats.Integer),
            Global(Sm64Addresses.BlinkTimer, Primitives.S16).ToVariable("game.blinkTimer", "Blinking Timer", Status, Formats.Integer),
            new MemoryField<uint>(Bases.Global(l => l.GlobalTimer), 0, Primitives.U32)
                .ToVariable("game.globalTimer", "Global Timer", Status, new GlobalTimerFormat()),

            new Variable<float>("mario.speedMultiplier", "Speed Multiplier", Advanced, Formats.Float, SpeedMultiplier,
                (f, v, p) =>
                {
                    if (f.Mario is not { } m || FloorTri(f) is not uint tri)
                        return;
                    double a = TriangleInfo.Default.FrictionMultiplier(f.Ram.S16(tri));
                    double cos = GameTrig.InGameCosine(m.IntendedYaw - m.SlideYaw);
                    double k = cos < 0 && m.HSpeed >= 0 ? 0.5 + 0.5 * m.HSpeed / 100 : 1;
                    if (Math.Abs(cos * k) > 1e-9)
                        p.WriteF32(m.Address + O.IntendedMag, (float)Math.Clamp((v - a) / (cos * k * 0.02) * 32, 0, 32));
                },
                description: "The factor Mario's ground speed is multiplied by next frame (friction + stick input). NaN off known floors. Writing it sets the intended magnitude (0-32)."),
            new Variable<int>("mario.igDYaw", "IG DYaw", Advanced, Formats.Integer,
                f => f.Mario is { } m && InGameDeltaAngle(f, m.FacingYaw) is int d ? d : null,
                (f, v, p) =>
                {
                    if (f.Mario is { } m && InGameDeltaAngle(f, m.FacingYaw) is int d)
                        FacingYaw.Write(f, (m.FacingYaw - d + v) & 0xFFFF, p);
                },
                description: "Facing yaw minus the yaw the game's atan2 table rounds it to: the error from in-game angles. Writing it moves the facing yaw off that rounded yaw."),
            new Variable<float>("mario.rotDispX", "Rotation Disp. X", Advanced, Formats.Float, f => RotationDisplacement(f)?.X,
                description: "How far the platform Mario stands on carries him next frame, by its spin."),
            new Variable<float>("mario.rotDispY", "Rotation Disp. Y", Advanced, Formats.Float, f => RotationDisplacement(f)?.Y),
            new Variable<float>("mario.rotDispZ", "Rotation Disp. Z", Advanced, Formats.Float, f => RotationDisplacement(f)?.Z),
            Field(O.TorsoYaw, Primitives.S16).ToVariable("mario.torsoYaw", "Torso Yaw", Advanced, Formats.SignedAngle),
            Field(O.TorsoPitch, Primitives.S16).ToVariable("mario.torsoPitch", "Torso Pitch", Advanced, Formats.SignedAngle),
            Field(O.TorsoRoll, Primitives.S16).ToVariable("mario.torsoRoll", "Torso Roll", Advanced, Formats.SignedAngle),
            Field(O.FirstPersonYaw, Primitives.S16).ToVariable("mario.firstPersonYaw", "First Person Yaw", Advanced, Formats.SignedAngle),
            Field(O.FirstPersonPitch, Primitives.S16).ToVariable("mario.firstPersonPitch", "First Person Pitch", Advanced, Formats.SignedAngle),
            Field(O.FirstPersonYaw2, Primitives.S16).ToVariable("mario.firstPersonYaw2", "First Person Yaw 2", Advanced, Formats.SignedAngle),
            Field(O.FirstPersonPitch2, Primitives.S16).ToVariable("mario.firstPersonPitch2", "First Person Pitch 2", Advanced, Formats.SignedAngle),
            Global(Sm64Addresses.Spawn, Primitives.S16, 0).ToVariable("game.spawnX", "Spawn X", Advanced, Formats.Integer),
            Global(Sm64Addresses.Spawn, Primitives.S16, 2).ToVariable("game.spawnY", "Spawn Y", Advanced, Formats.Integer),
            Global(Sm64Addresses.Spawn, Primitives.S16, 4).ToVariable("game.spawnZ", "Spawn Z", Advanced, Formats.Integer),
        ];
    }

    private static Variable<bool> FlagBit(string id, string label, uint mask) =>
        new(id, label, Status, Formats.Boolean, f => f.Mario is { } m ? (m.Flags & mask) != 0 : null,
            (f, v, p) => Flags.Write(f, v ? f.Mario!.Flags | mask : f.Mario!.Flags & ~mask, p));

    private static void PlaceMarioAroundHolp(GameFrame f, float? distance, int? angle, RamPatch p)
    {
        if (f.Mario is not { } m || HolpPosition(f) is not { } h)
            return;
        float dx = h.X - m.Position.X, dz = h.Z - m.Position.Z;
        double d = distance ?? Math.Sqrt(dx * dx + dz * dz);
        double a = angle ?? Angle.Towards(m.Position.X, m.Position.Z, h.X, h.Z);
        var (sx, sz) = Angle.Direction(a);
        p.WriteF32(m.Address + O.X, (float)(h.X - sx * d));
        p.WriteF32(m.Address + O.Z, (float)(h.Z - sz * d));
    }

    private sealed class GlobalTimerFormat : ValueFormat<uint>
    {
        public override string Format(uint value, DisplayOptions options) => value.ToString(System.Globalization.CultureInfo.InvariantCulture);
        public override bool TryParse(string text, DisplayOptions options, out uint value) => uint.TryParse(text, System.Globalization.CultureInfo.InvariantCulture, out value);
        public override double? ToNumber(uint value) => value;
    }
}

[Flags]
public enum Axes { X = 1, Y = 2, Z = 4, Lateral = X | Z, All = X | Y | Z }

public static class MarioCommands
{
    public static RamPatch Translate(GameFrame frame, float dx, float dy, float dz)
    {
        var patch = new RamPatch();
        if (frame.Mario is not { } m)
            return patch;
        patch.WriteF32(m.Address + O.X, m.Position.X + dx);
        patch.WriteF32(m.Address + O.Y, m.Position.Y + dy);
        patch.WriteF32(m.Address + O.Z, m.Position.Z + dz);
        return patch;
    }

    public static (float X, float Z) RelativeToWorld(int facingYaw, float forward, float right)
    {
        var (fx, fz) = Angle.Direction(facingYaw);
        var (rx, rz) = Angle.Direction(facingYaw - 0x4000);
        return ((float)(fx * forward + rx * right), (float)(fz * forward + rz * right));
    }

    public static RamPatch MoveRelative(GameFrame frame, float forward, float right, float up)
    {
        if (frame.Mario is not { } m)
            return new RamPatch();
        var (dx, dz) = RelativeToWorld(m.FacingYaw, forward, right);
        return Translate(frame, dx, up, dz);
    }

    public static RamPatch Teleport(GameFrame frame, float x, float? y, float z)
    {
        var patch = new RamPatch();
        if (frame.Mario is not { } m)
            return patch;
        patch.WriteF32(m.Address + O.X, x);
        patch.WriteF32(m.Address + O.Y, y ?? m.Position.Y);
        patch.WriteF32(m.Address + O.Z, z);
        return patch;
    }

    public static RamPatch AddHSpeed(GameFrame frame, float delta)
    {
        var patch = new RamPatch();
        if (frame.Mario is { } m)
            patch.WriteF32(m.Address + O.HSpeed, m.HSpeed + delta);
        return patch;
    }

    public static RamPatch AddVSpeed(GameFrame frame, float delta)
    {
        var patch = new RamPatch();
        if (frame.Mario is { } m)
            patch.WriteF32(m.Address + O.VelY, m.Velocity.Y + delta);
        return patch;
    }

    public static RamPatch AddYaw(GameFrame frame, int delta)
    {
        var patch = new RamPatch();
        if (frame.Mario is { } m)
            patch.WriteU16(m.Address + O.FacingYaw, (ushort)(m.FacingYaw + delta));
        return patch;
    }

    public static RamPatch AddSlideVelocity(GameFrame frame, float dx, float dz)
    {
        var patch = new RamPatch();
        if (frame.Mario is not { } m)
            return patch;
        float x = m.SlideVelX + dx, z = m.SlideVelZ + dz;
        patch.WriteF32(m.Address + O.SlideVelX, x);
        patch.WriteF32(m.Address + O.SlideVelZ, z);
        if (x != 0 || z != 0)
            patch.WriteU16(m.Address + O.SlideYaw, Angle.Towards(0, 0, x, z));
        return patch;
    }

    public static RamPatch AdjustSlide(GameFrame frame, float speedDelta, int yawDelta)
    {
        var patch = new RamPatch();
        if (frame.Mario is not { } m)
            return patch;
        float speed = m.SlidingSpeed;
        int yaw = speed == 0 ? m.SlideYaw : m.SlidingYaw;
        ushort newYaw = (ushort)(yaw + yawDelta);
        var (sx, sz) = Angle.Direction(newYaw);
        patch.WriteF32(m.Address + O.SlideVelX, (float)(sx * (speed + speedDelta)));
        patch.WriteF32(m.Address + O.SlideVelZ, (float)(sz * (speed + speedDelta)));
        patch.WriteU16(m.Address + O.SlideYaw, newYaw);
        return patch;
    }

    public static RamPatch GoToHolp(GameFrame frame, Axes axes)
    {
        var patch = new RamPatch();
        if (frame.Mario is not { } m || MarioVariables.HolpPosition(frame) is not { } h)
            return patch;
        if (axes.HasFlag(Axes.X))
            patch.WriteF32(m.Address + O.X, h.X);
        if (axes.HasFlag(Axes.Y))
            patch.WriteF32(m.Address + O.Y, h.Y);
        if (axes.HasFlag(Axes.Z))
            patch.WriteF32(m.Address + O.Z, h.Z);
        return patch;
    }

    public static RamPatch RetrieveHolp(GameFrame frame, Axes axes)
    {
        var patch = new RamPatch();
        if (frame.Mario is not { } m)
            return patch;
        if (axes.HasFlag(Axes.X))
            patch.WriteF32(m.Address + O.HolpX, m.Position.X);
        if (axes.HasFlag(Axes.Y))
            patch.WriteF32(m.Address + O.HolpY, m.Position.Y);
        if (axes.HasFlag(Axes.Z))
            patch.WriteF32(m.Address + O.HolpZ, m.Position.Z);
        return patch;
    }

    public static RamPatch MoveHolp(GameFrame frame, float forwardOrMinusZ, float rightOrX, float up, bool relative)
    {
        var patch = new RamPatch();
        if (frame.Mario is not { } m || MarioVariables.HolpPosition(frame) is not { } h)
            return patch;
        var (dx, dz) = relative ? RelativeToWorld(m.FacingYaw, forwardOrMinusZ, rightOrX) : (rightOrX, -forwardOrMinusZ);
        patch.WriteF32(m.Address + O.HolpX, h.X + dx);
        patch.WriteF32(m.Address + O.HolpY, h.Y + up);
        patch.WriteF32(m.Address + O.HolpZ, h.Z + dz);
        return patch;
    }

    public static RamPatch HandsFree(GameFrame frame)
    {
        var patch = new RamPatch();
        if (frame.Mario is not { } m || frame.Ram.U32(m.Address + O.HeldObject) == 0)
            return patch;
        patch.WriteU32(m.Address + O.Action, frame.Services.Actions.HandsFreeFor(m.Action));
        return patch;
    }

    private const uint UsMarioGraphNode = 0x800F0860;
    private const uint SharedChildOffset = 0x14;
    private static uint _hiddenMarioGraph;

    public static RamPatch ToggleVisibility(GameFrame frame)
    {
        var patch = new RamPatch();
        if (frame.Layout?.MarioObjectPointer is not uint pointer)
            return patch;
        uint marioObject = frame.Ram.U32(pointer);
        if (!frame.Ram.IsPointer(marioObject))
            return patch;
        uint graph = frame.Ram.U32(marioObject + SharedChildOffset);
        if (graph != 0)
        {
            _hiddenMarioGraph = graph;
            patch.WriteU32(marioObject + SharedChildOffset, 0);
        }
        else if (_hiddenMarioGraph != 0 || frame.Layout.Version == GameVersion.US)
            patch.WriteU32(marioObject + SharedChildOffset, _hiddenMarioGraph != 0 ? _hiddenMarioGraph : UsMarioGraphNode);
        return patch;
    }
}
