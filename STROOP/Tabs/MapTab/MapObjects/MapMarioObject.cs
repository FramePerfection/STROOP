using System;
using System.Drawing;
using STROOP.Utilities;
using STROOP.Structs.Configurations;
using System.Windows.Forms;

namespace STROOP.Tabs.MapTab.MapObjects
{
    [ObjectDescription("Mario", "Objects")]
    public class MapMarioObject : MapIconPointObject
    {
        public MapMarioObject() : base(null)
        {
            InternalRotates = true;
            positionAngleProvider = () => new System.Collections.Generic.List<PositionAngle>(new[] { PositionAngle.Mario });
        }

        public override Lazy<Image> GetInternalImage() => Config.ObjectAssociations.MarioMapImage;

        protected override bool IsGameObject => true;

        public override void InitSubTrackerContextMenuStrip(MapTab mapTab, ContextMenuStrip targetStrip)
        {
            base.InitSubTrackerContextMenuStrip(mapTab, targetStrip);
            MapObjectObject.AddObjectSubTrackers(tracker, GetName(), targetStrip, positionAngleProvider);
        }

        public override string GetName() => "Mario";
    }
}
