using System;
using System.Collections.Generic;
using System.Drawing;
using STROOP.Utilities;
using STROOP.Structs.Configurations;

namespace STROOP.Tabs.MapTab.MapObjects
{
    [ObjectDescription("Punch Detector", "Misc")]
    public class MapPunchDetectorObject : MapCylinderObject
    {
        public MapPunchDetectorObject() : base(null)
        {
        }

        protected override List<(float centerX, float centerZ, float radius, float minY, float maxY)> Get3DDimensions()
        {
            PositionAngle mario = PositionAngle.Mario;
            (double centerX, double centerZ) = MoreMath.AddVectorToPoint(50, mario.Angle, mario.X, mario.Z);
            double minY = mario.Y;
            double maxY = minY + 80;
            float radius = 5;
            return new List<(float centerX, float centerZ, float radius, float minY, float maxY)>()
            {
                ((float)centerX, (float)centerZ, radius, (float)minY, (float)maxY)
            };
        }

        public override Lazy<Image> GetInternalImage() => Config.ObjectAssociations.CylinderImage;

        public override string GetName()
        {
            return "Punch Detector";
        }

        public override float GetY()
        {
            return (float)PositionAngle.Mario.Y;
        }
    }
}
