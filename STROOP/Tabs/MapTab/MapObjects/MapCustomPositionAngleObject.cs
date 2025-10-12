using System;
using System.Drawing;
using STROOP.Utilities;
using STROOP.Structs.Configurations;
using System.Windows.Forms;

namespace STROOP.Tabs.MapTab.MapObjects
{
    [ObjectDescription("Custom PositionAngle", "Custom", nameof(Create))]
    public class MapCustomPositionAngleObject : MapIconPointObject
    {
        private readonly PositionAngle _posAngle;

        public MapCustomPositionAngleObject(ObjectCreateParams createParams, PositionAngle posAngle)
            : base(createParams)
        {
            _posAngle = posAngle;
            positionAngleProvider = () => new[] { _posAngle };
            InternalRotates = true;
        }

        public static MapCustomPositionAngleObject Create(ObjectCreateParams creationParameters)
        {
            string text = ObjectCreateParams.GetString(ref creationParameters, "Name", "Enter a PositionAngle.");
            PositionAngle posAngle = PositionAngle.FromString(text);
            return posAngle != null ? new MapCustomPositionAngleObject(creationParameters, posAngle) : null;
        }

        public override Lazy<Image> GetInternalImage() => Config.ObjectAssociations.GreenMarioMapImage;

        public override void InitSubTrackerContextMenuStrip(MapTab mapTab, ContextMenuStrip targetStrip)
        {
            base.InitSubTrackerContextMenuStrip(mapTab, targetStrip);

            targetStrip.Items.AddHandlerToItem("Add Tracker for Angle",
                tracker.MakeCreateTrackerHandler(mapTab, "CustomAngle", _ => new MapArrowObject(
                    positionAngleProvider,
                    o => o.Angle,
                    MapArrowObject.ArrowSource.Constant(1),
                    $"Angle")));
        }

        public override string GetName()
        {
            return _posAngle.GetMapName();
        }
    }
}
