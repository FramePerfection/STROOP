using System;
using System.Collections.Generic;
using System.Linq;
using STROOP.Structs;
using System.Windows.Forms;
using STROOP.Utilities;
using STROOP.Structs.Configurations;
using STROOP.Forms;
using STROOP.Variables;

namespace STROOP.Tabs
{
    public partial class CustomTab : STROOPTab
    {
        // TODO: Record values properly, not as their projected strings
        private Dictionary<int, List<string>> _recordedValues = new Dictionary<int, List<string>>();
        private int? _lastTimer = null;
        private int _numGaps = 0;
        private int _recordFreq = 1;

        private CopyUtilities.CopyType _copyType = CopyUtilities.CopyType.WithTabs;

        public CustomTab()
        {
            InitializeComponent();
        }

        public override string GetDisplayName() => "Custom";

        public override void InitializeTab()
        {
            base.InitializeTab();

            buttonOpenVars.Click += (sender, e) => VariablePanelCustom.OpenVariables();

            buttonSaveVars.Click += (sender, e) => VariablePanelCustom.SaveVariables();

            buttonCopyVars.Click += (sender, e) => CopyUtilities.Copy(VariablePanelCustom.GetCurrentlyVisibleCells(), _copyType);
            buttonCopyVars.ContextMenuStrip = new ContextMenuStrip();
            ToolStripMenuItem itemSetDefaultCopyType = new ToolStripMenuItem("Set Default Copy Type");
            buttonCopyVars.ContextMenuStrip.Items.Add(itemSetDefaultCopyType);
            buttonCopyVars.ContextMenuStrip.Items.Add(new ToolStripSeparator());
            ControlUtilities.AddCheckableDropDownItems(
                itemSetDefaultCopyType,
                CopyUtilities.GetCopyNames(),
                EnumUtilities.GetEnumValues<CopyUtilities.CopyType>(typeof(CopyUtilities.CopyType)),
                copyType => _copyType = copyType,
                _copyType);
            CopyUtilities.AddContextMenuStripFunctions(buttonCopyVars, VariablePanelCustom.GetCurrentlyVisibleCells);

            buttonClearVars.Click += (sender, e) => VariablePanelCustom.ClearVariables();
            ControlUtilities.AddContextMenuStripFunctions(
                buttonClearVars,
                new List<string>() { "Clear All Vars", "Clear Default Vars" },
                new List<Action>()
                {
                    () => VariablePanelCustom.ClearVariables(),
                    () => VariablePanelCustom.RemoveVariableGroup(VariableGroup.NoGroup),
                });

            checkBoxCustomRecordValues.Click += (sender, e) => ToggleRecording();

            buttonCustomShowValues.Click += (sender, e) => ShowRecordedValues();

            buttonCustomClearValues.Click += (sender, e) => ClearRecordedValues();
        }

        private void ToggleRecording()
        {
            RefreshRateConfig.LimitRefreshRate = !checkBoxCustomRecordValues.Checked;
        }

        private void ShowRecordedValues()
        {
            InfoForm infoForm = new InfoForm();

            List<string> variableNames = VariablePanelCustom.GetCurrentVariableNames();
            List<string> variableValueRowStrings = _recordedValues.ToList()
                .ConvertAll(pair => (pair.Key + 1) + "\t" + String.Join("\t", pair.Value));
            string variableValueText =
                "Timer\t" + String.Join("\t", variableNames) + "\r\n" +
                String.Join("\r\n", variableValueRowStrings);
            infoForm.SetText(
                "Variable Value Info",
                "Variable Values",
                variableValueText);

            infoForm.Show();
        }

        private void ClearRecordedValues()
        {
            _recordedValues.Clear();
            _lastTimer = null;
            _numGaps = 0;
            _recordFreq = 1;
        }

        public override void Update(bool updateView)
        {
            if (checkBoxCustomRecordValues.Checked)
            {
                int currentTimer = Config.Stream.GetInt32(MiscConfig.GlobalTimerAddress);

                bool alreadyContainsKey = _recordedValues.ContainsKey(currentTimer);
                bool recordEvenIfAlreadyHave = !checkBoxUseValueAtStartOfGlobalTimer.Checked;

                if (alreadyContainsKey)
                {
                    _recordFreq++;
                }
                else
                {
                    labelCustomRecordingFrequencyValue.Text = _recordFreq.ToString();
                    _recordFreq = 1;
                }

                if (_lastTimer.HasValue)
                {
                    int diff = currentTimer - _lastTimer.Value;
                    if (diff > 1) _numGaps += (diff - 1);
                }

                _lastTimer = currentTimer;

                if (!alreadyContainsKey || recordEvenIfAlreadyHave)
                {
                    List<string> currentValues = VariablePanelCustom.GetCurrentVariableValues();
                    _recordedValues[currentTimer] = currentValues;
                }
            }
            else
            {
                labelCustomRecordingFrequencyValue.Text = "0";
            }

            textBoxRecordValuesCount.Text = _recordedValues.Count.ToString();
            labelCustomRecordingGapsValue.Text = _numGaps.ToString();

            base.Update(updateView);
        }
    }
}
