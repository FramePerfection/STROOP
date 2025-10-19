using STROOP.Core;
using STROOP.Core.Utilities;
using STROOP.Models;
using STROOP.Structs;
using STROOP.Structs.Configurations;
using STROOP.Utilities;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using STROOP.Variables;
using STROOP.Variables.SM64MemoryLayout;
using STROOP.Variables.Utilities;

namespace STROOP.Tabs
{
    public partial class MemoryTab : STROOPTab
    {
        private readonly List<ValueText> _currentValueTexts;
        private readonly List<MemoryDescriptor> _objectPrecursors;
        private readonly List<MemoryDescriptor> _objectSpecificPrecursors;
        private List<MemoryDescriptor> _memTabPrecursors => _variablePanelMemory.GetCurrentVariablePrecursors().ToList();

        private uint? _address;

        public uint? Address
        {
            get => _address;
            set
            {
                _address = value;
                RefreshAddressTextbox();
            }
        }

        private uint _memorySize;

        private uint MemorySize
        {
            get => _memorySize;
            set
            {
                _memorySize = value;
                RefreshMemorySizeTextbox();
            }
        }

        private BehaviorCriteria? _behavior;

        private BehaviorCriteria? Behavior
        {
            get => _behavior;
            set
            {
                if (value == _behavior) return;
                _behavior = value;
                _objectSpecificPrecursors.Clear();
                if (_behavior.HasValue)
                    _objectSpecificPrecursors.AddRange(
                        Config.ObjectAssociations.GetWatchVarControls(_behavior.Value)
                            .ConvertAndRemoveNull(x => (x as IMemoryVariable)?.memoryDescriptor)
                    );
            }
        }

        private ObjectSnapshot _objectSnapshot;

        public MemoryTab()
        {
            InitializeComponent();

            if (Program.IsVisualStudioHostProcess()) return;

            // Initialize fields
            _address = null;
            _memorySize = ObjectConfig.StructSize;
            _behavior = null;
            _objectSnapshot = null;

            _currentValueTexts = new List<ValueText>();
            _objectPrecursors = XmlConfigParser.OpenWatchVariableControlPrecursors(_variablePanelMemory.DataPath)
                .ConvertAndRemoveNull(x => (x as IMemoryVariable)?.memoryDescriptor);
            _objectSpecificPrecursors = new List<MemoryDescriptor>();
        }

        public override string GetDisplayName() => "Memory";

        public override void InitializeTab()
        {
            base.InitializeTab();

            // Set up controls
            comboBoxMemoryTypes.DataSource = TypeUtilities.InGameTypeList;

            checkBoxMemoryLittleEndian.Click += (sender, e) => UpdateHexDisplay();
            comboBoxMemoryTypes.SelectedValueChanged += (sender, e) => UpdateHexDisplay();

            richTextBoxMemoryValues.Click += (sender, e) => MemoryValueClick();

            textBoxMemoryBaseAddress.AddEnterAction(() =>
                SetCustomAddress(ParsingUtilities.ParseHexNullable(textBoxMemoryBaseAddress.Text)));
            textBoxMemoryMemorySize.AddEnterAction(() =>
                SetCustomMemorySize(ParsingUtilities.ParseHexNullable(textBoxMemoryMemorySize.Text)));

            buttonMemoryCopyObject.Click += (sender, e) =>
            {
                if (!Address.HasValue) return;
                _objectSnapshot = new ObjectSnapshot(Address.Value);
            };

            Action<bool> pasteAction = (bool spareSecondary) =>
            {
                if (!Address.HasValue || _objectSnapshot == null) return;
                List<uint> addresses = new List<uint>() { Address.Value };
                if (GlobalKeyboard.IsCtrlDown())
                {
                    addresses = Config.ObjectSlotsManager.SelectedObjects.ConvertAll(obj => obj.Address);
                }

                _objectSnapshot.Apply(addresses, spareSecondary);
            };
            buttonMemoryPasteObject.Click += (sender, e) => pasteAction(false);
            ControlUtilities.AddContextMenuStripFunctions(
                buttonMemoryPasteObject,
                new List<string>()
                {
                    "Paste Object without Primary Variables",
                    "Paste Object without Secondary Variables",
                },
                new List<Action>()
                {
                    () => pasteAction(false),
                    () => pasteAction(true),
                });

            buttonMemoryMoveUpOnce.Click += (sender, e) => ScrollMemory(-1);
            buttonMemoryMoveDownOnce.Click += (sender, e) => ScrollMemory(1);

            int scrollSpeed = 60;

            Timer moveUpContinuouslyTimer = new Timer { Interval = scrollSpeed };
            moveUpContinuouslyTimer.Tick += (s, e) => ScrollMemory(-1);
            buttonMemoryMoveUpContinuously.MouseDown += (sender, e) => moveUpContinuouslyTimer.Start();
            buttonMemoryMoveUpContinuously.MouseUp += (sender, e) => moveUpContinuouslyTimer.Stop();

            Timer moveDownContinuouslyTimer = new Timer { Interval = scrollSpeed };
            moveDownContinuouslyTimer.Tick += (s, e) => ScrollMemory(1);
            buttonMemoryMoveDownContinuously.MouseDown += (sender, e) => moveDownContinuouslyTimer.Start();
            buttonMemoryMoveDownContinuously.MouseUp += (sender, e) => moveDownContinuouslyTimer.Stop();
        }

        public override HashSet<uint> selection => null;

        public override Action<IEnumerable<ObjectSlot>> objectSlotsClicked => objs =>
        {
            var selectedSlot = objs.Last();
            SetObjectAddress(selectedSlot.CurrentObject?.Address);
            UpdateHexDisplay();
        };

        private void ScrollMemory(int numLines)
        {
            uint? address = Address;
            if (!address.HasValue) return;

            int addressOffset = numLines * 0x10;
            uint newAddress = (uint)(address.Value + addressOffset);
            SetCustomAddress(newAddress);
        }

        public void SetCustomAddress(uint? address)
        {
            if (!address.HasValue)
            {
                RefreshAddressTextbox();
                return;
            }

            address = address - address % 4;
            if (address < 0x80000000 || address + MemorySize >= 0x80000000 + Config.RamSize)
            {
                RefreshAddressTextbox();
                return;
            }

            checkBoxMemoryUseObjAddress.Checked = false;
            Address = address.Value;
        }

        private void SetCustomMemorySize(uint? memorySize)
        {
            if (!memorySize.HasValue)
            {
                RefreshMemorySizeTextbox();
                return;
            }

            memorySize = memorySize.Value / 16 * 16;
            if (memorySize.Value == 0)
            {
                RefreshMemorySizeTextbox();
                return;
            }

            if (Address + memorySize.Value >= 0x80000000 + Config.RamSize)
            {
                RefreshMemorySizeTextbox();
                return;
            }

            checkBoxMemoryUseObjAddress.Checked = false;
            MemorySize = memorySize.Value;
        }

        public void SetObjectAddress(uint? address)
        {
            if (!address.HasValue) return;
            checkBoxMemoryUseObjAddress.Checked = true;
            Address = address.Value;
            MemorySize = ObjectConfig.StructSize;
        }

        private void RefreshAddressTextbox()
        {
            textBoxMemoryBaseAddress.Text =
                _address.HasValue ? HexUtilities.FormatValue(_address.Value, 8) : "";
        }

        private void RefreshMemorySizeTextbox()
        {
            textBoxMemoryMemorySize.Text = HexUtilities.FormatValue(_memorySize);
        }

        private void MemoryValueClick()
        {
            bool isCtrlKeyHeld = GlobalKeyboard.IsCtrlDown();
            bool isAltKeyHeld = GlobalKeyboard.IsAltDown();
            if (!isCtrlKeyHeld) return;
            int index = richTextBoxMemoryValues.SelectionStart;
            bool useObjAddress = checkBoxMemoryUseObjAddress.Checked;
            bool useHex = checkBoxMemoryHex.Checked;
            bool useObj = checkBoxMemoryObj.Checked;
            bool useRelativeName = checkBoxMemoryRelativeAddresses.Checked;
            if (isAltKeyHeld)
            {
                List<List<MemoryDescriptor>> precursorLists = new List<List<MemoryDescriptor>>() { _objectPrecursors, _objectSpecificPrecursors };
                _currentValueTexts.ForEach(valueText =>
                {
                    if (index >= valueText.StringIndex && index <= valueText.StringIndex + valueText.StringSize)
                    {
                        precursorLists.ForEach(precursors =>
                        {
                            foreach (var ctrl in _variablePanelMemory.AddVariables(valueText.GetOverlapped(precursors).Select(x => x.CreateView())))
                                ctrl.control.GroupList.Add("Custom");
                        });
                    }
                });
            }
            else
            {
                _currentValueTexts.ForEach(valueText =>
                {
                    if (index >= valueText.StringIndex && index <= valueText.StringIndex + valueText.StringSize)
                        _variablePanelMemory.AddVariable(valueText.CreatePrecursor(useObjAddress, useHex, useObj, useRelativeName));
                });
            }

            richTextBoxMemoryValues.Parent.Focus();
        }

        private class ValueText
        {
            public readonly int ByteIndex;
            public readonly int ByteSize;
            public readonly int StringIndex;
            public readonly int StringSize;
            private readonly uint MemoryAddress;
            private readonly Type MemoryType;

            public ValueText(
                int byteIndex,
                int byteSize,
                int stringIndex,
                int stringSize,
                uint memoryAddress,
                Type memoryType)
            {
                ByteIndex = byteIndex;
                ByteSize = byteSize;
                StringIndex = stringIndex;
                StringSize = stringSize;
                MemoryAddress = memoryAddress;
                MemoryType = memoryType;
            }

            public bool OverlapsData(List<MemoryDescriptor> precursors)
            {
                return GetOverlapped(precursors).Count > 0;
            }

            public List<MemoryDescriptor> GetOverlapped(List<MemoryDescriptor> precursors)
            {
                uint minOffset = MemoryAddress;
                uint maxOffset = MemoryAddress + (uint)ByteSize - 1;
                uint? minObjOffset = ObjectUtilities.GetObjectRelativeAddress(minOffset);
                uint? maxObjOffset = ObjectUtilities.GetObjectRelativeAddress(maxOffset);

                return precursors.FindAll((memoryDescriptor =>
                {
                    if (memoryDescriptor.Mask != null) return false;

                    uint minPrecursorOffset = memoryDescriptor.Offset;
                    uint maxPrecursorOffset = memoryDescriptor.Offset + (uint)memoryDescriptor.ByteCount.Value - 1;

                    if (memoryDescriptor.BaseAddressType == BaseAddressType.Object)
                    {
                        if (!minObjOffset.HasValue || !maxObjOffset.HasValue) return false;
                        return minObjOffset <= maxPrecursorOffset && maxObjOffset >= minPrecursorOffset;
                    }

                    if (memoryDescriptor.BaseAddressType == BaseAddressType.Relative)
                    {
                        return minOffset <= maxPrecursorOffset && maxOffset >= minPrecursorOffset;
                    }

                    return false;
                }));
            }

            public IVariable CreatePrecursor(bool useObjAddress, bool useHex, bool useObj, bool useRelativeName)
            {
                string subclass = useObj ? VariableSubclass.Object : VariableSubclass.Number;
                if (GlobalKeyboard.IsDown(Keys.A)) subclass = VariableSubclass.Angle;
                if (GlobalKeyboard.IsDown(Keys.B)) subclass = VariableSubclass.Boolean;
                if (GlobalKeyboard.IsDown(Keys.Q)) subclass = VariableSubclass.Object;
                if (GlobalKeyboard.IsDown(Keys.T)) subclass = VariableSubclass.Triangle;

                bool isObjectOrTriangle = subclass == VariableSubclass.Object || subclass == VariableSubclass.Triangle;

                Type effectiveType = isObjectOrTriangle
                    ? typeof(uint)
                    : MemoryType;

                string baseAddressType = useObjAddress ? BaseAddressType.Object : BaseAddressType.Relative;
                uint offset = useObjAddress ? (uint)ByteIndex : MemoryAddress;
                uint nameOffset = useRelativeName ? (uint)ByteIndex : MemoryAddress;

                var view = new MemoryDescriptor(effectiveType, baseAddressType, offset).CreateView();
                view.SetValueByKey(CommonVariableProperties.useHex, true.ToString());
                return view;
            }
        }

        public void UpdateHexDisplay()
        {
            uint? address = Address;
            if (!address.HasValue)
            {
                richTextBoxMemoryAddresses.Text = "";
                richTextBoxMemoryValues.Text = "";
                return;
            }

            // read from memory
            if (ObjectUtilities.GetObjectRelativeAddress(address.Value) == 0)
            {
                Behavior = new ObjectDataModel(address.Value).BehaviorCriteria;
            }
            else
            {
                Behavior = null;
            }

            byte[] bytes = Config.Stream.ReadRam(address.Value, (int)MemorySize, EndiannessType.Big);

            // read settings from controls
            bool littleEndian = checkBoxMemoryLittleEndian.Checked;
            bool relativeAddresses = checkBoxMemoryRelativeAddresses.Checked;
            uint startAddress = relativeAddresses ? 0 : address.Value;
            bool highlightObjVars = checkBoxMemoryHighlightObjVars.Checked;
            Type type = TypeUtilities.StringToType[(string)comboBoxMemoryTypes.SelectedItem];
            bool useHex = checkBoxMemoryHex.Checked;
            bool useObj = checkBoxMemoryObj.Checked;

            // update memory addresses
            richTextBoxMemoryAddresses.Text = FormatAddresses(startAddress, (int)MemorySize);

            // update memory values + highlighting
            int initialSelectionStart = richTextBoxMemoryValues.SelectionStart;
            int initialSelectionLength = richTextBoxMemoryValues.SelectionLength;
            richTextBoxMemoryValues.Text = FormatValues(bytes, address.Value, type, littleEndian, useHex, useObj);
            _currentValueTexts.ForEach((Action<ValueText>)(valueText =>
            {
                // Mem tab var
                if (valueText.OverlapsData(_memTabPrecursors))
                {
                    this.richTextBoxMemoryValues.SetBackColor(
                        valueText.StringIndex, valueText.StringSize, Color.LightBlue);
                }
                // Specific object var
                else if (highlightObjVars && valueText.OverlapsData(_objectSpecificPrecursors))
                {
                    this.richTextBoxMemoryValues.SetBackColor(
                        valueText.StringIndex, valueText.StringSize, Color.LightGreen);
                }
                // Generic object var
                else if (highlightObjVars && valueText.OverlapsData(_objectPrecursors))
                {
                    this.richTextBoxMemoryValues.SetBackColor(
                        valueText.StringIndex, valueText.StringSize, Color.LightPink);
                }
            }));
            richTextBoxMemoryValues.SelectionStart = initialSelectionStart;
            richTextBoxMemoryValues.SelectionLength = initialSelectionLength;
        }

        private static string FormatAddresses(uint startAddress, int totalMemorySize)
        {
            StringBuilder builder = new StringBuilder();
            for (int i = 0; i < totalMemorySize; i += 16)
            {
                string whiteSpace = "\n";
                if (i == 0) whiteSpace = "";
                builder.Append(whiteSpace);

                uint address = startAddress + (uint)i;
                builder.Append(HexUtilities.FormatValue(address, 8));
            }

            return builder.ToString();
        }

        private string FormatValues(
            byte[] bytes, uint baseAddress, Type type, bool isLittleEndian, bool useHex, bool useObj)
        {
            int typeSize = TypeUtilities.TypeSize[type];
            List<string> stringList = new List<string>();
            for (int i = 0; i < bytes.Length; i += typeSize)
            {
                string whiteSpace = " ";
                if (i % 4 == 0) whiteSpace = "  ";
                if (i % 16 == 0) whiteSpace = "\n ";
                if (i == 0) whiteSpace = " ";
                stringList.Add(whiteSpace);

                object value = TypeUtilities.ConvertBytes(type, bytes, i, isLittleEndian);
                if (useObj)
                {
                    uint uintValue = ParsingUtilities.ParseUInt(value);
                    value = Config.ObjectSlotsManager.GetDescriptiveSlotLabelFromAddress(uintValue, true);
                }
                else if (useHex)
                {
                    value = HexUtilities.FormatMemory(value, typeSize * 2, false);
                }

                stringList.Add(value.ToString());
            }

            List<int> indexList = Enumerable.Range(0, stringList.Count / 2).ToList()
                .ConvertAll(index => index * 2 + 1);
            int maxLength = indexList.Max(index => stringList[index].Length);
            indexList.ForEach(index =>
            {
                string oldString = stringList[index];
                string newString = oldString.PadLeft(maxLength, ' ');
                stringList[index] = newString;
            });

            _currentValueTexts.Clear();
            int totalLength = 0;
            for (int i = 0; i < stringList.Count; i++)
            {
                string stringValue = stringList[i];
                int stringLength = stringValue.Length;
                totalLength += stringLength;
                if (i % 2 == 1)
                {
                    int trimmedLength = stringValue.Trim().Length;
                    int valueIndex = (i - 1) / 2;
                    int byteIndex = valueIndex * typeSize;
                    int byteIndexEndian = isLittleEndian
                        ? EndianUtilities.SwapEndianness(byteIndex, typeSize)
                        : byteIndex;
                    ValueText valueText =
                        new ValueText(
                            byteIndexEndian,
                            typeSize,
                            totalLength - trimmedLength,
                            trimmedLength,
                            baseAddress + (uint)byteIndexEndian,
                            type);
                    _currentValueTexts.Add(valueText);
                }
            }

            StringBuilder builder = new StringBuilder();
            stringList.ForEach(stringValue => builder.Append(stringValue));
            return builder.ToString();
        }

        public override void Update(bool updateView)
        {
            if (!updateView) return;

            base.Update(updateView);

            if (checkBoxMemoryUpdateContinuously.Checked)
            {
                UpdateHexDisplay();
            }
        }
    }
}
