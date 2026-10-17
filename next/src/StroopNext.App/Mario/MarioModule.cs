using StroopNext.App.Controls;
using StroopNext.App.Modules;
using StroopNext.Memory;
using StroopNext.Session;
using StroopNext.Settings;
using StroopNext.Variables;

namespace StroopNext.App.Mario;

internal sealed class MarioModule : IStroopModule
{
    public string Id => "mario";
    public string Title => "Mario";
    public ModuleView CreateView(ModuleContext context) => new MarioView(context);
}

internal sealed class MarioView : ModuleView
{
    private static readonly Setting<double> MoveStep = new("moveStep", "Move step", 10) { Range = (0.001, 100000, 1) };
    private static readonly Setting<bool> MoveRelative = new("moveRelative", "Relative to facing yaw", true);
    private static readonly Setting<double> SpeedStep = new("speedStep", "Speed step", 1) { Range = (0.001, 1000, 0.5) };
    private static readonly Setting<int> YawStep = new("yawStep", "Yaw step (units)", 16) { Range = (1, 32768, 16) };

    private readonly VariableGrid _grid;
    private readonly Label _headline = new() { Dock = DockStyle.Top, Height = 34, Font = new Font("Segoe UI", 12f, FontStyle.Bold), TextAlign = ContentAlignment.MiddleLeft, Padding = new Padding(8, 0, 0, 0) };

    public MarioView(ModuleContext context) : base(context)
    {
        _grid = new VariableGrid(context, MarioVariables.All, MarioVariables.Groups);

        var split = new SplitContainer { Dock = DockStyle.Fill, FixedPanel = FixedPanel.Panel2, SplitterWidth = 6 };
        split.Panel1.Controls.Add(_grid);
        split.Panel1.Controls.Add(_headline);
        split.Panel2.Controls.Add(BuildCommands());
        Controls.Add(split);
        split.HandleCreated += (_, _) => split.SplitterDistance = Math.Max(200, split.Width - 300);
    }

    public override void OnFrame(GameFrame frame)
    {
        _grid.Render(frame);
        _headline.Text = frame.Mario is { } m
            ? $"{frame.Services.Actions.NameOf(m.Action) ?? $"action 0x{m.Action:X8}"}   ·   timer {frame.GlobalTimer}"
            : frame.IsKnownGame ? "Mario not available" : "No SM64 game detected";
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
            _headline.Font.Dispose();
        base.Dispose(disposing);
    }

    public override void OnDisplayOptionsChanged()
    {
        if (Context.CurrentFrame is { } frame)
            OnFrame(frame);
    }

    private Control BuildCommands()
    {
        var root = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.TopDown, WrapContents = false, AutoScroll = true, Padding = new Padding(6) };
        var settings = Context.Settings;

        var move = Section("Move", root);
        var moveStep = Number(settings, MoveStep);
        var relative = new CheckBox { Text = MoveRelative.Label, Checked = settings.Get(MoveRelative), AutoSize = true };
        relative.CheckedChanged += (_, _) => settings.Set(MoveRelative, relative.Checked);
        move.Controls.Add(Row(new Label { Text = "Step", AutoSize = true, Margin = new Padding(3, 7, 3, 3) }, moveStep));
        move.Controls.Add(relative);
        var pad = new TableLayoutPanel { ColumnCount = 3, RowCount = 3, AutoSize = true };
        float Step() => (float)moveStep.Value;
        void Move(float forward, float right, float up, string what) => Run(frame =>
            relative.Checked ? MarioCommands.MoveRelative(frame, forward * Step(), right * Step(), up * Step())
                             : MarioCommands.Translate(frame, right * Step(), up * Step(), -forward * Step()), what);
        pad.Controls.Add(Button("▲", () => Move(1, 0, 0, "Moved forward")), 1, 0);
        pad.Controls.Add(Button("◀", () => Move(0, -1, 0, "Moved left")), 0, 1);
        pad.Controls.Add(Button("▶", () => Move(0, 1, 0, "Moved right")), 2, 1);
        pad.Controls.Add(Button("▼", () => Move(-1, 0, 0, "Moved back")), 1, 2);
        pad.Controls.Add(Button("Y+", () => Move(0, 0, 1, "Moved up")), 2, 0);
        pad.Controls.Add(Button("Y−", () => Move(0, 0, -1, "Moved down")), 2, 2);
        move.Controls.Add(pad);
        move.Controls.Add(Hint("Relative: ▲ is Mario's facing direction. Absolute: ▲ is −Z, ▶ is +X (map up/right)."));

        var speed = Section("Speed", root);
        var speedStep = Number(settings, SpeedStep);
        speed.Controls.Add(Row(new Label { Text = "Step", AutoSize = true, Margin = new Padding(3, 7, 3, 3) }, speedStep));
        speed.Controls.Add(Row(
            new Label { Text = "H speed", AutoSize = true, Width = 60, Margin = new Padding(3, 7, 3, 3) },
            Button("−", () => Run(f => MarioCommands.AddHSpeed(f, -(float)speedStep.Value), "H speed decreased")),
            Button("+", () => Run(f => MarioCommands.AddHSpeed(f, (float)speedStep.Value), "H speed increased"))));
        speed.Controls.Add(Row(
            new Label { Text = "Y speed", AutoSize = true, Width = 60, Margin = new Padding(3, 7, 3, 3) },
            Button("−", () => Run(f => MarioCommands.AddVSpeed(f, -(float)speedStep.Value), "Y speed decreased")),
            Button("+", () => Run(f => MarioCommands.AddVSpeed(f, (float)speedStep.Value), "Y speed increased"))));

        var yaw = Section("Facing yaw", root);
        var yawStep = Number(settings, YawStep);
        yaw.Controls.Add(Row(new Label { Text = "Step", AutoSize = true, Margin = new Padding(3, 7, 3, 3) }, yawStep));
        yaw.Controls.Add(Row(
            Button("⟲ +", () => Run(f => MarioCommands.AddYaw(f, (int)yawStep.Value), "Yaw increased")),
            Button("⟳ −", () => Run(f => MarioCommands.AddYaw(f, -(int)yawStep.Value), "Yaw decreased"))));
        yaw.Controls.Add(Hint("+ turns counter-clockwise seen from above (towards +X from +Z)."));

        root.Controls.Add(Hint("Every value in the table is editable when shown in black: select it and type, or press F2. Angles accept units, 0x hex or degrees (\"90°\")."));
        return root;
    }

    private void Run(Func<GameFrame, RamPatch> command, string description)
    {
        if (Context.CurrentFrame is not { } frame)
        {
            Context.ReportError("No frame yet: attach to an emulator or open a savestate.");
            return;
        }
        Context.Apply(command(frame), description);
    }

    private static FlowLayoutPanel Section(string title, Control parent)
    {
        var box = new GroupBox { Text = title, AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink, MinimumSize = new Size(270, 0), Padding = new Padding(6) };
        var body = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.TopDown, WrapContents = false, AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink };
        box.Controls.Add(body);
        parent.Controls.Add(box);
        return body;
    }

    private static FlowLayoutPanel Row(params Control[] controls)
    {
        var row = new FlowLayoutPanel { AutoSize = true, WrapContents = false, Margin = new Padding(0) };
        row.Controls.AddRange(controls);
        return row;
    }

    private static Button Button(string text, Action onClick)
    {
        var button = new Button { Text = text, Width = 52, Height = 30 };
        button.Click += (_, _) => onClick();
        return button;
    }

    private static Label Hint(string text) => new() { Text = text, AutoSize = true, MaximumSize = new Size(260, 0), ForeColor = Color.DimGray, Margin = new Padding(3, 6, 3, 6) };

    private static NumericUpDown Number<T>(SettingsScope scope, Setting<T> setting)
    {
        var range = setting.Range ?? (0, 1000000, 1);
        var number = new NumericUpDown
        {
            Minimum = (decimal)range.Min,
            Maximum = (decimal)range.Max,
            Increment = (decimal)range.Step,
            DecimalPlaces = typeof(T) == typeof(int) ? 0 : 3,
            Width = 100,
        };
        number.Value = Math.Clamp(Convert.ToDecimal(scope.Get(setting), System.Globalization.CultureInfo.InvariantCulture), number.Minimum, number.Maximum);
        number.ValueChanged += (_, _) => scope.SetBoxed(setting, number.Value);
        return number;
    }
}
