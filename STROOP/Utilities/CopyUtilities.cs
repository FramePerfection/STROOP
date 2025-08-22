using System;
using System.Collections.Generic;
using System.Windows.Forms;

using OpenTK.Mathematics;

using STROOP.Controls.VariablePanel;
using STROOP.Structs;

namespace STROOP.Utilities
{
    public static class CopyUtilities
    {
        public static void Copy(List<WatchVariableControl> vars, CopyTypeEnum copyType)
        {
            int index = EnumUtilities.GetEnumValues<CopyTypeEnum>(typeof(CopyTypeEnum)).IndexOf(copyType);
            GetCopyActions(() => vars)[index]();
        }

        public static void AddContextMenuStripFunctions(
            Control control, Func<List<WatchVariableControl>> getVars)
        {
            ControlUtilities.AddContextMenuStripFunctions(
                control,
                GetCopyNames(),
                GetCopyActions(getVars));
        }

        public static void AddDropDownItems(
            ToolStripMenuItem control, Func<List<WatchVariableControl>> getVars)
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
                "Copy as Table",
                "Copy for Code",
            };
        }

        private static List<Action> GetCopyActions(Func<List<WatchVariableControl>> getVars)
        {
            return new List<Action>()
            {
                () => CopyWithSeparator(getVars(), ","),
                () => CopyWithSeparator(getVars(), " "),
                () => CopyWithSeparator(getVars(), "\t"),
                () => CopyWithSeparator(getVars(), "\r\n"),
                () => CopyWithSeparator(getVars(), ", "),
                () => CopyWithNames(getVars()),
                () => CopyAsTable(getVars()),
                () => CopyForCode(getVars()),
            };
        }

        private static void CopyWithSeparator(
            List<WatchVariableControl> controls, string separator)
        {
            if (controls.Count == 0) return;
            Clipboard.SetText(string.Join(separator, controls.ConvertAll(control => control.WatchVarWrapper.GetValueText())));
        }

        private static void CopyWithNames(List<WatchVariableControl> controls)
        {
            if (controls.Count == 0) return;
            List<string> lines = controls.ConvertAll(watchVar => watchVar.VarName + "\t" + watchVar.WatchVarWrapper.GetValueText());
            Clipboard.SetText(string.Join("\r\n", lines));
        }

        private static void CopyAsTable(List<WatchVariableControl> controls)
        {
            // TODO: reconsider CopyAsTable
            //if (controls.Count == 0) return;
            //List<string> hexAddresses = controls.Select(x => x.view as NamedVariableCollection.MemoryDescriptorView).Where(x => x != null).ConvertAll(address => HexUtilities.FormatValue(address));
            //string header = "Vars\t" + string.Join("\t", hexAddresses);

            //List<string> names = controls.ConvertAll(control => control.VarName);
            //List<List<object>> valuesTable = controls.ConvertAll(control => control.view.GetValues());
            //List<string> valuesStrings = new List<string>();
            //for (int i = 0; i < names.Count; i++)
            //{
            //    string line = names[i] + "\t" + string.Join("\t", valuesTable[i]);
            //    valuesStrings.Add(line);
            //}

            //string output = header + "\r\n" + string.Join("\r\n", valuesStrings);
            //Clipboard.SetText(output);
        }

        private static void CopyForCode(List<WatchVariableControl> controls)
        {
            if (controls.Count == 0) return;
            Func<string, string> varNameFunc;
            if (KeyboardUtilities.IsCtrlHeld())
            {
                string template = DialogUtilities.GetStringFromDialog("$");
                if (template == null) return;
                varNameFunc = varName => template.Replace("$", varName);
            }
            else
            {
                varNameFunc = varName => varName;
            }
            List<string> lines = new List<string>();
            foreach (WatchVariableControl watchVar in controls)
            {
                Type type = watchVar.GetMemoryType();
                string line = string.Format(
                    "{0} {1} = {2}{3};",
                    type != null ? TypeUtilities.TypeToString[type] : "double",
                    varNameFunc(watchVar.VarName.Replace(" ", "")),
                    // TODO: indicate that the watchVarWrapper should produce code conforming output (whatever that means)
                    watchVar.WatchVarWrapper.GetValueText(),
                    type == typeof(float) ? "f" : "");
                lines.Add(line);
            }
            if (lines.Count > 0)
            {
                Clipboard.SetText(string.Join("\r\n", lines));
            }
        }

        public static void CopyPosition(Vector3 v)
        {
            DataObject vec3Data = new DataObject("Position", v);
            vec3Data.SetText($"{v.X}; {v.Y}; {v.Z}");
            Clipboard.SetDataObject(vec3Data);
        }

        public static bool TryPastePosition(out Vector3 v)
        {
            v = default(Vector3);
            bool hasData = false;
            var clipboardObj = Clipboard.GetDataObject();
            if (!(hasData |= ParsingUtilities.TryParseVector3(clipboardObj.GetData(DataFormats.Text) as string, out v)))
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
