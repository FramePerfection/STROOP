﻿using STROOP.Structs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using STROOP.Utilities;
using STROOP.Controls;

namespace STROOP.Forms
{
    public partial class VariableCreationForm : Form
    {
        private bool _disableMapping = false;

        public VariableCreationForm()
        {
            InitializeComponent();
            comboBoxTypeValue.DataSource = TypeUtilities.InGameTypeList;
            comboBoxBaseValue.DataSource = Enum.GetValues(typeof(BaseAddressTypeEnum));
            comboBoxTypeValue.SelectedIndex = TypeUtilities.InGameTypeList.IndexOf("int");
            comboBoxBaseValue.SelectedIndex =
                EnumUtilities.GetEnumValues<BaseAddressTypeEnum>(
                    typeof(BaseAddressTypeEnum)).IndexOf(BaseAddressTypeEnum.Object);
            ControlUtilities.AddCheckableContextMenuStripFunctions(
                buttonAddVariable,
                new List<string>()
                {
                    "Disable Mapping",
                },
                new List<Func<bool>>()
                {
                    () =>
                    {
                        _disableMapping = !_disableMapping;
                        return _disableMapping;
                    },
                });
        }

        public void Initialize(WatchVariableFlowLayoutPanel varPanel)
        {
            buttonAddVariable.Click += (sender, e) =>
            {
                WatchVariableControl control = CreateWatchVariableControl();
                varPanel.AddVariable(control);
            };
        }

        private WatchVariableControl CreateWatchVariableControl()
        {
            string name = textBoxNameValue.Text;
            string memoryTypeString = comboBoxTypeValue.SelectedItem.ToString();
            BaseAddressTypeEnum baseAddressType = (BaseAddressTypeEnum)comboBoxBaseValue.SelectedItem;
            uint offset = ParsingUtilities.ParseHexNullable(textBoxOffsetValue.Text) ?? 0;

            var memoryType = TypeUtilities.StringToType[memoryTypeString];

            var isAbsolute = baseAddressType == BaseAddressTypeEnum.Absolute;

            WatchVariable watchVar = new WatchVariable(
                new WatchVariable.CustomView(typeof(WatchVariableNumberWrapper))
                {
                    Name = name,
                    _getterFunction = b => Structs.Configurations.Config.Stream.GetValue(memoryType, b, isAbsolute),
                    _setterFunction = (value, b) => Structs.Configurations.Config.Stream.SetValue(memoryType, value, b, isAbsolute),
                },
                baseAddressType,
                offset
                );
            return new WatchVariableControl(watchVar);
        }
    }
}
