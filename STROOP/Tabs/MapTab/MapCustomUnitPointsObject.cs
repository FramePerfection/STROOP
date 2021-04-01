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

namespace STROOP.Tabs.MapTab
{
    [ObjectDescription("Custom Unit Points", nameof(Create))]
    public class MapCustomUnitPointsObject : MapQuadObject
    {
        private readonly List<(int x, int z)> _unitPoints;

        public MapCustomUnitPointsObject(List<(int x, int z)> unitPoints)
            : base()
        {
            _unitPoints = unitPoints;

            Opacity = 0.5;
            Color = Color.Orange;
        }

        public static MapCustomUnitPointsObject Create()
        {
            (string, bool)? result = DialogUtilities.GetStringAndSideFromDialog(
                labelText: "Enter points as pairs or triplets of floats.",
                button1Text: "Pairs",
                button2Text: "Triplets");
            if (!result.HasValue) return null;
            (string text, bool useTriplets) = result.Value;
            List<(double x, double y, double z)> points = MapUtilities.ParsePoints(text, useTriplets);
            if (points == null) return null;
            List<(int x, int z)> unitPoints = points.ConvertAll(
                point => ((int)point.x, (int)point.z));
            return new MapCustomUnitPointsObject(unitPoints);
        }

        protected override List<List<(float x, float y, float z)>> GetQuadList()
        {
            return MapUtilities.ConvertUnitPointsToQuads(_unitPoints);
        }

        public override string GetName()
        {
            return "Custom Unit Points";
        }

        public override Lazy<Image> GetInternalImage() => Config.ObjectAssociations.CustomPointsImage;
    }
}
