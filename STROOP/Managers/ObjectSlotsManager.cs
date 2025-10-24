using STROOP.Core.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using STROOP.Structs;
using STROOP.Utilities;
using System.Drawing;
using STROOP.Structs.Configurations;
using STROOP.Models;
using STROOP.Variables;
using STROOP.Variables.SM64MemoryLayout;
using STROOP.Variables.Utilities;
using System.Collections.ObjectModel;

namespace STROOP.Managers
{
    public class ObjectSlotsManager
    {
        /// <summary>
        /// The default size of the object slot UI element
        /// </summary>
        public static readonly int DefaultSlotSize = 36;

        public enum SortMethodType
        {
            ProcessingOrder,
            MemoryOrder,
            DistanceToMario
        };

        public enum SlotLabelType
        {
            Recommended,
            SlotPosVs,
            SlotPos,
            SlotIndex,
            RngUsage
        };

        public enum SelectionMethodType
        {
            Clicked,
            Held,
            StoodOn,
            Interaction,
            Used,
            Floor,
            Wall,
            Ceiling,
            Closest
        };

        [InitializeBaseAddress]
        static void InitializeBaseAddress()
        {
            VariableUtilities.baseAddressGetters[BaseAddressType.Object] = () => Config.ObjectSlotsManager.SelectedSlotsAddresses;
            VariableUtilities.baseAddressGetters[BaseAddressType.ProcessGroup] = () =>
                Config.ObjectSlotsManager.SelectedObjects.ConvertAll(obj => obj.CurrentProcessGroup ?? uint.MaxValue);

            VariableUtilities.baseAddressGetters["Graphics"] = () =>
                Config.ObjectSlotsManager.SelectedSlotsAddresses.ConvertAll(objAddress => Config.Stream.GetUInt32(objAddress + ObjectConfig.BehaviorGfxOffset));

            VariableUtilities.baseAddressGetters["Animation"] = () =>
                Config.ObjectSlotsManager.SelectedSlotsAddresses.ConvertAll(objAddress => Config.Stream.GetUInt32(objAddress + ObjectConfig.AnimationOffset));

            VariableUtilities.baseAddressGetters["Waypoint"] = () =>
                Config.ObjectSlotsManager.SelectedSlotsAddresses.ConvertAll(objAddress => Config.Stream.GetUInt32(objAddress + ObjectConfig.WaypointOffset));
        }

        public uint? HoveredObjectAddress;

        public List<ObjectSlot> ObjectSlots;

        Dictionary<uint, Tuple<int?, int?>> _lockedSlotIndices = new Dictionary<uint, Tuple<int?, int?>>();
        public bool LabelsLocked = false;

        public readonly HashSet<uint> SelectedSlotsAddresses = new HashSet<uint>();

        public readonly Dictionary<uint, int> MarkedSlotsAddressesDictionary = new Dictionary<uint, int>();

        public List<ObjectDataModel> SelectedObjects = new List<ObjectDataModel>();

        private Dictionary<ObjectDataModel, string> _slotLabels = new Dictionary<ObjectDataModel, string>();
        public IReadOnlyDictionary<ObjectDataModel, string> SlotLabelsForObjects { get; private set; }

        public SortMethodType SortMethod = SortMethodType.ProcessingOrder;
        public SlotLabelType LabelMethod = SlotLabelType.Recommended;

        public readonly StroopMainForm mainForm;

        int slotSize = -1;
        public float _fontHeight { get; private set; }
        public Font Font { get; private set; }

        public ObjectSlotsManager(StroopMainForm mainForm, TabControl tabControlMain)
        {
            this.mainForm = mainForm;
            // Add SortMethods adn LabelMethods
            mainForm.comboBoxSortMethod.DataSource = Enum.GetValues(typeof(SortMethodType));
            mainForm.comboBoxLabelMethod.DataSource = Enum.GetValues(typeof(SlotLabelType));
            mainForm.comboBoxSelectionMethod.DataSource = Enum.GetValues(typeof(SelectionMethodType));

            // Create and setup object slots
            ObjectSlots = new List<ObjectSlot>();
            for (int i = 0; i < ObjectSlotsConfig.MaxSlots; i++)
            {
                var objectSlot = new ObjectSlot(this, i, new Size(DefaultSlotSize, DefaultSlotSize));
                objectSlot.Click += (sender, e) => OnSlotClick(sender, e);
                ObjectSlots.Add(objectSlot);
                mainForm.VariablePanelObjects.Controls.Add(objectSlot);
            }

            ;

            SlotLabelsForObjects = new ReadOnlyDictionary<ObjectDataModel, string>(_slotLabels);
            ChangeSlotSize(DefaultSlotSize);
        }

        public void ChangeSlotSize(int newSize)
        {
            if (newSize != slotSize)
            {
                slotSize = newSize;
                Font?.Dispose();
                Font = new Font(FontFamily.GenericSansSerif, Math.Max(6, 6 / 40.0f * slotSize));
                _fontHeight = Font.GetHeight(mainForm.CreateGraphics().DpiY);
            }

            foreach (var objSlot in ObjectSlots)
                objSlot.Size = new Size(newSize, newSize);
        }

        private void OnSlotClick(object sender, EventArgs e)
        {
            // Make sure the tab has loaded
            if (mainForm.tabControlMain.SelectedTab == null)
                return;

            ObjectSlot selectedSlot = sender as ObjectSlot;
            selectedSlot.Focus();

            bool isCtrlKeyHeld = GlobalKeyboard.IsCtrlDown();
            bool isShiftKeyHeld = GlobalKeyboard.IsShiftDown();
            bool isAltKeyHeld = GlobalKeyboard.IsAltDown();
            int? numberHeld = GlobalKeyboard.GetCurrentlyInputtedNumber();

            DoSlotClickUsingInput(selectedSlot, isCtrlKeyHeld, isShiftKeyHeld, isAltKeyHeld, numberHeld);
        }

        private void DoSlotClickUsingInput(
            ObjectSlot selectedSlot, bool isCtrlKeyHeld, bool isShiftKeyHeld, bool isAltKeyHeld, int? numberHeld)
        {
            bool isMarking = isAltKeyHeld || numberHeld.HasValue;
            int? markedColor = isAltKeyHeld ? 10 : numberHeld;
            bool shouldExtendRange = isShiftKeyHeld;
            TabPage tabDestination = isMarking ? null : mainForm.tabControlMain.SelectedTab;
            List<ObjectSlot> newSelection;
            if (isMarking)
            {
                if (shouldExtendRange)
                {
                    var markedSlots = new HashSet<uint>();
                    foreach (var kvp in MarkedSlotsAddressesDictionary)
                        markedSlots.Add(kvp.Key);
                    newSelection = ExtendSelection(selectedSlot, markedSlots);
                }
                else
                    newSelection = new List<ObjectSlot>() { selectedSlot };

                foreach (var objSlot in newSelection)
                    MarkedSlotsAddressesDictionary[objSlot.CurrentObject.Address] = markedColor.Value;
            }
            else
                foreach (var ctrl in tabDestination.Controls)
                    if (ctrl is Tabs.STROOPTab stroopTab)
                    {
                        var selection = stroopTab.selection;
                        if (selection == Config.ObjectSlotsManager.SelectedSlotsAddresses)
                            mainForm.comboBoxSelectionMethod.SelectedItem = SelectionMethodType.Clicked;
                        if (selection == null)
                            newSelection = new List<ObjectSlot>() { selectedSlot };
                        else if (shouldExtendRange)
                            newSelection = ExtendSelection(selectedSlot, selection);
                        else
                        {
                            selection.Clear();
                            selection.Add(selectedSlot.CurrentObject.Address);
                            newSelection = new List<ObjectSlot>() { selectedSlot };
                        }

                        stroopTab.objectSlotsClicked?.Invoke(newSelection);
                    }
        }

        public void SelectSlotByAddress(uint address)
        {
            ObjectSlot slot = ObjectSlots.FirstOrDefault(s => s.CurrentObject.Address == address);
            if (slot != null) DoSlotClickUsingInput(slot, false, false, false, null);
        }

        List<ObjectSlot> ExtendSelection(ObjectSlot selectedSlot, HashSet<uint> selection)
        {
            var newSelection = new List<ObjectSlot>();
            int? startRange = ObjectSlots.FirstOrDefault(s => s.CurrentObject.Address == selection.Last())?.Index;
            int endRange = selectedSlot.Index;

            if (startRange.HasValue)
            {
                int rangeSize = Math.Abs(endRange - startRange.Value);
                int iteratorDirection = endRange > startRange ? 1 : -1;

                for (int i = 0; i <= rangeSize; i++)
                {
                    int index = startRange.Value + i * iteratorDirection;
                    uint address = ObjectSlots[index].CurrentObject.Address;
                    selection.Add(address);
                    newSelection.Add(ObjectSlots[index]);
                }
            }

            return newSelection;
        }

        public void MarkAddresses(List<uint> addresses, int markColor)
        {
            foreach (uint address in addresses)
                MarkedSlotsAddressesDictionary[address] = markColor;
        }

        public void UnmarkAddresses(List<uint> addresses)
        {
            foreach (uint address in addresses)
                MarkedSlotsAddressesDictionary.Remove(address);
        }

        public void Update()
        {
            UpdateSelectionMethod();

            LabelMethod = (SlotLabelType)mainForm.comboBoxLabelMethod.SelectedItem;
            SortMethod = (SortMethodType)mainForm.comboBoxSortMethod.SelectedItem;

            // Processing sort order
            IEnumerable<ObjectDataModel> sortedObjects;
            switch (SortMethod)
            {
                case SortMethodType.ProcessingOrder:
                    // Data is already sorted by processing order
                    sortedObjects = DataModels.Objects.OrderBy(o => o?.ProcessIndex);
                    break;

                case SortMethodType.MemoryOrder:
                    // Order by address
                    sortedObjects = DataModels.Objects.OrderBy(o => o?.Address);
                    break;

                case SortMethodType.DistanceToMario:

                    // Order by address
                    var activeObjects = DataModels.Objects.Where(o => o?.IsActive ?? false).OrderBy(o => o?.DistanceToMarioCalculated);
                    var inActiveObjects = DataModels.Objects.Where(o => !o?.IsActive ?? true).OrderBy(o => o?.DistanceToMarioCalculated);

                    sortedObjects = activeObjects.Concat(inActiveObjects);
                    break;
                default:
                    throw new ArgumentOutOfRangeException("Uknown sort method type");
            }

            // Update slots
            UpdateSlots(sortedObjects);

            List<ObjectDataModel> objs = DataModels.Objects.Where(o => o != null && SelectedSlotsAddresses.Contains(o.Address)).ToList();
            SelectedObjects = objs;
        }

        private void UpdateSelectionMethod()
        {
            SelectionMethodType selectionMethodType = (SelectionMethodType)mainForm.comboBoxSelectionMethod.SelectedItem;
            switch (selectionMethodType)
            {
                case SelectionMethodType.Clicked:
                    // do nothing
                    break;
                case SelectionMethodType.Held:
                    SelectedSlotsAddresses.Clear();
                    uint heldObjectAddress = Config.Stream.GetUInt32(MarioConfig.StructAddress + MarioConfig.HeldObjectPointerOffset);
                    if (heldObjectAddress != 0) SelectedSlotsAddresses.Add(heldObjectAddress);
                    break;
                case SelectionMethodType.StoodOn:
                    SelectedSlotsAddresses.Clear();
                    uint stoodOnObjectAddress = Config.Stream.GetUInt32(MarioConfig.StoodOnObjectPointerAddress);
                    if (stoodOnObjectAddress != 0) SelectedSlotsAddresses.Add(stoodOnObjectAddress);
                    break;
                case SelectionMethodType.Interaction:
                    SelectedSlotsAddresses.Clear();
                    uint interactionObjectAddress = Config.Stream.GetUInt32(MarioConfig.StructAddress + MarioConfig.InteractionObjectPointerOffset);
                    if (interactionObjectAddress != 0) SelectedSlotsAddresses.Add(interactionObjectAddress);
                    break;
                case SelectionMethodType.Used:
                    SelectedSlotsAddresses.Clear();
                    uint usedObjectAddress = Config.Stream.GetUInt32(MarioConfig.StructAddress + MarioConfig.UsedObjectPointerOffset);
                    if (usedObjectAddress != 0) SelectedSlotsAddresses.Add(usedObjectAddress);
                    break;
                case SelectionMethodType.Floor:
                    SelectedSlotsAddresses.Clear();
                    uint floorTriangleAddress = Config.Stream.GetUInt32(MarioConfig.StructAddress + MarioConfig.FloorTriangleOffset);
                    if (floorTriangleAddress == 0) break;
                    uint floorObjectAddress = Config.Stream.GetUInt32(floorTriangleAddress + TriangleOffsetsConfig.AssociatedObject);
                    if (floorObjectAddress != 0) SelectedSlotsAddresses.Add(floorObjectAddress);
                    break;
                case SelectionMethodType.Wall:
                    SelectedSlotsAddresses.Clear();
                    uint wallTriangleAddress = Config.Stream.GetUInt32(MarioConfig.StructAddress + MarioConfig.WallTriangleOffset);
                    if (wallTriangleAddress == 0) break;
                    uint wallObjectAddress = Config.Stream.GetUInt32(wallTriangleAddress + TriangleOffsetsConfig.AssociatedObject);
                    if (wallObjectAddress != 0) SelectedSlotsAddresses.Add(wallObjectAddress);
                    break;
                case SelectionMethodType.Ceiling:
                    SelectedSlotsAddresses.Clear();
                    uint ceilingTriangleAddress = Config.Stream.GetUInt32(MarioConfig.StructAddress + MarioConfig.CeilingTriangleOffset);
                    if (ceilingTriangleAddress == 0) break;
                    uint ceilingObjectAddress = Config.Stream.GetUInt32(ceilingTriangleAddress + TriangleOffsetsConfig.AssociatedObject);
                    if (ceilingObjectAddress != 0) SelectedSlotsAddresses.Add(ceilingObjectAddress);
                    break;
                case SelectionMethodType.Closest:
                    SelectedSlotsAddresses.Clear();
                    SelectedSlotsAddresses.Add(DataModels.Mario.ClosestObject);
                    break;
            }
        }

        private void UpdateSlots(IEnumerable<ObjectDataModel> sortedObjects)
        {
            // Update labels
            if (!LabelsLocked)
            {
                _lockedSlotIndices.Clear();
                foreach (ObjectDataModel obj in DataModels.Objects.Where(o => o != null))
                    _lockedSlotIndices[obj.Address] = new Tuple<int?, int?>(obj.ProcessIndex, obj.VacantSlotIndex);
            }

            _slotLabels.Clear();
            foreach (ObjectDataModel obj in sortedObjects.Where(o => o != null))
                _slotLabels[obj] = GetSlotLabelFromObject(obj);

            // Update object slots
            foreach (var item in sortedObjects.Zip(ObjectSlots, (o, s) => new { Slot = s, Obj = o }))
                item.Slot.Update(item.Obj);
        }

        public List<ObjectDataModel> GetLoadedObjectsWithName(string name)
        {
            if (name == null) return new List<ObjectDataModel>();

            return DataModels.Objects.Where(o => o != null && o.IsActive
                                                           && o.BehaviorAssociation?.Name?.ToLower() == name.ToLower()).ToList();
        }

        public List<ObjectDataModel> GetLoadedObjectsWithPredicate(Func<ObjectDataModel, bool> func)
        {
            return DataModels.Objects.Where(o => o != null && o.IsActive && func(o)).ToList();
        }

        public ObjectDataModel GetObjectFromLabel(string name)
        {
            if (name == null) return null;
            name = name.ToLower().Trim();
            ObjectSlot slot = ObjectSlots.FirstOrDefault(s => s.Text.ToLower() == name);
            return slot?.CurrentObject;
        }

        public int? GetSlotIndexFromObj(ObjectDataModel obj)
        {
            return ObjectSlots.FirstOrDefault(o => o.CurrentObject?.Equals(obj) ?? false)?.Index;
        }

        public ObjectDataModel GetObjectFromAddress(uint objAddress)
        {
            return DataModels.Objects.FirstOrDefault(o => o?.Address == objAddress);
        }

        /*
         * Returns a string that's either:
         * - the slot label if a slot has the address
         * - null if no slot has the address
         */
        public string GetSlotLabelFromAddress(uint objAddress)
        {
            ObjectDataModel obj = GetObjectFromAddress(objAddress);
            return GetSlotLabelFromObject(obj);
        }

        public string GetDescriptiveSlotLabelFromAddress(uint objAddress, bool concise)
        {
            string noObjectString = concise ? ".." : "(no object)";
            string unusedObjectString = concise ? "UU" : "(unused object)";
            string unknownObjectString = concise ? ".." : "(unknown object)";
            string slotLabelPrefix = concise ? "" : "Slot ";
            string processGroupPrefix = concise ? "PG" : "PG ";

            if (objAddress == 0) return noObjectString;
            if (objAddress == ObjectSlotsConfig.UnusedSlotAddress) return unusedObjectString;

            byte? processGroup = ObjectUtilities.GetProcessGroup(objAddress);
            if (processGroup.HasValue) return processGroupPrefix + HexUtilities.FormatValue(processGroup.Value, 1, false);

            string slotLabel = GetSlotLabelFromAddress(objAddress);
            if (slotLabel == null) return unknownObjectString;
            return slotLabelPrefix + slotLabel;
        }

        public string GetSlotLabelFromObject(ObjectDataModel obj)
        {
            if (obj == null) return null;
            switch (LabelMethod)
            {
                case SlotLabelType.Recommended:
                    if (SortMethod == SortMethodType.MemoryOrder)
                        goto case SlotLabelType.SlotIndex;
                    else
                        goto case SlotLabelType.SlotPosVs;

                case SlotLabelType.SlotIndex:
                    return String.Format("{0}", (obj.Address - ObjectSlotsConfig.ObjectSlotsStartAddress)
                        / ObjectConfig.StructSize + (SavedSettingsConfig.StartSlotIndexsFromOne ? 1 : 0));

                case SlotLabelType.SlotPos:
                    return String.Format("{0}", _lockedSlotIndices[obj.Address].Item1
                                                + (SavedSettingsConfig.StartSlotIndexsFromOne ? 1 : 0));

                case SlotLabelType.SlotPosVs:
                    var vacantSlotIndex = _lockedSlotIndices[obj.Address].Item2;
                    if (!vacantSlotIndex.HasValue)
                        goto case SlotLabelType.SlotPos;

                    return String.Format("VS{0}", vacantSlotIndex.Value
                                                  + (SavedSettingsConfig.StartSlotIndexsFromOne ? 1 : 0));

                case SlotLabelType.RngUsage:
                    return ObjectRngUtilities.GetNumRngUsagesAsString(obj);

                default:
                    return "";
            }
        }
    }
}
