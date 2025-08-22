using System;
using System.Drawing;
using STROOP.Utilities;
using STROOP.Structs.Configurations;
using System.Windows.Forms;
using OpenTK;
using OpenTK.Mathematics;

namespace STROOP.Tabs.MapTab.MapObjects
{
    [ObjectDescription("Ghost", "Objects")]
    public class MapGhostObject : MapIconPointObject
    {
        public MapGhostObject()
            : base(null)
        {
            positionAngleProvider = () => AccessScope<StroopMainForm>.content.GetTab<GhostTab.GhostTab>().GetGhosts();
            InternalRotates = true;
        }

        protected override void DrawTopDown(MapGraphics graphics)
        {
            graphics.drawLayers[(int)MapGraphics.DrawLayers.FillBuffers].Add(() =>
            {
                foreach (var pa in positionAngleProvider())
                if (pa is GhostTab.Ghost.GhostPositionAngle a)
                    {
                        var transparent = graphics.view.mode == MapView.ViewMode.ThreeDimensional;
                        var alpha = hoverData.currentPositionAngle == a ? ObjectUtilities.HoverAlpha() : 1;
                        var angle = Rotates ? (float)a.Angle : 0x8000 - graphics.MapViewAngleValue;

                        DrawIcon(graphics,
                            transparent,
                            (float)a.X, (float)a.Y, (float)a.Z,
                            angle,
                            Config.ObjectAssociations.MarioMapNoHatImage.Value,
                            new Vector4(1, 1, 1, alpha));

                        DrawIcon(graphics,
                            transparent,
                            (float)a.X, (float)a.Y, (float)a.Z,
                            angle,
                            Config.ObjectAssociations.MarioMapHatOnlyImage.Value,
                            new Vector4(a.color.X, a.color.Y, a.color.Z, alpha));
                    }
            });
        }

        public override void InitSubTrackerContextMenuStrip(MapTab mapTab, ContextMenuStrip targetStrip)
        {
            base.InitSubTrackerContextMenuStrip(mapTab, targetStrip);

            targetStrip.Items.AddHandlerToItem("Add Tracker for Ghost Graphics Angle",
                 tracker.MakeCreateTrackerHandler(mapTab, "GhostGraphicsAngle", _ =>
                    new MapArrowObject(
                     positionAngleProvider,
                     __ => __.Angle,
                     MapArrowObject.ArrowSource.Constant(100),
                     $"Ghost Graphics Angle")));
        }

        public override Lazy<Image> GetInternalImage() => Config.ObjectAssociations.GreenMarioMapImage;

        public override string GetName() => "Ghost";
    }
}
