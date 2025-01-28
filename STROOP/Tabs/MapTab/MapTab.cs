using System;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using STROOP.Structs;
using STROOP.Utilities;
using System.Windows.Forms;
using System.Drawing;
using OpenTK;
using OpenTK.Graphics;
using STROOP.Structs.Configurations;
using STROOP.Tabs.MapTab.MapObjects;
using System.Xml.Linq;

namespace STROOP.Tabs.MapTab
{
    public partial class MapTab : STROOPTab, IDisposable
    {
        public static MapAssociations MapAssociations;

        const string DEFAULT_TRACKER_FILE = "MapTrackers.default.xml";
        const string CONFIG_NODE_NAME = "LastMapTrackerFileName";

        static XElement configNode;
        static string lastTrackerFileName = null;
        [InitializeConfigParser]
        static void InitConfigParser()
        {
            XmlConfigParser.AddConfigParser(CONFIG_NODE_NAME, _ =>
            {
                configNode = _;
                lastTrackerFileName = _.Attribute(XName.Get("path")).Value;
            });
        }

        static void SaveConfig()
        {
            if (configNode == null)
                Program.config.Root.Add(configNode = new XElement(XName.Get(CONFIG_NODE_NAME)));
            configNode.SetAttributeValue(XName.Get("path"), lastTrackerFileName);
            configNode.Document.Save(Program.CONFIG_FILE_NAME);
        }

        public GLControl glControlMap2D { get; private set; }
        public MapGraphics graphics { get; private set; }
        public List<MapPopout> popouts = new List<MapPopout>();

        private List<int> _currentObjIndexes = new List<int>();
        public List<IHoverData> hoverData { get; private set; } = new List<IHoverData>();

        public bool PauseMapUpdating = false;
        private bool _isLoaded2D = false;
        int _numLevelTriangles = 0;
        bool makeInGameCameraFollow = false;

        public bool HasMouseListeners => hoverData.Count > 0;
        Dictionary<uint, MapTracker> semaphoreTrackers = new Dictionary<uint, MapTracker>();
        List<(CheckBox checkBox, MapTracker tracker)> quickSemaphores = new List<(CheckBox checkBox, MapTracker tracker)>();
        Dictionary<string, MapTracker.CreateTracker> newTrackerByName = new Dictionary<string, MapTracker.CreateTracker>();
        Dictionary<(Type, string), Func<MapTracker>> addNewTrackers = new Dictionary<(Type, string), Func<MapTracker>>();
        HashSet<MapTracker> externalTrackers = new HashSet<MapTracker>();

        private HashSet<uint> _selection = new HashSet<uint>();
        HashSet<uint> trackedAddresses = new HashSet<uint>();

        public override HashSet<uint> selection => _selection;

        public MapTab()
        {
            InitializeComponent();
            if (Program.IsVisualStudioHostProcess()) return;
        }

        public MapTracker AddByCode(Type type, string initializer = null) => addNewTrackers[(type, initializer)]();

        public MapTracker AddExternal(MapObject obj)
        {
            var tracker = new MapTracker(this, null, obj);
            flowLayoutPanelMapTrackers.Controls.Add(tracker);
            externalTrackers.Add(tracker);
            return tracker;
        }

        public void RemoveTracker(MapTracker mapTracker)
        {
            flowLayoutPanelMapTrackers.Controls.Remove(mapTracker);
            externalTrackers.Remove(mapTracker);
        }

        public void AddSubTracker(MapTracker obj) => flowLayoutPanelMapTrackers.Controls.Add(obj);

        public override Action<IEnumerable<ObjectSlot>> objectSlotsClicked => objectSlots =>
        {
            bool? unselectFirst = null;
            foreach (var objectSlot in objectSlots)
            {
                bool hasKey = semaphoreTrackers.TryGetValue(objectSlot.CurrentObject.Address, out var tracker);
                if (unselectFirst == null)
                    unselectFirst = hasKey ^ selection.Count > 1;
                if (hasKey && unselectFirst.Value)
                    tracker.Kill();
                else if (!hasKey && !unselectFirst.Value)
                {
                    var newTracker = new MapTracker(this, $"ObjectSlot{objectSlot.Index}", new MapObjectObject(objectSlot.CurrentObject.Address));
                    flowLayoutPanelMapTrackers.Controls.Add(newTracker);
                }
            }
        };

        public IEnumerable<MapTracker> EnumerateTrackers() => flowLayoutPanelMapTrackers.EnumerateTrackers();

        public bool TracksObject(uint address) => IsActiveTab && trackedAddresses.Contains(address);

        public override string GetDisplayName() => "Map";

        public void Load2D()
        {
            // Create new graphics control
            var parentControl = splitContainerMap.Panel2;
            glControlMap2D = new GLControl(GraphicsMode.Default, 3, 3, GraphicsContextFlags.Default);
            glControlMap2D.Size = parentControl.ClientSize;
            glControlMap2D.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top;
            glControlMap2D.Location = new Point(0, 0);
            parentControl.Controls.Add(glControlMap2D);
            graphics = new MapGraphics(this, glControlMap2D);
            _isLoaded2D = true;

            using (new AccessScope<MapTab>(this))
            {
                graphics.Load(() => new Renderers.RendererCollection());
                InitializeControls();

                comboBoxViewMode.SelectedIndex = 0;
                if (!System.IO.File.Exists(DEFAULT_TRACKER_FILE))
                    flowLayoutPanelMapTrackers.Controls.Clear();
                else
                    LoadTrackerConfigAndDisplay(DEFAULT_TRACKER_FILE);
            }
            RequireGeometryUpdate();
        }

        public void DrawOn2DControl(MapGraphics graphics) => flowLayoutPanelMapTrackers.DrawOn2DControl(graphics);

        public MapLayout GetMapLayout(object mapLayoutChoice = null) =>
            (mapLayoutChoice ?? comboBoxMapOptionsLevel.SelectedItem) as MapLayout ?? MapAssociations.GetBestMap();


        bool displayingExtendedBoundaries = false;
        bool needsGeometryRefresh, _needsGeometryRefreshInternal;
        public bool NeedsGeometryRefresh() => needsGeometryRefresh;
        public void RequireGeometryUpdate() => _needsGeometryRefreshInternal = true;


        public BackgroundImage GetBackgroundImage(object backgroundChoice = null)
        {
            if ((backgroundChoice ?? comboBoxMapOptionsBackground.SelectedItem) is BackgroundImage result)
                return result;
            return MapAssociations.GetBestMap().Background;
        }

        public List<(float x, float z)> GetPuCenters(MapGraphics graphics)
        {
            int xMin = ((((int)graphics.MapViewXMin) / 65536) - 1) * 65536;
            int xMax = ((((int)graphics.MapViewXMax) / 65536) + 1) * 65536;
            int zMin = ((((int)graphics.MapViewZMin) / 65536) - 1) * 65536;
            int zMax = ((((int)graphics.MapViewZMax) / 65536) + 1) * 65536;
            List<(float x, float z)> centers = new List<(float x, float z)>();
            for (int x = xMin; x <= xMax; x += 65536)
            {
                for (int z = zMin; z <= zMax; z += 65536)
                {
                    centers.Add((x, z));
                }
            }
            return centers;
        }

        public List<(float x, float z)> GetPuCoordinates(MapGraphics graphics, float relX, float relZ)
            => GetPuCenters(graphics).ConvertAll(center => (center.x + relX, center.z + relZ));

        public int MaybeReverse(int value) => checkBoxMapOptionsReverseDragging.Checked ? -1 * value : value;

        void InitAddTrackerButton()
        {
            var adders = new Dictionary<string, Wrapper<(bool, ObjectDescriptionAttribute, Func<ToolStripMenuItem>)>>();
            foreach (var type in GeneralUtilities.EnumerateTypes(_ => _.IsSubclassOf(typeof(MapObject))))
            {
                var attrArray = type.GetCustomAttributes<ObjectDescriptionAttribute>();
                foreach (var attr in attrArray)
                {
                    var capturedType = type;
                    var creationIdentifier = attr.DisplayName.Replace(' ', '_');
                    MapTracker.CreateTracker newObjectFunc;
                    if (attr.Initializer == null)
                        newTrackerByName[creationIdentifier] = newObjectFunc =
                            _ => new MapTracker(this, creationIdentifier, (MapObject)Activator.CreateInstance(capturedType));
                    else
                        newTrackerByName[creationIdentifier] = newObjectFunc = creationParameters =>
                        {
                            var newObj = (MapObject)
                                    (capturedType.GetMethod(attr.Initializer, BindingFlags.Public | BindingFlags.Static)
                                    ?.Invoke(null, new object[] { creationParameters })
                                    ?? null);
                            return newObj != null ? new MapTracker(this, creationIdentifier, newObj) : null;
                        };

                    Func<MapTracker> addNewTracker = () =>
                        {
                            using (new AccessScope<MapTab>(this))
                            {
                                var tracker = newObjectFunc(null);
                                if (tracker != null)
                                    flowLayoutPanelMapTrackers.Controls.Add(tracker);
                                return tracker;
                            }
                        };

                    addNewTrackers[(type, attr.Initializer)] = addNewTracker;
                    adders[attr.DisplayName] = new Wrapper<(bool, ObjectDescriptionAttribute, Func<ToolStripMenuItem>)>((false, attr, () =>
                        {
                            var toolStripItem = new ToolStripMenuItem($"Add Tracker for {attr.DisplayName}");
                            toolStripItem.Click += (sender, e) => addNewTracker();
                            return toolStripItem;
                        }
                    ));
                }
            }

            buttonMapOptionsAddNewTracker.ContextMenuStrip = new ContextMenuStrip();
            var categoryItems = new Dictionary<string, ToolStripMenuItem>();
            ToolStripMenuItem GetCategoryItem(string categoryName)
            {
                ToolStripMenuItem categoryItem;
                if (!categoryItems.TryGetValue(categoryName, out categoryItem))
                {
                    categoryItems[categoryName] = categoryItem = new ToolStripMenuItem(categoryName);
                    buttonMapOptionsAddNewTracker.ContextMenuStrip.Items.Add(categoryItem);
                }
                return categoryItem;
            }

            if (System.IO.File.Exists("Config/MapTrackerOrder.xml"))
            {
                var doc = new System.Xml.XmlDocument();
                doc.Load("Config/MapTrackerOrder.xml");
                var currentNode = doc.SelectSingleNode("AddTracker").FirstChild;
                while (currentNode != null)
                {
                    if (currentNode.Name == "Category")
                    {
                        var categoryItem = GetCategoryItem(currentNode.Attributes["name"].Value);
                        var trackerNode = currentNode.FirstChild;
                        while (trackerNode != null)
                        {
                            switch (trackerNode.Name)
                            {
                                case "Tracker":
                                    if (adders.TryGetValue(trackerNode.Attributes["name"].Value, out var trackThis))
                                    {
                                        categoryItem.DropDownItems.Add(trackThis.value.Item3());
                                        trackThis.value.Item1 = true;
                                    }
                                    break;
                                case "Separator":
                                    categoryItem.DropDownItems.Add(new ToolStripSeparator());
                                    break;
                            }
                            trackerNode = trackerNode.NextSibling;
                        }
                    }
                    currentNode = currentNode.NextSibling;
                }
            }

            foreach (var trackerAddFunction in adders)
                if (!trackerAddFunction.Value.value.Item1)
                {
                    var categoryItem = GetCategoryItem(trackerAddFunction.Value.value.Item2.Category);
                    categoryItem.DropDownItems.Add(trackerAddFunction.Value.value.Item3());
                }
        }

        private void InitializeControls()
        {
            quickSemaphores = new List<(CheckBox checkBox, MapTracker tracker)>(new[]
            {
                (checkBoxMapOptionsTrackMario, new MapTracker(this, "SemaphoreMario", new MapMarioObject())),
                (checkBoxMapOptionsTrackHolp, new MapTracker(this, "SemaphoreHolp", new MapHolpObject())),
                (checkBoxMapOptionsTrackGhost, new MapTracker(this, "SemaphoreGhost", new MapGhostObject())),
                (checkBoxMapOptionsTrackCamera, new MapTracker(this, "SemaphoreCamera", new MapCameraObject())),
                (checkBoxMapOptionsTrackFloorTri, new MapTracker(this, "SemaphoreFloorTri", new MapMarioFloorObject())),
                (checkBoxMapOptionsTrackWallTri, new MapTracker(this, "SemaphoreWallTri", new MapMarioWallObject())),
                (checkBoxMapOptionsTrackCeilingTri, new MapTracker(this, "SemaphoreCeilingTri", new MapMarioCeilingObject())),
                (checkBoxMapOptionsTrackUnitGridlines, new MapTracker(this, "SemaphoreUnitGridlines", MapGridlinesObject.CreateUnits(null))),
            });
            foreach (var it in quickSemaphores)
            {
                var capture = it;
                newTrackerByName[capture.tracker.creationIdentifier] = _ =>
                {
                    capture.checkBox.Checked = true;
                    return capture.tracker;
                };
                capture.checkBox.CheckedChanged += (_, __) =>
                {
                    if (capture.checkBox.Checked)
                        flowLayoutPanelMapTrackers.Controls.Add(capture.tracker);
                };
            }
            for (int i = 0; i < ObjectSlotsConfig.MaxSlots; i++)
            {
                var capture = i;
                var creationIdentifier = $"ObjectSlot{i}";
                newTrackerByName[creationIdentifier] = _ =>
                {
                    var objectSlot = Config.ObjectSlotsManager.ObjectSlots[capture];
                    var newTracker = new MapTracker(
                        this,
                        creationIdentifier,
                        new MapObjectObject(objectSlot.CurrentObject.Address)
                        );
                    return newTracker;
                };
            }
            checkBoxMapOptionsTrackMario.Checked = true;

            // FlowLayoutPanel
            flowLayoutPanelMapTrackers.Initialize(new MapCurrentMapObject(), new MapCurrentBackgroundObject());

            // ComboBox for Level
            List<MapLayout> mapLayouts = MapAssociations.GetAllMaps();
            List<object> mapLayoutChoices = new List<object>() { "Recommended" };
            mapLayouts.ForEach(mapLayout => mapLayoutChoices.Add(mapLayout));
            comboBoxMapOptionsLevel.DataSource = mapLayoutChoices;

            // ComboBox for Background
            List<BackgroundImage> backgroundImages = MapAssociations.GetAllBackgroundImages();
            List<object> backgroundImageChoices = new List<object>() { "Recommended" };
            backgroundImages.ForEach(backgroundImage => backgroundImageChoices.Add(backgroundImage));
            comboBoxMapOptionsBackground.DataSource = backgroundImageChoices;

            // Buttons on Options
            InitAddTrackerButton();
            buttonMapOptionsAddNewTracker.Click += (sender, e) =>
                buttonMapOptionsAddNewTracker.ContextMenuStrip.Show(Cursor.Position);
            buttonMapOptionsClearAllTrackers.Click += (sender, e) =>
                flowLayoutPanelMapTrackers.Controls.Clear();
            ControlUtilities.AddContextMenuStripFunctions(
                 buttonMapOptionsClearAllTrackers,
                new List<string>()
                {
                    "Reset to Initial State",
                },
                new List<Action>()
                {
                    () => ResetToInitialState(),
                });

            // Buttons for Changing Scale
            buttonMapControllersScaleMinus.Click += (sender, e) => graphics.ChangeScale(-1, textBoxMapControllersScaleChange.Text);
            buttonMapControllersScalePlus.Click += (sender, e) => graphics.ChangeScale(1, textBoxMapControllersScaleChange.Text);
            buttonMapControllersScaleDivide.Click += (sender, e) => graphics.ChangeScale2(-1, textBoxMapControllersScaleChange2.Text);
            buttonMapControllersScaleTimes.Click += (sender, e) => graphics.ChangeScale2(1, textBoxMapControllersScaleChange2.Text);
            ControlUtilities.AddContextMenuStripFunctions(
                 groupBoxMapControllersScale,
                new List<string>()
                {
                    "Very Small Unit Squares",
                    "Small Unit Squares",
                    "Medium Unit Squares",
                    "Big Unit Squares",
                    "Very Big Unit Squares",
                },
                new List<Action>()
                {
                    () => graphics.SetCustomScale(6),
                    () => graphics.SetCustomScale(12),
                    () => graphics.SetCustomScale(18),
                    () => graphics.SetCustomScale(24),
                    () => graphics.SetCustomScale(40),
                });

            // Buttons for Changing Center
            buttonMapControllersCenterUp.Click += (sender, e) => graphics.ChangeCenter(0, -1, textBoxMapControllersCenterChange.Text);
            buttonMapControllersCenterDown.Click += (sender, e) => graphics.ChangeCenter(0, 1, textBoxMapControllersCenterChange.Text);
            buttonMapControllersCenterLeft.Click += (sender, e) => graphics.ChangeCenter(-1, 0, textBoxMapControllersCenterChange.Text);
            buttonMapControllersCenterRight.Click += (sender, e) => graphics.ChangeCenter(1, 0, textBoxMapControllersCenterChange.Text);
            buttonMapControllersCenterUpLeft.Click += (sender, e) => graphics.ChangeCenter(-1, -1, textBoxMapControllersCenterChange.Text);
            buttonMapControllersCenterUpRight.Click += (sender, e) => graphics.ChangeCenter(1, -1, textBoxMapControllersCenterChange.Text);
            buttonMapControllersCenterDownLeft.Click += (sender, e) => graphics.ChangeCenter(-1, 1, textBoxMapControllersCenterChange.Text);
            buttonMapControllersCenterDownRight.Click += (sender, e) => graphics.ChangeCenter(1, 1, textBoxMapControllersCenterChange.Text);
            ControlUtilities.AddContextMenuStripFunctions(
                 groupBoxMapControllersCenter,
                new List<string>() { "Center on Mario" },
                new List<Action>()
                {
                    () =>
                    {
                        float marioX = Config.Stream.GetSingle(MarioConfig.StructAddress + MarioConfig.XOffset);
                        float marioZ = Config.Stream.GetSingle(MarioConfig.StructAddress + MarioConfig.ZOffset);
                        graphics.SetCustomCenter(marioX + ";" + marioZ);
                    }
                });

            // Buttons for Changing Angle
            buttonMapControllersAngleCCW.Click += (sender, e) => graphics.ChangeAngle(-1, textBoxMapControllersAngleChange.Text);
            buttonMapControllersAngleCW.Click += (sender, e) => graphics.ChangeAngle(1, textBoxMapControllersAngleChange.Text);
            ControlUtilities.AddContextMenuStripFunctions(
                 groupBoxMapControllersAngle,
                new List<string>()
                {
                    "Use Mario Angle",
                    "Use Camera Angle",
                    "Use Centripetal Angle",
                },
                new List<Action>()
                {
                    () =>
                    {
                        ushort marioAngle = Config.Stream.GetUInt16(MarioConfig.StructAddress + MarioConfig.FacingYawOffset);
                        graphics.SetCustomAngle(marioAngle);
                    },
                    () =>
                    {
                        ushort cameraAngle = Config.Stream.GetUInt16(CameraConfig.StructAddress + CameraConfig.FacingYawOffset);
                        graphics.SetCustomAngle(cameraAngle);
                    },
                    () =>
                    {
                        ushort centripetalAngle = Config.Stream.GetUInt16(CameraConfig.StructAddress + CameraConfig.CentripetalAngleOffset);
                        double centripetalAngleReversed = MoreMath.ReverseAngle(centripetalAngle);
                        graphics.SetCustomAngle(centripetalAngleReversed);
                    },
                });

            // TextBoxes for Custom Values
            textBoxMapControllersScaleCustom.AddEnterAction(() => radioButtonMapControllersScaleCustom.Checked = true);
            textBoxMapControllersCenterCustom.AddEnterAction(() => radioButtonMapControllersCenterCustom.Checked = true);
            textBoxMapControllersAngleCustom.AddEnterAction(() => radioButtonMapControllersAngleCustom.Checked = true);

            checkBoxMapOptionsEnablePuView.Click += (sender, e) => graphics.MapViewEnablePuView = checkBoxMapOptionsEnablePuView.Checked;
            checkBoxMapOptionsScaleIconSizes.Click += (sender, e) => graphics.MapViewScaleIconSizes = checkBoxMapOptionsScaleIconSizes.Checked;
            checkBoxMapControllersCenterChangeByPixels.Click += (sender, e) => graphics.MapViewCenterChangeByPixels = checkBoxMapControllersCenterChangeByPixels.Checked;

            // Global Icon Size
            textBoxMapOptionsGlobalIconSize.AddEnterAction(() =>
           {
               float? parsed = ParsingUtilities.ParseFloatNullable(
                    textBoxMapOptionsGlobalIconSize.Text);
               if (!parsed.HasValue) return;
               SetGlobalIconSize(parsed.Value);
           });
            trackBarMapOptionsGlobalIconSize.AddManualChangeAction(() =>
               SetGlobalIconSize(trackBarMapOptionsGlobalIconSize.Value));
            MapUtilities.CreateTrackBarContextMenuStrip(trackBarMapOptionsGlobalIconSize);

            glControlMap2D.MouseDown += (sender, e) =>
            {
                if (e.Button == MouseButtons.Right)
                    ShowRightClickMenu();
            };
        }

        bool IsContextMenuOpen() => contextMenu != null && contextMenu.Visible;
        ContextMenuStrip contextMenu;
        void ShowRightClickMenu()
        {
            contextMenu = new ContextMenuStrip();
            var onClickPosition = graphics.mapCursorPosition;
            var copyPositionItem = new ToolStripMenuItem("Copy Cursor Position");
            copyPositionItem.Click += (e, args) => CopyUtilities.CopyPosition(onClickPosition);
            contextMenu.Items.Add(copyPositionItem);
            contextMenu.Items.Add(new ToolStripSeparator());

            if (graphics.view.mode == MapView.ViewMode.ThreeDimensional)
            {
                var pivotPositionItem = new ToolStripMenuItem("Pivot This Position");
                pivotPositionItem.Click += (e, args) =>
                {
                    graphics.view.camera3DMode = MapView.Camera3DMode.FocusOnPositionAngle;
                    graphics.view.focusPositionAngle = PositionAngle.Custom(onClickPosition);
                };
                contextMenu.Items.Add(pivotPositionItem);
                contextMenu.Items.Add(new ToolStripSeparator());
            }

            foreach (var a in hoverData)
                a.AddContextMenuItems(this, contextMenu);

            if (hoverData.Count > 0)
            {
                contextMenu.Items.Add(new ToolStripSeparator());
            }
            
            var openPopoutItem = new ToolStripMenuItem("Open Popout");
            openPopoutItem.Click += (e, args) =>
            {
                var popout = new MapPopout(this) { Owner = FindForm() };
                popout.Show();
                popout.FormClosed += (_, __) => popouts.Remove(popout);
                popouts.Add(popout);
            };
            contextMenu.Items.Add(openPopoutItem);

            contextMenu.Show(Cursor.Position);
        }

        private void LoadDefaultTrackers()
        {
            if (!System.IO.File.Exists(DEFAULT_TRACKER_FILE))
                flowLayoutPanelMapTrackers.Controls.Clear();
            else
                LoadTrackerConfigAndDisplay(DEFAULT_TRACKER_FILE);
        }

        private void ResetToInitialState()
        {
            comboBoxMapOptionsLevel.SelectedItem = "Recommended";
            comboBoxMapOptionsBackground.SelectedItem = "Recommended";
            radioButtonMapControllersScaleCourseDefault.Checked = true;
            radioButtonMapControllersCenterBestFit.Checked = true;
            radioButtonMapControllersAngle32768.Checked = true;
        }

        private void SetGlobalIconSize(float size)
        {
            flowLayoutPanelMapTrackers.SetGlobalIconSize(size);
            textBoxMapOptionsGlobalIconSize.SubmitText(size.ToString());
            trackBarMapOptionsGlobalIconSize.StartChangingByCode();
            ControlUtilities.SetTrackBarValueCapped(trackBarMapOptionsGlobalIconSize, size);
            trackBarMapOptionsGlobalIconSize.StopChangingByCode();
        }

        public override void InitializeTab()
        {
            base.InitializeTab();
            Load2D();
            AddViewModeContextMenu();
        }

        void AddViewModeContextMenu()
        {
            comboBoxViewMode.MouseDown += (_, e) =>
            {
                if (e.Button == MouseButtons.Right)
                {
                    var ctx = new ContextMenuStrip();

                    var itemRefreshLevelGeometry = new ToolStripMenuItem("Refresh Level Geometry");
                    itemRefreshLevelGeometry.Click += (__, ___) => RequireGeometryUpdate();
                    ctx.Items.Add(itemRefreshLevelGeometry);

                    if (graphics.view.mode == MapView.ViewMode.ThreeDimensional)
                    {
                        ctx.Items.Add(new ToolStripSeparator());

                        var itemDisplayLevelGeometry = new ToolStripMenuItem("Display Level Geometry");
                        itemDisplayLevelGeometry.Checked = graphics.view.display3DLevelGeometry;
                        itemDisplayLevelGeometry.Click += (__, ___) => itemDisplayLevelGeometry.Checked = graphics.view.display3DLevelGeometry = !graphics.view.display3DLevelGeometry;
                        ctx.Items.Add(itemDisplayLevelGeometry);

                        var itemDisplayCylinderOutlines = new ToolStripMenuItem("Draw Cylinder Outlines");
                        itemDisplayCylinderOutlines.Checked = graphics.view.drawCylinderOutlines;
                        itemDisplayCylinderOutlines.Click += (__, ___) => itemDisplayCylinderOutlines.Checked = graphics.view.drawCylinderOutlines = !graphics.view.drawCylinderOutlines;
                        ctx.Items.Add(itemDisplayCylinderOutlines);

                        ctx.Items.Add(new ToolStripSeparator());

                        var itemCameraModeInGame = new ToolStripMenuItem("In-Game View");
                        var itemCameraModePivot = new ToolStripMenuItem("Pivot");
                        var itemCameraModeFree = new ToolStripMenuItem("Free");
                        itemCameraModeInGame.Checked = graphics.view.camera3DMode == MapView.Camera3DMode.InGame;
                        itemCameraModePivot.Checked = graphics.view.camera3DMode == MapView.Camera3DMode.FocusOnPositionAngle;
                        itemCameraModeFree.Checked = graphics.view.camera3DMode == MapView.Camera3DMode.Free;

                        itemCameraModeInGame.Click += (__, ___) =>
                        {
                            graphics.view.camera3DMode = MapView.Camera3DMode.InGame;
                            itemCameraModePivot.Checked = itemCameraModeFree.Checked = !(itemCameraModeInGame.Checked = true);
                        };
                        itemCameraModePivot.Click += (__, ___) =>
                        {
                            graphics.view.camera3DMode = MapView.Camera3DMode.FocusOnPositionAngle;
                            itemCameraModeInGame.Checked = itemCameraModeFree.Checked = !(itemCameraModePivot.Checked = true);
                        };
                        itemCameraModeFree.Click += (__, ___) =>
                        {
                            graphics.view.camera3DMode = MapView.Camera3DMode.Free;
                            itemCameraModeInGame.Checked = itemCameraModePivot.Checked = !(itemCameraModeInGame.Checked = true);
                        };
                        ctx.Items.Add(itemCameraModeInGame);
                        ctx.Items.Add(itemCameraModePivot);
                        ctx.Items.Add(itemCameraModeFree);

                        var itemFollowInGame = new ToolStripMenuItem("Match Cam Hack to view");
                        itemFollowInGame.Checked = makeInGameCameraFollow;
                        itemFollowInGame.Click += (__, ___) =>
                        {
                            makeInGameCameraFollow = (itemFollowInGame.Checked = !itemFollowInGame.Checked);
                        };

                        ctx.Items.Add(new ToolStripSeparator());
                        ctx.Items.Add(itemFollowInGame);
                    }

                    if (graphics.view.mode == MapView.ViewMode.Orthogonal)
                    {
                        ctx.Items.Add(new ToolStripSeparator());

                        var itemDisplayLevelGeometry = new ToolStripMenuItem("Display Triangle Tracker Geometry");
                        itemDisplayLevelGeometry.Checked = graphics.view.displayOrthoLevelGeometry;
                        itemDisplayLevelGeometry.Click += (__, ___) => itemDisplayLevelGeometry.Checked = graphics.view.displayOrthoLevelGeometry = !graphics.view.displayOrthoLevelGeometry;
                        ctx.Items.Add(itemDisplayLevelGeometry);

                        var itemSetRelativeNearPlane = new ToolStripMenuItem("Set Relative Near Plane");
                        itemSetRelativeNearPlane.Click += (__, ___) =>
                            graphics.view.orthoRelativeNearPlane = (float)DialogUtilities.GetDoubleFromDialog(0, labelText: "Enter relative near plane value.");
                        ctx.Items.Add(itemSetRelativeNearPlane);

                        var itemClearRelativeNearPlane = new ToolStripMenuItem("Clear Relative Near Plane");
                        itemClearRelativeNearPlane.Click += (__, ___) => graphics.view.orthoRelativeNearPlane = float.NaN;
                        ctx.Items.Add(itemClearRelativeNearPlane);

                        var itemSetRelativeFarPlane = new ToolStripMenuItem("Set Relative Far Plane");
                        itemSetRelativeFarPlane.Click += (__, ___) =>
                            graphics.view.orthoRelativeFarPlane = (float)DialogUtilities.GetDoubleFromDialog(0, labelText: "Enter relative far plane value.");
                        ctx.Items.Add(itemSetRelativeFarPlane);

                        var itemClearRelativeFarPlane = new ToolStripMenuItem("Clear Relative Far Plane");
                        itemClearRelativeFarPlane.Click += (__, ___) => graphics.view.orthoRelativeFarPlane = float.NaN;
                        ctx.Items.Add(itemClearRelativeFarPlane);

                    }
                    ctx.Show(Cursor.Position);
                }
            };
        }

        public void UpdateHover()
        {
            using (new AccessScope<MapTab>(this))
            {
                if (Form.ActiveForm != null && glControlMap2D.ClientRectangle.Contains(glControlMap2D.PointToClient(Cursor.Position)))
                    graphics.UpdateFlyingControls(Config.Stream.lastFrameTime);
                if (!graphics.IsMouseDown(0))
                {
                    var newCursor = graphics.mapCursorPosition;
                    hoverData.Clear();
                    foreach (var tracker in flowLayoutPanelMapTrackers.EnumerateTrackers())
                        if (tracker.IsVisible)
                        {
                            var newHover = tracker.mapObject.GetHoverData(graphics, ref newCursor);
                            if (graphics.fixCursorPlane)
                            {
                                graphics.cursorViewPlaneDist = Vector3.Dot(graphics.view.ComputeViewDirection(), (newCursor - graphics.view.position));
                                graphics.UpdateCursor();
                            }

                            if (newHover != null)
                                hoverData.Add(newHover);
                        }
                }
            }
        }

        public override void Update(bool active)
        {
            if (!_isLoaded2D) return;

            semaphoreTrackers.Clear();
            trackedAddresses.Clear();
            foreach (var tracker in flowLayoutPanelMapTrackers.EnumerateTrackers())
            {
                if (tracker.mapObject is MapObjectObject obj)
                    semaphoreTrackers[((PositionAngle.ObjectPositionAngle)obj.positionAngleProvider().FirstOrDefault()).objectAddress.Value] = tracker;
                foreach (var pa in tracker.mapObject.positionAngleProvider())
                    if (pa is PositionAngle.ObjectPositionAngle objPA)
                    {
                        var a = objPA.objectAddress;
                        if (a.HasValue)
                            trackedAddresses.Add(a.Value);
                    }
            }

            graphics.UpdateCursor();
            if (displayingExtendedBoundaries != SavedSettingsConfig.UseExtendedLevelBoundaries)
            {
                displayingExtendedBoundaries = SavedSettingsConfig.UseExtendedLevelBoundaries;
                RequireGeometryUpdate();
            }

            if (!IsContextMenuOpen())
                UpdateHover();
            using (new AccessScope<MapTab>(this))
            {
                flowLayoutPanelMapTrackers.UpdateControl();

                active |= popouts.Count > 0;
                if (active)
                {
                    base.Update(active);
                    UpdateBasedOnObjectsSelectedOnMap();
                    UpdateDataTab();

                    if (!PauseMapUpdating)
                    {
                        glControlMap2D.Invalidate();
                        foreach (var frm in FindForm().OwnedForms)
                            if (frm is MapPopout coolio)
                                coolio.Redraw();
                    }
                }

                if (graphics.view.mode == MapView.ViewMode.ThreeDimensional && makeInGameCameraFollow)
                {
                    Config.Stream.SetValue(3, CamHackConfig.StructAddress + CamHackConfig.CameraModeOffset);
                    Config.Stream.SetValue(graphics.view.position.X, CamHackConfig.StructAddress + CamHackConfig.CameraXOffset);
                    Config.Stream.SetValue(graphics.view.position.Y, CamHackConfig.StructAddress + CamHackConfig.CameraYOffset);
                    Config.Stream.SetValue(graphics.view.position.Z, CamHackConfig.StructAddress + CamHackConfig.CameraZOffset);
                    var target = graphics.view.position + graphics.view.ComputeViewDirection();
                    Config.Stream.SetValue(target.X, CamHackConfig.StructAddress + CamHackConfig.FocusXOffset);
                    Config.Stream.SetValue(target.Y, CamHackConfig.StructAddress + CamHackConfig.FocusYOffset);
                    Config.Stream.SetValue(target.Z, CamHackConfig.StructAddress + CamHackConfig.FocusZOffset);
                }

                int numLevelTriangles = Config.Stream.GetInt32(TriangleConfig.LevelTriangleCountAddress);
                if (numLevelTriangles != _numLevelTriangles)
                {
                    RequireGeometryUpdate();
                    _numLevelTriangles = numLevelTriangles;
                }

                if (_needsGeometryRefreshInternal)
                {
                    needsGeometryRefresh = true;
                    _needsGeometryRefreshInternal = false;
                }
                else
                    needsGeometryRefresh = false;
            }
        }

        private void UpdateDataTab()
        {
            float marioX = Config.Stream.GetSingle(MarioConfig.StructAddress + MarioConfig.XOffset);
            float marioY = Config.Stream.GetSingle(MarioConfig.StructAddress + MarioConfig.YOffset);
            float marioZ = Config.Stream.GetSingle(MarioConfig.StructAddress + MarioConfig.ZOffset);

            int puX = PuUtilities.GetPuIndex(marioX);
            int puY = PuUtilities.GetPuIndex(marioY);
            int puZ = PuUtilities.GetPuIndex(marioZ);

            double qpuX = puX / 4.0;
            double qpuY = puY / 4.0;
            double qpuZ = puZ / 4.0;

            uint floorTriangleAddress = Config.Stream.GetUInt32(MarioConfig.StructAddress + MarioConfig.FloorTriangleOffset);
            float? yNorm = floorTriangleAddress == 0 ? (float?)null : Config.Stream.GetSingle(floorTriangleAddress + TriangleOffsetsConfig.NormY);

            byte level = Config.Stream.GetByte(MiscConfig.WarpDestinationAddress + MiscConfig.LevelOffset);
            byte area = Config.Stream.GetByte(MiscConfig.WarpDestinationAddress + MiscConfig.AreaOffset);
            ushort loadingPoint = Config.Stream.GetUInt16(MiscConfig.LoadingPointAddress);
            ushort missionLayout = Config.Stream.GetUInt16(MiscConfig.MissionAddress);

            MapLayout map = MapAssociations.GetBestMap();

            labelMapDataMapName.Text = map.Name;
            labelMapDataMapSubName.Text = map.SubName ?? "";
            labelMapDataPuCoordinateValues.Text = string.Format("[{0}:{1}:{2}]", puX, puY, puZ);
            labelMapDataQpuCoordinateValues.Text = string.Format("[{0}:{1}:{2}]", qpuX, qpuY, qpuZ);
            labelMapDataIdValues.Text = string.Format("[{0}:{1}:{2}:{3}]", level, area, loadingPoint, missionLayout);
            labelMapDataYNormValue.Text = yNorm?.ToString() ?? "(none)";
        }

        private void UpdateBasedOnObjectsSelectedOnMap()
        {
            foreach (var semaphore in quickSemaphores)
            {
                MapTracker semaphoreTracker = null;
                foreach (var tracker in flowLayoutPanelMapTrackers.EnumerateTrackers())
                    if (tracker.mapObject == semaphore.tracker.mapObject)
                    {
                        semaphoreTracker = tracker;
                        break;
                    }

                if (semaphoreTracker == null && semaphore.checkBox.Checked)
                    semaphore.checkBox.Checked = false;
                else if (semaphoreTracker != null && !semaphore.checkBox.Checked)
                    RemoveTracker(semaphoreTracker);
            }
        }

        private static readonly List<string> speedVarNames = new List<string>()
        {
            "2D Scroll Speed", "3D Scroll Speed", "3D Translate Speed", "3D Rotate Speed",
        };
        private static readonly List<string> inGameColoredVars = new List<string>() { };
        private static readonly List<string> cameraPosAndFocusColoredVars = new List<string>()
        {
            "Camera X", "Camera Y", "Camera Z", "Focus X", "Focus Y", "Focus Z", "FOV",
        };
        private static readonly List<string> cameraPosAndAngleColoredVars = new List<string>()
        {
            "Camera X", "Camera Y", "Camera Z", "Camera Yaw", "Camera Pitch", "Camera Roll", "FOV",
        };
        private static readonly List<string> followFocusRelativeAngleColoredVars = new List<string>()
        {
            "Focus Pos PA", "Focus Angle PA", "Following Radius", "Following Y Offset", "Following Yaw", "FOV",
        };
        private static readonly List<string> followFocusAbsoluteAngleColoredVars = new List<string>()
        {
            "Focus Pos PA", "Following Radius", "Following Y Offset", "Following Yaw", "FOV",
        };

        private void buttonMapOptionsLoadTrackerSettings_Click(object sender, EventArgs e)
        {
            var ctx = new ContextMenuStrip();
            var itemLoadLast = new ToolStripMenuItem("Load last");
            itemLoadLast.Enabled = lastTrackerFileName != null;
            itemLoadLast.Click += (_, __) => LoadTrackerConfigAndDisplay(lastTrackerFileName);
            ctx.Items.Add(itemLoadLast);

            var itemLoadDefaults = new ToolStripMenuItem("Load Defaults");
            itemLoadDefaults.Enabled = System.IO.File.Exists(DEFAULT_TRACKER_FILE);
            itemLoadDefaults.Click += (_, __) => LoadTrackerConfigAndDisplay(DEFAULT_TRACKER_FILE);
            ctx.Items.Add(itemLoadDefaults);

            var itemLoadFile = new ToolStripMenuItem("Load...");
            itemLoadFile.Click += (_, __) =>
            {
                var dlg = new OpenFileDialog();
                dlg.Filter = "Xml Config files (*.xml)|*.xml";
                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    LoadTrackerConfigAndDisplay(dlg.FileName);
                    lastTrackerFileName = dlg.FileName;
                    SaveConfig();
                }
            };
            ctx.Items.Add(itemLoadFile);

            ctx.Show(Cursor.Position);
        }

        void LoadTrackerConfigAndDisplay(string trackerFile)
        {
            if (LoadTrackerConfig(trackerFile, out var loadedTrackers))
            {
                foreach (var ctrl in flowLayoutPanelMapTrackers.Controls)
                    if (ctrl is MapTracker tracker && !externalTrackers.Contains(ctrl))
                        tracker.CleanUp();
                flowLayoutPanelMapTrackers.Controls.Clear();
                foreach (var tracker in loadedTrackers)
                    flowLayoutPanelMapTrackers.Controls.Add(tracker);
                foreach (var tracker in externalTrackers)
                    flowLayoutPanelMapTrackers.Controls.Add(tracker);
            }
            else
                MessageBox.Show("Failed to load tracker configuration!");
        }

        private void buttonMapOptionsSaveTrackerSettings_Click(object sender, EventArgs e)
        {
            var ctx = new ContextMenuStrip();
            var itemOverrideLast = new ToolStripMenuItem("Save last");
            itemOverrideLast.Enabled = lastTrackerFileName != null;
            itemOverrideLast.Click += (_, __) => SaveTrackerConfig(lastTrackerFileName);
            ctx.Items.Add(itemOverrideLast);

            var itemSaveDefault = new ToolStripMenuItem("Save default");
            itemSaveDefault.Click += (_, __) => SaveTrackerConfig(DEFAULT_TRACKER_FILE);
            ctx.Items.Add(itemSaveDefault);

            var itemSaveFile = new ToolStripMenuItem("Save as...");
            itemSaveFile.Click += (_, __) =>
            {
                var dlg = new SaveFileDialog();
                dlg.Filter = "Xml Config files (*.xml)|*.xml";
                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    SaveTrackerConfig(dlg.FileName);
                    lastTrackerFileName = dlg.FileName;
                    SaveConfig();
                }
            };
            ctx.Items.Add(itemSaveFile);

            ctx.Show(Cursor.Position);
        }

        bool LoadTrackerConfig(string targetFileName, out List<MapTracker> loadedTrackers)
        {
            loadedTrackers = new List<MapTracker>();
            try
            {
                var doc = new System.Xml.XmlDocument();
                doc.Load(targetFileName);

                foreach (System.Xml.XmlNode n in doc.FirstChild.ChildNodes)
                    if (n.NodeType == System.Xml.XmlNodeType.Element
                        && newTrackerByName.TryGetValue(n.Name, out var newObjFunc))
                    {
                        var createParamsNode = n.SelectSingleNode("CreationParameters");
                        var newObj = newObjFunc(createParamsNode != null ? new ObjectCreateParams(createParamsNode) : null);
                        if (newObj != null)
                        {
                            loadedTrackers.Add(newObj);
                            loadedTrackers.AddRange(newObj.LoadChildTrackers(n));
                        }
                    }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.ToString());
                foreach (var a in loadedTrackers)
                    a.mapObject.CleanUp();
                loadedTrackers = new List<MapTracker>();
                return false;
            }
            return true;
        }

        void SaveTrackerConfig(string targetFileName)
        {
            var doc = new System.Xml.XmlDocument();
            var root = doc.CreateElement("Trackers");
            foreach (var control in flowLayoutPanelMapTrackers.Controls)
                if (control is MapTracker tracker && tracker.parentTracker == null && tracker.creationIdentifier != null)
                {
                    var trackerNode = doc.CreateElement(tracker.creationIdentifier);
                    if (tracker.mapObject.creationParameters != null)
                        trackerNode.AppendChild(tracker.mapObject.creationParameters.CreateDocumentNode("CreationParameters", doc));
                    tracker.mapObject.SettingsSaveLoad.save(trackerNode);
                    tracker.SaveChildTrackers(trackerNode);
                    root.AppendChild(trackerNode);
                }
            doc.AppendChild(root);
            doc.Save(targetFileName);
        }

        private void comboBoxViewMode_SelectedIndexChanged(object sender, EventArgs e)
        {
            graphics.view.mode = (MapView.ViewMode)comboBoxViewMode.SelectedIndex;
        }
    }
}
