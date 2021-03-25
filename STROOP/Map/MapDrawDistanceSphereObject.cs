﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using OpenTK.Graphics.OpenGL;
using STROOP.Utilities;
using STROOP.Structs.Configurations;
using STROOP.Structs;
using OpenTK;
using System.Drawing.Imaging;

namespace STROOP.Map
{
    public class MapDrawDistanceSphereObject : MapSphereObject
    {
        private readonly PositionAngle _posAngle;

        public MapDrawDistanceSphereObject(PositionAngle posAngle)
            : base()
        {
            _posAngle = posAngle;
        }

        protected override List<(float centerX, float centerY, float centerZ, float radius3D)> Get3DDimensions()
        {
            uint objAddress = _posAngle.GetObjAddress();
            float drawDist = Config.Stream.GetSingle(objAddress + ObjectConfig.DrawDistOffset);
            return new List<(float centerX, float centerY, float centerZ, float radius3D)>()
            {
                ((float)_posAngle.X, (float)_posAngle.Y, (float)_posAngle.Z, drawDist)
            };
        }

        public override Lazy<Image> GetInternalImage() => Config.ObjectAssociations.SphereImage;

        public override string GetName()
        {
            return "Draw Distance Sphere for " + _posAngle.GetMapName();
        }

        public override PositionAngle GetPositionAngle()
        {
            return _posAngle;
        }
    }
}
