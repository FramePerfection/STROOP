using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using STROOP.Controls.VariablePanel;
using STROOP.Utilities;
using STROOP.Variables;
using STROOP.Variables.SM64MemoryLayout;
using STROOP.Variables.Utilities;

namespace STROOP.Forms
{
    public partial class VariableCreationForm : Form
    {
        private bool _disableMapping = false;

        public VariableCreationForm()
        {
            InitializeComponent();
            var baseTypeValues = VariableUtilities.baseAddressGetters.Keys.ToArray();
            comboBoxTypeValue.DataSource = TypeUtilities.InGameTypeList;
            comboBoxBaseValue.DataSource = baseTypeValues;
            comboBoxTypeValue.SelectedIndex = TypeUtilities.InGameTypeList.IndexOf("int");
            comboBoxBaseValue.SelectedIndex = Array.IndexOf(baseTypeValues, BaseAddressType.Object);

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

        public void Initialize(VariablePanel varPanel)
        {
            buttonAddVariable.Click += (sender, e) => varPanel.AddVariable(CreateVariable());
        }

        private VariablePrecursor CreateVariable()
        {
            string memoryTypeString = comboBoxTypeValue.SelectedItem.ToString();
            string baseAddressType = (string)comboBoxBaseValue.SelectedItem;
            uint offset = ParsingUtilities.ParseHexNullable(textBoxOffsetValue.Text) ?? 0;

            return (textBoxNameValue.Text, new MemoryDescriptor(TypeUtilities.StringToType[memoryTypeString], baseAddressType, offset).CreateVariable());
        }
    }
}
