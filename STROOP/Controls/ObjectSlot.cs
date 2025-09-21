using STROOP.Core;
using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Drawing;
using STROOP.Managers;
using STROOP.Utilities;
using STROOP.Structs;
using STROOP.Extensions;
using System.Drawing.Drawing2D;
using STROOP.Structs.Configurations;
using STROOP.Models;
using static STROOP.Managers.ObjectSlotsManager;
using System.Windows.Input;
using System.Xml.Linq;

namespace STROOP
{
    public class ObjectSlot : Panel
    {
        public class Overlay
        {
            static Dictionary<string, Image> nameToImage = new Dictionary<string, Image>();

            public delegate bool OverlayExpression(ObjectSlot slot);

            static readonly Overlay[] overlays;

            static Overlay()
            {
                OverlayExpression GetAddressExpression(Func<ObjectSlot, uint, bool> addressExpression) => obj =>
                {
                    if (Config.Stream == null)
                        return false;

                    uint? address = obj.CurrentObject?.Address ?? null;
                    if (address.HasValue) return addressExpression(obj, address.Value);
                    return false;
                };

                //Initialize overlay expressions
                var lst = new List<Overlay>();

                lst.Add(new Overlay("StoodOn", GetAddressExpression((obj, address) =>
                    OverlayConfig.ShowOverlayStoodOnObject && address == DataModels.Mario.StoodOnObject)));

                lst.Add(new Overlay("Ridden", GetAddressExpression((obj, address) =>
                    OverlayConfig.ShowOverlayRiddenObject && address == DataModels.Mario.RiddenObject)));

                lst.Add(new Overlay("Interaction", GetAddressExpression((obj, address) =>
                    OverlayConfig.ShowOverlayInteractionObject && address == DataModels.Mario.InteractionObject)));

                lst.Add(new Overlay("Held", GetAddressExpression((obj, address) =>
                    OverlayConfig.ShowOverlayHeldObject && address == DataModels.Mario.HeldObject)));

                lst.Add(new Overlay("Used", GetAddressExpression((obj, address) =>
                    OverlayConfig.ShowOverlayUsedObject && address == DataModels.Mario.UsedObject)));

                lst.Add(new Overlay("Closest", GetAddressExpression((obj, address) =>
                    OverlayConfig.ShowOverlayClosestObject && address == DataModels.Mario.ClosestObject)));

                lst.Add(new Overlay("Camera", GetAddressExpression((obj, address) =>
                    OverlayConfig.ShowOverlayCameraObject && address == DataModels.Camera.SecondaryObject)));

                lst.Add(new Overlay("CameraHack", GetAddressExpression((obj, address) =>
                    OverlayConfig.ShowOverlayCameraHackObject && address == DataModels.Camera.HackObject)));

                lst.Add(new Overlay("Model", GetAddressExpression((obj, address) =>
                    address == AccessScope<StroopMainForm>.content.GetTab<Tabs.ModelTab>().ModelObjectAddress)));

                lst.Add(new Overlay("Wall", GetAddressExpression((obj, address) =>
                    OverlayConfig.ShowOverlayWallObject && address == DataModels.Mario.WallTriangle?.AssociatedObject)));

                lst.Add(new Overlay("Floor", GetAddressExpression((obj, address) =>
                    OverlayConfig.ShowOverlayFloorObject && address == DataModels.Mario.FloorTriangle?.AssociatedObject)));

                lst.Add(new Overlay("Ceiling", GetAddressExpression((obj, address) =>
                    OverlayConfig.ShowOverlayCeilingObject && address == DataModels.Mario.CeilingTriangle?.AssociatedObject)));


                OverlayExpression GetHoveredExpression(Func<ObjectSlot, uint, ObjectDataModel, bool> hoverExpression) => obj =>
                {
                    uint? hoveredAddress = Config.ObjectSlotsManager.HoveredObjectAddress;
                    uint? address = obj.CurrentObject?.Address ?? null;
                    if (address.HasValue && hoveredAddress.HasValue) return hoverExpression(obj, address.Value, new ObjectDataModel(hoveredAddress.Value));
                    return false;
                };

                lst.Add(new Overlay("Parent", GetHoveredExpression((obj, address, hoveredObject) =>
                    (OverlayConfig.ShowOverlayParentObject || GlobalKeyboard.IsDown(Keys.P)) && address == hoveredObject.Parent)));

                lst.Add(new Overlay("ParentNone", GetHoveredExpression((obj, address, hoveredObject) =>
                    (OverlayConfig.ShowOverlayParentObject || GlobalKeyboard.IsDown(Keys.P)) && address == hoveredObject.Address
                                                                                             && hoveredObject.Parent == 0)));

                lst.Add(new Overlay("ParentUnused", GetHoveredExpression((obj, address, hoveredObject) =>
                    (OverlayConfig.ShowOverlayParentObject || GlobalKeyboard.IsDown(Keys.P)) && address == hoveredObject.Address
                                                                                             && hoveredObject.Parent == ObjectSlotsConfig.UnusedSlotAddress)));

                lst.Add(new Overlay("Child", GetHoveredExpression((obj, address, hoveredObject) =>
                    (OverlayConfig.ShowOverlayChildObject || GlobalKeyboard.IsDown(Keys.P)) && obj.CurrentObject?.Parent == hoveredObject.Address)));

                for (int i = 1; i <= 4; i++)
                {
                    int capture = i;
                    lst.Add(new Overlay($"Collision{capture}", GetAddressExpression((obj, address) =>
                    {
                        uint? hoveredAddress = Config.ObjectSlotsManager.HoveredObjectAddress;
                        uint collisionObjAddress = hoveredAddress.HasValue && GlobalKeyboard.IsDown(Keys.C)
                            ? hoveredAddress.Value
                            : Config.Stream.GetUInt32(MarioObjectConfig.PointerAddress);
                        return OverlayConfig.ShowOverlayCollisionObject && address == ObjectUtilities.GetCollisionObject(collisionObjAddress, capture);
                    })));
                }

                lst.Add(new Overlay("Locked", GetAddressExpression((obj, address) =>
                    !LockConfig.LockingDisabled && WatchVariableLockManager.ContainsAnyLocksForObject(address))));

                lst.Add(new Overlay("LockDisabled", GetAddressExpression((obj, address) =>
                    LockConfig.LockingDisabled && WatchVariableLockManager.ContainsAnyLocksForObject(address))));

                //TODO: Figure out what "LockReadOnly" is supposed to be
                Func<ObjectSlot, uint, bool> shownOnMap = (obj, address) =>
                    AccessScope<StroopMainForm>.content.GetTab<Tabs.MapTab.MapTab>().TracksObject(address);

                Func<ObjectSlot, uint, bool> shownOnModel = (obj, address) =>
                    AccessScope<StroopMainForm>.content.GetTab<Tabs.ModelTab>().ShowsObject(address);

                lst.Add(new Overlay("TrackedAndShown", GetAddressExpression(shownOnMap)));
                lst.Add(new Overlay("Model", GetAddressExpression(shownOnModel)));
                lst.Add(new Overlay("Selected", GetAddressExpression((obj, address) =>
                    !shownOnMap(obj, address) && !shownOnModel(obj, address) && obj._manager.SelectedSlotsAddresses.Contains(address))));

                overlays = lst.ToArray();
            }

            public static Dictionary<Overlay, Wrapper<bool>> NewObjectSlot()
            {
                var result = new Dictionary<Overlay, Wrapper<bool>>();
                foreach (var it in overlays)
                    result[it] = new Wrapper<bool>(false);
                return result;
            }

            public static void ParseXElement(string basePath, XElement element)
            {
                if (element.Name.ToString() == "OverlayImage")
                {
                    var name = element.Attribute(XName.Get("name")).Value;
                    var path = element.Attribute(XName.Get("path")).Value;
                    nameToImage[name] = Image.FromFile(basePath + path);
                }
            }

            public readonly OverlayExpression expression;
            public readonly string name;

            private Overlay(string xElementName, OverlayExpression expression)
            {
                this.name = xElementName;
                this.expression = expression;
            }

            public Image GetImage() => nameToImage[name];
        }

        const int BorderSize = 2;

        const float markedPenRelativeWidth = 0.16f;

        static Pen MakeMarkedPen(Color color)
        {
            var p = new Pen(color, 1);
            p.Alignment = PenAlignment.Inset;
            return p;
        }

        static Pen[] markPens = new[]
        {
            MakeMarkedPen(Color.Red),
            MakeMarkedPen(Color.Orange),
            MakeMarkedPen(Color.Yellow),
            MakeMarkedPen(Color.Green),
            MakeMarkedPen(Color.LightBlue),
            MakeMarkedPen(Color.Blue),
            MakeMarkedPen(Color.Purple),
            MakeMarkedPen(Color.Pink),
            MakeMarkedPen(Color.Gray),
            MakeMarkedPen(Color.White),
            MakeMarkedPen(Color.Black),
        };

        ObjectSlotsManager _manager;

        public int Index { get; private set; }
        public ObjectDataModel CurrentObject { get; set; }

        #region Drawing Variables

        Color _mainColor, _borderColor, _backColor;
        SolidBrush _borderBrush = new SolidBrush(Color.White), _backBrush = new SolidBrush(Color.White);
        SolidBrush _textBrush = new SolidBrush(Color.Black);
        Image _objectImage;
        Point _textLocation = new Point();
        string _text;

        #endregion

        public new bool Show = false;

        object _gfxLock = new object();

        public enum MouseStateType
        {
            None,
            Over,
            Down
        };

        private MouseStateType _mouseState;
        private MouseStateType _mouseEnteredState;

        private BehaviorCriteria _behavior;
        public BehaviorCriteria Behavior => _behavior;

        bool _isActive = false;

        public override string Text => _text;

        Color _textColor
        {
            get => _textBrush.Color;
            set
            {
                lock (_gfxLock)
                {
                    _textBrush.Color = value;
                }
            }
        }

        Dictionary<Overlay, Wrapper<bool>> overlayValues = Overlay.NewObjectSlot();
        int? markValue = null;

        public ObjectSlot(ObjectSlotsManager manager, int index, Size size)
        {
            _manager = manager;
            Size = size;
            Index = index;
            Font = new Font(FontFamily.GenericSansSerif, 6);

            this.MouseDown += OnDrag;
            this.MouseUp += (s, e) =>
            {
                _mouseState = _mouseEnteredState;
                UpdateColors();
            };
            this.MouseEnter += (s, e) =>
            {
                _manager.HoveredObjectAddress = CurrentObject?.Address;
                _mouseEnteredState = MouseStateType.Over;
                _mouseState = MouseStateType.Over;
                UpdateColors();
            };
            this.MouseLeave += (s, e) =>
            {
                _manager.HoveredObjectAddress = null;
                _mouseEnteredState = MouseStateType.None;
                _mouseState = MouseStateType.None;
                UpdateColors();
            };
            this.Cursor = System.Windows.Forms.Cursors.Hand;
            this.DoubleBuffered = true;

            SetUpContextMenuStrip();
        }

        private void SetUpContextMenuStrip()
        {
            ContextMenuStrip = new ContextMenuStrip();
            foreach (var tab_it in _manager.mainForm.EnumerateTabs())
                if (tab_it.objectSlotsClicked != null)
                {
                    var tab = tab_it;
                    var selectInTabItem = new ToolStripMenuItem($"Select in {tab.GetDisplayName()} Tab");
                    selectInTabItem.Click += (sender, e) =>
                    {
                        _manager.mainForm.SwitchTab(tab);
                        tab.UpdateOrInitialize(true);
                        tab.selection?.Clear();
                        tab.selection?.Add(CurrentObject.Address);
                        tab.objectSlotsClicked(new[] { this });
                    };
                    ContextMenuStrip.Items.Add(selectInTabItem);
                }

            Func<List<ObjectDataModel>> getObjects = () => GlobalKeyboard.IsCtrlDown()
                ? Config.ObjectSlotsManager.SelectedObjects
                : new List<ObjectDataModel>() { CurrentObject };

            ToolStripMenuItem itemGoto = new ToolStripMenuItem("Go to");
            itemGoto.Click += (sender, e) => ButtonUtilities.GotoObjects(getObjects());

            ToolStripMenuItem itemRetrieve = new ToolStripMenuItem("Retrieve");
            itemRetrieve.Click += (sender, e) => ButtonUtilities.RetrieveObjects(getObjects());

            ToolStripMenuItem itemGotoHome = new ToolStripMenuItem("Go to Home");
            itemGotoHome.Click += (sender, e) => ButtonUtilities.GotoObjectsHome(getObjects());

            ToolStripMenuItem itemRetrieveHome = new ToolStripMenuItem("Retrieve Home");
            itemRetrieveHome.Click += (sender, e) => ButtonUtilities.RetrieveObjectsHome(getObjects());

            ToolStripMenuItem itemRelease = new ToolStripMenuItem("Release");
            itemRelease.Click += (sender, e) => ButtonUtilities.ReleaseObject(getObjects());

            ToolStripMenuItem itemUnRelease = new ToolStripMenuItem("UnRelease");
            itemUnRelease.Click += (sender, e) => ButtonUtilities.UnReleaseObject(getObjects());

            ToolStripMenuItem itemInteract = new ToolStripMenuItem("Interact");
            itemInteract.Click += (sender, e) => ButtonUtilities.ReleaseObject(getObjects());

            ToolStripMenuItem itemUnInteract = new ToolStripMenuItem("UnInteract");
            itemUnInteract.Click += (sender, e) => ButtonUtilities.UnInteractObject(getObjects());

            ToolStripMenuItem itemClone = new ToolStripMenuItem("Clone");
            itemClone.Click += (sender, e) => ButtonUtilities.CloneObject(CurrentObject);

            ToolStripMenuItem itemUnClone = new ToolStripMenuItem("UnClone");
            itemUnClone.Click += (sender, e) => ButtonUtilities.UnCloneObject();

            ToolStripMenuItem itemUnload = new ToolStripMenuItem("Unload");
            itemUnload.Click += (sender, e) => ButtonUtilities.UnloadObject(getObjects());

            ToolStripMenuItem itemRevive = new ToolStripMenuItem("Revive");
            itemRevive.Click += (sender, e) => ButtonUtilities.ReviveObject(getObjects());

            ToolStripMenuItem itemRide = new ToolStripMenuItem("Ride");
            itemRide.Click += (sender, e) => ButtonUtilities.RideObject(CurrentObject);

            ToolStripMenuItem itemUnRide = new ToolStripMenuItem("UnRide");
            itemUnRide.Click += (sender, e) => ButtonUtilities.UnRideObject();

            ToolStripMenuItem itemUkikipedia = new ToolStripMenuItem("Ukikipedia");
            itemUkikipedia.Click += (sender, e) => ButtonUtilities.UkikipediaObject(CurrentObject);

            ToolStripMenuItem itemMark = new ToolStripMenuItem("Mark");
            itemMark.Click += (sender, e) =>
            {
                List<uint> addresses = getObjects().ConvertAll(obj => obj.Address);
                Config.ObjectSlotsManager.MarkAddresses(addresses, 10);
            };

            ToolStripMenuItem itemUnmark = new ToolStripMenuItem("Unmark");
            itemUnmark.Click += (sender, e) =>
            {
                List<uint> addresses = getObjects().ConvertAll(obj => obj.Address);
                Config.ObjectSlotsManager.UnmarkAddresses(addresses);
            };

            ToolStripMenuItem itemCopyAddress = new ToolStripMenuItem("Copy Address");
            itemCopyAddress.Click += (sender, e) => { Clipboard.SetText(string.Join(",", getObjects().ConvertAll(obj => HexUtilities.FormatValue(obj.Address)))); };

            ToolStripMenuItem itemCopyPosition = new ToolStripMenuItem("Copy Position");
            itemCopyPosition.Click += (sender, e) => Clipboard.SetText(
                String.Format("{0},{1},{2}", CurrentObject.X, CurrentObject.Y, CurrentObject.Z));

            ToolStripMenuItem itemPastePosition = new ToolStripMenuItem("Paste Position");
            itemPastePosition.Click += (sender, e) =>
            {
                List<string> stringList = ParsingUtilities.ParseStringList(Clipboard.GetText());
                int count = stringList.Count;
                if (count != 2 && count != 3) return;
                getObjects().ForEach(obj =>
                {
                    if (obj == null) return;

                    List<float?> floatList = stringList.ConvertAll(s => ParsingUtilities.ParseFloatNullable(s));
                    using (Config.Stream.Suspend())
                    {
                        if (count == 2)
                        {
                            if (floatList[0].HasValue) obj.SetX(floatList[0].Value);
                            if (floatList[1].HasValue) obj.SetZ(floatList[1].Value);
                        }
                        else
                        {
                            if (floatList[0].HasValue) obj.SetX(floatList[0].Value);
                            if (floatList[1].HasValue) obj.SetY(floatList[1].Value);
                            if (floatList[2].HasValue) obj.SetZ(floatList[2].Value);
                        }
                    }
                });
            };

            ToolStripMenuItem itemCopyGraphics = new ToolStripMenuItem("Copy Graphics");
            itemCopyGraphics.Click += (sender, e) => Clipboard.SetText(HexUtilities.FormatValue(CurrentObject.GraphicsID));

            ToolStripMenuItem itemPasteGraphics = new ToolStripMenuItem("Paste Graphics");
            itemPasteGraphics.Click += (sender, e) =>
            {
                uint? address = ParsingUtilities.ParseHexNullable(Clipboard.GetText());
                if (!address.HasValue) return;
                getObjects().ForEach(obj => { obj.GraphicsID = address.Value; });
            };

            ToolStripMenuItem itemCopyObject = new ToolStripMenuItem("Copy Object");
            itemCopyObject.Click += (sender, e) => { ObjectSnapshot.StoredObjectSnapshotList = getObjects().ConvertAll(obj => new ObjectSnapshot(obj.Address)); };

            ToolStripMenuItem itemPasteObject = new ToolStripMenuItem("Paste Object");
            itemPasteObject.Click += (sender, e) =>
            {
                if (ObjectSnapshot.StoredObjectSnapshotList.Count == 0) return;
                List<ObjectDataModel> objects = getObjects();
                for (int i = 0; i < objects.Count; i++)
                {
                    ObjectDataModel obj = objects[i];
                    ObjectSnapshot snapshot = ObjectSnapshot.StoredObjectSnapshotList[i % ObjectSnapshot.StoredObjectSnapshotList.Count];
                    snapshot.Apply(obj.Address, false);
                }
            };

            ContextMenuStrip.Items.Add(new ToolStripSeparator());
            ContextMenuStrip.Items.Add(itemGoto);
            ContextMenuStrip.Items.Add(itemRetrieve);
            ContextMenuStrip.Items.Add(itemGotoHome);
            ContextMenuStrip.Items.Add(itemRetrieveHome);
            ContextMenuStrip.Items.Add(new ToolStripSeparator());
            ContextMenuStrip.Items.Add(itemRelease);
            ContextMenuStrip.Items.Add(itemUnRelease);
            ContextMenuStrip.Items.Add(itemInteract);
            ContextMenuStrip.Items.Add(itemUnInteract);
            ContextMenuStrip.Items.Add(new ToolStripSeparator());
            ContextMenuStrip.Items.Add(itemClone);
            ContextMenuStrip.Items.Add(itemUnClone);
            ContextMenuStrip.Items.Add(itemUnload);
            ContextMenuStrip.Items.Add(itemRevive);
            ContextMenuStrip.Items.Add(itemRide);
            ContextMenuStrip.Items.Add(itemUnRide);
            ContextMenuStrip.Items.Add(itemUkikipedia);
            ContextMenuStrip.Items.Add(new ToolStripSeparator());
            ContextMenuStrip.Items.Add(itemMark);
            ContextMenuStrip.Items.Add(itemUnmark);
            ContextMenuStrip.Items.Add(new ToolStripSeparator());
            ContextMenuStrip.Items.Add(itemCopyAddress);
            ContextMenuStrip.Items.Add(itemCopyPosition);
            ContextMenuStrip.Items.Add(itemPastePosition);
            ContextMenuStrip.Items.Add(itemCopyGraphics);
            ContextMenuStrip.Items.Add(itemPasteGraphics);
            ContextMenuStrip.Items.Add(itemCopyObject);
            ContextMenuStrip.Items.Add(itemPasteObject);
        }

        public bool UpdateColors()
        {
            var oldBorderColor = _borderColor;
            var oldBackColor = _backColor;
            bool imageUpdated = false;
            var newColor = _mainColor;
            switch (_mouseState)
            {
                case MouseStateType.Down:
                    _borderColor = newColor.Darken(0.5);
                    _backColor = newColor.Darken(0.5).Lighten(0.5);
                    break;
                case MouseStateType.Over:
                    _borderColor = newColor.Lighten(0.5);
                    _backColor = newColor.Lighten(0.85);
                    break;
                default:
                    _borderColor = newColor;
                    _backColor = newColor.Lighten(0.7);
                    break;
            }

            Image newImage = Config.ObjectAssociations.GetObjectImage(_behavior, !_isActive)?.Value ?? Config.ObjectAssociations.DefaultImage.Value;
            if (_objectImage != newImage)
            {
                lock (_gfxLock)
                    _objectImage = newImage;
                imageUpdated = true;
            }

            bool colorUpdated = false;
            colorUpdated |= (_backColor != oldBackColor);
            colorUpdated |= (_borderColor != oldBorderColor);

            if (colorUpdated)
            {
                lock (_gfxLock)
                {
                    _borderBrush.Color = _borderColor;
                    _backBrush.Color = _backColor;
                }
            }


            if (!imageUpdated && !colorUpdated)
                return false;

            Invalidate();
            return true;
        }

        private void OnDrag(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left) return;
            _mouseState = MouseStateType.Down;
            UpdateColors();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            e.Graphics.InterpolationMode = InterpolationMode.NearestNeighbor;
            e.Graphics.CompositingQuality = CompositingQuality.HighSpeed;
            lock (_gfxLock)
            {
                // Border
                e.Graphics.FillRectangle(_borderBrush, new Rectangle(new Point(), Size));

                // Background
                e.Graphics.FillRectangle(_backBrush, new Rectangle(BorderSize, BorderSize, Width - BorderSize * 2, Height - BorderSize * 2));

                // Draw Text
                e.Graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SingleBitPerPixel;
                var textLocation = new Point(Width + 1, Height - BorderSize - (int)_manager._fontHeight + 1);
                TextRenderer.DrawText(e.Graphics, _text, _manager.Font, textLocation, _textColor, TextFormatFlags.HorizontalCenter | TextFormatFlags.Top);
                if (textLocation != _textLocation)
                    _textLocation = textLocation;

                // Draw Object Image
                if (_objectImage != null)
                {
                    var objectImageRec = (new Rectangle(BorderSize, BorderSize + 1,
                            Width - BorderSize * 2, _textLocation.Y - 1 - BorderSize))
                        .Zoom(_objectImage.Size);
                    e.Graphics.DrawImage(_objectImage, objectImageRec);
                }
            }

            // Draw Overlays
            if (markValue.HasValue)
            {
                var pen = markPens[markValue.Value - 1];
                pen.Width = markedPenRelativeWidth * Size.Width;
                e.Graphics.DrawRectangle(pen, new Rectangle(new Point(), Size));
            }

            foreach (var overlay in overlayValues)
                if (overlay.Value.value)
                    e.Graphics.DrawImage(overlay.Key.GetImage(), new Rectangle(new Point(), Size));
        }

        public void Update(ObjectDataModel obj)
        {
            CurrentObject = obj;

            uint? address = CurrentObject?.Address;
            bool redraw = false;

            // Update Overlays
            foreach (var overlay in overlayValues)
            {
                var newValue = overlay.Key.expression(this);
                if (newValue != overlay.Value.value)
                    redraw = true;
                overlay.Value.value = newValue;
            }

            var newMarkValue = address.HasValue && _manager.MarkedSlotsAddressesDictionary.ContainsKey(address.Value)
                ? _manager.MarkedSlotsAddressesDictionary[address.Value]
                : (int?)null;

            if (newMarkValue != markValue)
            {
                redraw = true;
                markValue = newMarkValue;
            }

            Color mainColor =
                (SlotLabelType)AccessScope<StroopMainForm>.content.comboBoxLabelMethod.SelectedItem == SlotLabelType.RngUsage ? ObjectRngUtilities.GetColor(CurrentObject) : ObjectSlotsConfig.GetProcessingGroupColor(CurrentObject?.CurrentProcessGroup);
            Color textColor = _manager.LabelsLocked ? Color.Blue : Color.Black;
            string text = CurrentObject != null ? _manager.SlotLabelsForObjects[CurrentObject] : "";

            // Update UI element

            bool updateColors = false;

            if (text != _text)
            {
                _text = text;
                redraw = true;
            }

            if (textColor != _textColor)
            {
                _textColor = textColor;
                redraw = true;
            }

            if (mainColor != _mainColor)
            {
                _mainColor = mainColor;
                updateColors = true;
            }

            if (_behavior != (CurrentObject?.BehaviorCriteria ?? default(BehaviorCriteria)))
            {
                _behavior = CurrentObject?.BehaviorCriteria ?? default(BehaviorCriteria);
                updateColors = true;
            }

            if (_isActive != (CurrentObject?.IsActive ?? false))
            {
                _isActive = CurrentObject?.IsActive ?? false;
                updateColors = true;
            }

            if (updateColors)
            {
                if (UpdateColors())
                    redraw = false; // UpdateColors already calls refresh
            }

            if (redraw)
                Invalidate();
        }

        public override string ToString()
        {
            string objectString = CurrentObject?.ToString() ?? "(no object)";
            return objectString + " " + _text;
        }
    }
}
