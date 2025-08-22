using OpenTK;
using STROOP.Utilities;
using System;
using System.Windows.Forms;
using System.Collections.Generic;
using OpenTK.Mathematics;

namespace STROOP.Tabs.MapTab.MapObjects
{
    partial class MapObject
    {
        protected abstract class PointHoverData : IHoverData
        {
            protected readonly MapObject parent;
            protected abstract void SetPosition(Vector3 position);
            protected abstract Vector3 GetPosition();
            public abstract void SetLookAt(Vector3 lookAt);

            public PointHoverData(MapObject parent)
            {
                this.parent = parent;
            }

            public virtual void LeftClick(Vector3 position) { }

            public virtual void RightClick(Vector3 position) { }

            public virtual DragMask CanDrag() => parent.dragMask;

            public void DragTo(Vector3 newPosition, bool setY)
            {
                var dragMask = CanDrag();
                var oldPosition = GetPosition();
                if (!dragMask.HasFlag(DragMask.X))
                    newPosition.X = oldPosition.X;
                if (!dragMask.HasFlag(DragMask.Y) || !setY)
                    newPosition.Y = oldPosition.Y;
                if (!dragMask.HasFlag(DragMask.Z))
                    newPosition.Z = oldPosition.Z;
                SetPosition(newPosition);
            }

            public virtual void Pivot(MapTab tab)
            {
                tab.graphics.view.camera3DMode = MapView.Camera3DMode.FocusOnPositionAngle;
                tab.graphics.view.focusPositionAngle = PositionAngle.Custom(GetPosition(), 0);
            }

            public virtual void AddContextMenuItems(MapTab tab, ContextMenuStrip menu)
            {
                var myItem = new ToolStripMenuItem(ToString());
                var copyPositionItem = new ToolStripMenuItem("Copy Position");
                copyPositionItem.Click += (_, __) =>
                {
                    CopyUtilities.CopyPosition(GetPosition());
                };
                myItem.DropDownItems.Add(copyPositionItem);

                var pastePositionItem = new ToolStripMenuItem("Paste Position");
                pastePositionItem.Click += (_, __) =>
                {
                    if (CopyUtilities.TryPastePosition(out Vector3 v))
                        SetPosition(v);
                };
                myItem.DropDownItems.Add(pastePositionItem);

                var typePositionItem = new ToolStripMenuItem("Type in Position");
                typePositionItem.Click += (_, __) =>
                {
                    var pos = GetPosition();
                    var pts = MapUtilities.ParsePoints(DialogUtilities.GetStringFromDialog($"{pos.X}\r\n{pos.Y}\r\n{pos.Z}", "Enter position as 3 floats"), true);
                    if (pts != null)
                        foreach (var pt in pts)
                        {
                            SetPosition(new Vector3(pt.x, pt.y, pt.z));
                            break;
                        }
                };
                myItem.DropDownItems.Add(typePositionItem);

                var makeReferencePointItem = new ToolStripMenuItem("Make Reference Point...");
                HashSet<string> existingNames = new HashSet<string>();
                foreach (var tracker in tab.EnumerateTrackers())
                    if (tracker.mapObject is MapCustomIconPoints customPoints)
                    {
                        existingNames.Add(customPoints.GetName());
                        makeReferencePointItem.DropDownItems.AddHandlerToItem(
                            customPoints.GetName(),
                            () => customPoints.AddPoint(GetPosition())
                            );
                    }
                makeReferencePointItem.DropDownItems.AddHandlerToItem(
                    "New Collection",
                    () =>
                    {
                        var attemptName = "Reference";
                        uint counter = 1;
                        while (existingNames.Contains(attemptName + (counter == 1 ? "" : counter.ToString())))
                            counter++;
                        if (counter > 1)
                            attemptName += counter;
                        var tracker = tab.AddByCode(typeof(MapCustomIconPoints));
                        if (tracker?.mapObject is MapCustomIconPoints customPoints)
                        {
                            customPoints.Clear();
                            customPoints.name = attemptName;
                            customPoints.enableDragging = false;
                            customPoints.AddPoint(GetPosition());
                        }
                    }
                    );
                myItem.DropDownItems.Add(makeReferencePointItem);

                if (tab.graphics.view.mode != MapView.ViewMode.TopDown)
                {
                    var pivotItem = new ToolStripMenuItem("Make Pivot Point");
                    pivotItem.Click += (_, __) => Pivot(tab);
                    myItem.DropDownItems.Add(pivotItem);
                }
                menu.Items.Add(myItem);
            }
        }

        protected class MapObjectHoverData : PointHoverData, IPositionCalculatorProvider
        {
            public PositionAngle currentPositionAngle;
            public MapObjectHoverData(MapObject parent) : base(parent) { }

            protected override void SetPosition(Vector3 position)
            {
                if (currentPositionAngle == null)
                    return;
                currentPositionAngle.SetX(position.X);
                currentPositionAngle.SetY(position.Y);
                currentPositionAngle.SetZ(position.Z);
            }
            protected override Vector3 GetPosition() => currentPositionAngle?.position ?? Vector3.Zero;

            public override void Pivot(MapTab tab)
            {
                if (currentPositionAngle == null)
                    return;
                tab.graphics.view.Pivot(currentPositionAngle);
            }

            public override string ToString() => currentPositionAngle.ToString();

            IEnumerable<(string name, Func<Vector3> func)> IPositionCalculatorProvider.GetPositionCalculators()
            {
                var capture = currentPositionAngle;
                yield return (capture.GetMapName(), () => capture.position);
            }

            public override void SetLookAt(Vector3 lookAt)
            {
                var radians = -Math.Atan2(lookAt.Z - currentPositionAngle.position.Z, lookAt.X - currentPositionAngle.position.X) + Math.PI / 2;
                currentPositionAngle.SetAngle(MoreMath.RadiansToAngleUnits(radians));
            }

            public override void AddContextMenuItems(MapTab tab, ContextMenuStrip menu)
            {
                base.AddContextMenuItems(tab, menu);
                if (currentPositionAngle is PositionAngle.IHoldsObjectAddress addressHolder)
                {
                    var address = addressHolder.GetAddress();
                    var item = new ToolStripMenuItem("Open in Object tab");
                    item.Click += (_, __) =>
                    {
                        var mainForm = AccessScope<StroopMainForm>.content;
                        var objectTab = mainForm.GetTab<ObjectTab>();
                        var knopf = objectTab.selection;
                        knopf.Clear();
                        knopf.Add(address);
                        mainForm.SwitchTab(objectTab);
                    };
                    menu.Items.Add(item);
                }
            }
        }
    }
}
