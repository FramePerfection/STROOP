using System;
using System.Collections.Generic;
using STROOP.Forms;
using STROOP.Variables;
using STROOP.Variables.Views;

namespace STROOP.Controls.VariablePanel
{
    public class WatchVariableStringWrapper : WatchVariableWrapper<string>
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

        static Dictionary<string, WatchVariableSetting> settingsForSpecials = new Dictionary<string, WatchVariableSetting>();

        public WatchVariableStringWrapper(IVariableView<string> watchVar, WatchVariableControl watchVarControl)
            : base(watchVar, watchVarControl)
        {
            AddStringContextMenuStripItems(watchVarControl.view.GetValueByKey(CommonViewProperties.specialType));
        }

        private void AddStringContextMenuStripItems(string specialType)
        {
            if (specialType != null && specialTypeContextMenuHandlers.TryGetValue(specialType, out editValueHandler))
            {
                WatchVariableSetting applicableSetting;
                if (!settingsForSpecials.TryGetValue(specialType, out applicableSetting))
                    settingsForSpecials[specialType] = applicableSetting = new WatchVariableSetting($"Select {specialType}...",
                        (ctrl, obj) =>
                        {
                            editValueHandler();
                            return false;
                        });
                _watchVarControl.AddSetting(applicableSetting);
            }
        }

        public void AddContextMenuHandler(string name, Action<string> handler, params string[] options)
        {
            var opts = new (string, Func<object>, Func<WatchVariableControl, bool>)[options.Length];
            for (int i = 0; i < options.Length; i++)
            {
                var optionName = options[i];
                opts[i] = (optionName, () => optionName, ctrl => false);
            }

            WatchVariableSetting setting = new WatchVariableSetting(name, (ctrl, obj) =>
            {
                handler((string)obj);
                return false;
            }, opts);
            _watchVarControl.AddSetting(setting);
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
