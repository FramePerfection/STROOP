using System.Linq;
using STROOP.Controls.VariablePanel;
using STROOP.Core.Variables;
using STROOP.Structs.Configurations;
using STROOP.Utilities;
using STROOP.Variables.SM64MemoryLayout;

namespace STROOP.Tabs.BruteforceTab.Surfaces.GeneralPurpose.MethodControllers
{
    public class CheckAction : IMethodController
    {
        void IMethodController.Remove()
        {
        }

        void IMethodController.SetTargetFunc(ScoringFunc target)
        {
            var view = new NamedVariableCollection.CustomView<string>(typeof(WatchVariableSelectionWrapper<WatchVariableStringWrapper, string>))
            {
                Name = "Set to current"
            };

            var panel = target.watchVariablePanelParameters;
            var currentActionVariable = panel.GetWatchVariableControlsByName("action")[0];

            if (currentActionVariable != null)
            {
                // TODO: Reimplement using setting
                //(currentActionVariable.WatchVarWrapper as WatchVariableNumberWrapper)?.ToggleDisplayAsHex(true);

                // Init to current action if not loaded from file?
                if (currentActionVariable.view.GetNumberValues<uint>().FirstOrDefault() == 0)
                    currentActionVariable.SetValue(Config.Stream.GetInt32(MarioConfig.StructAddress + MarioConfig.ActionOffset));

                var ctrl = (WatchVariableSelectionWrapper<WatchVariableStringWrapper, string>)panel.AddVariable(view).WatchVarWrapper;
                ctrl.DisplaySingleOption = true;
                ctrl.options.Add(("Set action now", () =>
                        {
                            var action = Config.Stream.GetInt32(MarioConfig.StructAddress + MarioConfig.ActionOffset);
                            currentActionVariable.SetValue(action);
                            return null;
                        }
                ));
            }
        }
    }
}
