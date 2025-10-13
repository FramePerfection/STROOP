using STROOP.Core;
using System.Linq;
using STROOP.Structs.Configurations;
using STROOP.Utilities;
using STROOP.Structs;
using STROOP.Variables;
using STROOP.Variables.Views;

namespace STROOP.Controls.VariablePanel
{
    public class WatchVariableAddressWrapper : WatchVariableNumberWrapper<uint>
    {
        static WatchVariableSetting ViewAddressSetting = new WatchVariableSetting(
            "View Address",
            (ctrl, obj) =>
            {
                if (ctrl.WatchVarWrapper is WatchVariableAddressWrapper addressWrapper)
                {
                    uint uintValue = (uint)addressWrapper.view._getterFunction().FirstOrDefault();
                    if (uintValue == 0) return false;
                    if (ObjectUtilities.IsObjectAddress(uintValue))
                        AccessScope<StroopMainForm>.content.GetTab<Tabs.MemoryTab>().SetObjectAddress(uintValue);
                    else
                        AccessScope<StroopMainForm>.content.GetTab<Tabs.MemoryTab>().SetCustomAddress(uintValue);
                    Config.TabControlMain.SelectedTab = Config.TabControlMain.TabPages["tabPageMemory"];
                }

                return false;
            });

        public WatchVariableAddressWrapper(IVariableView<uint> watchVar, WatchVariableControl watchVarControl)
            : base(watchVar.WithKeyedValue(CommonViewProperties.useHex, true), watchVarControl)
        {
            AddAddressContextMenuStripItems();
        }

        private void AddAddressContextMenuStripItems()
        {
            _watchVarControl.AddSetting(ViewAddressSetting);
        }

        public override string GetClass() => "Address";
    }
}
