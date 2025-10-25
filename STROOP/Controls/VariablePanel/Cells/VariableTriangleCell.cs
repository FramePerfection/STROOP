using STROOP.Core;
using STROOP.Variables;
using STROOP.Variables.Utilities;

namespace STROOP.Controls.VariablePanel.Cells
{
    public class VariableTriangleCell : VariableAddressCell
    {
        static WinFormsVariableSetting SelectTriangleSetting = new WinFormsVariableSetting("Select Triangle", (ctrl, _) =>
        {
            if (ctrl.varCell is VariableTriangleCell triangleWrapper)
            {
                var value = triangleWrapper.CombineValues<uint>();
                if (value.meaning == CombinedValuesMeaning.SameValue)
                    AccessScope<StroopMainForm>.content.GetTab<Tabs.TrianglesTab>().SetCustomTriangleAddresses(value.value);
            }

            return false;
        });

        public VariableTriangleCell(IVariable<uint> watchVar, WinFormsVariableControl watchVarControl)
            : base(watchVar, watchVarControl)
        {
            control.AddSetting(SelectTriangleSetting);
        }

        public override string GetClass() => "Triangle";
    }
}
