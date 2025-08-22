using System;
using System.Drawing;
using STROOP.Utilities;
using STROOP.Structs.Configurations;
using System.Collections.Generic;
using OpenTK;
using System.Windows.Forms;
using OpenTK.Mathematics;
using STROOP.Structs;

namespace STROOP.Tabs.MapTab.MapObjects
{
    [ObjectDescription("Custom Camera Path", "Custom")]
    public class MapCustomCameraPath : MapIconPointObject
    {
        class KeyFrame : PositionAngle.CustomPositionAngle
        {
            public class Target : PositionAngle.CustomPositionAngle
            {
                public readonly KeyFrame parent;
                public Target(KeyFrame parent) : base(Vector3.Zero) { this.parent = parent; }
            }
            public uint frame;
            public uint waitFor = 0;
            public readonly PositionAngle targetPoint;
            public KeyFrame() : base(Vector3.Zero)
            {
                targetPoint = new Target(this);
                frame = Config.Stream.GetUInt32(MiscConfig.GlobalTimerAddress);
            }
        }

        class KeyFrameHoverData : PointHoverData
        {
            public KeyFrame currentKeyFrame;
            public bool target;
            PositionAngle currentPositionAngle => target ? currentKeyFrame.targetPoint : currentKeyFrame;
            public KeyFrameHoverData(MapCustomCameraPath parent)
                : base(parent)
            { }

            static uint lastEnteredTime = 30;

            public override void AddContextMenuItems(MapTab tab, ContextMenuStrip menu)
            {
                base.AddContextMenuItems(tab, menu);
                var targetItem = menu.Items.GetSubItem(ToString());

                var alignPositionItem = new ToolStripMenuItem("Align with view");
                alignPositionItem.Click += (_, __) =>
                {
                    currentKeyFrame.position = tab.graphics.view.position;
                    currentKeyFrame.targetPoint.position = tab.graphics.view.position + tab.graphics.view.ComputeViewDirection() * 400;
                };

                var waitForItem = new ToolStripMenuItem("Wait for... (adjust timings)");
                waitForItem.Click += (_, __) =>
                {
                    var result = DialogUtilities.GetStringFromDialog(lastEnteredTime.ToString(), "Enter waiting time:");
                    if (uint.TryParse(result, out uint d))
                    {
                        ((MapCustomCameraPath)parent).SetWaitingTime(currentKeyFrame, d);
                        lastEnteredTime = d;
                    }
                };

                targetItem.DropDownItems.Add(waitForItem);

                var moveByItem = new ToolStripMenuItem("Move by... (adjust timings)");
                moveByItem.Click += (_, __) =>
                {
                    var result = DialogUtilities.GetStringFromDialog(lastEnteredTime.ToString(), "Enter waiting time:");
                    if (int.TryParse(result, out int d))
                        ((MapCustomCameraPath)parent).MoveBy(currentKeyFrame, (uint)d);
                };

                targetItem.DropDownItems.Add(moveByItem);


                var keyFrameTimeItem = new ToolStripMenuItem("Set Key Frame (global Timer)");
                keyFrameTimeItem.Click += (_, __) =>
                {
                    var result = DialogUtilities.GetDoubleFromDialog(0, currentKeyFrame.frame.ToString());
                    if (result != 0)
                    {
                        currentKeyFrame.frame = (uint)result;
                        ((MapCustomCameraPath)parent).SortKeyFrames();
                    }
                };

                var keyFrameDeleteItem = new ToolStripMenuItem("Delete Key Frame");
                keyFrameDeleteItem.Click += (_, __) => ((MapCustomCameraPath)parent).RemoveKeyFrame(currentKeyFrame);

                targetItem.DropDownItems.Add(keyFrameTimeItem);
                targetItem.DropDownItems.Add(keyFrameDeleteItem);
            }

            public override void SetLookAt(Vector3 lookAt) { }

            protected override void SetPosition(Vector3 newPosition)
            {
                currentPositionAngle.position = newPosition;
            }
            protected override Vector3 GetPosition() => currentPositionAngle.position;

            public override string ToString() => $"{(MapCustomCameraPath)parent}[{((MapCustomCameraPath)parent).GetIndex(currentKeyFrame)}]({currentKeyFrame.frame})";
        }


        ToolStripMenuItem itemPlayback;
        bool playback => itemPlayback.Checked;

        List<KeyFrame> keyFrames = new List<KeyFrame>();

        void SortKeyFrames()
        {
            keyFrames.Sort((a, b) => a.frame.CompareTo(b.frame));
        }

        void SetWaitingTime(KeyFrame frame, uint newWaitingTime)
        {
            var diff = newWaitingTime - frame.waitFor;
            frame.waitFor = newWaitingTime;
            bool found = false;
            foreach (var f in keyFrames)
                if (!found)
                {
                    if (f == frame)
                        found = true;
                }
                else
                    f.frame += diff;
        }

        void MoveBy(KeyFrame frame, uint diff)
        {
            frame.frame += diff;
            bool found = false;
            foreach (var f in keyFrames)
                if (!found)
                {
                    if (f == frame)
                        found = true;
                }
                else
                    f.frame += diff;
            SortKeyFrames();
        }

        void RemoveKeyFrame(KeyFrame frame)
        {
            keyFrames.Remove(frame);
        }

        int GetIndex(KeyFrame frame) => keyFrames.IndexOf(frame);

        KeyFrameHoverData actualHoverData;

        IEnumerable<PositionAngle> EnumeratePosAngles()
        {
            foreach (var k in keyFrames)
            {
                yield return k;
                yield return k.targetPoint;
            }
        }

        public MapCustomCameraPath()
            : base(null)
        {
            positionAngleProvider = EnumeratePosAngles;
            actualHoverData = new KeyFrameHoverData(this);
        }

        public override IHoverData GetHoverData(MapGraphics graphics, ref Vector3 position)
        {
            MapObjectHoverData sas = (MapObjectHoverData)base.GetHoverData(graphics, ref position);
            if (sas == null)
                return null;
            if (sas.currentPositionAngle is KeyFrame.Target sos)
            {
                actualHoverData.target = true;
                actualHoverData.currentKeyFrame = sos.parent;
            }
            else
            {
                actualHoverData.target = false;
                actualHoverData.currentKeyFrame = (KeyFrame)sas.currentPositionAngle;
            }
            return actualHoverData;
        }

        protected override void DrawTopDown(MapGraphics graphics)
        {
            graphics.drawLayers[(int)MapGraphics.DrawLayers.FillBuffers].Add(() =>
            {
                KeyFrame lastKeyFrame = null;
                foreach (var a in keyFrames)
                {
                    DrawIcon(graphics,
                        graphics.view.mode == MapView.ViewMode.ThreeDimensional,
                        (float)a.X, (float)a.Y, (float)a.Z,
                        Rotates ? (float)a.Angle : 0x8000 - graphics.MapViewAngleValue,
                        GetInternalImage()?.Value,
                        new Vector4(1, 1, 1, actualHoverData.currentKeyFrame == a ? ObjectUtilities.HoverAlpha() : 1));

                    float desiredDiameter = Size * 2;
                    if (graphics.view.mode == MapView.ViewMode.ThreeDimensional)
                        desiredDiameter *= Get3DIconScale(graphics, (float)a.targetPoint.X, (float)a.targetPoint.Y, (float)a.targetPoint.Z);

                    graphics.circleRenderer.AddInstance(
                        graphics.view.mode == MapView.ViewMode.ThreeDimensional,
                        graphics.BillboardMatrix * Matrix4.CreateScale(desiredDiameter) * Matrix4.CreateTranslation(a.targetPoint.position),
                        1,
                        new Vector4(0.5f, 0.5f, 0.5f, 0.5f),
                        new Vector4(0, 0, 0, 1),
                        Renderers.ShapeRenderer.Shapes.Circle);

                    if (lastKeyFrame != null)
                        graphics.lineRenderer.Add(lastKeyFrame.position, a.position, new Vector4(1, 1, 0, 1), 2);
                    graphics.lineRenderer.Add(a.position, a.targetPoint.position, new Vector4(1, 0, 1, 1), 2);
                    lastKeyFrame = a;
                }
            });
        }

        protected override ContextMenuStrip GetContextMenuStrip(MapTracker targetTracker)
        {
            var _contextMenuStrip = base.GetContextMenuStrip(targetTracker);

            var itemAddKeyframe = new ToolStripMenuItem("Add Keyframe");
            itemAddKeyframe.Click += (_, __) =>
            {
                var f = new KeyFrame();
                f.position = targetTracker.mapTab.graphics.view.position;
                f.targetPoint.position = targetTracker.mapTab.graphics.view.position + targetTracker.mapTab.graphics.view.ComputeViewDirection() * 400;
                keyFrames.Add(f);
            };
            _contextMenuStrip.Items.Add(itemAddKeyframe);

            itemPlayback = new ToolStripMenuItem("Playback");
            itemPlayback.Checked = true;
            itemPlayback.Click += (_, __) => itemPlayback.Checked = !itemPlayback.Checked;
            _contextMenuStrip.Items.Add(itemPlayback);

            return _contextMenuStrip;
        }

        public override Lazy<Image> GetInternalImage() => Config.ObjectAssociations.CameraMapImage;

        public override string GetName() => "Custom Camera Path";

        public override void Update()
        {
            base.Update();
            if (playback && keyFrames.Count > 0)
            {
                var globalTimer = Config.Stream.GetUInt32(MiscConfig.GlobalTimerAddress);
                KeyFrame current = null, last = null;
                foreach (var p in keyFrames)
                {
                    if (p.frame > globalTimer)
                    {
                        current = p;
                        break;
                    }
                    last = p;
                }
                Vector3 resultPos = last?.position ?? current.position;
                Vector3 resultTarget = (last?.targetPoint ?? current.targetPoint).position;
                var startFrame = (last?.frame ?? 0) + (last?.waitFor ?? 0);
                if (last != null && current != null && globalTimer > startFrame)
                {
                    float interpolate = (float)(globalTimer - startFrame) / (current.frame - startFrame);
                    resultPos = last.position + (current.position - last.position) * interpolate;
                    resultTarget = last.targetPoint.position + (current.targetPoint.position - last.targetPoint.position) * interpolate;
                }

                Config.Stream.SetValue(3, CamHackConfig.StructAddress + CamHackConfig.CameraModeOffset);
                Config.Stream.SetValue(resultPos.X, CamHackConfig.StructAddress + CamHackConfig.CameraXOffset);
                Config.Stream.SetValue(resultPos.Y, CamHackConfig.StructAddress + CamHackConfig.CameraYOffset);
                Config.Stream.SetValue(resultPos.Z, CamHackConfig.StructAddress + CamHackConfig.CameraZOffset);
                Config.Stream.SetValue(resultTarget.X, CamHackConfig.StructAddress + CamHackConfig.FocusXOffset);
                Config.Stream.SetValue(resultTarget.Y, CamHackConfig.StructAddress + CamHackConfig.FocusYOffset);
                Config.Stream.SetValue(resultTarget.Z, CamHackConfig.StructAddress + CamHackConfig.FocusZOffset);
            }
        }


        public override (SaveSettings save, LoadSettings load) SettingsSaveLoad => (
            (System.Xml.XmlNode node) =>
            {
                base.SettingsSaveLoad.save(node);
                var pathNode = node.OwnerDocument.CreateElement("Path");
                foreach (var frame in keyFrames)
                {
                    var frameNode = node.OwnerDocument.CreateElement("KeyFrame");
                    frameNode.SetAttribute("frame", frame.frame.ToString());
                    frameNode.SetAttribute("waitFor", frame.waitFor.ToString());
                    frameNode.SetAttribute("positionX", frame.position.X.ToString());
                    frameNode.SetAttribute("positionY", frame.position.Y.ToString());
                    frameNode.SetAttribute("positionZ", frame.position.Z.ToString());
                    frameNode.SetAttribute("targetX", frame.targetPoint.position.X.ToString());
                    frameNode.SetAttribute("targetY", frame.targetPoint.position.Y.ToString());
                    frameNode.SetAttribute("targetZ", frame.targetPoint.position.Z.ToString());
                    pathNode.AppendChild(frameNode);
                }
                node.AppendChild(pathNode);
            }
        ,
            (System.Xml.XmlNode node) =>
            {
                base.SettingsSaveLoad.load(node);
                keyFrames.Clear();
                var pathNode = node.SelectSingleNode("Path");
                if (pathNode != null)
                    foreach (System.Xml.XmlNode n in pathNode.SelectNodes("KeyFrame"))
                    {
                        var newFrame = new KeyFrame();
                        Vector3 pos = Vector3.Zero, tar = Vector3.Zero;
                        foreach (System.Xml.XmlAttribute attr in n.Attributes)
                            switch (attr.Name)
                            {
                                case "frame": uint.TryParse(attr.Value, out newFrame.frame); break;
                                case "waitFor": uint.TryParse(attr.Value, out newFrame.waitFor); break;
                                case "positionX": float.TryParse(attr.Value, out pos.X); break;
                                case "positionY": float.TryParse(attr.Value, out pos.Y); break;
                                case "positionZ": float.TryParse(attr.Value, out pos.Z); break;
                                case "targetX": float.TryParse(attr.Value, out tar.X); break;
                                case "targetY": float.TryParse(attr.Value, out tar.Y); break;
                                case "targetZ": float.TryParse(attr.Value, out tar.Z); break;
                            }
                        newFrame.position = pos;
                        newFrame.targetPoint.position = tar;
                        keyFrames.Add(newFrame);
                    }
                SortKeyFrames();
            }
        );
    }
}
