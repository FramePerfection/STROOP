using OpenTK;
using OpenTK.Mathematics;
using STROOP.Utilities;

namespace STROOP.Tabs.MapTab.MapObjects
{
    public abstract class MapIconPointObject : MapIconObject
    {
        protected MapIconPointObject(ObjectCreateParams creationParameters)
        : base(creationParameters) { }

        protected override void DrawTopDown(MapGraphics graphics)
        {
            graphics.drawLayers[(int)MapGraphics.DrawLayers.FillBuffers].Add(() =>
            {
                foreach (var a in positionAngleProvider())
                    DrawIcon(graphics,
                        graphics.view.mode == MapView.ViewMode.ThreeDimensional,
                        (float)a.X, (float)a.Y, (float)a.Z,
                        Rotates ? (float)a.Angle : 0x8000 - graphics.MapViewAngleValue,
                        GetInternalImage()?.Value,
                        new Vector4(1, 1, 1, hoverData.currentPositionAngle == a ? ObjectUtilities.HoverAlpha() : 1));
            });
        }

        protected override void DrawOrthogonal(MapGraphics graphics) => DrawTopDown(graphics);

        public override bool ParticipatesInGlobalIconSize() => true;
        
        public override IHoverData GetHoverData(MapGraphics graphics, ref Vector3 position)
        {
            var radius = Size / graphics.MapViewScaleValue;
            var cursorPos = graphics.mapCursorPosition;
            var closestDist = float.PositiveInfinity;
            if (!graphics.IsMouseDown(0))
            {
                hoverData.currentPositionAngle = null;
                foreach (var a in positionAngleProvider())
                    if (graphics.view.mode == MapView.ViewMode.TopDown)
                    {
                        if (graphics.HoverTopDown(new Vector3((float)a.X, cursorPos.Y, (float)a.Z), radius))
                        {
                            hoverData.currentPositionAngle = a;
                            break;
                        }
                    }
                    else if (graphics.view.mode == MapView.ViewMode.Orthogonal)
                    {
                        if (graphics.HoverOrthogonal(a.position, radius))
                        {
                            hoverData.currentPositionAngle = a;
                            break;
                        }
                    }
                    else if (graphics.view.mode == MapView.ViewMode.ThreeDimensional)
                    {
                        var rad = Size * Get3DIconScale(graphics, (float)a.X, (float)a.Y, (float)a.Z);
                        if (graphics.Hover3D(a.position, rad))
                        {
                            var newDist = (a.position - graphics.view.position).LengthSquared;
                            if (closestDist > newDist)
                            {
                                hoverData.currentPositionAngle = a;
                                closestDist = newDist;
                            }
                        }
                    }
            }
            if (hoverData.currentPositionAngle != null)
            {
                position = hoverData.currentPositionAngle.position;
                return hoverData;
            }
            return null;
        }
    }
}
