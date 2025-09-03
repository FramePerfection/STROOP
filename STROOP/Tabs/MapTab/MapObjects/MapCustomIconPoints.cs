using System;
using System.Drawing;
using STROOP.Utilities;
using STROOP.Structs.Configurations;
using System.Collections.Generic;
using System.Windows.Forms;
using OpenTK;
using OpenTK.Mathematics;

namespace STROOP.Tabs.MapTab.MapObjects
{
    [ObjectDescription("Custom Icon Points", "Custom")]
    public class MapCustomIconPoints : MapIconPointObject
    {
        class CustomPointHoverData : MapObjectHoverData
        {
            public CustomPointHoverData(MapCustomIconPoints parent) : base(parent) { }
            public override void AddContextMenuItems(MapTab tab, ContextMenuStrip menu)
            {
                base.AddContextMenuItems(tab, menu);
                menu.Items.GetSubItem(ToString()).DropDownItems.AddHandlerToItem(
                    "Remove",
                    () => ((MapCustomIconPoints)parent).positionAngles.Remove(currentPositionAngle)
                    );
            }
            public override string ToString() => $"{parent.GetName()} {currentPositionAngle.position}";
        }

        public string name = "Custom Points";

        List<PositionAngle> positionAngles = new List<PositionAngle>();

        public MapCustomIconPoints() : base(null)
        {
            positionAngleProvider = () => positionAngles;
            hoverData = new CustomPointHoverData(this);
            CreateNewPoint(AccessScope<MapTab>.content);
            enableDragging = true;
        }

        public void Clear() => positionAngles.Clear();
        public void AddPoint(Vector3 position) => positionAngles.Add(PositionAngle.Custom(position));

        protected override ContextMenuStrip GetContextMenuStrip(MapTracker targetTracker)
        {
            var mapTab = targetTracker.mapTab;
            var strip = base.GetContextMenuStrip(targetTracker);
            var itemAddPoint = new ToolStripMenuItem("Add point");
            itemAddPoint.Click += (_, __) => CreateNewPoint(mapTab);
            strip.Items.Insert(0, itemAddPoint);
            var itemName = new ToolStripMenuItem("Set Name");
            itemName.Click += (_, __) => name = DialogUtilities.GetStringFromDialog(name, "Enter Name") ?? name;
            strip.Items.Insert(1, itemName);
            return strip;
        }

        void CreateNewPoint(MapTab mapTab)
        {
            if (mapTab == null)
                return;
            var newPointPos = mapTab.graphics.view.position;
            if (mapTab.graphics.view.mode == MapView.ViewMode.ThreeDimensional)
                newPointPos += mapTab.graphics.view.ComputeViewDirection() * 50;
            positionAngles.Add(PositionAngle.Custom(newPointPos));
        }

        public override Lazy<Image> GetInternalImage() => Config.ObjectAssociations.PointImage;

        public override string GetName() => name;

        public override (SaveSettings save, LoadSettings load) SettingsSaveLoad =>
        (
            node =>
            {
                SaveValueNode(node, "Name", name);
                SaveValueNode(node, "Points", ParsingUtilities.CreatePointList(positionAngles.ConvertAll(_ => ((float)_.X, (float)_.Y, (float)_.Z))));
            }
        ,
            node =>
            {
                name = LoadValueNode(node, "Name") ?? "Custom Points";
                positionAngles = new List<PositionAngle>(
                    ParsingUtilities.ParsePointList(LoadValueNode(node, "Points")).ConvertAll(_ => PositionAngle.Custom(new Vector3(_.Item1, _.Item2, _.Item3)))
                    );
            }
        );
    }
}
