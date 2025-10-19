using STROOP.Variables;

namespace STROOP.Controls.VariablePanel.Cells
{
    public class VariableTriangleCell : VariableAddressCell
    {
        static WinFormsVariableSetting SelectTriangleSetting = new WinFormsVariableSetting("Select Triangle", (ctrl, _) =>
        {
            // if (ctrl.varWrapper is WatchVariableTriangleWrapper triangleWrapper)
            // {
            //     var value = triangleWrapper.CombineValues();
            //     if (value.meaning == CombinedValuesMeaning.SameValue)
            //         AccessScope<StroopMainForm>.content.GetTab<Tabs.TrianglesTab>().SetCustomTriangleAddresses(value.value);
            // }

            return false;
        });

        public VariableTriangleCell(IVariable<uint> watchVar, WinFormsVariableControl watchVarControl)
            : base(watchVar, watchVarControl)
        {
            AddTriangleContextMenuStripItems();
        }

        private void AddTriangleContextMenuStripItems()
        {
            control.AddSetting(SelectTriangleSetting);
        }

        public override string GetClass() => "Triangle";
    }
}
