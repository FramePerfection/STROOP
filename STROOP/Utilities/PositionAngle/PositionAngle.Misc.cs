using STROOP.Structs;
using STROOP.Structs.Configurations;

namespace STROOP.Utilities
{
    partial class PositionAngle
    {
        public class SnowPositionAngle : PositionAngle
        {
            readonly uint index;

            public SnowPositionAngle(uint index)
            {
                this.index = index;
            }

            bool GetAddress(out uint address)
            {
                address = 0;
                short numSnowParticles = Config.Stream.GetInt16(SnowConfig.CounterAddress);
                if (index < 0 || index >= numSnowParticles) return false;
                uint snowStart = Config.Stream.GetUInt32(SnowConfig.SnowArrayPointerAddress);
                uint structOffset = index * SnowConfig.ParticleStructSize;
                address = snowStart + structOffset;
                return true;
            }

            public override double X => GetAddress(out var addr) ? Config.Stream.GetInt32(addr + SnowConfig.XOffset) : double.NaN;
            public override double Y => GetAddress(out var addr) ? Config.Stream.GetInt32(addr + SnowConfig.YOffset) : double.NaN;
            public override double Z => GetAddress(out var addr) ? Config.Stream.GetInt32(addr + SnowConfig.ZOffset) : double.NaN;
            public override double Angle => double.NaN;
        }

        public class GoombaProjectionPositionAngle : PositionAngle
        {
            readonly uint address;

            public GoombaProjectionPositionAngle(uint address)
            {
                this.address = address;
            }

            private (double x, double z) GetGoombaProjection()
            {
                double startX = Config.Stream.GetSingle(address + ObjectConfig.XOffset);
                double startZ = Config.Stream.GetSingle(address + ObjectConfig.ZOffset);
                double hSpeed = Config.Stream.GetSingle(address + ObjectConfig.HSpeedOffset);
                int countdown = Config.Stream.GetInt32(address + ObjectConfig.GoombaCountdownOffset);
                ushort targetAngle = MoreMath.NormalizeAngleUshort(Config.Stream.GetInt32(address + ObjectConfig.GoombaTargetAngleOffset));
                return MoreMath.AddVectorToPoint(hSpeed * countdown, targetAngle, startX, startZ);
            }

            public override double X => GetGoombaProjection().x;
            public override double Y => Config.Stream.GetSingle(address + ObjectConfig.YOffset);
            public override double Z => GetGoombaProjection().z;
            public override double Angle => MoreMath.NormalizeAngleUshort(Config.Stream.GetInt32(address + ObjectConfig.GoombaTargetAngleOffset));
        }
    }
}
