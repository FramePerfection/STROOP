using StroopNext.App.Modules;
using StroopNext.Memory;
using StroopNext.Session;
using StroopNext.Settings;
using StroopNext.Variables;

namespace StroopNext.App.Controls;

internal sealed class VariableGrid : DataGridView
{
    private const int NameColumn = 0, ValueColumn = 1, DeltaColumn = 2;

    private readonly ModuleContext _context;
    private readonly SettingsScope _settings;
    private readonly ToolTip _errorTip = new();
    private readonly Dictionary<string, List<DataGridViewRow>> _groupRows = new();
    private readonly Font _groupFont;
    private readonly Font _valueFont = new("Consolas", 10f);

    public VariableGrid(ModuleContext context, IEnumerable<Variable> variables, IReadOnlyList<string> groupOrder)
    {
        _context = context;
        _settings = context.Settings.Child("grid");
        _groupFont = new Font(Font, FontStyle.Bold);

        Dock = DockStyle.Fill;
        AllowUserToAddRows = false;
        AllowUserToDeleteRows = false;
        AllowUserToResizeRows = false;
        RowHeadersVisible = false;
        SelectionMode = DataGridViewSelectionMode.CellSelect;
        MultiSelect = false;
        EditMode = DataGridViewEditMode.EditOnKeystrokeOrF2;
        BackgroundColor = SystemColors.Window;
        BorderStyle = BorderStyle.None;
        CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
        AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        DoubleBuffered = true;
        ShowCellToolTips = true;
        DefaultCellStyle.Font = _valueFont;
        ColumnHeadersDefaultCellStyle.Font = Font;

        Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Variable", ReadOnly = true, FillWeight = 38, DefaultCellStyle = { Font = Font } });
        Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Value", FillWeight = 42 });
        Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Δ frame", ReadOnly = true, FillWeight = 20, DefaultCellStyle = { ForeColor = Color.DimGray } });
        foreach (DataGridViewColumn column in Columns)
            column.SortMode = DataGridViewColumnSortMode.NotSortable;

        foreach (var group in groupOrder)
        {
            var members = variables.Where(v => v.Group == group).ToList();
            if (members.Count == 0)
                continue;
            AddGroupHeader(group);
            var rows = new List<DataGridViewRow>();
            foreach (var variable in members)
                rows.Add(AddVariableRow(variable));
            _groupRows[group] = rows;
            SetCollapsed(group, _settings.Get(CollapsedSetting(group)));
        }

        CellBeginEdit += OnCellBeginEdit;
        CellEndEdit += OnCellEndEdit;
        CellClick += OnCellClick;
        ContextMenuStrip = BuildContextMenu();
    }

    private static Setting<bool> CollapsedSetting(string group) => new($"collapsed.{group}", group, false);

    private void AddGroupHeader(string group)
    {
        int index = Rows.Add(group, "", "");
        var row = Rows[index];
        row.Tag = group;
        row.ReadOnly = true;
        row.DefaultCellStyle.BackColor = SystemColors.ControlLight;
        row.DefaultCellStyle.SelectionBackColor = SystemColors.ControlLight;
        row.DefaultCellStyle.SelectionForeColor = SystemColors.ControlText;
        row.DefaultCellStyle.Font = _groupFont;
    }

    private DataGridViewRow AddVariableRow(Variable variable)
    {
        int index = Rows.Add("   " + variable.Label, "", "");
        var row = Rows[index];
        row.Tag = variable;
        row.Cells[ValueColumn].ReadOnly = !variable.CanWrite;
        if (!variable.CanWrite)
            row.Cells[ValueColumn].Style.ForeColor = Color.FromArgb(70, 70, 90);
        string tip = $"{variable.Id}{(variable.CanWrite ? " · editable (F2 or type)" : " · read-only")}";
        if (variable.Description is { } description)
            tip = description + Environment.NewLine + tip;
        row.Cells[NameColumn].ToolTipText = tip;
        return row;
    }

    private void SetCollapsed(string group, bool collapsed)
    {
        foreach (var row in _groupRows[group])
            row.Visible = !collapsed;
        foreach (DataGridViewRow row in Rows)
            if (row.Tag is string g && g == group)
                row.Cells[NameColumn].Value = (collapsed ? "▸ " : "▾ ") + group;
        _settings.Set(CollapsedSetting(group), collapsed);
    }

    private void OnCellClick(object? sender, DataGridViewCellEventArgs e)
    {
        if (e.RowIndex >= 0 && Rows[e.RowIndex].Tag is string group && _groupRows.ContainsKey(group))
            SetCollapsed(group, _groupRows[group].Count > 0 && _groupRows[group][0].Visible);
    }

    public void Render(GameFrame frame)
    {
        var display = _context.Display;
        foreach (DataGridViewRow row in Rows)
        {
            if (row.Tag is not Variable variable || !row.Visible)
                continue;
            if (!(IsCurrentCellInEditMode && CurrentCell?.RowIndex == row.Index))
                SetIfChanged(row.Cells[ValueColumn], variable.Format(frame, display));
            string? delta = variable.FormatDelta(frame, display);
            SetIfChanged(row.Cells[DeltaColumn], delta is null or "0" or "+0" ? "" : delta);
        }
    }

    private static void SetIfChanged(DataGridViewCell cell, string text)
    {
        if (!Equals(cell.Value, text))
            cell.Value = text;
    }

    private string? _textBeforeEdit;

    private void OnCellBeginEdit(object? sender, DataGridViewCellCancelEventArgs e)
    {
        if (e.ColumnIndex != ValueColumn || Rows[e.RowIndex].Tag is not Variable { CanWrite: true })
        {
            e.Cancel = true;
            return;
        }
        _textBeforeEdit = Rows[e.RowIndex].Cells[ValueColumn].Value as string;
    }

    private void OnCellEndEdit(object? sender, DataGridViewCellEventArgs e)
    {
        if (Rows[e.RowIndex].Tag is not Variable variable || _context.CurrentFrame is not { } frame)
            return;
        string text = Rows[e.RowIndex].Cells[ValueColumn].Value as string ?? "";
        if (text == _textBeforeEdit)
            return;
        var patch = new RamPatch();
        if (variable.TryWrite(frame, text, _context.Display, patch, out string? error))
            _context.Apply(patch, $"{variable.Label} ← {text}");
        else
        {
            _context.ReportError(error ?? "Invalid value.");
            var rect = GetCellDisplayRectangle(ValueColumn, e.RowIndex, true);
            _errorTip.Show(error, this, rect.Left, rect.Bottom, 2500);
        }
        Rows[e.RowIndex].Cells[ValueColumn].Value = variable.Format(frame, _context.Display);
    }

    private ContextMenuStrip BuildContextMenu()
    {
        var menu = new ContextMenuStrip();
        menu.Items.Add("Copy value", null, (_, _) => CopyCurrent(ValueColumn));
        menu.Items.Add("Copy variable id", null, (_, _) =>
        {
            if (CurrentCell is { } cell && Rows[cell.RowIndex].Tag is Variable v)
                Clipboard.SetText(v.Id);
        });
        menu.Items.Add("Copy all values", null, (_, _) => CopyAll());
        menu.Items.Add(new ToolStripSeparator());
        menu.Items.Add("Expand all groups", null, (_, _) => SetAll(false));
        menu.Items.Add("Collapse all groups", null, (_, _) => SetAll(true));
        return menu;
    }

    private void SetAll(bool collapsed)
    {
        foreach (var group in _groupRows.Keys)
            SetCollapsed(group, collapsed);
    }

    private void CopyCurrent(int column)
    {
        if (CurrentCell is { } cell && Rows[cell.RowIndex].Cells[column].Value is string text && text.Length > 0)
            Clipboard.SetText(text);
    }

    private void CopyAll()
    {
        if (_context.CurrentFrame is not { } frame)
            return;
        var lines = Rows.Cast<DataGridViewRow>()
            .Select(r => r.Tag)
            .OfType<Variable>()
            .Select(v => $"{v.Label}\t{v.Format(frame, _context.Display)}");
        Clipboard.SetText(string.Join(Environment.NewLine, lines));
        _context.ReportStatus("Copied all values (tab-separated).");
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _errorTip.Dispose();
            _groupFont.Dispose();
            _valueFont.Dispose();
            ContextMenuStrip?.Dispose();
        }
        base.Dispose(disposing);
    }
}
