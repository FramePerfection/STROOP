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
using System.Windows.Forms;

namespace STROOP.Tabs.MapTab
{
    public class MapMarioMovingArrowObject : MapArrowObject
    {
        private readonly PositionAngle _posAngle;

        public MapMarioMovingArrowObject(PositionAngle posAngle)
            : base()
        {
            _posAngle = posAngle;
        }

        public override PositionAngle GetPositionAngle()
        {
            return _posAngle;
        }

        protected override double GetYaw()
        {
            return Config.Stream.GetUInt16(MarioConfig.StructAddress + MarioConfig.MovingYawOffset);
        }

        protected override double GetRecommendedSize()
        {
            return Config.Stream.GetSingle(MarioConfig.StructAddress + MarioConfig.HSpeedOffset);
        }

        public override string GetName()
        {
            return "Mario Moving Arrow";
        }
    }
}
