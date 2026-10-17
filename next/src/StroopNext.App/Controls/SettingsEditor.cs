using System.Globalization;
using StroopNext.Settings;

namespace StroopNext.App.Controls;

internal static class SettingsEditor
{
    public static Control Build(IEnumerable<Setting> settings, SettingsScope scope, Action onChanged)
    {
        var panel = new TableLayoutPanel { ColumnCount = 2, AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink, Dock = DockStyle.Top, Padding = new Padding(12, 0, 0, 4) };
        panel.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        panel.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        var tips = new ToolTip();
        panel.Disposed += (_, _) => tips.Dispose();

        foreach (var setting in settings)
        {
            var editor = CreateEditor(setting, scope, onChanged);
            if (editor is CheckBox)
            {
                panel.Controls.Add(editor);
                panel.SetColumnSpan(editor, 2);
            }
            else
            {
                panel.Controls.Add(new Label { Text = setting.Label, AutoSize = true, Anchor = AnchorStyles.Left, Margin = new Padding(3, 6, 3, 3) });
                panel.Controls.Add(editor);
            }
            if (setting.Description is { } description)
                tips.SetToolTip(editor, description);
        }
        return panel;
    }

    private static Control CreateEditor(Setting setting, SettingsScope scope, Action onChanged)
    {
        object? value = scope.GetBoxed(setting);
        switch (value)
        {
            case bool b:
                var check = new CheckBox { Text = setting.Label, Checked = b, AutoSize = true };
                check.CheckedChanged += (_, _) => { scope.SetBoxed(setting, check.Checked); onChanged(); };
                return check;

            case int or double:
                var range = setting.GetType().GetProperty("Range")?.GetValue(setting) as (double Min, double Max, double Step)?;
                bool isInt = value is int;
                var number = new NumericUpDown
                {
                    Minimum = (decimal)(range?.Min ?? -1e6),
                    Maximum = (decimal)(range?.Max ?? 1e6),
                    Increment = (decimal)(range?.Step ?? 1),
                    DecimalPlaces = isInt ? 0 : DecimalsFor(range?.Step ?? 0.01),
                    Width = 90,
                };
                number.Value = Math.Clamp(Convert.ToDecimal(value, CultureInfo.InvariantCulture), number.Minimum, number.Maximum);
                number.ValueChanged += (_, _) =>
                {
                    scope.SetBoxed(setting, number.Value);
                    onChanged();
                };
                return number;

            case Enum e:
                var combo = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Width = 140 };
                foreach (var option in Enum.GetValues(e.GetType()))
                    combo.Items.Add(option);
                combo.SelectedItem = e;
                combo.SelectedIndexChanged += (_, _) => { scope.SetBoxed(setting, combo.SelectedItem); onChanged(); };
                return combo;

            default:
                var text = new TextBox { Text = value?.ToString() ?? "", Width = 140 };
                text.Leave += (_, _) => { scope.SetBoxed(setting, text.Text); onChanged(); };
                return text;
        }
    }

    private static int DecimalsFor(double step)
    {
        int decimals = 0;
        while (decimals < 4 && Math.Abs(step * Math.Pow(10, decimals) - Math.Round(step * Math.Pow(10, decimals))) > 1e-9)
            decimals++;
        return decimals;
    }
}
