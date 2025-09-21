using System;
using System.Collections.Generic;
using System.Drawing;
using STROOP.Utilities;
using STROOP.Structs;
using OpenTK;
using System.Windows.Forms;
using OpenTK.Graphics;
using System.Globalization;
using OpenTK.Mathematics;

namespace STROOP.Tabs.MapTab.MapObjects
{
    public class ObjectCreateParams
    {
        List<(string key, string value)> xmlEntries = new List<(string, string)>();

        public ObjectCreateParams()
        {
        }

        public ObjectCreateParams(System.Xml.XmlNode node)
        {
            foreach (System.Xml.XmlNode n in node.SelectNodes("*"))
                xmlEntries.Add((n.Name, n.InnerText));
        }

        public System.Xml.XmlNode CreateDocumentNode(string name, System.Xml.XmlDocument doc)
        {
            var node = doc.CreateElement(name);
            foreach (var f in xmlEntries)
            {
                var innerNode = doc.CreateElement(f.key);
                innerNode.InnerText = f.value;
                node.AppendChild(innerNode);
            }

            return node;
        }

        public void AddValue(string key, string value) => xmlEntries.Add((key, value));

        public string GetValue(string key, string defaultValue = "")
        {
            foreach (var a in xmlEntries)
                if (a.key == key)
                    return a.value;
            return defaultValue;
        }

        public IEnumerable<string> GetValues(string key)
        {
            foreach (var a in xmlEntries)
                if (a.key == key)
                    yield return a.value;
        }

        public static List<(float x, float y, float z)> GetCustomPoints(ref ObjectCreateParams creationParameters, string nodeName)
        {
            List<(float x, float y, float z)> points = null;
            if (creationParameters != null)
                points = ParsingUtilities.ParsePointList(creationParameters.GetValue(nodeName));

            if (points == null)
            {
                (string, bool)? result = DialogUtilities.GetStringAndSideFromDialog(
                    labelText: "Enter points as pairs or triplets of floats.",
                    button1Text: "Pairs",
                    button2Text: "Triplets");
                if (!result.HasValue) return null;
                (string text, bool useTriplets) = result.Value;
                points = MapUtilities.ParsePoints(text, useTriplets);
                if (creationParameters == null)
                    creationParameters = new ObjectCreateParams();
                creationParameters.AddValue(nodeName, ParsingUtilities.CreatePointList(points));
            }

            return points;
        }

        public static string GetString(ref ObjectCreateParams creationParameters, string nodeName, string dialogPrompt)
        {
            string result = null;
            if (creationParameters != null)
                result = creationParameters.GetValue(nodeName, null);
            if (result == null)
            {
                if (creationParameters == null)
                    creationParameters = new ObjectCreateParams();
                result = DialogUtilities.GetStringFromDialog(labelText: dialogPrompt);
                creationParameters.AddValue(nodeName, result);
            }

            return result;
        }
    }

    public abstract partial class MapObject
    {
        public readonly ObjectCreateParams creationParameters;

        protected virtual bool hasDragPoints => true;

        ToolStripMenuItem itemEnableDragging = new ToolStripMenuItem("Enable dragging");
        ToolStripMenuItem itemEnableDraggingX = new ToolStripMenuItem("Enable X");
        ToolStripMenuItem itemEnableDraggingY = new ToolStripMenuItem("Enable Y");
        ToolStripMenuItem itemEnableDraggingZ = new ToolStripMenuItem("Enable Z");
        ToolStripMenuItem itemEnableDraggingAngle = new ToolStripMenuItem("Enable Angle (hold ctrl)");

        public bool enableDragging
        {
            get => itemEnableDragging.Checked;
            set => itemEnableDragging.Checked
                = itemEnableDraggingX.Checked
                    = itemEnableDraggingY.Checked
                        = itemEnableDraggingZ.Checked
                            = itemEnableDraggingAngle.Checked = value;
        }

        public DragMask dragMask =>
            (itemEnableDraggingX.Checked ? DragMask.X : 0)
            | (itemEnableDraggingY.Checked ? DragMask.Y : 0)
            | (itemEnableDraggingZ.Checked ? DragMask.Z : 0)
            | (itemEnableDraggingAngle.Checked ? DragMask.Angle : 0);

        public virtual IHoverData GetHoverData(MapGraphics graphics, ref Vector3 position) => null;

        public MapTab currentMapTab => AccessScope<MapTab>.content;
        protected event Action OnCleanup = null;

        public delegate IEnumerable<PositionAngle> PositionAngleProvider();

        public static PositionAngleProvider NoPositionAngles = () => new PositionAngle[0];

        public PositionAngleProvider positionAngleProvider { get; protected set; } = NoPositionAngles;

        public event Action SizeChanged;
        float _Size = 25;

        public float Size
        {
            get { return _Size; }
            set
            {
                _Size = value;
                SizeChanged?.Invoke();
            }
        }

        public double Opacity = 1;

        public byte OpacityByte
        {
            get => (byte)(Opacity * 255);
            set => Opacity = value / 255f;
        }

        public int OpacityPercent
        {
            get => (int)(Opacity * 100);
            set => Opacity = value / 100.0;
        }

        public float OutlineWidth = 1;
        public Color Color = SystemColors.Control;
        public Color4 Color4 => new Color4(Color.R, Color.G, Color.B, OpacityByte);
        public Color OutlineColor = Color.Black;

        public bool? CustomRotates = null;
        public bool InternalRotates = false;

        public bool Rotates
        {
            get => CustomRotates ?? InternalRotates;
        }

        public bool ShowTriUnits = false;

        public MapObject()
        {
        }

        protected MapObject(ObjectCreateParams creationParameters)
        {
            this.creationParameters = creationParameters;
        }

        public static float Get3DIconScale(MapGraphics graphics, float x, float y, float z) => (0.5f * (float)Math.Tan(1) * (new Vector3(x, y, z) - graphics.view.position).Length) / graphics.glControl.Height;

        public void DrawIcon(
            MapGraphics graphics,
            bool sortTransparent,
            float x, float y, float z, float angle,
            Image image,
            Vector4 color) =>
            DrawIcon(
                graphics,
                sortTransparent,
                x, y, z, graphics.view.mode != MapView.ViewMode.TopDown ? 0x8000 : angle,
                Size,
                image,
                color);

        public static void DrawIcon(
            MapGraphics graphics,
            bool sortTransparent,
            float x, float y, float z, float angle,
            float size,
            Image image,
            Vector4 color)
        {
            if (image == null)
                return;
            float desiredDiameter = size * 2;
            if (graphics.view.mode == MapView.ViewMode.ThreeDimensional)
                desiredDiameter *= Get3DIconScale(graphics, x, y, z);
            else if (!graphics.MapViewScaleIconSizes)
                desiredDiameter /= graphics.MapViewScaleValue;
            float scale = Math.Max(image.Height / desiredDiameter, image.Width / desiredDiameter);
            float dsads = (float)MoreMath.AngleUnitsToRadians(angle);
            Matrix4 transform = Matrix4.CreateScale(image.Width / scale, image.Height / scale, 1)
                                * Matrix4.CreateRotationZ(-dsads)
                                * graphics.BillboardMatrix
                                * Matrix4.CreateTranslation(x, y, z);

            var textureIndex = graphics.rendererCollection.GetObjectTextureLayer(image);
            if (sortTransparent)
                graphics.objectRenderer.AddTransparentInstance(transform, textureIndex, color);
            else
                graphics.objectRenderer.AddInstance(transform, textureIndex, color);
        }

        public void Draw(MapGraphics graphics)
        {
            switch (graphics.view.mode)
            {
                case MapView.ViewMode.TopDown:
                    DrawTopDown(graphics);
                    break;
                case MapView.ViewMode.Orthogonal:
                    DrawOrthogonal(graphics);
                    break;
                case MapView.ViewMode.ThreeDimensional:
                    Draw3D(graphics);
                    break;
            }
        }

        protected abstract void DrawTopDown(MapGraphics graphics);
        protected abstract void DrawOrthogonal(MapGraphics graphics);
        protected virtual void Draw3D(MapGraphics graphics) => DrawOrthogonal(graphics);


        public abstract string GetName();

        protected Lazy<Image> _customImage = null;
        public abstract Lazy<Image> GetInternalImage();

        public Lazy<Image> GetImage()
        {
            return _customImage ?? GetInternalImage();
        }

        protected MapTrackerIconType _iconType = MapTrackerIconType.TopDownImage;

        public virtual void SetIconType(MapTrackerIconType iconType, Lazy<Image> image = null)
        {
            if ((iconType == MapTrackerIconType.CustomImage) != (image != null))
                throw new ArgumentOutOfRangeException();

            _iconType = iconType;
            _customImage = image;
        }

        public virtual float GetY()
        {
            if (positionAngleProvider == null)
                return float.NegativeInfinity;
            var maxY = float.NegativeInfinity;
            foreach (var obj in positionAngleProvider())
                if (obj.Y > maxY)
                    maxY = (float)obj.Y;
            return maxY;
        }

        public bool ShouldDisplay(MapTrackerVisibilityType visiblityType) => true;

        public override string ToString() => GetName();

        public MapTracker tracker { get; private set; }

        public ContextMenuStrip BindToTracker(MapTracker tracker)
        {
            this.tracker = tracker;
            return GetContextMenuStrip(this.tracker);
        }

        protected virtual ContextMenuStrip GetContextMenuStrip(MapTracker targetTracker)
        {
            var _contextMenuStrip = new ContextMenuStrip();
            if (hasDragPoints)
            {
                itemEnableDragging.Click += (_, __) => enableDragging = dragMask == DragMask.None;
                foreach (var item in new[] { itemEnableDraggingX, itemEnableDraggingY, itemEnableDraggingZ, itemEnableDraggingAngle })
                {
                    itemEnableDragging.DropDownItems.Add(item);
                    item.Click += (_, __) => item.Checked = !item.Checked;
                }

                _contextMenuStrip.Items.Add(itemEnableDragging);
            }

            return _contextMenuStrip;
        }

        public virtual void InitSubTrackerContextMenuStrip(MapTab mapTab, ContextMenuStrip targetStrip)
        {
            MapObjectObject.AddPositionAngleSubTrackers(GetName(), tracker, targetStrip, positionAngleProvider);
        }

        public virtual void Update()
        {
        }

        public virtual bool ParticipatesInGlobalIconSize() => false;

        public delegate void SaveSettings(System.Xml.XmlNode node);

        public delegate void LoadSettings(System.Xml.XmlNode node);

        public virtual (SaveSettings save, LoadSettings load) SettingsSaveLoad => (
            (System.Xml.XmlNode node) =>
            {
                SaveValueNode(node, "EnableDragging", enableDragging.ToString());
                SaveValueNode(node, "Size", Size.ToString());
                SaveValueNode(node, "Color", Color.ToArgb().ToString("X"));
                SaveValueNode(node, "OutlineColor", OutlineColor.ToArgb().ToString("X"));
                SaveValueNode(node, "OutlineWidth", OutlineWidth.ToString());
            }
            , (System.Xml.XmlNode node) =>
            {
                if (bool.TryParse(LoadValueNode(node, "EnableDragging"), out bool _enableDragging))
                    enableDragging = _enableDragging;
                if (float.TryParse(LoadValueNode(node, "Size"), out float size))
                    Size = size;
                if (int.TryParse(LoadValueNode(node, "Color"), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out int color))
                    Color = Color.FromArgb(color);
                if (int.TryParse(LoadValueNode(node, "OutlineColor"), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out int outlineColor))
                    OutlineColor = Color.FromArgb(outlineColor);
                if (float.TryParse(LoadValueNode(node, "OutlineWidth"), out float outlineWidth))
                    OutlineWidth = outlineWidth;
            }
        );

        protected void SaveValueNode(System.Xml.XmlNode parentNode, string valueName, string value)
        {
            var valueNode = parentNode.OwnerDocument.CreateElement(valueName);
            valueNode.SetAttribute("value", value);
            parentNode.AppendChild(valueNode);
        }

        protected string LoadValueNode(System.Xml.XmlNode parentNode, string valueName)
        {
            var valueNode = parentNode.SelectSingleNode(valueName);
            if (valueNode == null)
                return null;
            foreach (System.Xml.XmlAttribute attr in valueNode.Attributes)
                if (attr.Name == "value")
                    return attr.Value;
            return null;
        }

        public virtual void CleanUp()
        {
            OnCleanup?.Invoke();
        }
    }
}
