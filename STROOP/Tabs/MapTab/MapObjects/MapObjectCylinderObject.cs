using System;
using System.Collections.Generic;
using System.Drawing;
using STROOP.Utilities;
using STROOP.Structs.Configurations;
using STROOP.Structs;
using STROOP.Variables.SM64MemoryLayout;

namespace STROOP.Tabs.MapTab.MapObjects
{
    public class MapObjectCylinderObject : MapCylinderObject
    {
        public delegate (float centerX, float centerZ, float radius, float minY, float maxY) GetDimensions(PositionAngle positionAngle);

        public GetDimensions getDimensions;
        public string name;

        public MapObjectCylinderObject(PositionAngleProvider positionAngleProvider, GetDimensions getDimensions, string name)
            : base(null)
        {
            this.positionAngleProvider = positionAngleProvider;
            this.getDimensions = getDimensions;
            this.name = name;
        }

        protected override List<(float centerX, float centerZ, float radius, float minY, float maxY)> Get3DDimensions()
        {
            var lst = new List<(float centerX, float centerZ, float radius, float minY, float maxY)>();
            foreach (var obj in positionAngleProvider())
                lst.Add(getDimensions(obj));
            return lst;
        }

        public override Lazy<Image> GetInternalImage() => Config.ObjectAssociations.CylinderImage;

        public override string GetName() => $"{name} for {PositionAngle.NameOfMultiple(positionAngleProvider())}";

        public static class Dimensions
        {
            public static GetDimensions HitBox = posAngle =>
            {
                uint objAddress = PositionAngle.GetObjectAddress(posAngle);
                float objY = Config.Stream.GetSingle(objAddress + ObjectConfig.YOffset);
                float hitboxRadius = Config.Stream.GetSingle(objAddress + ObjectConfig.HitboxRadiusOffset);
                float hitboxHeight = Config.Stream.GetSingle(objAddress + ObjectConfig.HitboxHeightOffset);
                float hitboxDownOffset = Config.Stream.GetSingle(objAddress + ObjectConfig.HitboxDownOffsetOffset);
                float hitboxMinY = objY - hitboxDownOffset;
                float hitboxMaxY = hitboxMinY + hitboxHeight;
                return ((float)posAngle.X, (float)posAngle.Z, hitboxRadius, hitboxMinY, hitboxMaxY);
            };

            public static GetDimensions EffectiveHitBox = posAngle =>
            {
                uint objAddress = PositionAngle.GetObjectAddress(posAngle);
                float objY = Config.Stream.GetSingle(objAddress + ObjectConfig.YOffset);
                float hitboxRadius = Config.Stream.GetSingle(objAddress + ObjectConfig.HitboxRadiusOffset);
                float hitboxHeight = Config.Stream.GetSingle(objAddress + ObjectConfig.HitboxHeightOffset);
                float hitboxDownOffset = Config.Stream.GetSingle(objAddress + ObjectConfig.HitboxDownOffsetOffset);
                float hitboxMinY = objY - hitboxDownOffset;
                float hitboxMaxY = hitboxMinY + hitboxHeight;

                uint marioObjRef = Config.Stream.GetUInt32(MarioObjectConfig.PointerAddress);
                float marioHitboxRadius = Config.Stream.GetSingle(marioObjRef + ObjectConfig.HitboxRadiusOffset);
                float effectiveRadius = hitboxRadius + marioHitboxRadius;

                float marioHitboxHeight = Config.Stream.GetSingle(marioObjRef + ObjectConfig.HitboxHeightOffset);

                return ((float)posAngle.X, (float)posAngle.Z, effectiveRadius, hitboxMinY - marioHitboxHeight, hitboxMaxY);
            };

            public static GetDimensions HurtBox = posAngle =>
            {
                uint objAddress = PositionAngle.GetObjectAddress(posAngle);
                float objY = Config.Stream.GetSingle(objAddress + ObjectConfig.YOffset);
                float hurtboxRadius = Config.Stream.GetSingle(objAddress + ObjectConfig.HurtboxRadiusOffset);
                float hurtboxHeight = Config.Stream.GetSingle(objAddress + ObjectConfig.HurtboxHeightOffset);
                float hitboxDownOffset = Config.Stream.GetSingle(objAddress + ObjectConfig.HitboxDownOffsetOffset);
                float hurtboxMinY = objY - hitboxDownOffset;
                float hurtboxMaxY = hurtboxMinY + hurtboxHeight;
                return ((float)posAngle.X, (float)posAngle.Z, hurtboxRadius, hurtboxMinY, hurtboxMaxY);
            };

            public static GetDimensions EffectiveHurtBox = posAngle =>
            {
                uint objAddress = PositionAngle.GetObjectAddress(posAngle);
                float objY = Config.Stream.GetSingle(objAddress + ObjectConfig.YOffset);
                float hurtboxRadius = Config.Stream.GetSingle(objAddress + ObjectConfig.HurtboxRadiusOffset);
                float hurtboxHeight = Config.Stream.GetSingle(objAddress + ObjectConfig.HurtboxHeightOffset);
                float hitboxDownOffset = Config.Stream.GetSingle(objAddress + ObjectConfig.HitboxDownOffsetOffset);
                float hurtboxMinY = objY - hitboxDownOffset;
                float hurtboxMaxY = hurtboxMinY + hurtboxHeight;

                uint marioObjRef = Config.Stream.GetUInt32(MarioObjectConfig.PointerAddress);
                float marioHurtboxRadius = Config.Stream.GetSingle(marioObjRef + ObjectConfig.HurtboxRadiusOffset);
                float effectiveRadius = hurtboxRadius + marioHurtboxRadius;

                float marioHurtboxHeight = Config.Stream.GetSingle(marioObjRef + ObjectConfig.HurtboxHeightOffset);

                return ((float)posAngle.X, (float)posAngle.Z, effectiveRadius, hurtboxMinY - marioHurtboxHeight, hurtboxMaxY);
            };

            public static GetDimensions CustomSize(Func<(float, float, float)> customSizeProvider) => posAngle =>
            {
                (float radius, float offsetY, float height) = customSizeProvider();
                return ((float)posAngle.X, (float)posAngle.Z, radius, (float)posAngle.Y + offsetY, (float)posAngle.Y + offsetY + height);
            };
        }
    }
}
