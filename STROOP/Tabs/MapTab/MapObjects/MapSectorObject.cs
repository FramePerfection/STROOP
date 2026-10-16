using System;
using System.Collections.Generic;
using System.Drawing;
using STROOP.Utilities;
using STROOP.Structs.Configurations;
using OpenTK;
using System.Windows.Forms;
using OpenTK.Mathematics;
using STROOP.Variables.Utilities;

namespace STROOP.Tabs.MapTab.MapObjects
{
    public class MapSectorObject : MapObject
    {
        protected readonly static int NUM_POINTS_2D = 257;

        private float _angleRadius;

        public MapSectorObject(PositionAngleProvider positionAngleProvider)
            : base()
        {
            this.positionAngleProvider = positionAngleProvider;
            _angleRadius = 4096;

            Size = 1000;
            Opacity = 0.5;
            Color = Color.Yellow;
        }

        protected override void DrawTopDown(MapGraphics graphics)
        {
            graphics.drawLayers[(int)MapGraphics.DrawLayers.FillBuffers].Add(() =>
            {
                var dimenstionList = GetDimensions();
                foreach ((float centerX, float centerY, float centerZ, float radius, float angle, float angleRadius) in dimenstionList)
                {
                    var pfft = MoreMath.AngleUnitsToRadians(angle - angleRadius);
                    var pfft2 = MoreMath.AngleUnitsToRadians(angle + angleRadius);
                    graphics.lineRenderer.Add(
                        new Vector3(centerX, centerY, centerZ),
                        new Vector3(centerX + (float)Math.Sin(pfft) * radius, centerY, centerZ + (float)Math.Cos(pfft) * radius),
                        OpenTKUtilities.ColorToVec4(OutlineColor),
                        OutlineWidth);

                    graphics.lineRenderer.Add(
                        new Vector3(centerX, centerY, centerZ),
                        new Vector3(centerX + (float)Math.Sin(pfft2) * radius, centerY, centerZ + (float)Math.Cos(pfft2) * radius),
                        OpenTKUtilities.ColorToVec4(OutlineColor),
                        OutlineWidth);
                }
            });
        }

        protected override void DrawOrthogonal(MapGraphics graphics) => DrawTopDown(graphics);

        protected override void Draw3D(MapGraphics graphics)
        {
            graphics.drawLayers[(int)MapGraphics.DrawLayers.FillBuffers].Add(() =>
            {
                var fillColor = OpenTKUtilities.ColorToVec4(Color, OpacityByte);
                var outlineColor = OpenTKUtilities.ColorToVec4(OutlineColor);
                foreach ((float centerX, float centerY, float centerZ, float radius, float angle, float angleRadius) in GetDimensions())
                {
                    float angleSpan = Math.Min(2 * Math.Abs(angleRadius), 65536);
                    int numSegments = Math.Max(2, (int)Math.Ceiling((NUM_POINTS_2D - 1) * angleSpan / 65536));
                    var center = new Vector3(centerX, centerY, centerZ);

                    Vector3 PointAt(int i)
                    {
                        double radians = MoreMath.AngleUnitsToRadians(angle - angleSpan / 2 + angleSpan * i / numSegments);
                        return new Vector3(centerX + (float)Math.Sin(radians) * radius, centerY, centerZ + (float)Math.Cos(radians) * radius);
                    }

                    Vector3 previous = PointAt(0);
                    if (angleSpan < 65536)
                        graphics.lineRenderer.Add(center, previous, outlineColor, OutlineWidth);
                    for (int i = 1; i <= numSegments; i++)
                    {
                        Vector3 current = PointAt(i);
                        graphics.triangleRenderer.Add(center, previous, current, false, fillColor, outlineColor, Vector3.Zero, true);
                        graphics.lineRenderer.Add(previous, current, outlineColor, OutlineWidth);
                        previous = current;
                    }

                    if (angleSpan < 65536)
                        graphics.lineRenderer.Add(center, previous, outlineColor, OutlineWidth);
                }
            });
        }

        protected List<(float centerX, float centerY, float centerZ, float radius, float angle, float angleRadius)> GetDimensions()
        {
            var lst = new List<(float centerX, float centerY, float centerZ, float radius, float angle, float angleRadius)>();
            foreach (var _posAngle in positionAngleProvider())
            {
                (double x, double y, double z, double angle) = _posAngle.GetValues();
                lst.Add(((float)x, (float)y, (float)z, Size, (float)angle, _angleRadius));
            }

            return lst;
        }

        public override Lazy<Image> GetInternalImage() => Config.ObjectAssociations.ArrowImage;

        public override string GetName() => $"Sector for {PositionAngle.NameOfMultiple(positionAngleProvider())}";

        protected override ContextMenuStrip GetContextMenuStrip(MapTracker targetTracker)
        {
            ToolStripMenuItem itemSetAngleRadius = new ToolStripMenuItem("Set Angle Radius");
            itemSetAngleRadius.Click += (sender, e) =>
            {
                string text = DialogUtilities.GetStringFromDialog(labelText: "Enter the angle radius for sector:");
                float? angleRadius = ParsingUtilities.ParseFloatNullable(text);
                if (angleRadius.HasValue)
                    _angleRadius = angleRadius.Value;
            };

            var _contextMenuStrip = new ContextMenuStrip();
            _contextMenuStrip.Items.Add(itemSetAngleRadius);

            return _contextMenuStrip;
        }

        public override (SaveSettings save, LoadSettings load) SettingsSaveLoad => (
            (System.Xml.XmlNode node) =>
            {
                base.SettingsSaveLoad.save(node);
                SaveValueNode(node, "AngleRadius", _angleRadius.ToString());
            }
            ,
            (System.Xml.XmlNode node) =>
            {
                base.SettingsSaveLoad.load(node);
                if (float.TryParse(LoadValueNode(node, "AngleRadius"), out float angleRadius))
                    _angleRadius = angleRadius;
            }
        );
    }
}
