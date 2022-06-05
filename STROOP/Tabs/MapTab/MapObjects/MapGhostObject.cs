﻿using System;
using System.Drawing;
using System.Collections.Generic;
using STROOP.Utilities;
using STROOP.Structs.Configurations;
using OpenTK;
using System.Windows.Forms;

namespace STROOP.Tabs.MapTab.MapObjects
{
    [ObjectDescription("Ghost", "Objects")]
    public class MapGhostObject : MapIconPointObject
    {
        class GhostPositionAngle : PositionAngle
        {
            public static GhostPositionAngle instance = new GhostPositionAngle();
            private GhostPositionAngle() : base() { }
            Vector3 Position => AccessScope<StroopMainForm>.content?.GetTab<GhostTab.GhostTab>()?.GhostPosition ?? new Vector3();
            public override double X => Position.X;
            public override double Y => Position.Y;
            public override double Z => Position.Z;
            public override double Angle => AccessScope<StroopMainForm>.content?.GetTab<GhostTab.GhostTab>()?.GhostAngle ?? 0;

            public override bool SetX(double value) => false;
            public override bool SetY(double value) => false;
            public override bool SetZ(double value) => false;
            public override bool SetAngle(double value) => false;
            public override string GetMapName() => "Ghost";
        }

        public MapGhostObject()
            : base(null)
        {
            positionAngleProvider = () => new List<PositionAngle>(new[] { GhostPositionAngle.instance });
            InternalRotates = true;
        }

        public override void InitSubTrackerContextMenuStrip(MapTab mapTab, ContextMenuStrip targetStrip)
        {
            base.InitSubTrackerContextMenuStrip(mapTab, targetStrip);

            targetStrip.Items.AddHandlerToItem("Add Tracker for Ghost Graphics Angle",
                 tracker.MakeCreateTrackerHandler(mapTab, "GhostGraphicsAngle", _ =>
                    new MapArrowObject(
                     positionAngleProvider,
                     __ => GhostPositionAngle.instance.Angle,
                     MapArrowObject.ArrowSource.Constant(100),
                     $"Ghost Graphics Angle")));
        }

        public override Lazy<Image> GetInternalImage() => Config.ObjectAssociations.GreenMarioMapImage;

        public override string GetName()
        {
            return "Ghost";
        }
    }
}
