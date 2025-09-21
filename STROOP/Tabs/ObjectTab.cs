using STROOP.Structs;
using STROOP.Utilities;
using STROOP.Managers;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using STROOP.Structs.Configurations;
using STROOP.Forms;
using STROOP.Models;
using STROOP.Core.Variables;

namespace STROOP.Tabs
{
    public partial class ObjectTab : STROOPTab
    {
        Image _multiImage = null;
        List<BehaviorCriteria> _lastBehaviors = new List<BehaviorCriteria>();
        BehaviorCriteria? _lastGeneralizedBehavior;

        #region UI Properties
        string _slotIndex;
        public string SlotIndex
        {
            get
            {
                return _slotIndex;
            }
            set
            {
                if (_slotIndex != value)
                {
                    _slotIndex = value;
                    labelObjSlotIndValue.Text = _slotIndex;
                }
            }
        }

        string _slotPos;
        public string SlotPos
        {
            get
            {
                return _slotPos;
            }
            set
            {
                if (_slotPos != value)
                {
                    _slotPos = value;
                    labelObjSlotPosValue.Text = _slotPos;
                }
            }
        }

        string _behavior;
        public string Behavior
        {
            get
            {
                return _behavior;
            }
            set
            {
                if (_behavior != value)
                {
                    _behavior = value;
                    labelObjBhvValue.Text = value;
                }
            }
        }

        public string ObjectName
        {
            get
            {
                return textBoxObjName.Text;
            }
            set
            {
                if (textBoxObjName.Text != value)
                    textBoxObjName.Text = value;
            }
        }

        public Color ObjectBackColor
        {
            set
            {
                if (panelObjectBorder.BackColor != value)
                {
                    panelObjectBorder.BackColor = value;
                    pictureBoxObject.BackColor = value.Lighten(0.7);
                }
            }
            get
            {
                return panelObjectBorder.BackColor;
            }
        }

        public Image Image
        {
            get
            {
                return pictureBoxObject.Image;
            }
            set
            {
                if (pictureBoxObject.Image != value)
                    pictureBoxObject.Image = value;
            }
        }
        #endregion

        private HashSet<uint> _addresses
        {
            get => Config.StroopMainForm.ObjectSlotsManager.SelectedSlotsAddresses;
        }
        ObjectSlotsManager _objectSlots;
        private List<ObjectDataModel> _objects => _objectSlots.SelectedObjects;

        private static readonly List<string> ALL_VAR_GROUPS =
            new List<string>()
            {
                VariableGroup.Basic,
                VariableGroup.Intermediate,
                VariableGroup.Advanced,
                VariableGroup.ObjectSpecific,
                VariableGroup.ProcessGroup,
                VariableGroup.Collision,
                VariableGroup.Movement,
                VariableGroup.Transformation,
                VariableGroup.Coordinate,
                VariableGroup.Rng,
                VariableGroup.Custom,
            };

        private static readonly List<string> VISIBLE_VAR_GROUPS =
            new List<string>()
            {
                VariableGroup.Basic,
                VariableGroup.Intermediate,
                VariableGroup.ObjectSpecific,
                VariableGroup.Custom,
            };

        public ObjectTab()
        {
            InitializeComponent();
            watchVariablePanelObject.SetGroups(ALL_VAR_GROUPS, VISIBLE_VAR_GROUPS);
        }

        public override string GetDisplayName() => "Object";

        public override void InitializeTab()
        {
            base.InitializeTab();
            this.watchVariablePanelObject.SetGroups(ALL_VAR_GROUPS, VISIBLE_VAR_GROUPS);
            _objectSlots = Config.StroopMainForm.ObjectSlotsManager;

            labelObjAddValue.Click += ObjAddressLabel_Click;
            labelObjAdd.Click += ObjAddressLabel_Click;

            Panel objPanel = splitContainerObject.Panel1.Controls["panelObj"] as Panel;

            buttonObjGoto.Click += (sender, e) => ButtonUtilities.GotoObjects(_objects);
            ControlUtilities.AddContextMenuStripFunctions(
                buttonObjGoto,
                new List<string>() { "Goto", "Goto Laterally", "Goto X", "Goto Y", "Goto Z", null, "Goto Center Top", "Goto Center Laterally" },
                new List<Action>() {
                    () => ButtonUtilities.GotoObjects(_objects, (true, true, true)),
                    () => ButtonUtilities.GotoObjects(_objects, (true, false, true)),
                    () => ButtonUtilities.GotoObjects(_objects, (true, false, false)),
                    () => ButtonUtilities.GotoObjects(_objects, (false, true, false)),
                    () => ButtonUtilities.GotoObjects(_objects, (false, false, true)),
                    () => { },
                    () => ButtonUtilities.GotoObjectsCenter(_objects, false),
                    () => ButtonUtilities.GotoObjectsCenter(_objects, true),
                });

            buttonObjRetrieve.Click += (sender, e) => ButtonUtilities.RetrieveObjects(_objects);
            ControlUtilities.AddContextMenuStripFunctions(
                buttonObjRetrieve,
                new List<string>() { "Retrieve", "Retrieve Laterally", "Retrieve X", "Retrieve Y", "Retrieve Z" },
                new List<Action>() {
                    () => ButtonUtilities.RetrieveObjects(_objects, (true, true, true)),
                    () => ButtonUtilities.RetrieveObjects(_objects, (true, false, true)),
                    () => ButtonUtilities.RetrieveObjects(_objects, (true, false, false)),
                    () => ButtonUtilities.RetrieveObjects(_objects, (false, true, false)),
                    () => ButtonUtilities.RetrieveObjects(_objects, (false, false, true)),
                });

            buttonObjGotoHome.Click += (sender, e) => ButtonUtilities.GotoObjectsHome(_objects);
            ControlUtilities.AddContextMenuStripFunctions(
                buttonObjGotoHome,
                new List<string>()
                {
                    "Goto Home",
                    "Goto Home Laterally",
                    "Goto Home X",
                    "Goto Home Y",
                    "Goto Home Z",
                    null,
                    "Object Goto Home",
                    "Object Goto Home Laterally",
                    "Object Goto Home X",
                    "Object Goto Home Y",
                    "Object Goto Home Z",
                },
                new List<Action>()
                {
                    () => ButtonUtilities.GotoObjectsHome(_objects, (true, true, true)),
                    () => ButtonUtilities.GotoObjectsHome(_objects, (true, false, true)),
                    () => ButtonUtilities.GotoObjectsHome(_objects, (true, false, false)),
                    () => ButtonUtilities.GotoObjectsHome(_objects, (false, true, false)),
                    () => ButtonUtilities.GotoObjectsHome(_objects, (false, false, true)),
                    () => { },
                    () => ButtonUtilities.ObjectGotoObjectsHome(_objects, (true, true, true)),
                    () => ButtonUtilities.ObjectGotoObjectsHome(_objects, (true, false, true)),
                    () => ButtonUtilities.ObjectGotoObjectsHome(_objects, (true, false, false)),
                    () => ButtonUtilities.ObjectGotoObjectsHome(_objects, (false, true, false)),
                    () => ButtonUtilities.ObjectGotoObjectsHome(_objects, (false, false, true)),
                });

            buttonObjRetrieveHome.Click += (sender, e) => ButtonUtilities.RetrieveObjectsHome(_objects);
            ControlUtilities.AddContextMenuStripFunctions(
                buttonObjRetrieveHome,
                new List<string>()
                {
                    "Retrieve Home",
                    "Retrieve Home Laterally",
                    "Retrieve Home X",
                    "Retrieve Home Y",
                    "Retrieve Home Z",
                    null,
                    "Retrieve Home to Object",
                    "Retrieve Home Laterally to Object",
                    "Retrieve Home X to Object",
                    "Retrieve Home Y to Object",
                    "Retrieve Home Z to Object",
                },
                new List<Action>() {
                    () => ButtonUtilities.RetrieveObjectsHome(_objects, (true, true, true)),
                    () => ButtonUtilities.RetrieveObjectsHome(_objects, (true, false, true)),
                    () => ButtonUtilities.RetrieveObjectsHome(_objects, (true, false, false)),
                    () => ButtonUtilities.RetrieveObjectsHome(_objects, (false, true, false)),
                    () => ButtonUtilities.RetrieveObjectsHome(_objects, (false, false, true)),
                    () => { },
                    () => ButtonUtilities.RetrieveObjectsHomeToObject(_objects, (true, true, true)),
                    () => ButtonUtilities.RetrieveObjectsHomeToObject(_objects, (true, false, true)),
                    () => ButtonUtilities.RetrieveObjectsHomeToObject(_objects, (true, false, false)),
                    () => ButtonUtilities.RetrieveObjectsHomeToObject(_objects, (false, true, false)),
                    () => ButtonUtilities.RetrieveObjectsHomeToObject(_objects, (false, false, true)),
                });

            buttonObjRelease.Initialize(
                "Release",
                "UnRelease",
                () => ButtonUtilities.ReleaseObject(_objects),
                () => ButtonUtilities.UnReleaseObject(_objects),
                () => _objects.Count > 0 && _objects.All(o =>
                    o.ReleaseStatus == ObjectConfig.ReleaseStatusThrownValue
                    || o.ReleaseStatus == ObjectConfig.ReleaseStatusDroppedValue));
            ControlUtilities.AddContextMenuStripFunctions(
                buttonObjRelease,
                new List<string>() { "Release by Throwing", "Release by Dropping", "UnRelease" },
                new List<Action>() {
                    () => ButtonUtilities.ReleaseObject(_objects, true),
                    () => ButtonUtilities.ReleaseObject(_objects, false),
                    () => ButtonUtilities.UnReleaseObject(_objects),
                });

            buttonObjInteract.Initialize(
                "Interact",
                "UnInteract",
                () => ButtonUtilities.InteractObject(_objects),
                () => ButtonUtilities.UnInteractObject(_objects),
                () => _objects.Count > 0 && _objects.All(o => o.InteractionStatus != 0));
            ControlUtilities.AddContextMenuStripFunctions(
                buttonObjInteract,
                new List<string>() { "Interact", "UnInteract" },
                new List<Action>() {
                    () => ButtonUtilities.InteractObject(_objects),
                    () => ButtonUtilities.UnInteractObject(_objects),
                });

            buttonObjClone.Initialize(
                "Clone",
                "UnClone",
                () => ButtonUtilities.CloneObject(_objects.FirstOrDefault()),
                () => ButtonUtilities.UnCloneObject(),
                () => _objects.Count > 0 && _objects.FirstOrDefault().Address == DataModels.Mario.HeldObject);
            ControlUtilities.AddContextMenuStripFunctions(
                buttonObjClone,
                new List<string>() {
                    "Clone with Action Update",
                    "Clone without Action Update",
                    "UnClone with Action Update",
                    "UnClone without Action Update",
                },
                new List<Action>() {
                    () => ButtonUtilities.CloneObject(_objects.FirstOrDefault(), true),
                    () => ButtonUtilities.CloneObject(_objects.FirstOrDefault(), false),
                    () => ButtonUtilities.UnCloneObject(true),
                    () => ButtonUtilities.UnCloneObject(false),
                });

            buttonObjUnload.Initialize(
                "Unload",
                "Revive",
                () => ButtonUtilities.UnloadObject(_objects),
                () => ButtonUtilities.ReviveObject(_objects),
                () => _objects.Count > 0 && _objects.All(o => !o.IsActive));
            ControlUtilities.AddContextMenuStripFunctions(
                buttonObjUnload,
                new List<string>() { "Unload", "Revive" },
                new List<Action>() {
                    () => ButtonUtilities.UnloadObject(_objects),
                    () => ButtonUtilities.ReviveObject(_objects),
                });

            buttonObjRide.Initialize(
                "Ride",
                "UnRide",
                () => ButtonUtilities.RideObject(_objects.FirstOrDefault()),
                () => ButtonUtilities.UnRideObject(),
                () => _objects.Count > 0 && _objects.FirstOrDefault().Address == DataModels.Mario.RiddenObject);
            ControlUtilities.AddContextMenuStripFunctions(
                buttonObjRide,
                new List<string>() {
                    "Ride with Action Update",
                    "Ride without Action Update",
                    "UnRide with Action Update",
                    "UnRide without Action Update",
                },
                new List<Action>() {
                    () => ButtonUtilities.RideObject(_objects.FirstOrDefault(), true),
                    () => ButtonUtilities.RideObject(_objects.FirstOrDefault(), false),
                    () => ButtonUtilities.UnRideObject(true),
                    () => ButtonUtilities.UnRideObject(false),
        });

            buttonObjUkikipedia.Click += (sender, e) => ButtonUtilities.UkikipediaObject(_objects.FirstOrDefault());

            ControlUtilities.InitializeThreeDimensionController(
                CoordinateSystem.Euler,
                true,
                groupBoxObjPos,
                "ObjPos",
                (float hOffset, float vOffset, float nOffset, bool useRelative) =>
                {
                    ButtonUtilities.TranslateObjects(
                        _objects,
                        hOffset,
                        nOffset,
                        -1 * vOffset,
                        useRelative,
                        GlobalKeyboard.IsCtrlDown(),
                        GlobalKeyboard.IsAltDown());
                });

            ControlUtilities.InitializeScalarController(
                buttonObjAngleYawN,
                buttonObjAngleYawP,
                textBoxObjAngleYaw,
                (float yawValue) =>
                {
                    ButtonUtilities.RotateObjects(_objects, (int)Math.Round(yawValue), 0, 0, GlobalKeyboard.IsCtrlDown(), GlobalKeyboard.IsAltDown());
                });
            ControlUtilities.InitializeScalarController(
                buttonObjAnglePitchN,
                buttonObjAnglePitchP,
                textBoxObjAnglePitch,
                (float pitchValue) =>
                {
                    ButtonUtilities.RotateObjects(_objects, 0, (int)Math.Round(pitchValue), 0, GlobalKeyboard.IsCtrlDown(), GlobalKeyboard.IsAltDown());
                });
            ControlUtilities.InitializeScalarController(
                buttonObjAngleRollN,
                buttonObjAngleRollP,
                textBoxObjAngleRoll,
                (float rollValue) =>
                {
                    ButtonUtilities.RotateObjects(_objects, 0, 0, (int)Math.Round(rollValue), GlobalKeyboard.IsCtrlDown(), GlobalKeyboard.IsAltDown());
                });

            ControlUtilities.InitializeScaleController(
                buttonObjScaleWidthN,
                buttonObjScaleWidthP,
                buttonObjScaleHeightN,
                buttonObjScaleHeightP,
                buttonObjScaleDepthN,
                buttonObjScaleDepthP,
                buttonObjScaleAggregateN,
                buttonObjScaleAggregateP,
                textBoxObjScaleWidth,
                textBoxObjScaleHeight,
                textBoxObjScaleDepth,
                textBoxObjScaleAggregate,
                checkBoxObjScaleAggregate,
                checkBoxObjScaleMultiply,
                (float widthChange, float heightChange, float depthChange, bool multiply) =>
                {
                    ButtonUtilities.ScaleObjects(_objects, widthChange, heightChange, depthChange, multiply);
                });

            ControlUtilities.InitializeThreeDimensionController(
                CoordinateSystem.Euler,
                true,
                groupBoxObjHome,
                "ObjHome",
                (float hOffset, float vOffset, float nOffset, bool useRelative) =>
                {
                    ButtonUtilities.TranslateObjectHomes(
                        _objects,
                        hOffset,
                        nOffset,
                        -1 * vOffset,
                        useRelative);
                });
        }

        // Having an empty action assigned to this adds the context menu entry to the ObjectSlot controls
        public override Action<IEnumerable<ObjectSlot>> objectSlotsClicked => objectSlots => { };
        
        public void SetBehaviorWatchVariables(IEnumerable<NamedVariableCollection.IView> watchVars, Color color)
        {
            watchVariablePanelObject.RemoveVariableGroup(VariableGroup.ObjectSpecific);
            foreach (var ctrl in watchVariablePanelObject.AddVariables(watchVars))
                ctrl.BaseColor = color;
        }

        private void ObjAddressLabel_Click(object sender, EventArgs e)
        {
            if (_objects.Count == 0)
                return;

            var variableTitle = "Object Address" + (_objects.Count > 1 ? " (First of Multiple)" : "");
            var variableInfo = new VariableViewerForm(
                variableTitle,
                "(none)",
                "Object",
                "Relative + " + HexUtilities.FormatValue(_objects.First().Address, 8),
                HexUtilities.FormatValue(0, 8).ToString(),
                HexUtilities.FormatValue(_objects.First().Address, 8),
                HexUtilities.FormatValue(Config.Stream.GetAbsoluteAddress(_objects.First().Address).ToUInt32(), 8),
                HexUtilities.FormatValue(_objects.First().Address, 8)
            );
            variableInfo.Show();
        }

        public override void Update(bool updateView)
        {
            if (!updateView)
                return;

            buttonObjRelease.UpdateButton();
            buttonObjInteract.UpdateButton();
            buttonObjClone.UpdateButton();
            buttonObjUnload.UpdateButton();
            buttonObjRide.UpdateButton();

            UpdateUI();

            base.Update(updateView);
        }

        void UpdateUI()
        {
            if (!_objects.Any())
            {
                ObjectName = "No Object Selected";
                Image = null;
                ObjectBackColor = ObjectSlotsConfig.VacantSlotColor;
                Behavior = "";
                SlotIndex = "";
                SlotPos = "";
                labelObjAddValue.Text = "";
                _lastGeneralizedBehavior = null;
                SetBehaviorWatchVariables(Array.Empty<NamedVariableCollection.IView>(), Color.White);
            }
            else if (_objects.Count() == 1)
            {
                ObjectDataModel obj = _objects.First();
                var newBehavior = obj.BehaviorCriteria;
                if (!BehaviorCriteria.HasSameAssociation(_lastGeneralizedBehavior, newBehavior))
                {
                    Behavior = $"0x{obj.SegmentedBehavior & 0x00FFFFFF:X4}";
                    SetBehaviorWatchVariables(
                        Config.ObjectAssociations.GetWatchVarControls(newBehavior),
                        ObjectSlotsConfig.GetProcessingGroupColor(obj.BehaviorProcessGroup)
                        .Lighten(0.8));
                    _lastGeneralizedBehavior = newBehavior;
                }
                ObjectName = Config.ObjectAssociations.GetObjectName(newBehavior);
                Image = Config.ObjectAssociations.GetObjectImage(newBehavior).Value;
                ObjectBackColor = ObjectSlotsConfig.GetProcessingGroupColor(obj.CurrentProcessGroup);
                int slotPos = obj.VacantSlotIndex ?? obj.ProcessIndex;
                SlotIndex = (Config.StroopMainForm.ObjectSlotsManager.GetSlotIndexFromObj(obj)
                    + (SavedSettingsConfig.StartSlotIndexsFromOne ? 1 : 0))?.ToString() ?? "";
                SlotPos = $"{(obj.VacantSlotIndex.HasValue ? "VS " : "")}{slotPos + (SavedSettingsConfig.StartSlotIndexsFromOne ? 1 : 0)}";
                labelObjAddValue.Text = $"0x{_objects.First().Address:X8}";
            }
            else
            {
                List<BehaviorCriteria> newBehaviors = _objects.ConvertAll(o => o.BehaviorCriteria);

                // Find new generalized criteria
                BehaviorCriteria? multiBehavior = _objects.First().BehaviorCriteria;
                foreach (ObjectDataModel obj in _objects)
                    multiBehavior = multiBehavior?.Generalize(obj.BehaviorCriteria);

                // Find general process group
                byte? processGroup = _objects.First().CurrentProcessGroup;
                if (_objects.Any(o => o.CurrentProcessGroup != processGroup)) processGroup = null;

                // Update behavior and watach variables
                if (_lastGeneralizedBehavior != multiBehavior)
                {
                    if (multiBehavior.HasValue)
                    {
                        Behavior = $"0x{multiBehavior.Value.BehaviorAddress:X4}";
                        SetBehaviorWatchVariables(
                            Config.ObjectAssociations.GetWatchVarControls(multiBehavior.Value),
                            ObjectSlotsConfig.GetProcessingGroupColor(processGroup).Lighten(0.8));
                    }
                    else
                    {
                        Behavior = "";
                        SetBehaviorWatchVariables(Array.Empty<NamedVariableCollection.IView>(), Color.White);
                    }
                    _lastGeneralizedBehavior = multiBehavior;
                }
                if (!newBehaviors.SequenceEqual(_lastBehaviors))
                {
                    // Generate new image
                    _multiImage?.Dispose();
                    List<Image> images = newBehaviors.ConvertAll(
                        criteria => Config.ObjectAssociations.GetObjectImage(criteria, false).Value);
                    _multiImage = ImageUtilities.CreateMultiImage(images, 256, 256);

                    _lastBehaviors = newBehaviors.ToList();
                }

                Image = _multiImage;
                ObjectName = _objects.Count + " Objects Selected";
                ObjectBackColor = ObjectSlotsConfig.GetProcessingGroupColor(processGroup);
                SlotIndex = "";
                SlotPos = "";
                labelObjAddValue.Text = "";
            }
        }
    }
}
