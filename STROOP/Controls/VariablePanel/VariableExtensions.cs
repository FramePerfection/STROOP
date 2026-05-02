using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace STROOP.Controls.VariablePanel;

public static class VariableExtensions
{
    public static void CreateContextMenuEntry(this WinFormsVariableSetting setting, ToolStripItemCollection target, Func<List<IWinFormsVariableCell>> getWatchVars)
    {
        string mainItemText = setting.Name;
        if (setting.DropDownValues.Length > 0)
            mainItemText += "...";

        var optionsItem = new ToolStripMenuItem(mainItemText);
        foreach (var option in setting.DropDownValues)
        {
            var item = new ToolStripMenuItem(option.name);
            var getter = option.valueGetter;
            item.Click += (_, __) =>
            {
                var value = getter();
                getWatchVars().ForEach(v => v.control.ApplySettings(setting.Name, value));
            };

            if (option.isSelected != null)
            {
                bool? firstValue = null;
                CheckState state = CheckState.Unchecked;
                foreach (var c in getWatchVars())
                {
                    bool selected = option.isSelected(c.control);
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

        if (setting.DropDownValues.Length == 0)
            optionsItem.Click += (_, __) => getWatchVars().ForEach(v => v.control.ApplySettings(setting.Name, null));

        target.Add(optionsItem);
    }
}
