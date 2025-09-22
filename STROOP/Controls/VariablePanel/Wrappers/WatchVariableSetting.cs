using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using STROOP.Utilities;
using STROOP.Variables;

namespace STROOP.Controls.VariablePanel
{
    public class WatchVariableSetting
    {
        public readonly string Name;
        public readonly Func<WatchVariableControl, object, bool> SetterFunction;
        public readonly (string name, Func<object> valueGetter, Func<WatchVariableControl, bool> isSelected)[] DropDownValues;

        /// <summary>Constructs a new WatchVariableSetting</summary>
        /// <param name="name">The name of the setting as displayed in the DropdownBox</param>
        /// <param name="setterFunction">The function that applies a selected value to a WatchVariableControl. If no <see cref="DropDownValues"/> are provided, this will be called with <see langword="null"/></param>
        /// <param name="dropDownValues">
        /// A list of tuples representing selectable values, where 'name' is a readable representation of the selection, 'valueGetter' is a function returning the value associated with the setter, and 'isSelected' yields whether this option can be seen as selected
        /// </param>
        public WatchVariableSetting(
            string name,
            Func<WatchVariableControl, object, bool> setterFunction,
            params (string name, Func<object> valueGetter, Func<WatchVariableControl, bool> isSelected)[] dropDownValues
        )
        {
            this.Name = name;
            this.SetterFunction = setterFunction;
            this.DropDownValues = dropDownValues;
        }

        public void CreateContextMenuEntry(ToolStripItemCollection target, Func<List<WatchVariableControl>> getWatchVars)
        {
            string mainItemText = Name;
            if (DropDownValues.Length > 0)
                mainItemText += "...";

            var optionsItem = new ToolStripMenuItem(mainItemText);
            foreach (var option in DropDownValues)
            {
                var item = new ToolStripMenuItem(option.name);
                var getter = option.valueGetter;
                item.Click += (_, __) =>
                {
                    var value = getter();
                    getWatchVars().ForEach(v => v.ApplySettings(Name, value));
                };

                if (option.isSelected != null)
                {
                    bool? firstValue = null;
                    CheckState state = CheckState.Unchecked;
                    foreach (var c in getWatchVars())
                    {
                        bool selected = option.isSelected(c);
                        if (firstValue == null)
                            firstValue = selected;
                        else if (selected != firstValue)
                            state = CheckState.Indeterminate;
                    }

                    if (state == CheckState.Indeterminate)
                        item.CheckState = CheckState.Indeterminate;
                    else
                        item.Checked = !firstValue.HasValue ? false : firstValue.Value;
                }

                optionsItem.DropDownItems.Add(item);
            }

            if (DropDownValues.Length == 0)
                optionsItem.Click += (_, __) => getWatchVars().ForEach(v => v.ApplySettings(Name, null));

            target.Add(optionsItem);
        }
    }

    partial class WatchVariableControl
    {
        class DefaultSettings
        {
            public static readonly WatchVariableSetting HighlightSetting = new WatchVariableSetting(
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

            public static readonly WatchVariableSetting HighlightColorSetting = new WatchVariableSetting(
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

            public static readonly WatchVariableSetting LockSetting = new WatchVariableSetting(
                "Lock",
                (ctrl, obj) =>
                {
                    if (obj is bool newLock && ctrl.view is NamedVariableCollection.IMemoryDescriptorView memoryDescriptorView)
                        return memoryDescriptorView.describedMemoryState.SetLocked(newLock, null);
                    else
                        return false;
                },
                ("Lock", () => true, ctrl => (ctrl.view as NamedVariableCollection.IMemoryDescriptorView)?.describedMemoryState.locked ?? false),
                ("Don't Lock", () => false, ctrl => !((ctrl.view as NamedVariableCollection.IMemoryDescriptorView)?.describedMemoryState.locked ?? true))
            );


            public static readonly WatchVariableSetting FixAddressSetting = new WatchVariableSetting(
                "Fix Address",
                (ctrl, obj) =>
                {
                    if (ctrl.view is NamedVariableCollection.IMemoryDescriptorView memoryDescriptorView)
                    {
                        if (obj is bool newFixAddress)
                            memoryDescriptorView.describedMemoryState.ToggleFixedAddress(newFixAddress);
                        else if (obj == null)
                            memoryDescriptorView.describedMemoryState.ToggleFixedAddress(false);
                        return true;
                    }

                    return false;
                },
                ("Default", () => null, ctrl => !((ctrl.view as NamedVariableCollection.IMemoryDescriptorView)?.describedMemoryState.fixedAddresses ?? false)),
                ("Fix Address", () => true, ctrl => (ctrl.view as NamedVariableCollection.IMemoryDescriptorView)?.describedMemoryState.fixedAddresses ?? false),
                ("Don't Fix Address", () => false, ctrl => !((ctrl.view as NamedVariableCollection.IMemoryDescriptorView)?.describedMemoryState.fixedAddresses ?? true))
            );

            private static readonly object RevertToDefaultColor = new object();

            public static readonly WatchVariableSetting BackgroundColorSetting = new WatchVariableSetting(
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
                ((Func<(string, Func<object>, Func<WatchVariableControl, bool>)[]>)(() =>
                {
                    var lst = new List<(string, Func<object>, Func<WatchVariableControl, bool>)>();
                    lst.Add(("Default", () => RevertToDefaultColor, ctrl => ctrl.BaseColor == ctrl._initialBaseColor));
                    foreach (KeyValuePair<string, string> pair in ColorUtilities.ColorToParamsDictionary)
                    {
                        Color color = ColorTranslator.FromHtml(pair.Value);
                        string colorString = pair.Key;
                        if (colorString == "LightBlue") colorString = "Light Blue";
                        lst.Add((colorString, () => color, ctrl => ctrl.BaseColor == color));
                    }

                    lst.Add(("Control (No Color)", () => SystemColors.Control, ctrl => ctrl.BaseColor == SystemColors.Control));
                    lst.Add(("Custom Color", () => ColorUtilities.GetColorFromDialog(SystemColors.Control), null));
                    return lst.ToArray();
                }))()
            );
        }
    }
}
