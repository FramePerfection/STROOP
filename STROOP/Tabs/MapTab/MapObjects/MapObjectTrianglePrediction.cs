using System;
using System.Collections.Generic;
using System.Drawing;
using STROOP.Utilities;
using STROOP.Structs.Configurations;
using STROOP.Models;
using STROOP.Tabs.MapTab.DataUtil;
using System.Windows.Forms;

namespace STROOP.Tabs.MapTab.MapObjects
{
    public class MapObjectWallPrediction : MapWallObject
    {
        ObjectTrianglePrediction predictionProvider;

        public MapObjectWallPrediction(PositionAngleProvider p)
        {
            positionAngleProvider = p;
            predictionProvider = new ObjectTrianglePrediction(p, t => t.IsWall());
        }

        public override Lazy<Image> GetInternalImage() => Config.ObjectAssociations.TriangleWallImage;

        public override string GetName() => $"Wall triangles for {PositionAngle.NameOfMultiple(positionAngleProvider())}";

        protected override ContextMenuStrip GetContextMenuStrip(MapTracker targetTracker)
        {
            var _contextMenuStrip = base.GetContextMenuStrip(targetTracker);
            predictionProvider.AddContextMenuItems(_contextMenuStrip.Items);
            return _contextMenuStrip;
        }

        public override void Update()
        {
            predictionProvider.Update();
            base.Update();
        }

        protected override List<TriangleDataModel> GetTrianglesOfAnyDist() => predictionProvider.GetTriangles();
    }

    public class MapObjectFloorPrediction : MapFloorObject
    {
        ObjectTrianglePrediction predictionProvider;

        public MapObjectFloorPrediction(PositionAngleProvider p) : base(null)
        {
            positionAngleProvider = p;
            predictionProvider = new ObjectTrianglePrediction(p, t => t.IsFloor());
        }

        protected override ContextMenuStrip GetContextMenuStrip(MapTracker targetTracker)
        {
            var _contextMenuStrip = base.GetContextMenuStrip(targetTracker);
            predictionProvider.AddContextMenuItems(_contextMenuStrip.Items);
            return _contextMenuStrip;
        }

        public override Lazy<Image> GetInternalImage() => Config.ObjectAssociations.TriangleFloorImage;

        public override string GetName() => $"Floor triangles for {PositionAngle.NameOfMultiple(positionAngleProvider())}";

        public override void Update()
        {
            predictionProvider.Update();
            base.Update();
        }

        protected override List<TriangleDataModel> GetTrianglesOfAnyDist() => predictionProvider.GetTriangles();
    }

    public class MapObjectCeilingPrediction : MapCeilingObject
    {
        ObjectTrianglePrediction predictionProvider;

        public MapObjectCeilingPrediction(PositionAngleProvider p)
        {
            positionAngleProvider = p;
            predictionProvider = new ObjectTrianglePrediction(p, t => t.IsCeiling());
        }

        protected override ContextMenuStrip GetContextMenuStrip(MapTracker targetTracker)
        {
            var _contextMenuStrip = base.GetContextMenuStrip(targetTracker);
            predictionProvider.AddContextMenuItems(_contextMenuStrip.Items);
            return _contextMenuStrip;
        }

        public override Lazy<Image> GetInternalImage() => Config.ObjectAssociations.TriangleCeilingImage;

        public override string GetName() => $"Ceiling triangles for {PositionAngle.NameOfMultiple(positionAngleProvider())}";

        public override void Update()
        {
            predictionProvider.Update();
            base.Update();
        }

        protected override List<TriangleDataModel> GetTrianglesOfAnyDist() => predictionProvider.GetTriangles();
    }
}
