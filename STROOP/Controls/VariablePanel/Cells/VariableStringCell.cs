using STROOP.Forms;
using STROOP.Variables;
using STROOP.Variables.VariablePanel;
using System;
using System.Collections.Generic;

namespace STROOP.Controls.VariablePanel.Cells
{
    public class VariableStringCell : VariableCell<WinFormsVariablePanelUiContext, string>
    {
        public static Dictionary<string, Action> specialTypeContextMenuHandlers = new Dictionary<string, Action>()
        {
            ["ActionDescription"] = () => SelectionForm.ShowActionDescriptionSelectionForm(),
            ["PrevActionDescription"] = () => SelectionForm.ShowPreviousActionDescriptionSelectionForm(),
            ["AnimationDescription"] = () => SelectionForm.ShowAnimationDescriptionSelectionForm(),
            ["TriangleTypeDescription"] = () => SelectionForm.ShowTriangleTypeDescriptionSelectionForm(),
            ["DemoCounterDescription"] = () => SelectionForm.ShowDemoCounterDescriptionSelectionForm(),
            ["TtcSpeedSettingDescription"] = () => SelectionForm.ShowTtcSpeedSettingDescriptionSelectionForm(),
            ["AreaTerrainDescription"] = () => SelectionForm.ShowAreaTerrainDescriptionSelectionForm(),
        };

        static Dictionary<string, WinFormsVariableSetting> settingsForSpecials = new Dictionary<string, WinFormsVariableSetting>();

        public VariableStringCell(IVariable<string> watchVar, WinFormsVariableControl watchVarControl)
            : base(watchVar, watchVarControl)
        {
            AddStringContextMenuStripItems(view.GetValueByKey(CommonVariableProperties.specialType));
        }

        private void AddStringContextMenuStripItems(string specialType)
        {
            if (specialType != null && specialTypeContextMenuHandlers.TryGetValue(specialType, out editValueHandler))
            {
                WinFormsVariableSetting applicableSetting;
                if (!settingsForSpecials.TryGetValue(specialType, out applicableSetting))
                    settingsForSpecials[specialType] = applicableSetting = new WinFormsVariableSetting($"Select {specialType}...",
                        (ctrl, obj) =>
                        {
                            editValueHandler();
                            return false;
                        });
                control.AddSetting(applicableSetting);
            }
        }

        public void AddContextMenuHandler(string name, Action<string> handler, params string[] options)
        {
            var opts = new (string, Func<object>, Func<WinFormsVariableControl, bool>)[options.Length];
            for (int i = 0; i < options.Length; i++)
            {
                var optionName = options[i];
                opts[i] = (optionName, () => optionName, ctrl => false);
            }

            WinFormsVariableSetting setting = new WinFormsVariableSetting(name, (ctrl, obj) =>
            {
                handler((string)obj);
                return false;
            }, opts);
            control.AddSetting(setting);
        }

        public override string GetClass() => "String";

        public override string DisplayValue(string value) => value;

        public override bool TryParseValue(string value, out string result)
        {
            result = value;
            return true;
        }
    }
}
