﻿using STROOP.Structs;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using STROOP.Extensions;
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
            throw new NotImplementedException();

            //string name = textBoxNameValue.Text;
            //string memoryType = comboBoxTypeValue.SelectedItem.ToString();
            //BaseAddressTypeEnum baseAddressType = (BaseAddressTypeEnum)comboBoxBaseValue.SelectedItem;
            //uint offset = ParsingUtilities.ParseHexNullable(textBoxOffsetValue.Text) ?? 0;

            //bool useOffsetDefault =
            //    baseAddressType != BaseAddressTypeEnum.Absolute &&
            //    baseAddressType != BaseAddressTypeEnum.Relative &&
            //    baseAddressType != BaseAddressTypeEnum.None;

            //WatchVariable watchVar = new WatchVariable(
            //    memoryTypeName: memoryType,
            //    baseAddressType: baseAddressType,
            //    offsetUS: useOffsetDefault ? (uint?)null : offset,
            //    offsetJP: useOffsetDefault ? (uint?)null : offset,
            //    offsetSH: useOffsetDefault ? (uint?)null : offset,
            //    offsetEU: useOffsetDefault ? (uint?)null : offset,
            //    offsetDefault: useOffsetDefault ? offset : (uint?)null,
            //    mask: null,
            //    shift: null,
            //    handleMapping: !_disableMapping);
            //return watchVar.CreateWatchVariableControl();
        }
    }
}
