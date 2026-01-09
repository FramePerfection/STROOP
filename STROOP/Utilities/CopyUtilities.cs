using System;
using System.Collections.Generic;
using System.Windows.Forms;
using OpenTK.Mathematics;
using STROOP.Controls.VariablePanel;
using STROOP.Variables.VariablePanel;

namespace STROOP.Utilities
{
    public static class CopyUtilities
    {
        public enum CopyType
        {
            WithCommas,
            WithSpaces,
            WithTabs,
            WithLineBreaks,
            WithCommasAndSpaces,
            WithNames,
        }

        public static void Copy(List<IVariableCellUi<WinFormsVariablePanelUiContext>> vars, CopyType copyType)
        {
            int index = EnumUtilities.GetEnumValues<CopyType>(typeof(CopyType)).IndexOf(copyType);
            GetCopyActions(() => vars)[index]();
        }

        public static void AddContextMenuStripFunctions(
            Control control, Func<List<IVariableCellUi<WinFormsVariablePanelUiContext>>> getVars)
        {
            ControlUtilities.AddContextMenuStripFunctions(
                control,
                GetCopyNames(),
                GetCopyActions(getVars));
        }

        public static void AddDropDownItems(
            ToolStripMenuItem control, Func<List<IVariableCellUi<WinFormsVariablePanelUiContext>>> getVars)
        {
            ControlUtilities.AddDropDownItems(
                control,
                GetCopyNames(),
                GetCopyActions(getVars));
        }

        public static List<string> GetCopyNames()
        {
            return new List<string>()
            {
                "Copy with Commas",
                "Copy with Spaces",
                "Copy with Tabs",
                "Copy with Line Breaks",
                "Copy with Commas and Spaces",
                "Copy with Names",
            };
        }

        private static List<Action> GetCopyActions(Func<List<IVariableCellUi<WinFormsVariablePanelUiContext>>> getVars)
        {
            return new List<Action>()
            {
                () => CopyWithSeparator(getVars(), ","),
                () => CopyWithSeparator(getVars(), " "),
                () => CopyWithSeparator(getVars(), "\t"),
                () => CopyWithSeparator(getVars(), "\r\n"),
                () => CopyWithSeparator(getVars(), ", "),
                () => CopyWithNames(getVars()),
            };
        }

        private static void CopyWithSeparator(
            List<IVariableCellUi<WinFormsVariablePanelUiContext>> controls, string separator)
        {
            if (controls.Count == 0) return;
            Clipboard.SetText(string.Join(separator, controls.ConvertAll(cell => cell.GetValueText())));
        }

        private static void CopyWithNames(List<IVariableCellUi<WinFormsVariablePanelUiContext>> controls)
        {
            if (controls.Count == 0) return;
            List<string> lines = controls.ConvertAll(cell => cell.control.VarName + "\t" + cell.GetValueText());
            Clipboard.SetText(string.Join("\r\n", lines));
        }

        public static void CopyPosition(Vector3 v)
        {
            DataObject vec3Data = new DataObject("Position", v);
            vec3Data.SetText($"{v.X}, {v.Y}, {v.Z}");
            Clipboard.SetDataObject(vec3Data);
        }

        public static bool TryPastePosition(out Vector3 v)
        {
            v = default(Vector3);
            bool hasData = false;
            var clipboardObj = Clipboard.GetDataObject();
            if (!(hasData |= OpenTKUtilities.TryParseVector3(clipboardObj.GetData(DataFormats.Text) as string, out v)))
            {
                if (Clipboard.GetData("Position") is Vector3 dataVector)
                {
                    hasData = true;
                    v = dataVector;
                }
            }

            return hasData;
        }
    }
}
