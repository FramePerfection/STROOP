using System;
using System.Collections.Generic;
using System.Drawing;
using STROOP.Utilities;
using STROOP.Structs.Configurations;
using STROOP.Structs;

namespace STROOP.Tabs.MapTab.MapObjects
{
    public class MapObjectSphereObject : MapSphereObject
    {
        public delegate (float centerX, float centerY, float centerZ, float radius) GetDimensions(PositionAngle positionAngle);

        public PositionAngleProvider posAngle;
        public GetDimensions getDimensions;
        public string name;

        public MapObjectSphereObject(PositionAngleProvider posAngle, GetDimensions getDimensions, string name)
            : base(null)
        {
            this.posAngle = posAngle;
            this.getDimensions = getDimensions;
            this.name = name;
        }

        protected override List<(float centerX, float centerY, float centerZ, float radius3D)> Get3DDimensions()
        {
            var lst = new List<(float centerX, float centerY, float centerZ, float radius)>();
            foreach (var obj in posAngle())
                lst.Add(getDimensions(obj));
            return lst;
        }

        public override Lazy<Image> GetInternalImage() => Config.ObjectAssociations.SphereImage;

        public override string GetName() => $"{name} for {PositionAngle.NameOfMultiple(positionAngleProvider())}";

        public static class Dimensions
        {
            public static GetDimensions Tangibility = posAngle =>
            {
                uint objAddress = PositionAngle.GetObjectAddress(posAngle);
                float tangibleDist = Config.Stream.GetSingle(objAddress + ObjectConfig.TangibleDistOffset);
                return ((float)posAngle.X, (float)posAngle.Y, (float)posAngle.Z, tangibleDist);
            };

            public static GetDimensions DrawDistance = posAngle =>
            {
                uint objAddress = PositionAngle.GetObjectAddress(posAngle);
                float drawDist = Config.Stream.GetSingle(objAddress + ObjectConfig.DrawDistOffset);
                return ((float)posAngle.X, (float)posAngle.Y, (float)posAngle.Z, drawDist);
            };

            public static GetDimensions CustomSize(Func<float> customSizeProvider) =>
                posAngle =>
                    ((float)posAngle.X, (float)posAngle.Y, (float)posAngle.Z, customSizeProvider());
        }
    }
}
