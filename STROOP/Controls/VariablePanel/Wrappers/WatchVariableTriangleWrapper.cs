using STROOP.Core;
using STROOP.Utilities;
using STROOP.Variables;
using STROOP.Variables.Views;

namespace STROOP.Controls.VariablePanel
{
    public class WatchVariableTriangleWrapper : WatchVariableAddressWrapper
    {
        static WatchVariableSetting SelectTriangleSetting = new WatchVariableSetting("Select Triangle", (ctrl, _) =>
        {
            if (ctrl.WatchVarWrapper is WatchVariableTriangleWrapper triangleWrapper)
            {
                var value = triangleWrapper.CombineValues();
                if (value.meaning == CombinedValuesMeaning.SameValue)
                    AccessScope<StroopMainForm>.content.GetTab<Tabs.TrianglesTab>().SetCustomTriangleAddresses((uint)value.value);
            }

            return false;
        });

        public WatchVariableTriangleWrapper(IVariableView<uint> watchVar, WatchVariableControl watchVarControl)
            : base(watchVar, watchVarControl)
        {
            AddTriangleContextMenuStripItems();
        }

        private void AddTriangleContextMenuStripItems()
        {
            _watchVarControl.AddSetting(SelectTriangleSetting);
        }

        public override string GetClass() => "Triangle";
    }
}
