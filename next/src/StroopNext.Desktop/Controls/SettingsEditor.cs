using System.Globalization;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using StroopNext.Settings;

namespace StroopNext.Desktop.Controls;

internal static class SettingsEditor
{
    public static Control Build(IEnumerable<Setting> settings, SettingsScope scope, Action onChanged)
    {
        var panel = new StackPanel { Spacing = 4, Margin = new Thickness(22, 0, 0, 6) };
        foreach (var setting in settings)
        {
            var editor = CreateEditor(setting, scope, onChanged);
            if (setting.Description is { } description)
                ToolTip.SetTip(editor, description);
            if (editor is CheckBox)
                panel.Children.Add(editor);
            else
            {
                var row = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 8 };
                row.Children.Add(new TextBlock { Text = setting.Label, VerticalAlignment = VerticalAlignment.Center });
                row.Children.Add(editor);
                panel.Children.Add(row);
            }
        }
        return panel;
    }

    private static Control CreateEditor(Setting setting, SettingsScope scope, Action onChanged)
    {
        object? value = scope.GetBoxed(setting);
        switch (value)
        {
            case bool b:
                var check = new CheckBox { Content = setting.Label, IsChecked = b };
                check.IsCheckedChanged += (_, _) => { scope.SetBoxed(setting, check.IsChecked == true); onChanged(); };
                return check;

            case int or double:
                var range = setting.GetType().GetProperty("Range")?.GetValue(setting) as (double Min, double Max, double Step)?;
                bool isInt = value is int;
                var number = new NumericUpDown
                {
                    Minimum = (decimal)(range?.Min ?? -1e6),
                    Maximum = (decimal)(range?.Max ?? 1e6),
                    Increment = (decimal)(range?.Step ?? 1),
                    FormatString = isInt ? "0" : "0." + new string('#', Math.Max(1, DecimalsFor(range?.Step ?? 0.01))),
                    Width = 130,
                };
                number.Value = Math.Clamp(Convert.ToDecimal(value, CultureInfo.InvariantCulture), number.Minimum, number.Maximum);
                number.ValueChanged += (_, _) =>
                {
                    if (number.Value is not { } v)
                        return;
                    scope.SetBoxed(setting, v);
                    onChanged();
                };
                return number;

            case Enum e:
                var combo = new ComboBox { Width = 160, ItemsSource = Enum.GetValues(e.GetType()), SelectedItem = e };
                combo.SelectionChanged += (_, _) =>
                {
                    if (combo.SelectedItem is null)
                        return;
                    scope.SetBoxed(setting, combo.SelectedItem);
                    onChanged();
                };
                return combo;

            default:
                var text = new TextBox { Text = value?.ToString() ?? "", Width = 160 };
                text.LostFocus += (_, _) => { scope.SetBoxed(setting, text.Text); onChanged(); };
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
