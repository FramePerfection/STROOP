using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using OpenTK;
using OpenTK.Mathematics;
using STROOP.Controls;
using STROOP.Controls.VariablePanel;
using STROOP.Controls.VariablePanel.Cells;
using STROOP.Core.Utilities;
using STROOP.Extensions;
using STROOP.Forms;
using STROOP.Structs.Configurations;
using STROOP.Utilities;
using STROOP.Variables;
using STROOP.Variables.Utilities;
using STROOP.Variables.VariablePanel;
using System.Globalization;

namespace STROOP.Structs
{
    using BinaryScalarOperation = Func<double, double, double>;

    public static class WatchVariableSelectionUtilities
    {
        static IEnumerable<INumberVariableCell> FilterNumberVariables(IEnumerable<IWinFormsVariableCell> cells)
            => cells.OfType<INumberVariableCell>().Where(x => x is not IVariableCellData<string>);

        static double GetNumberValue(this INumberVariableCell cell)
            => (double)(Convert.ChangeType(cell.CombineValues().value, TypeCode.Double) ?? double.NaN);

        static bool SetValue(this INumberVariableCell cell, double value)
            => cell.TrySetValue(value.ToString(CultureInfo.InvariantCulture));

        static readonly List<string> VarInfoLabels =
        [
            "Name",
            "Class",
            "Type",
            "BaseType + Offset",
            "N64 Base Address",
            "Emulator Base Address",
            "N64 Address",
            "Emulator Address",
        ];

        public static List<ToolStripItem> CreateSelectionToolStripItems(
            List<IWinFormsVariableCell> vars,
            VariablePanel panel)
        {
            var itemList = new List<ToolStripItem>();

            ToolStripMenuItem itemShowAlways = new ToolStripMenuItem("Always visible");
            itemShowAlways.CheckState = GeneralUtilities.GetMeaningfulValue(
                () => vars.ConvertAll(v => (CheckState?)(v.control.alwaysVisible ? CheckState.Checked : CheckState.Unchecked)),
                CheckState.Indeterminate,
                null) ?? CheckState.Indeterminate;
            itemShowAlways.MouseDown += (_, __) =>
            {
                itemShowAlways.Checked = !itemShowAlways.Checked;
                foreach (var v in vars)
                    v.control.alwaysVisible = itemShowAlways.Checked;
                itemShowAlways.PreventClosingMenuStrip();
            };
            itemList.Add(itemShowAlways);
            itemList.Add(new ToolStripSeparator());

            ToolStripMenuItem itemCopy = new ToolStripMenuItem("Copy...");
            CopyUtilities.AddDropDownItems(itemCopy, () => vars);
            itemList.Add(itemCopy);

            ToolStripMenuItem itemPaste = new ToolStripMenuItem("Paste");
            itemPaste.Click += (sender, e) =>
            {
                List<string> stringList = ParsingUtilities.ParseStringList(Clipboard.GetText());
                if (stringList.Count == 0) return;


                using (Config.Stream.Suspend())
                {
                    for (int i = 0; i < vars.Count; i++)
                    {
                        vars[i].control.SetValue(stringList[i % stringList.Count]);
                    }
                }
            };
            itemList.Add(itemPaste);
            itemList.Add(new ToolStripSeparator());

            ToolStripMenuItem itemShowVariableXml = new ToolStripMenuItem("Show Variable XML");
            itemShowVariableXml.Click += (sender, e) =>
            {
                InfoForm infoForm = new InfoForm();
                infoForm.SetText(
                    "Variable Info",
                    "Variable XML",
                    String.Join("\r\n", vars.ConvertAll(cell => cell.control.ToXml())));
                infoForm.Show();
            };
            itemList.Add(itemShowVariableXml);

            ToolStripMenuItem itemShowVariableInfo = new ToolStripMenuItem("Show Variable Info");
            itemShowVariableInfo.Click += (sender, e) =>
            {
                InfoForm infoForm = new InfoForm();
                infoForm.SetText(
                    "Variable Info",
                    "Variable Info",
                    String.Join("\t",
                        VarInfoLabels) +
                    "\r\n" +
                    String.Join(
                        "\r\n",
                        vars.ConvertAll(cell => cell.control.GetVarInfo())
                            .ConvertAll(infoList => String.Join("\t", infoList))));
                infoForm.Show();
            };
            itemList.Add(itemShowVariableInfo);
            itemList.Add(new ToolStripSeparator());

            Dictionary<BinaryOperationName, BinaryScalarOperation> binaryMathOperations = new Dictionary<BinaryOperationName, BinaryScalarOperation>()
            {
                [BinaryOperationName.Add] = (a, b) => a + b,
                [BinaryOperationName.Subtract] = (a, b) => a - b,
                [BinaryOperationName.Multiply] = (a, b) => a * b,
                [BinaryOperationName.Divide] = (a, b) => a / b,
                [BinaryOperationName.Exponent] = (a, b) => Math.Pow(a, b),
                [BinaryOperationName.Modulo] = (a, b) => a % b,
                [BinaryOperationName.NonNegativeModulo] = (a, b) => MoreMath.NonNegativeModulus(a, b),
            };

            Dictionary<BinaryOperationName, BinaryScalarOperation> binaryMathOperationsInverse1 = new Dictionary<BinaryOperationName, BinaryScalarOperation>()
            {
                [BinaryOperationName.Add] = (sum, b) => sum - b,
                [BinaryOperationName.Subtract] = (diff, b) => b + diff,
                [BinaryOperationName.Multiply] = (product, b) => product / b,
                [BinaryOperationName.Divide] = (quotient, b) => b * quotient,
            };

            Dictionary<BinaryOperationName, BinaryScalarOperation> binaryMathOperationsInverse2 = new Dictionary<BinaryOperationName, BinaryScalarOperation>()
            {
                [BinaryOperationName.Add] = (sum, b) => sum - b,
                [BinaryOperationName.Subtract] = (diff, b) => b - diff,
                [BinaryOperationName.Multiply] = (product, a) => product / a,
                [BinaryOperationName.Divide] = (quotient, a) => a / quotient,
            };

            void createBinaryMathOperationVariable(BinaryOperationName operation)
            {
                var cells = FilterNumberVariables(vars).ToArray();

                if (binaryMathOperations.TryGetValue(operation, out var func))
                    for (int i = 0; i < cells.Length / 2; i++)
                    {
                        var cell1 = cells[i];
                        var cell2 = cells[i + cells.Length / 2];

                        binaryMathOperationsInverse1.TryGetValue(operation, out BinaryScalarOperation inverseSetter1);
                        binaryMathOperationsInverse2.TryGetValue(operation, out BinaryScalarOperation inverseSetter2);

                        var view = new CustomVariable<double>(VariableSubclass.Number)
                        {
                            Name = $"{cell1.control.VarName} {MathOperationUtilities.GetSymbol(operation)} {cell2.control.VarName}",
                            getter = () => func(cell1.GetNumberValue(), cell2.GetNumberValue()).Yield(),
                            setter = val =>
                            {
                                if (!GlobalKeyboard.IsCtrlDown())
                                {
                                    var wrapper1Value = (double)Convert.ChangeType(cell1.CombineValues().value, TypeCode.Double)!;
                                    return inverseSetter2 == null
                                        ? Array.Empty<bool>()
                                        : cell1.SetValue(inverseSetter2(val, wrapper1Value)).Yield();
                                }
                                else
                                {
                                    var wrapper2Value = (double)Convert.ChangeType(cell2.CombineValues().value, TypeCode.Double)!;
                                    return inverseSetter1 == null
                                        ? Array.Empty<bool>()
                                        : cell2.SetValue(inverseSetter1(val, wrapper2Value)).Yield();
                                }
                            }
                        };
                        panel.AddVariable(view);
                    }
            }

            void createAggregateMathOperationVariable(AggregateMathOperation operation)
            {
                if (vars.Count == 0) return;
                var getter = WatchVariableSpecialUtilities.AddAggregateMathOperationEntry(vars, operation);
                var view = new CustomVariable<double>(VariableSubclass.Number)
                {
                    Name = $"{operation}({vars.First().control.VarName}-{vars.Last().control.VarName})",
                    getter = getter,
                    setter = SpecialVariableDefaults<double>.DEFAULT_SETTER
                };
                panel.AddVariable(view);
            }

            void createDistanceMathOperationVariable(bool use3D)
            {
                var cells = FilterNumberVariables(vars).ToArray();
                bool satisfies2D = !use3D && vars.Count >= 4;
                bool satisfies3D = use3D && vars.Count >= 6;
                if (!satisfies2D && !satisfies3D) return;

                string name = use3D
                    ? string.Format(
                        "({0},{1},{2}) to ({3},{4},{5})",
                        cells[0].control.VarName,
                        cells[1].control.VarName,
                        cells[2].control.VarName,
                        cells[3].control.VarName,
                        cells[4].control.VarName,
                        cells[5].control.VarName)
                    : string.Format(
                        "({0},{1}) to ({2},{3})",
                        cells[0].control.VarName,
                        cells[1].control.VarName,
                        cells[2].control.VarName,
                        cells[3].control.VarName);

                var values = VariableUtilities.GetNumberValues(vars).Select(Enumerable.ToArray).ToArray();

                IVariable<double>.ValueGetter getter3D = () =>
                {
                    var x1 = values[0];
                    var y1 = values[1];
                    var z1 = values[2];
                    var x2 = values[3];
                    var y2 = values[4];
                    var z2 = values[5];
                    var min = values.Min(x => x.Length);
                    var result = new List<double>(min);
                    for (int i = 0; i < min; i++)
                        result.Add(new Vector3d(x2[i] - x1[i], y2[i] - y1[i], z2[i] - z1[i]).Length);
                    return result;
                };
                IVariable<double>.ValueGetter getter2D = () =>
                {
                    var x1 = values[0];
                    var y1 = values[1];
                    var x2 = values[3];
                    var y2 = values[4];
                    var min = values.Min(x => x.Length);
                    var result = new List<double>(min);
                    for (int i = 0; i < min; i++)
                        result.Add(new Vector2d(x2[i] - x1[i], y2[i] - y1[i]).Length);
                    return result;
                };
                IVariable<double>.ValueSetter setter3D = value =>
                {
                    var x1 = values[0];
                    var y1 = values[1];
                    var z1 = values[2];
                    var x2 = values[3];
                    var y2 = values[4];
                    var z2 = values[5];
                    bool toggle = GlobalKeyboard.IsCtrlDown();
                    int off = toggle ? 0 : 3;
                    var min = values.Min(x => x.Length);
                    var result = new List<bool>(min);
                    for (int i = 0; i < min; i++)
                    {
                        Vector3d a = new Vector3d(x1[i], y1[i], z1[i]);
                        Vector3d b = new Vector3d(x2[i], y2[i], z2[i]);
                        if (toggle)
                        {
                            var tmp = a;
                            a = b;
                            b = tmp;
                        }

                        b = a + Vector3d.Normalize(b - a) * value;
                        result.Add(cells[off].SetValue(b.X) && cells[off].SetValue(b.Y) && cells[off].SetValue(b.Z));
                    }

                    return result;
                };
                IVariable<double>.ValueSetter setter2D = value =>
                {
                    var x1 = values[0];
                    var y1 = values[1];
                    var x2 = values[2];
                    var y2 = values[3];
                    bool toggle = GlobalKeyboard.IsCtrlDown();
                    int off = toggle ? 0 : 2;
                    var min = values.Min(x => x.Length);
                    var result = new List<bool>(min);
                    for (int i = 0; i < min; i++)
                    {
                        Vector2d a = new Vector2d(x1[i], y1[i]);
                        Vector2d b = new Vector2d(x2[i], y2[i]);
                        if (toggle)
                        {
                            var tmp = a;
                            a = b;
                            b = tmp;
                        }

                        b = a + Vector2d.Normalize(b - a) * (double)value;
                        result.Add(cells[off].SetValue(b.X) && cells[off].SetValue(b.Y));
                    }

                    return result;
                };

                var view = new CustomVariable<double>(VariableSubclass.Number)
                {
                    Name = name,
                    getter = use3D ? getter3D : getter2D,
                    setter = use3D ? setter3D : setter2D
                };
                panel.AddVariable(view);
            }

            ToolStripMenuItem itemAddVariables = new ToolStripMenuItem("Add Variable(s)...");
            ControlUtilities.AddDropDownItems(
                itemAddVariables,
                new List<string>()
                {
                    "Addition",
                    "Subtraction",
                    "Multiplication",
                    "Division",
                    "Modulo",
                    "Non-Negative Modulo",
                    "Exponent",
                    null,
                    "Mean",
                    "Median",
                    "Min",
                    "Max",
                    null,
                    "2D Distance",
                    "3D Distance",
                    null,
                },
                new List<Action>()
                {
                    () => createBinaryMathOperationVariable(BinaryOperationName.Add),
                    () => createBinaryMathOperationVariable(BinaryOperationName.Subtract),
                    () => createBinaryMathOperationVariable(BinaryOperationName.Multiply),
                    () => createBinaryMathOperationVariable(BinaryOperationName.Divide),
                    () => createBinaryMathOperationVariable(BinaryOperationName.Modulo),
                    () => createBinaryMathOperationVariable(BinaryOperationName.NonNegativeModulo),
                    () => createBinaryMathOperationVariable(BinaryOperationName.Exponent),
                    () => { },
                    () => createAggregateMathOperationVariable(AggregateMathOperation.Mean),
                    () => createAggregateMathOperationVariable(AggregateMathOperation.Median),
                    () => createAggregateMathOperationVariable(AggregateMathOperation.Min),
                    () => createAggregateMathOperationVariable(AggregateMathOperation.Max),
                    () => { },
                    () => createDistanceMathOperationVariable(use3D: false),
                    () => createDistanceMathOperationVariable(use3D: true),
                    () => { },
                });
            itemList.Add(itemAddVariables);
            itemList.Add(new ToolStripSeparator());

            var itemReorder = new ToolStripMenuItem("Move");
            itemReorder.Click += (sender, e) => panel.BeginMoveSelected();
            itemList.Add(itemReorder);

            ToolStripMenuItem itemRemove = new ToolStripMenuItem("Remove");
            itemRemove.Click += (sender, e) => panel.RemoveVariables(vars);
            itemList.Add(itemRemove);

            ToolStripMenuItem itemRename = new ToolStripMenuItem("Rename...");
            itemRename.Click += (sender, e) =>
            {
                string template = DialogUtilities.GetStringFromDialog("$");
                if (template == null) return;
                foreach (WinFormsVariableControl control in vars)
                {
                    control.VarName = template.Replace("$", control.VarName);
                }
            };
            itemList.Add(itemRename);
            itemList.Add(new ToolStripSeparator());

            ToolStripMenuItem itemOpenController = new ToolStripMenuItem("Open Controller");
            itemOpenController.Click += (sender, e) =>
                new VariableControllerForm(
                    vars.ConvertAll(cell => cell.control.VarName),
                    vars
                ).Show();
            itemList.Add(itemOpenController);

            ToolStripMenuItem itemOpenPopOut = new ToolStripMenuItem("Open Pop Out");
            itemOpenPopOut.Click += (sender, e) =>
            {
                VariablePopOutForm form = new VariablePopOutForm();
                form.Initialize(vars);
                form.ShowForm();
            };
            itemList.Add(itemOpenPopOut);

            return itemList;
        }
    }
}
