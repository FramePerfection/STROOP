using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using STROOP.Structs;
using STROOP.Utilities;
using STROOP.Tabs.MapTab.MapObjects;

namespace STROOP.Tabs.MapTab
{
    public partial class MapTracker : UserControl
    {
        public delegate MapTracker CreateTracker(ObjectCreateParams creationParameters);

        private static readonly Image ImageEyeOpen = Properties.Resources.image_eye_open2;
        private static readonly Image ImageEyeClosed = Properties.Resources.image_eye_closed2;

        public readonly MapObject mapObject;
        public readonly MapTab mapTab;

        private List<Image> _images;

        private bool _isVisible;
        private bool _showTriUnits;

        private string _customName;

        public bool IsVisible => _isVisible;
        public readonly string creationIdentifier;
        public readonly MapTracker parentTracker;
        List<MapTracker> childTrackers = new List<MapTracker>();
        Dictionary<string, CreateTracker> createChildTrackers = new Dictionary<string, CreateTracker>();

        public delegate void RemovedFromMapEventHandler();

        public event RemovedFromMapEventHandler Killed;
        public Func<bool> ConfirmRemoveFromMap;

        public IEnumerable<MapTracker> EnumerateChildTrackers()
        {
            foreach (var t in childTrackers)
                yield return t;
        }

        public MapTracker(MapTracker parentTracker, string creationIdentifier, MapObject mapObject)
            : this(parentTracker.mapTab, creationIdentifier, mapObject)
        {
            this.parentTracker = parentTracker;
            parentTracker.childTrackers.Add(this);
            parentTracker.Killed += Kill;
        }

        public void Kill()
        {
            if (!(ConfirmRemoveFromMap?.Invoke() ?? true))
                return;
            CleanUp();
            mapTab.RemoveTracker(this);
            Killed?.Invoke();
            Dispose();
        }

        public MapTracker(MapTab mapTab, string creationIdentifier, MapObject mapObject)
        {
            this.mapTab = mapTab;
            this.creationIdentifier = creationIdentifier;

            using (new AccessScope<MapTab>(mapTab))
            {
                InitializeComponent();

                this.mapObject = mapObject;

                _images = new List<Image>();

                _isVisible = true;
                _showTriUnits = false;

                pictureBoxPicture.ContextMenuStrip = new ContextMenuStrip();
                ToolStripMenuItem itemUseTopDownImage = new ToolStripMenuItem("Use Top Down Image");
                ToolStripMenuItem itemUseObjectSlotImage = new ToolStripMenuItem("Use Object Slot Image");
                ToolStripMenuItem itemUseCustomImage = new ToolStripMenuItem("Use Custom Image");
                List<ToolStripMenuItem> pictureBoxItems = new List<ToolStripMenuItem>()
                {
                    itemUseTopDownImage, itemUseObjectSlotImage, itemUseCustomImage
                };
                itemUseTopDownImage.Click += (sender, e) =>
                {
                    this.mapObject.SetIconType(MapTrackerIconType.TopDownImage);
                    pictureBoxItems.ForEach(item => item.Checked = item == itemUseTopDownImage);
                };
                itemUseObjectSlotImage.Click += (sender, e) =>
                {
                    this.mapObject.SetIconType(MapTrackerIconType.ObjectSlotImage);
                    pictureBoxItems.ForEach(item => item.Checked = item == itemUseObjectSlotImage);
                };
                itemUseCustomImage.Click += (sender, e) =>
                {
                    Image image = DialogUtilities.GetImage();
                    Lazy<Image> imageMap = new Lazy<Image>(() => image);
                    if (image == null) return;
                    this.mapObject.SetIconType(MapTrackerIconType.CustomImage, imageMap);
                    pictureBoxItems.ForEach(item => item.Checked = item == itemUseCustomImage);
                };
                itemUseTopDownImage.Checked = true;
                pictureBoxPicture.ContextMenuStrip.Items.Add(itemUseTopDownImage);
                pictureBoxPicture.ContextMenuStrip.Items.Add(itemUseObjectSlotImage);
                pictureBoxPicture.ContextMenuStrip.Items.Add(itemUseCustomImage);

                _customName = null;
                textBoxName.AddEnterAction(() => _customName = textBoxName.Text);
                textBoxName.AddLostFocusAction(() => _customName = textBoxName.Text);
                textBoxName.AddDoubleClickAction(() => textBoxName.SelectAll());
                textBoxName.ContextMenuStrip = new ContextMenuStrip();
                ToolStripMenuItem itemResetCustomName = new ToolStripMenuItem("Reset Custom Name");
                itemResetCustomName.Click += (sender, e) => _customName = null;
                textBoxName.ContextMenuStrip.Items.Add(itemResetCustomName);

                checkBoxRotates.Click += (sender, e) => this.mapObject.CustomRotates = checkBoxRotates.Checked;
                checkBoxRotates.ContextMenuStrip = new ContextMenuStrip();
                ToolStripMenuItem itemResetCustomRotates = new ToolStripMenuItem("Reset Custom Rotates");
                itemResetCustomRotates.Click += (sender, e) => this.mapObject.CustomRotates = null;
                checkBoxRotates.ContextMenuStrip.Items.Add(itemResetCustomRotates);

                tableLayoutPanel.BorderWidth = 2;
                tableLayoutPanel.ShowBorder = true;

                comboBoxVisibilityType.DataSource = Enum.GetValues(typeof(MapTrackerVisibilityType));
                comboBoxVisibilityType.SelectedItem = MapTrackerVisibilityType.VisibleWhenLoaded;

                comboBoxOrderType.DataSource = Enum.GetValues(typeof(MapTrackerOrderType));
                comboBoxOrderType.SelectedItem = MapTrackerOrderType.OrderByY;

                SetSize(null);
                SetOpacity(null);
                SetOutlineWidth(null);
                SetColor(null);
                SetOutlineColor(null);

                mapObject.SizeChanged += () => UpdateSizeControls(mapObject.Size);

                textBoxSize.AddEnterAction(() => textBoxSize_EnterAction());
                trackBarSize.AddManualChangeAction(() => trackBarSize_ValueChanged());
                textBoxOpacity.AddEnterAction(() => textBoxOpacity_EnterAction());
                trackBarOpacity.AddManualChangeAction(() => trackBarOpacity_ValueChanged());
                textBoxOutlineWidth.AddEnterAction(() => textBoxOutlineWidth_EnterAction());
                trackBarOutlineWidth.AddManualChangeAction(() => trackBarOutlineWidth_ValueChanged());
                colorSelector.AddColorChangeAction((Color color) => SetColor(color));
                colorSelectorOutline.AddColorChangeAction((Color color) => SetOutlineColor(color));

                pictureBoxCog.ContextMenuStrip = this.mapObject.BindToTracker(this);
                if (pictureBoxCog.ContextMenuStrip.Items.Count == 0)
                    pictureBoxCog.ContextMenuStrip.Items.Add(
                        new ToolStripMenuItem("No options available") { Enabled = false }
                    );
                pictureBoxCog.Click += (sender, e) => pictureBoxCog.ContextMenuStrip.Show(Cursor.Position);

                MapUtilities.CreateTrackBarContextMenuStrip(trackBarSize);
                MapUtilities.CreateTrackBarContextMenuStrip(trackBarOutlineWidth);
                InitializePlusContextMenuStrip();

                UpdateControl();
            }
        }

        public Action MakeCreateTrackerHandler(MapTab target, string identifier, Func<ObjectCreateParams, MapObject> newObjFunc)
        {
            CreateTracker createTracker = creationParameters => new MapTracker(this, identifier, newObjFunc(creationParameters));
            createChildTrackers[identifier] = createTracker;
            return () => target.AddSubTracker(createTracker(null));
        }

        public void SaveChildTrackers(System.Xml.XmlNode node)
        {
            if (childTrackers.Count == 0)
                return;
            var childTrackersNode = node.OwnerDocument.CreateElement("Trackers");
            foreach (var childTracker in EnumerateChildTrackers())
            {
                var childNode = node.OwnerDocument.CreateElement(childTracker.creationIdentifier);
                childTracker.mapObject.SettingsSaveLoad.save(childNode);
                childTracker.SaveChildTrackers(childNode);
                childTrackersNode.AppendChild(childNode);
            }

            node.AppendChild(childTrackersNode);
        }

        public List<MapTracker> LoadChildTrackers(System.Xml.XmlNode node)
        {
            mapObject.SettingsSaveLoad.load(node);
            var childTrackersNode = node.SelectSingleNode("Trackers");
            if (childTrackersNode == null)
                return new List<MapTracker>();
            var childTrackers = new List<MapTracker>();
            foreach (System.Xml.XmlNode n in childTrackersNode.ChildNodes)
                if (n.NodeType == System.Xml.XmlNodeType.Element
                    && createChildTrackers.TryGetValue(n.Name, out var newTrackerFunc))
                {
                    var t = newTrackerFunc(null);
                    childTrackers.Add(t);
                    childTrackers.AddRange(t.LoadChildTrackers(n));
                }

            return childTrackers;
        }

        private void InitializePlusContextMenuStrip()
        {
            pictureBoxPlus.ContextMenuStrip = new ContextMenuStrip();
            mapObject.InitSubTrackerContextMenuStrip(mapTab, pictureBoxPlus.ContextMenuStrip);
            pictureBoxPlus.Click += (sender, e) => pictureBoxPlus.ContextMenuStrip.Show(Cursor.Position);
        }

        public List<MapObject> GetMapObjectsToDisplay()
        {
            if (!_isVisible || !mapObject.ShouldDisplay((MapTrackerVisibilityType)comboBoxVisibilityType.SelectedItem))
                return new List<MapObject>();
            return new List<MapObject>(new[] { mapObject });
        }

        public MapTrackerOrderType GetOrderType()
        {
            return (MapTrackerOrderType)comboBoxOrderType.SelectedItem;
        }

        private void trackBarSize_ValueChanged()
        {
            SetSize(trackBarSize.Value);
        }

        private void textBoxSize_EnterAction()
        {
            SetSize(ParsingUtilities.ParseFloatNullable(textBoxSize.Text));
        }

        /** null if controls should be refreshed */
        private void SetSize(float? sizeNullable)
        {
            bool updateMapObjs = sizeNullable != null;
            float size = sizeNullable ?? mapObject.Size;
            if (updateMapObjs)
                mapObject.Size = size;
            UpdateSizeControls(size);
        }

        void UpdateSizeControls(float size)
        {
            textBoxSize.SubmitText(size.ToString());
            trackBarSize.StartChangingByCode();
            ControlUtilities.SetTrackBarValueCapped(trackBarSize, size);
            trackBarSize.StopChangingByCode();
        }

        private void trackBarOpacity_ValueChanged()
        {
            SetOpacity(trackBarOpacity.Value);
        }

        private void textBoxOpacity_EnterAction()
        {
            SetOpacity(ParsingUtilities.ParseIntNullable(textBoxOpacity.Text));
        }

        /** null if controls should be refreshed */
        private void SetOpacity(int? opacityNullable)
        {
            bool updateMapObjs = opacityNullable != null;
            int opacity = opacityNullable ?? mapObject.OpacityPercent;
            if (updateMapObjs)
                mapObject.OpacityPercent = opacity;
            textBoxOpacity.SubmitText(opacity.ToString());
            trackBarOpacity.StartChangingByCode();
            ControlUtilities.SetTrackBarValueCapped(trackBarOpacity, opacity);
            trackBarOpacity.StopChangingByCode();
        }

        private void trackBarOutlineWidth_ValueChanged()
        {
            SetOutlineWidth(trackBarOutlineWidth.Value);
        }

        private void textBoxOutlineWidth_EnterAction()
        {
            SetOutlineWidth(ParsingUtilities.ParseFloatNullable(textBoxOutlineWidth.Text));
        }

        /** null if controls should be refreshed */
        private void SetOutlineWidth(float? outlineWidthNullable)
        {
            bool updateMapObjs = outlineWidthNullable != null;
            float outlineWidth = outlineWidthNullable ?? mapObject.OutlineWidth;
            if (updateMapObjs)
                mapObject.OutlineWidth = outlineWidth;
            textBoxOutlineWidth.SubmitText(outlineWidth.ToString());
            trackBarOutlineWidth.StartChangingByCode();
            ControlUtilities.SetTrackBarValueCapped(trackBarOutlineWidth, outlineWidth);
            trackBarOutlineWidth.StopChangingByCode();
        }

        /** null if controls should be refreshed */
        public void SetColor(Color? colorNullable)
        {
            bool updateMapObjs = colorNullable != null;
            Color color = colorNullable ?? mapObject.Color;
            if (updateMapObjs)
                mapObject.Color = color;
            colorSelector.SelectedColor = color;
        }

        /** null if controls should be refreshed */
        public void SetOutlineColor(Color? outlineColorNullable)
        {
            bool updateMapObjs = outlineColorNullable != null;
            Color outlineColor = outlineColorNullable ?? mapObject.OutlineColor;
            if (updateMapObjs)
                mapObject.OutlineColor = outlineColor;
            colorSelectorOutline.SelectedColor = outlineColor;
        }

        private void CheckBoxShowTriUnits_CheckedChanged(object sender, EventArgs e)
        {
            _showTriUnits = checkBoxShowTriUnits.Checked;
            mapObject.ShowTriUnits = _showTriUnits;
        }

        private void pictureBoxRedX_Click(object sender, EventArgs e)
        {
            Kill();
        }

        private void pictureBoxEye_Click(object sender, EventArgs e)
        {
            _isVisible = !_isVisible;
            pictureBoxEye.BackgroundImage = _isVisible ? ImageEyeOpen : ImageEyeClosed;
        }

        private void pictureBoxUpArrow_Click(object sender, EventArgs e)
        {
            int numMoves = GlobalKeyboard.GetCurrentlyInputtedNumber() ?? 1;
            if (GlobalKeyboard.IsCtrlDown()) numMoves = 0;
            this.GetParent<MapTrackerFlowLayoutPanel>().MoveUpControl(this, numMoves);
        }

        private void pictureBoxDownArrow_Click(object sender, EventArgs e)
        {
            int numMoves = GlobalKeyboard.GetCurrentlyInputtedNumber() ?? 1;
            if (GlobalKeyboard.IsCtrlDown()) numMoves = 0;
            this.GetParent<MapTrackerFlowLayoutPanel>().MoveDownControl(this, numMoves);
        }

        public void SetGlobalIconSize(float size)
        {
            if (mapObject.ParticipatesInGlobalIconSize())
                SetSize(size);
        }

        public void UpdateControl()
        {
            mapObject.Update();
            pictureBoxPicture.Image = mapObject.GetImage()?.Value;
            textBoxName.SubmitTextLoosely(_customName ?? string.Join(", ", mapObject.GetName()));
        }

        public void CleanUp()
        {
            mapObject.CleanUp();
            parentTracker?.childTrackers.Remove(this);
        }

        public override string ToString()
        {
            return string.Join(", ", mapObject);
        }
    }
}
