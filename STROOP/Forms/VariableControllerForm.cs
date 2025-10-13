using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using STROOP.Controls.VariablePanel;
using STROOP.Core.Utilities;
using STROOP.Structs;
using STROOP.Structs.Configurations;
using STROOP.Utilities;
using STROOP.Variables;
using STROOP.Variables.Views;

namespace STROOP.Forms
{
    public partial class VariableControllerForm : Form, IUpdatableForm
    {
        private static readonly Color COLOR_BLUE = Color.FromArgb(220, 255, 255);
        private static readonly Color COLOR_RED = Color.FromArgb(255, 220, 220);
        private static readonly Color COLOR_PURPLE = Color.FromArgb(200, 190, 230);

        private readonly List<string> _varNames;
        private readonly List<WatchVariableWrapper> _watchVarWrappers;
        private readonly List<DescribedMemoryState> _variableMemoryStates;

        public VariableControllerForm(string varName, WatchVariableWrapper watchVarWrapper) :
            this(new List<string>() { varName }, new List<WatchVariableWrapper>() { watchVarWrapper })
        {
        }

        public VariableControllerForm(List<string> varNames, List<WatchVariableWrapper> watchVarWrappers)
        {
            _varNames = varNames;
            _watchVarWrappers = watchVarWrappers;

            // TODO: Create and correctly use own DescribedMemoryState?
            _variableMemoryStates = _watchVarWrappers.ConvertAndRemoveNull(x => (x._view as IMemoryBasedVariableView)?.describedMemoryState);

            InitializeComponent();
            FormManager.AddForm(this);
            FormClosing += (sender, e) => FormManager.RemoveForm(this);

            _textBoxVarName.Text = String.Join(",", _varNames);

            ToolStripMenuItem itemInvertedAdd = new ToolStripMenuItem("Inverted");
            ToolStripMenuItem itemInvertedSubtract = new ToolStripMenuItem("Inverted");
            Action<bool> setInverted = (bool inverted) =>
            {
                tableLayoutPanel1.Controls.Remove(_buttonAdd);
                tableLayoutPanel1.Controls.Remove(_buttonSubtract);
                if (inverted)
                {
                    tableLayoutPanel1.Controls.Add(_buttonAdd, 0, 2);
                    tableLayoutPanel1.Controls.Add(_buttonSubtract, 2, 2);
                }
                else
                {
                    tableLayoutPanel1.Controls.Add(_buttonAdd, 2, 2);
                    tableLayoutPanel1.Controls.Add(_buttonSubtract, 0, 2);
                }

                itemInvertedAdd.Checked = inverted;
                itemInvertedSubtract.Checked = inverted;
            };
            itemInvertedAdd.Click += (sender, e) => setInverted(!itemInvertedAdd.Checked);
            itemInvertedSubtract.Click += (sender, e) => setInverted(!itemInvertedSubtract.Checked);
            _buttonAdd.ContextMenuStrip = new ContextMenuStrip();
            _buttonSubtract.ContextMenuStrip = new ContextMenuStrip();
            _buttonAdd.ContextMenuStrip.Items.Add(new ToolStripSeparator());
            _buttonAdd.ContextMenuStrip.Items.Add(itemInvertedAdd);
            _buttonSubtract.ContextMenuStrip.Items.Add(new ToolStripSeparator());
            _buttonSubtract.ContextMenuStrip.Items.Add(itemInvertedSubtract);

            _buttonGet.Click += (s, e) => _textBoxGetSet.Text = GetValues();

            _buttonSet.Click += (s, e) => SetValues();
            _textBoxGetSet.AddEnterAction(() => SetValues());

            _checkBoxFixAddress.Click += (s, e) => ToggleFixedAddress();

            UpdateFixedCheckState();
        }

        private string GetValues()
        {
            List<object> values = new List<object>();
            for (int i = 0; i < _watchVarWrappers.Count; i++)
                values.Add(_watchVarWrappers[i].GetValueText());
            return String.Join(",", values);
        }

        private void SetValues()
        {
            List<string> values = ParsingUtilities.ParseStringList(_textBoxGetSet.Text);
            if (values.Count == 0) return;

            using (Config.Stream.Suspend())
            {
                for (int i = 0; i < _watchVarWrappers.Count; i++)
                    _watchVarWrappers[i].TrySetValue(values[i % values.Count]);
            }
        }

        private void UpdateFixedCheckState()
        {
            _checkBoxFixAddress.CheckState = BoolUtilities.GetCheckState(_variableMemoryStates.ConvertAll(x => x.fixedAddresses));
            _textBoxCurrentValue.BackColor = GetColorForCheckState(_checkBoxFixAddress.CheckState);
        }

        private Color GetColorForCheckState(CheckState checkState)
        {
            switch (checkState)
            {
                case CheckState.Unchecked:
                    return COLOR_BLUE;
                case CheckState.Checked:
                    return COLOR_RED;
                case CheckState.Indeterminate:
                    return COLOR_PURPLE;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public void UpdateForm()
        {
            _textBoxCurrentValue.Text = GetValues();
        }

        public void ToggleFixedAddress()
        {
            bool fixedAddress = _checkBoxFixAddress.Checked;
            foreach (var memoryState in _variableMemoryStates)
                memoryState.ToggleFixedAddress(fixedAddress);
            UpdateFixedCheckState();
        }
    }
}
