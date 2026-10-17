using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Input.Platform;
using Avalonia.Layout;
using Avalonia.Media;
using StroopNext.Desktop.Modules;
using StroopNext.Memory;
using StroopNext.Session;
using StroopNext.Settings;
using StroopNext.Variables;

namespace StroopNext.Desktop.Controls;

internal sealed class VariableGrid : UserControl
{
    private const string Columns = "38*,42*,20*";

    private readonly ModuleContext _context;
    private readonly SettingsScope _settings;
    private readonly StackPanel _rowsPanel = new();
    private readonly List<Row> _rows = new();
    private readonly Dictionary<string, (TextBlock Header, List<Row> Rows)> _groups = new();
    private readonly TextBox _editor = new()
    {
        FontFamily = AppTheme.Monospace,
        Padding = new Thickness(6, 2),
        MinHeight = 0,
        VerticalAlignment = VerticalAlignment.Center,
    };
    private Row? _editing;
    private string? _textBeforeEdit;
    private Row? _menuRow;

    private sealed class Row(Variable variable, Border root, Grid cells, TextBlock value, TextBlock delta)
    {
        public Variable Variable { get; } = variable;
        public Border Root { get; } = root;
        public Grid Cells { get; } = cells;
        public TextBlock Value { get; } = value;
        public TextBlock Delta { get; } = delta;
        public int DeltaSign { get; set; }
    }

    private readonly IReadOnlySet<string> _collapsedByDefault;

    public VariableGrid(ModuleContext context, IEnumerable<Variable> variables, IReadOnlyList<string> groupOrder, IReadOnlySet<string>? collapsedByDefault = null)
    {
        _context = context;
        _settings = context.Settings.Child("grid");
        _collapsedByDefault = collapsedByDefault ?? new HashSet<string>();

        foreach (var group in groupOrder)
        {
            var members = variables.Where(v => v.Group == group).ToList();
            if (members.Count == 0)
                continue;
            var header = AddGroupHeader(group, members.Count);
            var rows = members.Select(AddVariableRow).ToList();
            _groups[group] = (header, rows);
            SetCollapsed(group, _settings.Get(CollapsedSetting(group)));
        }

        _editor.KeyDown += (_, e) =>
        {
            if (e.Key == Key.Enter)
            {
                EndEdit(commit: true);
                e.Handled = true;
            }
            else if (e.Key == Key.Escape)
            {
                EndEdit(commit: false);
                e.Handled = true;
            }
        };
        _editor.LostFocus += (_, _) => EndEdit(commit: true);

        ContextMenu = BuildContextMenu();
        var titles = Cells(
            new TextBlock { Text = "VARIABLE", Classes = { "title" }, Margin = new Thickness(10, 8, 4, 6) },
            new TextBlock { Text = "VALUE", Classes = { "title" }, Margin = new Thickness(6, 8, 4, 6) },
            new TextBlock { Text = "Δ FRAME", Classes = { "title" }, Margin = new Thickness(6, 8, 4, 6) });
        var layout = new DockPanel();
        DockPanel.SetDock(titles, Dock.Top);
        layout.Children.Add(titles);
        layout.Children.Add(new ScrollViewer { Content = _rowsPanel, HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled });
        Content = layout;
    }

    private Setting<bool> CollapsedSetting(string group) => new($"collapsed.{group}", group, _collapsedByDefault.Contains(group));

    public event Action<Variable, Control>? PickRequested;

    private static Grid Cells(Control name, Control value, Control delta)
    {
        var grid = new Grid { ColumnDefinitions = new ColumnDefinitions(Columns) };
        Grid.SetColumn(value, 1);
        Grid.SetColumn(delta, 2);
        grid.Children.AddRange([name, value, delta]);
        return grid;
    }

    private TextBlock AddGroupHeader(string group, int count)
    {
        var header = new TextBlock { FontWeight = FontWeight.SemiBold, VerticalAlignment = VerticalAlignment.Center };
        var line = new DockPanel();
        var counter = new TextBlock { Text = count.ToString(System.Globalization.CultureInfo.InvariantCulture), Classes = { "muted" }, FontSize = 11, VerticalAlignment = VerticalAlignment.Center };
        DockPanel.SetDock(counter, Dock.Right);
        line.Children.AddRange([counter, header]);
        var border = new Border { Child = line, Classes = { "group" }, Cursor = new Cursor(StandardCursorType.Hand) };
        border.PointerPressed += (_, e) =>
        {
            if (e.GetCurrentPoint(border).Properties.IsLeftButtonPressed)
                SetCollapsed(group, !_settings.Get(CollapsedSetting(group)));
        };
        _rowsPanel.Children.Add(border);
        return header;
    }

    private Row AddVariableRow(Variable variable)
    {
        var name = new TextBlock { Text = variable.Label, Margin = new Thickness(22, 4, 4, 4), VerticalAlignment = VerticalAlignment.Center, TextTrimming = TextTrimming.CharacterEllipsis };
        string tip = $"{variable.Id}{(variable.CanWrite ? " · editable: click the value, type, Enter (Esc cancels)" : " · read-only")}";
        if (variable.Description is { } description)
            tip = description + Environment.NewLine + tip;
        ToolTip.SetTip(name, tip);

        var value = new TextBlock
        {
            FontFamily = AppTheme.Monospace,
            Margin = new Thickness(6, 4),
            VerticalAlignment = VerticalAlignment.Center,
            Foreground = variable.CanWrite ? AppTheme.TextBrush : AppTheme.MutedBrush,
        };
        if (variable.CanWrite)
            value.Cursor = new Cursor(StandardCursorType.Ibeam);
        var delta = new TextBlock { FontFamily = AppTheme.Monospace, FontSize = 12, Margin = new Thickness(6, 4), VerticalAlignment = VerticalAlignment.Center };

        var cells = Cells(name, value, delta);
        var root = new Border { Child = cells, Classes = { "row" }, Background = Brushes.Transparent };
        var row = new Row(variable, root, cells, value, delta);
        root.PointerPressed += (_, _) => _menuRow = row;
        root.ContextRequested += (_, _) => UpdateMenu(row);
        if (variable.CanWrite)
            value.PointerPressed += (_, e) =>
            {
                if (e.GetCurrentPoint(value).Properties.IsLeftButtonPressed)
                {
                    BeginEdit(row);
                    e.Handled = true;
                }
            };

        _rowsPanel.Children.Add(root);
        _rows.Add(row);
        return row;
    }

    private void SetCollapsed(string group, bool collapsed)
    {
        var (header, rows) = _groups[group];
        foreach (var row in rows)
            row.Root.IsVisible = !collapsed;
        header.Text = (collapsed ? "▸  " : "▾  ") + group;
        _settings.Set(CollapsedSetting(group), collapsed);
    }

    public void Render(GameFrame frame)
    {
        var display = _context.Display;
        foreach (var row in _rows)
        {
            if (!row.Root.IsVisible)
                continue;
            if (row != _editing)
            {
                string text = row.Variable.Format(frame, display);
                if (row.Value.Text != text)
                    row.Value.Text = text;
            }
            string? delta = row.Variable.FormatDelta(frame, display);
            string deltaText = delta is null or "0" or "+0" ? "" : delta;
            if (row.Delta.Text == deltaText)
                continue;
            row.Delta.Text = deltaText;
            int sign = deltaText.Length == 0 ? 0 : deltaText[0] is '-' or '−' ? -1 : 1;
            if (sign != row.DeltaSign)
            {
                row.DeltaSign = sign;
                row.Delta.Foreground = sign < 0 ? AppTheme.DecreaseBrush : AppTheme.IncreaseBrush;
            }
        }
    }

    private void BeginEdit(Row row)
    {
        if (_editing == row)
            return;
        EndEdit(commit: true);
        _editing = row;
        _textBeforeEdit = row.Value.Text;
        _editor.Text = row.Value.Text;
        row.Value.IsVisible = false;
        Grid.SetColumn(_editor, 1);
        row.Cells.Children.Add(_editor);
        _editor.Focus();
        _editor.SelectAll();
    }

    private void EndEdit(bool commit)
    {
        if (_editing is not { } row)
            return;
        _editing = null;
        row.Cells.Children.Remove(_editor);
        row.Value.IsVisible = true;
        if (!commit || _context.CurrentFrame is not { } frame)
            return;
        string text = _editor.Text ?? "";
        if (text == _textBeforeEdit)
            return;
        var patch = new RamPatch();
        if (row.Variable.TryWrite(frame, text, _context.Display, patch, out string? error))
            _context.Apply(patch, $"{row.Variable.Label} ← {text}");
        else
            _context.ReportError(error ?? "Invalid value.");
        row.Value.Text = row.Variable.Format(frame, _context.Display);
    }

    private MenuItem? _selectObject, _pick;

    private void UpdateMenu(Row row)
    {
        _menuRow = row;
        _selectObject!.IsVisible = row.Variable is Variable<ObjectReference>;
        _pick!.IsVisible = row.Variable is Variable<uint> { ValueFormat: ActionFormat } && row.Variable.CanWrite;
    }

    private ContextMenu BuildContextMenu()
    {
        var menu = new ContextMenu();
        _selectObject = new MenuItem { Header = "Select this object (map)", IsVisible = false };
        _selectObject.Click += (_, _) =>
        {
            if (_menuRow?.Variable is Variable<ObjectReference> v && _context.CurrentFrame is { } frame && v.Read(frame) is { Address: not 0 } r)
            {
                _context.Selection.SelectOnly(r.Address);
                _context.ReportStatus($"Selected {v.Format(frame, _context.Display)}.");
            }
        };
        _pick = new MenuItem { Header = "Choose from the list…", IsVisible = false };
        _pick.Click += (_, _) =>
        {
            if (_menuRow is { } row)
                PickRequested?.Invoke(row.Variable, row.Value);
        };
        menu.Items.Add(_selectObject);
        menu.Items.Add(_pick);
        menu.Items.Add(Item("Copy value", () => _menuRow?.Value.Text));
        menu.Items.Add(Item("Copy variable id", () => _menuRow?.Variable.Id));
        menu.Items.Add(Item("Copy all values", CopyAll));
        menu.Items.Add(new Separator());
        var expand = new MenuItem { Header = "Expand all groups" };
        expand.Click += (_, _) => SetAll(false);
        var collapse = new MenuItem { Header = "Collapse all groups" };
        collapse.Click += (_, _) => SetAll(true);
        menu.Items.Add(expand);
        menu.Items.Add(collapse);
        return menu;
    }

    private MenuItem Item(string header, Func<string?> text)
    {
        var item = new MenuItem { Header = header };
        item.Click += async (_, _) =>
        {
            if (text() is { Length: > 0 } value && TopLevel.GetTopLevel(this)?.Clipboard is { } clipboard)
                await clipboard.SetTextAsync(value);
        };
        return item;
    }

    private string? CopyAll()
    {
        if (_context.CurrentFrame is not { } frame)
            return null;
        _context.ReportStatus("Copied all values (tab-separated).");
        return string.Join(Environment.NewLine, _rows.Select(r => $"{r.Variable.Label}\t{r.Variable.Format(frame, _context.Display)}"));
    }

    private void SetAll(bool collapsed)
    {
        foreach (var group in _groups.Keys)
            SetCollapsed(group, collapsed);
    }
}
