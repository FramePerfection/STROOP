using STROOP.Structs;
using STROOP.Structs.Configurations;
using System;
using OpenTK.Mathematics;
using STROOP.Variables.SM64MemoryLayout;

namespace STROOP.Utilities
{
    partial class PositionAngle
    {
        public class CustomPositionAngle : PositionAngle
        {
            double customX, customY, customZ, customAngle;

            public CustomPositionAngle(Vector3 pos, ushort angle = 0)
            {
                customX = pos.X;
                customY = pos.Y;
                customZ = pos.Z;
                customAngle = angle;
            }

            public CustomPositionAngle(double x, double y, double z, double ang)
            {
                customX = x;
                customY = y;
                customZ = z;
                customAngle = ang;
            }

            public override double X => customX;
            public override double Y => customY;
            public override double Z => customZ;
            public override double Angle => customAngle;

            public override bool SetX(double value)
            {
                customX = value;
                return true;
            }

            public override bool SetY(double value)
            {
                customY = value;
                return true;
            }

            public override bool SetZ(double value)
            {
                customZ = value;
                return true;
            }

            public override bool SetAngle(double value)
            {
                customAngle = value;
                return true;
            }

            public override string ToString() => "Custom";
        }

        public class MarioPositionAngle : MemoryPositionAngle, IHoldsObjectAddress
        {
            public MarioPositionAngle()
                : base(() => MarioConfig.StructAddress, MarioConfig.XOffset, MarioConfig.YOffset, MarioConfig.ZOffset, MarioConfig.FacingYawOffset, "Mario")
            {
            }

            bool SetCoordinateComponent(double value, uint structOffset, uint objOffset)
            {
                using (Config.Stream.Suspend())
                {
                    bool success = Config.Stream.SetValue((float)value, MarioConfig.StructAddress + structOffset);
                    if (GlobalKeyboard.IsAltDown())
                        success &= Config.Stream.SetValue((float)value, MarioConfig.StructAddress + objOffset);
                    return success;
                }
            }

            public override bool SetX(double value) => SetCoordinateComponent(value, MarioConfig.XOffset, ObjectConfig.XOffset);
            public override bool SetY(double value) => SetCoordinateComponent(value, MarioConfig.YOffset, ObjectConfig.YOffset);
            public override bool SetZ(double value) => SetCoordinateComponent(value, MarioConfig.ZOffset, ObjectConfig.ZOffset);

            uint IHoldsObjectAddress.GetAddress() => Config.Stream.GetUInt32(MarioObjectConfig.PointerAddress);
        }

        public class ObjectPositionAngle : MemoryPositionAngle, IHoldsObjectAddress
        {
            public uint? objectAddress => baseGetter();

            public ObjectPositionAngle(Func<uint?> baseGetter)
                : base(baseGetter, ObjectConfig.XOffset, ObjectConfig.YOffset, ObjectConfig.ZOffset, ObjectConfig.YawFacingOffset)
            {
            }

            public override bool SetAngle(double value)
            {
                uint? objAddress = baseGetter();
                if (!objAddress.HasValue) return false;
                bool success = true;
                success &= Config.Stream.SetValue(MoreMath.NormalizeAngleUshort(value), objAddress.Value + ObjectConfig.YawFacingOffset);
                success &= Config.Stream.SetValue(MoreMath.NormalizeAngleUshort(value), objAddress.Value + ObjectConfig.YawMovingOffset);
                return success;
            }

            uint IHoldsObjectAddress.GetAddress() => baseGetter().Value;

            public override string GetMapName()
            {
                var addr = baseGetter();
                return addr.HasValue ? GetMapNameForObject(addr.Value) : "(None)";
            }

            public override string ToString() => GetMapName();
        }

        public class ObjectHomePositionAngle : MemoryPositionAngle
        {
            public ObjectHomePositionAngle(Func<uint?> baseGetter)
                : base(baseGetter, ObjectConfig.HomeXOffset, ObjectConfig.HomeYOffset, ObjectConfig.HomeZOffset)
            {
            }

            public override string GetMapName()
            {
                var addr = baseGetter();
                return "Home for " + (addr.HasValue ? GetMapNameForObject(addr.Value) : "(None)");
            }
        }

        public class MemoryPositionAngle : PositionAngle
        {
            uint? xOffset, yOffset, zOffset, angleOffset;
            protected readonly Func<uint?> baseGetter;
            Func<double> angleGetter;
            readonly string name;

            public MemoryPositionAngle(Func<uint?> baseGetter, uint? xOffset, uint? yOffset, uint? zOffset, uint? angleOffset = null, string name = null)
            {
                this.baseGetter = baseGetter;
                this.xOffset = xOffset;
                this.yOffset = yOffset;
                this.zOffset = zOffset;
                this.angleOffset = angleOffset;
                this.name = name;
            }

            public MemoryPositionAngle(Func<uint?> baseGetter, uint? xOffset, uint? yOffset, uint? zOffset, Func<double> angleGetter, string name = null)
                : this(baseGetter, xOffset, yOffset, zOffset, (uint?)null, name)
            {
                this.angleGetter = angleGetter;
            }

            double Get(uint? offset)
            {
                if (offset == null) return double.NaN;
                var b = baseGetter();
                return b.HasValue ? Config.Stream.GetSingle(b.Value + offset.Value) : double.NaN;
            }

            public override double X => Get(xOffset);
            public override double Y => Get(yOffset);
            public override double Z => Get(zOffset);

            public override double Angle
            {
                get
                {
                    if (angleGetter != null)
                        return angleGetter();
                    var b = baseGetter();
                    return b.HasValue && angleOffset.HasValue ? Config.Stream.GetUInt16(b.Value + angleOffset.Value) : double.NaN;
                }
            }

            bool Set(Type type, object value, uint? offset)
            {
                if (offset == null) return false;
                var b = baseGetter();
                return b.HasValue ? Config.Stream.SetValue(type, value, b.Value + offset.Value) : false;
            }

            public override bool SetX(double value) => Set(typeof(float), (float)value, xOffset);
            public override bool SetY(double value) => Set(typeof(float), (float)value, yOffset);
            public override bool SetZ(double value) => Set(typeof(float), (float)value, zOffset);
            public override bool SetAngle(double value) => Set(typeof(ushort), (ushort)value, angleOffset);

            public override string ToString() => name ?? GetType().Name;
        }

        public class TrianglePositionAngle : PositionAngle
        {
            readonly Func<uint?> addressGetter;
            readonly uint index;

            public TrianglePositionAngle(Func<uint?> addressGetter, uint index)
            {
                this.addressGetter = addressGetter;
                this.index = index;
            }

            public override double X
            {
                get
                {
                    uint? address = addressGetter();
                    if (address == null) return double.NaN;
                    return TriangleOffsetsConfig.GetXIndex(address.Value, index - 1);
                }
            }

            public override double Y
            {
                get
                {
                    uint? address = addressGetter();
                    if (address == null) return double.NaN;
                    return TriangleOffsetsConfig.GetYIndex(address.Value, index - 1);
                }
            }

            public override double Z
            {
                get
                {
                    uint? address = addressGetter();
                    if (address == null) return double.NaN;
                    return TriangleOffsetsConfig.GetZIndex(address.Value, index - 1);
                }
            }

            public override double Angle => Double.NaN;

            public override bool SetX(double value)
            {
                uint? address = addressGetter();
                if (address == null) return false;
                if (index <= 3)
                    return TriangleOffsetsConfig.SetXIndex((short)value, address.Value, index - 1);
                return false;
            }

            public override bool SetY(double value)
            {
                uint? address = addressGetter();
                if (address == null) return false;
                if (index <= 3)
                    return TriangleOffsetsConfig.SetYIndex((short)value, address.Value, index - 1);
                return false;
            }

            public override bool SetZ(double value)
            {
                uint? address = addressGetter();
                if (address == null) return false;
                if (index <= 3)
                    return TriangleOffsetsConfig.SetZIndex((short)value, address.Value, index - 1);
                return false;
            }
        }
    }
}
