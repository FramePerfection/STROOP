using STROOP.Structs;
using STROOP.Structs.Configurations;
using STROOP.Utilities;
using STROOP.Variables;
using STROOP.Variables.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace STROOP.Tabs
{
    public partial class SearchTab : STROOPTab
    {
        private enum ValueRelationship
        {
            EqualTo,
            NotEqualTo,

            GreaterThan,
            LessThan,
            GreaterThanOrEqualTo,
            LessThanOrEqualTo,

            Changed,
            DidNotChange,
            Increased,
            Decreased,
            IncreasedBy,
            DecreasedBy,

            BetweenExclusive,
            BetweenInclusive,

            EverythingPasses,
        };

        private readonly Dictionary<uint, object> _dictionary;
        private readonly Dictionary<uint, object> _undoDictionary;
        private Type _memoryType;
        private bool _useHex;

        public SearchTab()
        {
            InitializeComponent();

            _dictionary = new Dictionary<uint, object>();
            _undoDictionary = new Dictionary<uint, object>();
        }

        public override string GetDisplayName() => "Search";

        public override void InitializeTab()
        {
            base.InitializeTab();
            _memoryType = typeof(byte);
            _useHex = false;

            comboBoxSearchMemoryType.DataSource = TypeUtilities.InGameTypeList;
            comboBoxSearchMemoryType.SelectedItem = "short";

            comboBoxSearchValueRelationship.DataSource = Enum.GetValues(typeof(ValueRelationship));

            buttonSearchFirstScan.Click += (sender, e) => DoFirstScan();
            buttonSearchNextScan.Click += (sender, e) => DoNextScan();
            buttonSearchAddSelectedAsVars.Click += (sender, e) => AddTableRowsAsVars(ControlUtilities.GetTableSelectedRows(dataGridViewSearch));
            buttonSearchAddAllAsVars.Click += (sender, e) => AddTableRowsAsVars(ControlUtilities.GetTableAllRows(dataGridViewSearch));
            buttonSearchUndoScan.Click += (sender, e) => UndoScan();
            buttonSearchClearResults.Click += (sender, e) => ClearResults();
            labelSearchProgress.Visible = false;
        }

        private void AddTableRowsAsVars(List<DataGridViewRow> rows)
        {
            foreach (DataGridViewRow row in rows)
            {
                uint? addressNullable = ParsingUtilities.ParseHexNullable(row.Cells[0].Value);
                if (!addressNullable.HasValue) continue;
                uint address = addressNullable.Value;

                MemoryDescriptor watchVar = new MemoryDescriptor(_memoryType, BaseAddressType.Relative, address);
                watchVariablePanelSearch.AddVariable(watchVar.CreateView());
            }
        }

        private void DoFirstScan()
        {
            string memoryTypeString = (string)comboBoxSearchMemoryType.SelectedItem;
            _memoryType = TypeUtilities.StringToType[memoryTypeString];
            int memoryTypeSize = TypeUtilities.TypeSize[_memoryType];
            _useHex = textBoxSearchValue.Text.StartsWith("0x");

            (object searchValue1, object searchValue2) = ParseSearchValue(textBoxSearchValue.Text, _memoryType);
            object oldMemoryValue = null;

            TransferDictionary(_dictionary, _undoDictionary);
            _dictionary.Clear();
            StartProgressBar();
            for (uint address = 0x80000000; address < 0x80000000 + Config.RamSize - memoryTypeSize; address += (uint)memoryTypeSize)
            {
                object memoryValue = Config.Stream.GetValue(_memoryType, address);
                if (ValueQualifies(memoryValue, oldMemoryValue, searchValue1, searchValue2, _memoryType))
                {
                    _dictionary[address] = memoryValue;
                }

                int offset = (int)(address - 0x80000000);
                if (offset % 1024 == 0)
                {
                    SetProgressCount(offset, (int)Config.RamSize);
                }
            }

            StopProgressBar();

            UpdateControlsBasedOnDictionary();
        }

        private void DoNextScan()
        {
            (object searchValue1, object searchValue2) = ParseSearchValue(textBoxSearchValue.Text, _memoryType);

            List<KeyValuePair<uint, object>> pairs = _dictionary.ToList();
            TransferDictionary(_dictionary, _undoDictionary);
            _dictionary.Clear();
            StartProgressBar();
            for (int i = 0; i < pairs.Count; i++)
            {
                KeyValuePair<uint, object> pair = pairs[i];
                uint address = pair.Key;
                object oldMemoryValue = pair.Value;
                object memoryValue = Config.Stream.GetValue(_memoryType, address);
                if (ValueQualifies(memoryValue, oldMemoryValue, searchValue1, searchValue2, _memoryType))
                {
                    _dictionary[address] = memoryValue;
                }

                if (pairs.Count > 10000)
                {
                    if (i % 1024 == 0)
                    {
                        SetProgressCount(i, pairs.Count);
                    }
                }
                else
                {
                    SetProgressCount(i, pairs.Count);
                }
            }

            StopProgressBar();

            UpdateControlsBasedOnDictionary();
        }

        private (object searchValue1, object searchValue2) ParseSearchValue(string text, Type type)
        {
            List<string> stringValues = ParsingUtilities.ParseStringList(text);
            string stringValue1 = stringValues.Count >= 1 ? stringValues[0] : null;
            string stringValue2 = stringValues.Count >= 2 ? stringValues[1] : null;
            return (ParsingUtilities.ParseValueNullable(stringValue1, type),
                ParsingUtilities.ParseValueNullable(stringValue2, type));
        }

        private void ClearResults()
        {
            _dictionary.Clear();
            UpdateControlsBasedOnDictionary();
        }

        private void UpdateControlsBasedOnDictionary()
        {
            labelSearchNumResults.Text = _dictionary.Count.ToString() + " Results";
            dataGridViewSearch.Rows.Clear();
            if (_dictionary.Count > 10000) return;
            _dictionary.Keys.ToList().ForEach((Action<uint>)(key =>
            {
                this.dataGridViewSearch.Rows.Add(
                    HexUtilities.FormatValue(key),
                    _useHex ? HexUtilities.FormatValue(_dictionary[key]) : _dictionary[key]);
            }));
        }

        private void StartProgressBar()
        {
            labelSearchProgress.Visible = true;
            labelSearchProgress.Update();
        }

        private void StopProgressBar()
        {
            labelSearchProgress.Visible = false;
            labelSearchProgress.Update();
            progressBarSearch.Value = 0;
            progressBarSearch.Update();
        }

        private void SetProgressCount(int value, int maximum)
        {
            string maximumString = maximum.ToString();
            string valueString = string.Format("{0:D" + maximumString.Length + "}", value);
            double percent = Math.Round(100d * value / maximum, 1);
            string percentString = percent.ToString("N1");
            labelSearchProgress.Text = string.Format(
                "{0}% ({1} / {2})", percentString, valueString, maximumString);
            labelSearchProgress.Update();
            progressBarSearch.Maximum = maximum;
            progressBarSearch.Value = value;
            progressBarSearch.Update();
        }

        private void TransferDictionary(Dictionary<uint, object> sender, Dictionary<uint, object> receiver)
        {
            receiver.Clear();
            foreach (uint key in sender.Keys)
            {
                receiver[key] = sender[key];
            }
        }

        private void UndoScan()
        {
            TransferDictionary(_undoDictionary, _dictionary);
            UpdateControlsBasedOnDictionary();
        }

        public override void Update(bool updateView)
        {
            if (!updateView) return;

            base.Update(updateView);
        }

        private bool ValueQualifies(object memoryObject, object oldMemoryObject, object searchObject1, object searchObject2, Type type)
        {
            if (type == typeof(byte))
            {
                byte? memoryValue = ParsingUtilities.ParseByteNullable(memoryObject);
                if (!memoryValue.HasValue) return false;
                byte? oldMemoryValue = ParsingUtilities.ParseByteNullable(oldMemoryObject);
                byte? searchValue1 = ParsingUtilities.ParseByteNullable(searchObject1);
                byte? searchValue2 = ParsingUtilities.ParseByteNullable(searchObject2);
                ValueRelationship valueRelationship = (ValueRelationship)comboBoxSearchValueRelationship.SelectedItem;
                switch (valueRelationship)
                {
                    case ValueRelationship.EqualTo:
                        if (!searchValue1.HasValue) return false;
                        return memoryValue.Value == searchValue1.Value;
                    case ValueRelationship.NotEqualTo:
                        if (!searchValue1.HasValue) return false;
                        return memoryValue.Value != searchValue1.Value;
                    case ValueRelationship.GreaterThan:
                        if (!searchValue1.HasValue) return false;
                        return memoryValue.Value > searchValue1.Value;
                    case ValueRelationship.LessThan:
                        if (!searchValue1.HasValue) return false;
                        return memoryValue.Value < searchValue1.Value;
                    case ValueRelationship.GreaterThanOrEqualTo:
                        if (!searchValue1.HasValue) return false;
                        return memoryValue.Value >= searchValue1.Value;
                    case ValueRelationship.LessThanOrEqualTo:
                        if (!searchValue1.HasValue) return false;
                        return memoryValue.Value <= searchValue1.Value;
                    case ValueRelationship.Changed:
                        if (!oldMemoryValue.HasValue) return false;
                        return memoryValue.Value != oldMemoryValue.Value;
                    case ValueRelationship.DidNotChange:
                        if (!oldMemoryValue.HasValue) return false;
                        return memoryValue.Value == oldMemoryValue.Value;
                    case ValueRelationship.Increased:
                        if (!oldMemoryValue.HasValue) return false;
                        return memoryValue.Value > oldMemoryValue.Value;
                    case ValueRelationship.Decreased:
                        if (!oldMemoryValue.HasValue) return false;
                        return memoryValue.Value < oldMemoryValue.Value;
                    case ValueRelationship.IncreasedBy:
                        if (!oldMemoryValue.HasValue || !searchValue1.HasValue) return false;
                        return memoryValue.Value == oldMemoryValue.Value + searchValue1.Value;
                    case ValueRelationship.DecreasedBy:
                        if (!oldMemoryValue.HasValue || !searchValue1.HasValue) return false;
                        return memoryValue.Value == oldMemoryValue.Value - searchValue1.Value;
                    case ValueRelationship.BetweenExclusive:
                        if (!searchValue1.HasValue || !searchValue2.HasValue) return false;
                        return searchValue1.Value < memoryValue.Value && memoryValue.Value < searchValue2.Value;
                    case ValueRelationship.BetweenInclusive:
                        if (!searchValue1.HasValue || !searchValue2.HasValue) return false;
                        return searchValue1.Value <= memoryValue.Value && memoryValue.Value <= searchValue2.Value;
                    case ValueRelationship.EverythingPasses:
                        return true;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            if (type == typeof(sbyte))
            {
                sbyte? memoryValue = ParsingUtilities.ParseSByteNullable(memoryObject);
                if (!memoryValue.HasValue) return false;
                sbyte? oldMemoryValue = ParsingUtilities.ParseSByteNullable(oldMemoryObject);
                sbyte? searchValue1 = ParsingUtilities.ParseSByteNullable(searchObject1);
                sbyte? searchValue2 = ParsingUtilities.ParseSByteNullable(searchObject2);
                ValueRelationship valueRelationship = (ValueRelationship)comboBoxSearchValueRelationship.SelectedItem;
                switch (valueRelationship)
                {
                    case ValueRelationship.EqualTo:
                        if (!searchValue1.HasValue) return false;
                        return memoryValue.Value == searchValue1.Value;
                    case ValueRelationship.NotEqualTo:
                        if (!searchValue1.HasValue) return false;
                        return memoryValue.Value != searchValue1.Value;
                    case ValueRelationship.GreaterThan:
                        if (!searchValue1.HasValue) return false;
                        return memoryValue.Value > searchValue1.Value;
                    case ValueRelationship.LessThan:
                        if (!searchValue1.HasValue) return false;
                        return memoryValue.Value < searchValue1.Value;
                    case ValueRelationship.GreaterThanOrEqualTo:
                        if (!searchValue1.HasValue) return false;
                        return memoryValue.Value >= searchValue1.Value;
                    case ValueRelationship.LessThanOrEqualTo:
                        if (!searchValue1.HasValue) return false;
                        return memoryValue.Value <= searchValue1.Value;
                    case ValueRelationship.Changed:
                        if (!oldMemoryValue.HasValue) return false;
                        return memoryValue.Value != oldMemoryValue.Value;
                    case ValueRelationship.DidNotChange:
                        if (!oldMemoryValue.HasValue) return false;
                        return memoryValue.Value == oldMemoryValue.Value;
                    case ValueRelationship.Increased:
                        if (!oldMemoryValue.HasValue) return false;
                        return memoryValue.Value > oldMemoryValue.Value;
                    case ValueRelationship.Decreased:
                        if (!oldMemoryValue.HasValue) return false;
                        return memoryValue.Value < oldMemoryValue.Value;
                    case ValueRelationship.IncreasedBy:
                        if (!oldMemoryValue.HasValue || !searchValue1.HasValue) return false;
                        return memoryValue.Value == oldMemoryValue.Value + searchValue1.Value;
                    case ValueRelationship.DecreasedBy:
                        if (!oldMemoryValue.HasValue || !searchValue1.HasValue) return false;
                        return memoryValue.Value == oldMemoryValue.Value - searchValue1.Value;
                    case ValueRelationship.BetweenExclusive:
                        if (!searchValue1.HasValue || !searchValue2.HasValue) return false;
                        return searchValue1.Value < memoryValue.Value && memoryValue.Value < searchValue2.Value;
                    case ValueRelationship.BetweenInclusive:
                        if (!searchValue1.HasValue || !searchValue2.HasValue) return false;
                        return searchValue1.Value <= memoryValue.Value && memoryValue.Value <= searchValue2.Value;
                    case ValueRelationship.EverythingPasses:
                        return true;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            if (type == typeof(short))
            {
                short? memoryValue = ParsingUtilities.ParseShortNullable(memoryObject);
                if (!memoryValue.HasValue) return false;
                short? oldMemoryValue = ParsingUtilities.ParseShortNullable(oldMemoryObject);
                short? searchValue1 = ParsingUtilities.ParseShortNullable(searchObject1);
                short? searchValue2 = ParsingUtilities.ParseShortNullable(searchObject2);
                ValueRelationship valueRelationship = (ValueRelationship)comboBoxSearchValueRelationship.SelectedItem;
                switch (valueRelationship)
                {
                    case ValueRelationship.EqualTo:
                        if (!searchValue1.HasValue) return false;
                        return memoryValue.Value == searchValue1.Value;
                    case ValueRelationship.NotEqualTo:
                        if (!searchValue1.HasValue) return false;
                        return memoryValue.Value != searchValue1.Value;
                    case ValueRelationship.GreaterThan:
                        if (!searchValue1.HasValue) return false;
                        return memoryValue.Value > searchValue1.Value;
                    case ValueRelationship.LessThan:
                        if (!searchValue1.HasValue) return false;
                        return memoryValue.Value < searchValue1.Value;
                    case ValueRelationship.GreaterThanOrEqualTo:
                        if (!searchValue1.HasValue) return false;
                        return memoryValue.Value >= searchValue1.Value;
                    case ValueRelationship.LessThanOrEqualTo:
                        if (!searchValue1.HasValue) return false;
                        return memoryValue.Value <= searchValue1.Value;
                    case ValueRelationship.Changed:
                        if (!oldMemoryValue.HasValue) return false;
                        return memoryValue.Value != oldMemoryValue.Value;
                    case ValueRelationship.DidNotChange:
                        if (!oldMemoryValue.HasValue) return false;
                        return memoryValue.Value == oldMemoryValue.Value;
                    case ValueRelationship.Increased:
                        if (!oldMemoryValue.HasValue) return false;
                        return memoryValue.Value > oldMemoryValue.Value;
                    case ValueRelationship.Decreased:
                        if (!oldMemoryValue.HasValue) return false;
                        return memoryValue.Value < oldMemoryValue.Value;
                    case ValueRelationship.IncreasedBy:
                        if (!oldMemoryValue.HasValue || !searchValue1.HasValue) return false;
                        return memoryValue.Value == oldMemoryValue.Value + searchValue1.Value;
                    case ValueRelationship.DecreasedBy:
                        if (!oldMemoryValue.HasValue || !searchValue1.HasValue) return false;
                        return memoryValue.Value == oldMemoryValue.Value - searchValue1.Value;
                    case ValueRelationship.BetweenExclusive:
                        if (!searchValue1.HasValue || !searchValue2.HasValue) return false;
                        return searchValue1.Value < memoryValue.Value && memoryValue.Value < searchValue2.Value;
                    case ValueRelationship.BetweenInclusive:
                        if (!searchValue1.HasValue || !searchValue2.HasValue) return false;
                        return searchValue1.Value <= memoryValue.Value && memoryValue.Value <= searchValue2.Value;
                    case ValueRelationship.EverythingPasses:
                        return true;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            if (type == typeof(ushort))
            {
                ushort? memoryValue = ParsingUtilities.ParseUShortNullable(memoryObject);
                if (!memoryValue.HasValue) return false;
                ushort? oldMemoryValue = ParsingUtilities.ParseUShortNullable(oldMemoryObject);
                ushort? searchValue1 = ParsingUtilities.ParseUShortNullable(searchObject1);
                ushort? searchValue2 = ParsingUtilities.ParseUShortNullable(searchObject2);
                ValueRelationship valueRelationship = (ValueRelationship)comboBoxSearchValueRelationship.SelectedItem;
                switch (valueRelationship)
                {
                    case ValueRelationship.EqualTo:
                        if (!searchValue1.HasValue) return false;
                        return memoryValue.Value == searchValue1.Value;
                    case ValueRelationship.NotEqualTo:
                        if (!searchValue1.HasValue) return false;
                        return memoryValue.Value != searchValue1.Value;
                    case ValueRelationship.GreaterThan:
                        if (!searchValue1.HasValue) return false;
                        return memoryValue.Value > searchValue1.Value;
                    case ValueRelationship.LessThan:
                        if (!searchValue1.HasValue) return false;
                        return memoryValue.Value < searchValue1.Value;
                    case ValueRelationship.GreaterThanOrEqualTo:
                        if (!searchValue1.HasValue) return false;
                        return memoryValue.Value >= searchValue1.Value;
                    case ValueRelationship.LessThanOrEqualTo:
                        if (!searchValue1.HasValue) return false;
                        return memoryValue.Value <= searchValue1.Value;
                    case ValueRelationship.Changed:
                        if (!oldMemoryValue.HasValue) return false;
                        return memoryValue.Value != oldMemoryValue.Value;
                    case ValueRelationship.DidNotChange:
                        if (!oldMemoryValue.HasValue) return false;
                        return memoryValue.Value == oldMemoryValue.Value;
                    case ValueRelationship.Increased:
                        if (!oldMemoryValue.HasValue) return false;
                        return memoryValue.Value > oldMemoryValue.Value;
                    case ValueRelationship.Decreased:
                        if (!oldMemoryValue.HasValue) return false;
                        return memoryValue.Value < oldMemoryValue.Value;
                    case ValueRelationship.IncreasedBy:
                        if (!oldMemoryValue.HasValue || !searchValue1.HasValue) return false;
                        return memoryValue.Value == oldMemoryValue.Value + searchValue1.Value;
                    case ValueRelationship.DecreasedBy:
                        if (!oldMemoryValue.HasValue || !searchValue1.HasValue) return false;
                        return memoryValue.Value == oldMemoryValue.Value - searchValue1.Value;
                    case ValueRelationship.BetweenExclusive:
                        if (!searchValue1.HasValue || !searchValue2.HasValue) return false;
                        return searchValue1.Value < memoryValue.Value && memoryValue.Value < searchValue2.Value;
                    case ValueRelationship.BetweenInclusive:
                        if (!searchValue1.HasValue || !searchValue2.HasValue) return false;
                        return searchValue1.Value <= memoryValue.Value && memoryValue.Value <= searchValue2.Value;
                    case ValueRelationship.EverythingPasses:
                        return true;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            if (type == typeof(int))
            {
                int? memoryValue = ParsingUtilities.ParseIntNullable(memoryObject);
                if (!memoryValue.HasValue) return false;
                int? oldMemoryValue = ParsingUtilities.ParseIntNullable(oldMemoryObject);
                int? searchValue1 = ParsingUtilities.ParseIntNullable(searchObject1);
                int? searchValue2 = ParsingUtilities.ParseIntNullable(searchObject2);
                ValueRelationship valueRelationship = (ValueRelationship)comboBoxSearchValueRelationship.SelectedItem;
                switch (valueRelationship)
                {
                    case ValueRelationship.EqualTo:
                        if (!searchValue1.HasValue) return false;
                        return memoryValue.Value == searchValue1.Value;
                    case ValueRelationship.NotEqualTo:
                        if (!searchValue1.HasValue) return false;
                        return memoryValue.Value != searchValue1.Value;
                    case ValueRelationship.GreaterThan:
                        if (!searchValue1.HasValue) return false;
                        return memoryValue.Value > searchValue1.Value;
                    case ValueRelationship.LessThan:
                        if (!searchValue1.HasValue) return false;
                        return memoryValue.Value < searchValue1.Value;
                    case ValueRelationship.GreaterThanOrEqualTo:
                        if (!searchValue1.HasValue) return false;
                        return memoryValue.Value >= searchValue1.Value;
                    case ValueRelationship.LessThanOrEqualTo:
                        if (!searchValue1.HasValue) return false;
                        return memoryValue.Value <= searchValue1.Value;
                    case ValueRelationship.Changed:
                        if (!oldMemoryValue.HasValue) return false;
                        return memoryValue.Value != oldMemoryValue.Value;
                    case ValueRelationship.DidNotChange:
                        if (!oldMemoryValue.HasValue) return false;
                        return memoryValue.Value == oldMemoryValue.Value;
                    case ValueRelationship.Increased:
                        if (!oldMemoryValue.HasValue) return false;
                        return memoryValue.Value > oldMemoryValue.Value;
                    case ValueRelationship.Decreased:
                        if (!oldMemoryValue.HasValue) return false;
                        return memoryValue.Value < oldMemoryValue.Value;
                    case ValueRelationship.IncreasedBy:
                        if (!oldMemoryValue.HasValue || !searchValue1.HasValue) return false;
                        return memoryValue.Value == oldMemoryValue.Value + searchValue1.Value;
                    case ValueRelationship.DecreasedBy:
                        if (!oldMemoryValue.HasValue || !searchValue1.HasValue) return false;
                        return memoryValue.Value == oldMemoryValue.Value - searchValue1.Value;
                    case ValueRelationship.BetweenExclusive:
                        if (!searchValue1.HasValue || !searchValue2.HasValue) return false;
                        return searchValue1.Value < memoryValue.Value && memoryValue.Value < searchValue2.Value;
                    case ValueRelationship.BetweenInclusive:
                        if (!searchValue1.HasValue || !searchValue2.HasValue) return false;
                        return searchValue1.Value <= memoryValue.Value && memoryValue.Value <= searchValue2.Value;
                    case ValueRelationship.EverythingPasses:
                        return true;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            if (type == typeof(uint))
            {
                uint? memoryValue = ParsingUtilities.ParseUIntNullable(memoryObject);
                if (!memoryValue.HasValue) return false;
                uint? oldMemoryValue = ParsingUtilities.ParseUIntNullable(oldMemoryObject);
                uint? searchValue1 = ParsingUtilities.ParseUIntNullable(searchObject1);
                uint? searchValue2 = ParsingUtilities.ParseUIntNullable(searchObject2);
                ValueRelationship valueRelationship = (ValueRelationship)comboBoxSearchValueRelationship.SelectedItem;
                switch (valueRelationship)
                {
                    case ValueRelationship.EqualTo:
                        if (!searchValue1.HasValue) return false;
                        return memoryValue.Value == searchValue1.Value;
                    case ValueRelationship.NotEqualTo:
                        if (!searchValue1.HasValue) return false;
                        return memoryValue.Value != searchValue1.Value;
                    case ValueRelationship.GreaterThan:
                        if (!searchValue1.HasValue) return false;
                        return memoryValue.Value > searchValue1.Value;
                    case ValueRelationship.LessThan:
                        if (!searchValue1.HasValue) return false;
                        return memoryValue.Value < searchValue1.Value;
                    case ValueRelationship.GreaterThanOrEqualTo:
                        if (!searchValue1.HasValue) return false;
                        return memoryValue.Value >= searchValue1.Value;
                    case ValueRelationship.LessThanOrEqualTo:
                        if (!searchValue1.HasValue) return false;
                        return memoryValue.Value <= searchValue1.Value;
                    case ValueRelationship.Changed:
                        if (!oldMemoryValue.HasValue) return false;
                        return memoryValue.Value != oldMemoryValue.Value;
                    case ValueRelationship.DidNotChange:
                        if (!oldMemoryValue.HasValue) return false;
                        return memoryValue.Value == oldMemoryValue.Value;
                    case ValueRelationship.Increased:
                        if (!oldMemoryValue.HasValue) return false;
                        return memoryValue.Value > oldMemoryValue.Value;
                    case ValueRelationship.Decreased:
                        if (!oldMemoryValue.HasValue) return false;
                        return memoryValue.Value < oldMemoryValue.Value;
                    case ValueRelationship.IncreasedBy:
                        if (!oldMemoryValue.HasValue || !searchValue1.HasValue) return false;
                        return memoryValue.Value == oldMemoryValue.Value + searchValue1.Value;
                    case ValueRelationship.DecreasedBy:
                        if (!oldMemoryValue.HasValue || !searchValue1.HasValue) return false;
                        return memoryValue.Value == oldMemoryValue.Value - searchValue1.Value;
                    case ValueRelationship.BetweenExclusive:
                        if (!searchValue1.HasValue || !searchValue2.HasValue) return false;
                        return searchValue1.Value < memoryValue.Value && memoryValue.Value < searchValue2.Value;
                    case ValueRelationship.BetweenInclusive:
                        if (!searchValue1.HasValue || !searchValue2.HasValue) return false;
                        return searchValue1.Value <= memoryValue.Value && memoryValue.Value <= searchValue2.Value;
                    case ValueRelationship.EverythingPasses:
                        return true;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            if (type == typeof(float))
            {
                float? memoryValue = ParsingUtilities.ParseFloatNullable(memoryObject);
                if (!memoryValue.HasValue) return false;
                float? oldMemoryValue = ParsingUtilities.ParseFloatNullable(oldMemoryObject);
                float? searchValue1 = ParsingUtilities.ParseFloatNullable(searchObject1);
                float? searchValue2 = ParsingUtilities.ParseFloatNullable(searchObject2);
                ValueRelationship valueRelationship = (ValueRelationship)comboBoxSearchValueRelationship.SelectedItem;
                switch (valueRelationship)
                {
                    case ValueRelationship.EqualTo:
                        if (!searchValue1.HasValue) return false;
                        return memoryValue.Value == searchValue1.Value;
                    case ValueRelationship.NotEqualTo:
                        if (!searchValue1.HasValue) return false;
                        return memoryValue.Value != searchValue1.Value;
                    case ValueRelationship.GreaterThan:
                        if (!searchValue1.HasValue) return false;
                        return memoryValue.Value > searchValue1.Value;
                    case ValueRelationship.LessThan:
                        if (!searchValue1.HasValue) return false;
                        return memoryValue.Value < searchValue1.Value;
                    case ValueRelationship.GreaterThanOrEqualTo:
                        if (!searchValue1.HasValue) return false;
                        return memoryValue.Value >= searchValue1.Value;
                    case ValueRelationship.LessThanOrEqualTo:
                        if (!searchValue1.HasValue) return false;
                        return memoryValue.Value <= searchValue1.Value;
                    case ValueRelationship.Changed:
                        if (!oldMemoryValue.HasValue) return false;
                        return memoryValue.Value != oldMemoryValue.Value;
                    case ValueRelationship.DidNotChange:
                        if (!oldMemoryValue.HasValue) return false;
                        return memoryValue.Value == oldMemoryValue.Value;
                    case ValueRelationship.Increased:
                        if (!oldMemoryValue.HasValue) return false;
                        return memoryValue.Value > oldMemoryValue.Value;
                    case ValueRelationship.Decreased:
                        if (!oldMemoryValue.HasValue) return false;
                        return memoryValue.Value < oldMemoryValue.Value;
                    case ValueRelationship.IncreasedBy:
                        if (!oldMemoryValue.HasValue || !searchValue1.HasValue) return false;
                        return memoryValue.Value == oldMemoryValue.Value + searchValue1.Value;
                    case ValueRelationship.DecreasedBy:
                        if (!oldMemoryValue.HasValue || !searchValue1.HasValue) return false;
                        return memoryValue.Value == oldMemoryValue.Value - searchValue1.Value;
                    case ValueRelationship.BetweenExclusive:
                        if (!searchValue1.HasValue || !searchValue2.HasValue) return false;
                        return searchValue1.Value < memoryValue.Value && memoryValue.Value < searchValue2.Value;
                    case ValueRelationship.BetweenInclusive:
                        if (!searchValue1.HasValue || !searchValue2.HasValue) return false;
                        return searchValue1.Value <= memoryValue.Value && memoryValue.Value <= searchValue2.Value;
                    case ValueRelationship.EverythingPasses:
                        return true;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            if (type == typeof(double))
            {
                double? memoryValue = ParsingUtilities.ParseDoubleNullable(memoryObject);
                if (!memoryValue.HasValue) return false;
                double? oldMemoryValue = ParsingUtilities.ParseDoubleNullable(oldMemoryObject);
                double? searchValue1 = ParsingUtilities.ParseDoubleNullable(searchObject1);
                double? searchValue2 = ParsingUtilities.ParseDoubleNullable(searchObject2);
                ValueRelationship valueRelationship = (ValueRelationship)comboBoxSearchValueRelationship.SelectedItem;
                switch (valueRelationship)
                {
                    case ValueRelationship.EqualTo:
                        if (!searchValue1.HasValue) return false;
                        return memoryValue.Value == searchValue1.Value;
                    case ValueRelationship.NotEqualTo:
                        if (!searchValue1.HasValue) return false;
                        return memoryValue.Value != searchValue1.Value;
                    case ValueRelationship.GreaterThan:
                        if (!searchValue1.HasValue) return false;
                        return memoryValue.Value > searchValue1.Value;
                    case ValueRelationship.LessThan:
                        if (!searchValue1.HasValue) return false;
                        return memoryValue.Value < searchValue1.Value;
                    case ValueRelationship.GreaterThanOrEqualTo:
                        if (!searchValue1.HasValue) return false;
                        return memoryValue.Value >= searchValue1.Value;
                    case ValueRelationship.LessThanOrEqualTo:
                        if (!searchValue1.HasValue) return false;
                        return memoryValue.Value <= searchValue1.Value;
                    case ValueRelationship.Changed:
                        if (!oldMemoryValue.HasValue) return false;
                        return memoryValue.Value != oldMemoryValue.Value;
                    case ValueRelationship.DidNotChange:
                        if (!oldMemoryValue.HasValue) return false;
                        return memoryValue.Value == oldMemoryValue.Value;
                    case ValueRelationship.Increased:
                        if (!oldMemoryValue.HasValue) return false;
                        return memoryValue.Value > oldMemoryValue.Value;
                    case ValueRelationship.Decreased:
                        if (!oldMemoryValue.HasValue) return false;
                        return memoryValue.Value < oldMemoryValue.Value;
                    case ValueRelationship.IncreasedBy:
                        if (!oldMemoryValue.HasValue || !searchValue1.HasValue) return false;
                        return memoryValue.Value == oldMemoryValue.Value + searchValue1.Value;
                    case ValueRelationship.DecreasedBy:
                        if (!oldMemoryValue.HasValue || !searchValue1.HasValue) return false;
                        return memoryValue.Value == oldMemoryValue.Value - searchValue1.Value;
                    case ValueRelationship.BetweenExclusive:
                        if (!searchValue1.HasValue || !searchValue2.HasValue) return false;
                        return searchValue1.Value < memoryValue.Value && memoryValue.Value < searchValue2.Value;
                    case ValueRelationship.BetweenInclusive:
                        if (!searchValue1.HasValue || !searchValue2.HasValue) return false;
                        return searchValue1.Value <= memoryValue.Value && memoryValue.Value <= searchValue2.Value;
                    case ValueRelationship.EverythingPasses:
                        return true;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            throw new ArgumentOutOfRangeException();
        }
    }
}
