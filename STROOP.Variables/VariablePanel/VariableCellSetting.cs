using STROOP.Variables.Utilities;
using System.Drawing;

namespace STROOP.Variables.VariablePanel
{
    public class VariableCellSetting<TUiContext>
        where TUiContext : IUiContext
    {
        public readonly string Name;
        public readonly Func<VariableCellControl<TUiContext>, object, bool> SetterFunction;
        public readonly (string name, Func<object> valueGetter, Func<VariableCellControl<TUiContext>, bool> isSelected)[] DropDownValues;

        /// <summary>Constructs a new WatchVariableSetting</summary>
        /// <param name="name">The name of the setting as displayed in the DropdownBox</param>
        /// <param name="setterFunction">The function that applies a selected value to a WatchVariableControl. If no <see cref="DropDownValues"/> are provided, this will be called with <see langword="null"/></param>
        /// <param name="dropDownValues">
        /// A list of tuples representing selectable values, where 'name' is a readable representation of the selection, 'valueGetter' is a function returning the value associated with the setter, and 'isSelected' yields whether this option can be seen as selected
        /// </param>
        public VariableCellSetting(
            string name,
            Func<VariableCellControl<TUiContext>, object, bool> setterFunction,
            params (string name, Func<object> valueGetter, Func<VariableCellControl<TUiContext>, bool> isSelected)[] dropDownValues
        )
        {
            this.Name = name;
            this.SetterFunction = setterFunction;
            this.DropDownValues = dropDownValues;
        }
    }

    partial class VariableCellControl<TUiContext>
    {
        class DefaultSettings
        {
            public static readonly VariableCellSetting<TUiContext> HighlightCellSetting = new VariableCellSetting<TUiContext>(
                "Highlight",
                (ctrl, obj) =>
                {
                    if (obj is bool newHighlighted)
                        ctrl.Highlighted = newHighlighted;
                    else return false;
                    return true;
                },
                ("Highlight", () => true, ctrl => ctrl.Highlighted),
                ("Don't Highlight", () => false, ctrl => !ctrl.Highlighted)
            );

            public static readonly VariableCellSetting<TUiContext> HighlightColorCellSetting = new VariableCellSetting<TUiContext>(
                "Highlight Color",
                (ctrl, obj) =>
                {
                    if (obj is Color newColor)
                    {
                        ctrl.HighlightColor = newColor;
                        ctrl.Highlighted = true;
                    }
                    else return false;

                    return true;
                },
                ("Red", () => Color.Red, ctrl => ctrl.HighlightColor == Color.Red),
                ("Orange", () => Color.Orange, ctrl => ctrl.HighlightColor == Color.Orange),
                ("Yellow", () => Color.Yellow, ctrl => ctrl.HighlightColor == Color.Yellow),
                ("Green", () => Color.Green, ctrl => ctrl.HighlightColor == Color.Green),
                ("Blue", () => Color.Blue, ctrl => ctrl.HighlightColor == Color.Blue),
                ("Purple", () => Color.Purple, ctrl => ctrl.HighlightColor == Color.Purple),
                ("Pink", () => Color.Pink, ctrl => ctrl.HighlightColor == Color.Pink),
                ("Brown", () => Color.Brown, ctrl => ctrl.HighlightColor == Color.Brown),
                ("Black", () => Color.Black, ctrl => ctrl.HighlightColor == Color.Black),
                ("White", () => Color.White, ctrl => ctrl.HighlightColor == Color.White)
            );

            public static readonly VariableCellSetting<TUiContext> FixAddressCellSetting = new VariableCellSetting<TUiContext>(
                "Fix Address",
                (ctrl, obj) =>
                {
                    if (ctrl.view is IMemoryVariable memoryDescriptorView)
                    {
                        if (obj is bool newFixAddress)
                            memoryDescriptorView.describedMemoryState.ToggleFixedAddress(newFixAddress);
                        else if (obj == null)
                            memoryDescriptorView.describedMemoryState.ToggleFixedAddress(false);
                        return true;
                    }

                    return false;
                },
                ("Default", () => null, ctrl => !((ctrl.view as IMemoryVariable)?.describedMemoryState.fixedAddresses ?? false)),
                ("Fix Address", () => true, ctrl => (ctrl.view as IMemoryVariable)?.describedMemoryState.fixedAddresses ?? false),
                ("Don't Fix Address", () => false, ctrl => !((ctrl.view as IMemoryVariable)?.describedMemoryState.fixedAddresses ?? true))
            );

            private static readonly object RevertToDefaultColor = new object();

            public static readonly VariableCellSetting<TUiContext> BackgroundColorCellSetting = new VariableCellSetting<TUiContext>(
                "Background Color",
                (ctrl, obj) =>
                {
                    if (obj is Color newColor)
                        ctrl.BaseColor = newColor;
                    else if (obj == RevertToDefaultColor)
                        ctrl.BaseColor = ctrl._initialBaseColor;
                    else return false;
                    return true;
                },
                ((Func<(string, Func<object>, Func<VariableCellControl<TUiContext>, bool>)[]>)(() =>
                {
                    var lst = new List<(string, Func<object>, Func<VariableCellControl<TUiContext>, bool>)>();
                    lst.Add(("Default", () => RevertToDefaultColor, ctrl => ctrl.BaseColor == ctrl._initialBaseColor));
                    foreach (KeyValuePair<string, string> pair in ColorUtilities.ColorToParamsDictionary)
                    {
                        Color color = ColorTranslator.FromHtml(pair.Value);
                        string colorString = pair.Key;
                        if (colorString == "LightBlue") colorString = "Light Blue";
                        lst.Add((colorString, () => color, ctrl => ctrl.BaseColor == color));
                    }

                    lst.Add(("Control (No Color)", () => SystemColors.Control, ctrl => ctrl.BaseColor == SystemColors.Control));
                    // lst.Add(("Custom Color", () => ColorUtilities.GetColorFromDialog(SystemColors.Control), null));
                    return lst.ToArray();
                }))()
            );
        }
    }
}
