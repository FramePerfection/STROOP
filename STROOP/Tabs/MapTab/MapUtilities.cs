using System;
using System.Collections.Generic;
using System.Linq;
using OpenTK.Graphics.OpenGL;
using STROOP.Core;
using STROOP.Utilities;
using STROOP.Structs;
using STROOP.Models;
using STROOP.Variables.Utilities;
using System.Windows.Forms;

namespace STROOP.Tabs.MapTab
{
    public static class MapUtilities
    {
        public static int WhiteTexture { get; private set; }
        private static readonly byte[] _whiteTexData = new byte[] { 0xFF };

        static MapUtilities()
        {
            AccessScope<MapTab>.content.graphics.DoGLInit(() =>
            {
                WhiteTexture = GL.GenTexture();
                GL.BindTexture(TextureTarget.Texture2D, WhiteTexture);
                GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba8, 1, 1, 0, OpenTK.Graphics.OpenGL.PixelFormat.Luminance, PixelType.UnsignedByte, _whiteTexData);
            });
        }

        public static List<TriangleDataModel> GetTriangles(uint triAddresses)
        {
            return GetTriangles(new List<uint>() { triAddresses });
        }

        public static List<TriangleDataModel> GetTriangles(List<uint> triAddresses)
        {
            return triAddresses.FindAll(triAddress => triAddress != 0)
                .ConvertAll(triAddress => TriangleDataModel.Create(triAddress));
        }

        public static (float x1, float z1, float x2, float z2, bool xProjection, double pushAngle)? Get2DWallDataFromTri(
            TriangleDataModel tri, float? heightNullable = null)
        {
            double uphillAngle = VariableSpecialUtilities.GetTriangleUphillAngle(tri);
            double pushAngle = MoreMath.ReverseAngle(uphillAngle);

            if (!heightNullable.HasValue)
            {
                if (tri.X1 == tri.X2 && tri.Z1 == tri.Z2) // v2 is redundant
                    return (tri.X1, tri.Z1, tri.X3, tri.Z3, tri.XProjection, pushAngle);
                if (tri.X1 == tri.X3 && tri.Z1 == tri.Z3) // v3 is redundant
                    return (tri.X1, tri.Z1, tri.X2, tri.Z2, tri.XProjection, pushAngle);
                if (tri.X2 == tri.X3 && tri.Z2 == tri.Z3) // v3 is redundant
                    return (tri.X1, tri.Z1, tri.X2, tri.Z2, tri.XProjection, pushAngle);

                double dist12 = MoreMath.GetDistanceBetween(tri.X1, tri.Z1, tri.X2, tri.Z2);
                double dist13 = MoreMath.GetDistanceBetween(tri.X1, tri.Z1, tri.X3, tri.Z3);
                double dist23 = MoreMath.GetDistanceBetween(tri.X2, tri.Z2, tri.X3, tri.Z3);

                if (dist12 >= dist13 && dist12 >= dist23)
                    return (tri.X1, tri.Z1, tri.X2, tri.Z2, tri.XProjection, pushAngle);
                else if (dist13 >= dist23)
                    return (tri.X1, tri.Z1, tri.X3, tri.Z3, tri.XProjection, pushAngle);
                else
                    return (tri.X2, tri.Z2, tri.X3, tri.Z3, tri.XProjection, pushAngle);
            }

            float height = heightNullable.Value;
            (float pointAX, float pointAZ) = GetHeightOnLine(height, tri.X1, tri.Y1, tri.Z1, tri.X2, tri.Y2, tri.Z2);
            (float pointBX, float pointBZ) = GetHeightOnLine(height, tri.X1, tri.Y1, tri.Z1, tri.X3, tri.Y3, tri.Z3);
            (float pointCX, float pointCZ) = GetHeightOnLine(height, tri.X2, tri.Y2, tri.Z2, tri.X3, tri.Y3, tri.Z3);

            List<(float x, float z)> points = new List<(float x, float z)>();
            if (!float.IsNaN(pointAX) && !float.IsNaN(pointAZ)) points.Add((pointAX, pointAZ));
            if (!float.IsNaN(pointBX) && !float.IsNaN(pointBZ)) points.Add((pointBX, pointBZ));
            if (!float.IsNaN(pointCX) && !float.IsNaN(pointCZ)) points.Add((pointCX, pointCZ));

            if (points.Count == 3)
            {
                double distAB = MoreMath.GetDistanceBetween(pointAX, pointAZ, pointBX, pointBZ);
                double distAC = MoreMath.GetDistanceBetween(pointAX, pointAZ, pointCX, pointCZ);
                double distBC = MoreMath.GetDistanceBetween(pointBX, pointBZ, pointCX, pointCZ);
                if (distAB >= distAC && distAB >= distBC)
                {
                    points.RemoveAt(2); // AB is biggest, so remove C
                }
                else if (distAC >= distBC)
                {
                    points.RemoveAt(1); // AC is biggest, so remove B
                }
                else
                {
                    points.RemoveAt(0); // BC is biggest, so remove A
                }
            }

            if (points.Count == 2)
            {
                return (points[0].x, points[0].z, points[1].x, points[1].z, tri.XProjection, pushAngle);
            }

            return null;
        }

        private static (float x, float z) GetHeightOnLine(
            float height, float x1, float y1, float z1, float x2, float y2, float z2)
        {
            if (y1 == y2 || height < Math.Min(y1, y2) || height > Math.Max(y1, y2))
                return (float.NaN, float.NaN);

            float p = (height - y1) / (y2 - y1);
            float px = x1 + p * (x2 - x1);
            float pz = z1 + p * (z2 - z1);
            return (px, pz);
        }

        public static void CreateTrackBarContextMenuStrip(TrackBar trackBar)
        {
            List<int> maxValues = Enumerable.Range(1, 9).ToList().ConvertAll(p => (int)Math.Pow(10, p));
            trackBar.ContextMenuStrip = new ContextMenuStrip();
            List<ToolStripMenuItem> items = maxValues.ConvertAll(
                maxValue => new ToolStripMenuItem("Max of " + maxValue));
            for (int i = 0; i < items.Count; i++)
            {
                int maxValue = maxValues[i];
                ToolStripMenuItem item = items[i];
                item.Click += (sender, e) =>
                {
                    trackBar.Maximum = maxValue;
                    items.ForEach(it => it.Checked = it == item);
                };
                if (trackBar.Maximum == maxValue) item.Checked = true;
                trackBar.ContextMenuStrip.Items.Add(item);
            }
        }

        public static List<(float x, float y, float z)> ParsePoints(string text, bool useTriplets)
        {
            if (text == null) return null;

            List<float?> nullableDoubleList = ParsingUtilities.ParseStringList(text)
                .ConvertAll(word => (float?)ParsingUtilities.ParseDoubleNullable(word));
            if (nullableDoubleList.Any(nullableDouble => !nullableDouble.HasValue))
            {
                return null;
            }

            List<float> doubleList = nullableDoubleList.ConvertAll(nullableDouble => nullableDouble.Value);

            int numbersPerGroup = useTriplets ? 3 : 2;
            if (doubleList.Count % numbersPerGroup != 0)
            {
                return null;
            }

            List<(float x, float y, float z)> points = new List<(float x, float y, float z)>();
            for (int i = 0; i < doubleList.Count; i += numbersPerGroup)
            {
                (float x, float y, float z) point =
                    useTriplets ? (doubleList[i], doubleList[i + 1], doubleList[i + 2]) : (doubleList[i], 0, doubleList[i + 1]);
                points.Add(point);
            }

            return points;
        }
    }
}
