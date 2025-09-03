using System;
using System.Collections.Generic;
using System.Linq;
using System.Drawing;
using STROOP.Utilities;
using STROOP.Structs.Configurations;
using STROOP.Structs;
using OpenTK;
using System.Windows.Forms;
using OpenTK.Mathematics;

namespace STROOP.Tabs.MapTab.MapObjects
{
    public class MapPathObject : MapObject
    {
        class PositionAngleComparer : IEqualityComparer<PositionAngle>
        {
            public bool Equals(PositionAngle x, PositionAngle y)
            {
                if (x == y)
                    return true;
                if (!x.CompareType(y))
                    return false;
                if (x is PositionAngle.ObjectPositionAngle && PositionAngle.GetObjectAddress(x) == PositionAngle.GetObjectAddress(y) && x.GetMapName() == y.GetMapName())
                    return true;
                return false;
            }

            public int GetHashCode(PositionAngle obj) => obj.ToString().GetHashCode();
        }

        private readonly Dictionary<PositionAngle, Dictionary<uint, Vector3>> _dictionary = new Dictionary<PositionAngle, Dictionary<uint, Vector3>>(new PositionAngleComparer());

        private (byte level, byte area, ushort loadingPoint, ushort missionLayout) _currentLocationStats;
        private bool resetPathOnLevelChange => _itemResetPathOnLevelChange.Checked;
        private bool useBlending => _itemUseBlending.Checked;
        private bool isPaused => _itemPause.Checked;
        private int _numSkips;
        private HashSet<uint> _skippedKeys;
        private uint _highestGlobalTimerValue;
        private int _modulo;

        private ToolStripMenuItem _itemResetPathOnLevelChange;
        private ToolStripMenuItem _itemUseBlending;
        private ToolStripMenuItem _itemPause;

        public MapPathObject(PositionAngleProvider positionAngleProvider)
            : base()
        {
            base.positionAngleProvider = positionAngleProvider;
            _currentLocationStats = MapTab.MapAssociations.GetCurrentLocationStats();
            _numSkips = 0;
            _skippedKeys = new HashSet<uint>();
            _highestGlobalTimerValue = 0;
            _modulo = 1;

            Size = 300;
            OutlineWidth = 3;
            Color = Color.Yellow;
            OutlineColor = Color.Red;
        }

        private List<Vector3> GetDictionaryValues(Dictionary<uint, Vector3> dic)
        {
            return dic.Keys.ToList()
                .FindAll(key => key % _modulo == 0)
                .ConvertAll(key => dic[key]);
        }

        protected override void DrawTopDown(MapGraphics graphics)
        {
            graphics.drawLayers[(int)MapGraphics.DrawLayers.FillBuffers].Add(() =>
            {
                if (OutlineWidth == 0) return;

                foreach (var dic in _dictionary)
                {
                    List<Vector3> vertices = GetDictionaryValues(dic.Value);
                    Vector3 lastVertex = default(Vector3);
                    int counter = 0;
                    foreach (var vertex in vertices)
                    {
                        Vector4 color = ColorUtilities.ColorToVec4(useBlending ? ColorUtilities.InterpolateColor(OutlineColor, Color, (double)counter / (vertices.Count - 1)) : Color, OpacityByte);
                        if (counter > 0)
                            graphics.lineRenderer.Add(vertex, lastVertex, color, OutlineWidth);
                        counter++;
                        lastVertex = vertex;
                    }
                }
            });
        }
        protected override void DrawOrthogonal(MapGraphics graphics) => DrawTopDown(graphics);

        public override void Update()
        {
            (byte level, byte area, ushort loadingPoint, ushort missionLayout) currentLocationStats =
                MapTab.MapAssociations.GetCurrentLocationStats();
            if (currentLocationStats.level != _currentLocationStats.level ||
                currentLocationStats.area != _currentLocationStats.area ||
                currentLocationStats.loadingPoint != _currentLocationStats.loadingPoint ||
                currentLocationStats.missionLayout != _currentLocationStats.missionLayout)
            {
                _currentLocationStats = currentLocationStats;
                if (resetPathOnLevelChange)
                {
                    _dictionary.Clear();
                    _numSkips = 5;
                    _skippedKeys.Clear();
                }
            }

            if (!isPaused)
            {
                uint globalTimer = Config.Stream.GetUInt32(MiscConfig.GlobalTimerAddress);
                foreach (var _posAngle in positionAngleProvider())
                {
                    float x = (float)_posAngle.X;
                    float y = (float)_posAngle.Y;
                    float z = (float)_posAngle.Z;
                    Dictionary<uint, Vector3> dic;
                    if (!_dictionary.TryGetValue(_posAngle, out dic))
                        _dictionary[_posAngle] = dic = new Dictionary<uint, Vector3>();

                    if (globalTimer < _highestGlobalTimerValue)
                    {
                        var tempDictionary = new Dictionary<uint, Vector3>();
                        foreach (uint key in dic.Keys)
                        {
                            tempDictionary[key] = dic[key];
                        }
                        _dictionary.Clear();
                        foreach (uint key in tempDictionary.Keys)
                        {
                            if (key <= globalTimer)
                            {
                                dic[key] = tempDictionary[key];
                                _highestGlobalTimerValue = key;
                            }
                        }
                    }

                    if (!dic.ContainsKey(globalTimer))
                    {
                        if (_numSkips > 0)
                        {
                            if (!_skippedKeys.Contains(globalTimer))
                            {
                                _skippedKeys.Add(globalTimer);
                                _numSkips--;
                            }
                        }
                        else
                        {
                            dic[globalTimer] = new Vector3(x, y, z);
                            _highestGlobalTimerValue = globalTimer;
                        }
                    }
                }
            }
        }

        protected override ContextMenuStrip GetContextMenuStrip(MapTracker targetTracker)
        {
            ToolStripMenuItem itemResetPath = new ToolStripMenuItem("Reset Path");
            itemResetPath.Click += (sender, e) => _dictionary.Clear();

            _itemResetPathOnLevelChange = new ToolStripMenuItem("Reset Path on Level Change");
            _itemResetPathOnLevelChange.Click += (sender, e) => _itemResetPathOnLevelChange.Checked = !_itemResetPathOnLevelChange.Checked;

            _itemUseBlending = new ToolStripMenuItem("Use Blending");
            _itemUseBlending.Click += (sender, e) => _itemUseBlending.Checked = !_itemUseBlending.Checked;
            _itemUseBlending.Checked = true;

            _itemPause = new ToolStripMenuItem("Pause");
            _itemPause.Click += (sender, e) => _itemPause.Checked = !_itemPause.Checked;

            ToolStripMenuItem itemSetModulo = new ToolStripMenuItem("Set Modulo");
            itemSetModulo.Click += (sender, e) =>
            {
                string text = DialogUtilities.GetStringFromDialog(labelText: "Enter modulo.");
                int? moduloNullable = ParsingUtilities.ParseIntNullable(text);
                if (moduloNullable.HasValue && moduloNullable.Value > 0)
                    _modulo = moduloNullable.Value;
            };

            var _contextMenuStrip = new ContextMenuStrip();
            _contextMenuStrip.Items.Add(itemResetPath);
            _contextMenuStrip.Items.Add(_itemResetPathOnLevelChange);
            _contextMenuStrip.Items.Add(_itemUseBlending);
            _contextMenuStrip.Items.Add(_itemPause);
            _contextMenuStrip.Items.Add(itemSetModulo);

            return _contextMenuStrip;
        }

        public override (SaveSettings save, LoadSettings load) SettingsSaveLoad => (
            (System.Xml.XmlNode node) =>
            {
                base.SettingsSaveLoad.save(node);
                SaveValueNode(node, "ResetPathOnLevelChange", resetPathOnLevelChange.ToString());
                SaveValueNode(node, "UseBlending", useBlending.ToString());
                SaveValueNode(node, "Pause", isPaused.ToString());
                SaveValueNode(node, "GlobalTimerModulo", _modulo.ToString());
            }
        ,
            (System.Xml.XmlNode node) =>
            {
                base.SettingsSaveLoad.load(node);
                if (bool.TryParse(LoadValueNode(node, "ResetPathOnLevelChange"), out var v))
                    _itemResetPathOnLevelChange.Checked = v;
                if (bool.TryParse(LoadValueNode(node, "UseBlending"), out v))
                    _itemUseBlending.Checked = v;
                if (bool.TryParse(LoadValueNode(node, "Pause"), out v))
                    _itemPause.Checked = v;
                if (int.TryParse(LoadValueNode(node, "ResetPathOnLevelChange"), out int modulo))
                    _modulo = modulo;
            }
        );

        public override string GetName() => $"Path for {PositionAngle.NameOfMultiple(positionAngleProvider())}";

        public override Lazy<Image> GetInternalImage() => Config.ObjectAssociations.PathImage;
    }
}
