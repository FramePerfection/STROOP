using System.Globalization;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using StroopNext.Desktop.Controls;
using StroopNext.Desktop.Modules;
using StroopNext.Game;
using StroopNext.Memory;
using StroopNext.Session;
using StroopNext.Settings;
using StroopNext.Variables;

namespace StroopNext.Desktop.Mario;

internal sealed class MarioModule : IStroopModule
{
    public string Id => "mario";
    public string Title => "Mario";
    public ModuleView CreateView(ModuleContext context) => new MarioView(context);
}

internal sealed class MarioView : ModuleView
{
    private static readonly Setting<double> MoveStep = new("moveStep", "Move step", 100) { Range = (0.001, 100000, 10) };
    private static readonly Setting<double> MoveStepY = new("moveStepY", "Y step", 100) { Range = (0.001, 100000, 10) };
    private static readonly Setting<bool> MoveRelative = new("moveRelative", "Relative to facing yaw", true);
    private static readonly Setting<double> SpeedStep = new("speedStep", "Speed step", 1) { Range = (0.001, 1000, 0.5) };
    private static readonly Setting<int> YawStep = new("yawStep", "Yaw step (units)", 1024) { Range = (1, 32768, 16) };
    private static readonly Setting<double> HolpStep = new("holpStep", "HOLP step", 100) { Range = (0.001, 100000, 10) };
    private static readonly Setting<bool> HolpRelative = new("holpRelative", "HOLP relative to facing yaw", true);

    private readonly VariableGrid _grid;
    private readonly TextBlock _headline = new() { FontSize = 18, FontWeight = FontWeight.SemiBold, TextTrimming = TextTrimming.CharacterEllipsis };
    private readonly TextBlock _subline = new() { Classes = { "muted", "mono" }, FontSize = 12 };

    public MarioView(ModuleContext context) : base(context)
    {
        _grid = new VariableGrid(context, MarioVariables.All, MarioVariables.Groups, MarioVariables.CollapsedByDefault);
        _grid.PickRequested += (variable, anchor) => ShowActionPicker(variable, anchor);

        var heading = new StackPanel { Spacing = 2, Margin = new Thickness(12, 10, 12, 8) };
        heading.Children.AddRange([_headline, _subline]);
        var table = new Border { Classes = { "card" }, Padding = new Thickness(0, 0, 0, 4), Child = _grid, ClipToBounds = true };
        var left = new DockPanel { Margin = new Thickness(8, 0, 0, 8) };
        DockPanel.SetDock(heading, Dock.Top);
        left.Children.Add(heading);
        left.Children.Add(table);

        var commands = new ScrollViewer { Content = BuildCommands() };
        var layout = new Grid { ColumnDefinitions = new ColumnDefinitions("*,8,320") };
        Grid.SetColumn(left, 0);
        var splitter = new GridSplitter { ResizeDirection = GridResizeDirection.Columns };
        Grid.SetColumn(splitter, 1);
        Grid.SetColumn(commands, 2);
        layout.Children.AddRange([left, splitter, commands]);
        Content = layout;
    }

    public override void OnFrame(GameFrame frame)
    {
        _grid.Render(frame);
        string headline = frame.Mario is { } m
            ? frame.Services.Actions.NameOf(m.Action) ?? $"action 0x{m.Action:X8}"
            : frame.IsKnownGame ? "Mario not available" : "No SM64 game detected";
        string subline = frame.Mario is { } mario ? $"action 0x{mario.Action:X8}  ·  timer {frame.GlobalTimer}" : $"timer {frame.GlobalTimer}";
        if (_headline.Text != headline)
            _headline.Text = headline;
        if (_subline.Text != subline)
            _subline.Text = subline;
    }

    public override void OnDisplayOptionsChanged()
    {
        if (Context.CurrentFrame is { } frame)
            OnFrame(frame);
    }

    private Control BuildCommands()
    {
        var root = new StackPanel { Spacing = 10, Margin = new Thickness(0, 10, 10, 10) };
        var settings = Context.Settings;

        var move = Section("Move", root);
        var xzStep = Number(settings, MoveStep);
        var yStep = Number(settings, MoveStepY);
        var relative = new CheckBox { Content = MoveRelative.Label, IsChecked = settings.Get(MoveRelative) };
        relative.IsCheckedChanged += (_, _) => settings.Set(MoveRelative, relative.IsChecked == true);
        move.Children.Add(Row(Label("XZ step", 56), xzStep));
        move.Children.Add(Row(Label("Y step", 56), yStep));
        move.Children.Add(relative);
        void Move(int forward, int right, int up, string what)
        {
            float scale = forward != 0 && right != 0 ? 1 / MathF.Sqrt(2) : 1;
            float f = forward * (float)(xzStep.Value ?? 0) * scale, r = right * (float)(xzStep.Value ?? 0) * scale, u = up * (float)(yStep.Value ?? 0);
            Run(frame => relative.IsChecked == true ? MarioCommands.MoveRelative(frame, f, r, u) : MarioCommands.Translate(frame, r, u, -f), what);
        }
        move.Children.Add(Pad(Move, "Mario moved"));
        move.Children.Add(Hint("Relative: ▲ is Mario's facing direction. Absolute: ▲ is −Z, ▶ is +X (map up/right)."));

        var speed = Section("Speed", root);
        var speedStep = Number(settings, SpeedStep);
        float S() => (float)(speedStep.Value ?? 0);
        speed.Children.Add(Row(Label("Step", 56), speedStep));
        speed.Children.Add(PlusMinus("H speed", () => Run(f => MarioCommands.AddHSpeed(f, -S()), "H speed decreased"), () => Run(f => MarioCommands.AddHSpeed(f, S()), "H speed increased")));
        speed.Children.Add(PlusMinus("Y speed", () => Run(f => MarioCommands.AddVSpeed(f, -S()), "Y speed decreased"), () => Run(f => MarioCommands.AddVSpeed(f, S()), "Y speed increased")));
        speed.Children.Add(PlusMinus("Slide X", () => Run(f => MarioCommands.AddSlideVelocity(f, -S(), 0), "Sliding X speed decreased"), () => Run(f => MarioCommands.AddSlideVelocity(f, S(), 0), "Sliding X speed increased")));
        speed.Children.Add(PlusMinus("Slide Z", () => Run(f => MarioCommands.AddSlideVelocity(f, 0, -S()), "Sliding Z speed decreased"), () => Run(f => MarioCommands.AddSlideVelocity(f, 0, S()), "Sliding Z speed increased")));
        speed.Children.Add(PlusMinus("Slide H", () => Run(f => MarioCommands.AdjustSlide(f, -S(), 0), "Sliding speed decreased"), () => Run(f => MarioCommands.AdjustSlide(f, S(), 0), "Sliding speed increased")));
        speed.Children.Add(Hint("Sliding commands keep the moving yaw in sync with the slide, as the game does."));

        var yaw = Section("Angles", root);
        var yawStep = Number(settings, YawStep);
        int Y() => (int)(yawStep.Value ?? 0);
        yaw.Children.Add(Row(Label("Step", 56), yawStep));
        yaw.Children.Add(PlusMinus("Facing", () => Run(f => MarioCommands.AddYaw(f, -Y()), "Yaw decreased"), () => Run(f => MarioCommands.AddYaw(f, Y()), "Yaw increased")));
        yaw.Children.Add(PlusMinus("Slide", () => Run(f => MarioCommands.AdjustSlide(f, 0, -Y()), "Slide turned clockwise"), () => Run(f => MarioCommands.AdjustSlide(f, 0, Y()), "Slide turned counter-clockwise")));
        yaw.Children.Add(Hint("+ turns counter-clockwise seen from above (towards +X from +Z)."));

        var holp = Section("HOLP", root);
        holp.Children.Add(Row(
            AxisButton("Go to HOLP", axes => Run(f => MarioCommands.GoToHolp(f, axes), "Mario moved to the HOLP")),
            AxisButton("Retrieve HOLP", axes => Run(f => MarioCommands.RetrieveHolp(f, axes), "HOLP moved to Mario"))));
        var holpStep = Number(settings, HolpStep);
        var holpRelative = new CheckBox { Content = "Relative to facing yaw", IsChecked = settings.Get(HolpRelative) };
        holpRelative.IsCheckedChanged += (_, _) => settings.Set(HolpRelative, holpRelative.IsChecked == true);
        holp.Children.Add(Row(Label("Step", 56), holpStep));
        holp.Children.Add(holpRelative);
        void MoveHolp(int forward, int right, int up, string what)
        {
            float step = (float)(holpStep.Value ?? 0), scale = forward != 0 && right != 0 ? 1 / MathF.Sqrt(2) : 1;
            Run(frame => MarioCommands.MoveHolp(frame, forward * step * scale, right * step * scale, up * step, holpRelative.IsChecked == true), what);
        }
        holp.Children.Add(Pad(MoveHolp, "HOLP moved"));
        holp.Children.Add(Hint("Both buttons have a menu (right-click or ▾) to copy only some axes: lateral, X, Y or Z."));

        var actions = Section("Actions", root);
        var pick = Button("Set action…", null, 0);
        pick.Click += (_, _) => ShowActionPicker(MarioVariables.All.First(v => v.Id == "mario.action"), pick);
        var handsFree = Button("Hands-free", () => Run(MarioCommands.HandsFree, "Mario is now hands-free"), 0);
        ToolTip.SetTip(handsFree, "Switches to the action that holds nothing, so the held object can be dropped without its release code. Needs a held object.");
        var visibility = Button("Toggle visibility", () => Run(MarioCommands.ToggleVisibility, "Mario's visibility toggled"), 0);
        ToolTip.SetTip(visibility, "Hides or shows Mario's model (his graph node), without touching his state.");
        actions.Children.Add(Row(pick, handsFree));
        actions.Children.Add(visibility);

        root.Children.Add(Hint("Values in full colour are editable: click one, type, Enter (Esc cancels). Angles accept units, 0x hex or degrees (\"90°\"). Right-click a row for more."));
        return root;
    }

    private void Run(Func<GameFrame, RamPatch> command, string description)
    {
        if (Context.CurrentFrame is not { } frame)
        {
            Context.ReportError("No frame yet: attach to an emulator or open a savestate.");
            return;
        }
        var patch = command(frame);
        if (patch.IsEmpty)
        {
            Context.ReportError($"Nothing to do: {description.ToLowerInvariant()} needs data this game state does not have.");
            return;
        }
        Context.Apply(patch, description);
    }

    private void ShowActionPicker(Variable variable, Control anchor)
    {
        var all = ActionTable.Default.All.OrderBy(a => a.Value, StringComparer.OrdinalIgnoreCase).ToList();
        var search = new TextBox { PlaceholderText = "Search an action…", Width = 300 };
        var list = new ListBox { Height = 320, Width = 300 };
        void Filter()
        {
            string q = search.Text ?? "";
            list.ItemsSource = all
                .Where(a => q.Length == 0 || a.Value.Contains(q, StringComparison.OrdinalIgnoreCase) || a.Key.ToString("X8", CultureInfo.InvariantCulture).Contains(q, StringComparison.OrdinalIgnoreCase))
                .Select(a => $"{a.Value}  (0x{a.Key:X8})")
                .ToList();
        }
        Filter();
        search.TextChanged += (_, _) => Filter();
        var panel = new StackPanel { Spacing = 6 };
        panel.Children.AddRange([search, list]);
        var flyout = new Flyout { Content = panel, Placement = PlacementMode.BottomEdgeAlignedLeft };
        void Choose()
        {
            if (list.SelectedItem is not string text || Context.CurrentFrame is not { } frame)
                return;
            var patch = new RamPatch();
            if (variable.TryWrite(frame, text, Context.Display, patch, out string? error))
                Context.Apply(patch, $"{variable.Label} ← {text}");
            else
                Context.ReportError(error ?? "Invalid action.");
            flyout.Hide();
        }
        list.DoubleTapped += (_, _) => Choose();
        list.KeyDown += (_, e) =>
        {
            if (e.Key == Key.Enter)
                Choose();
        };
        search.KeyDown += (_, e) =>
        {
            if (e.Key == Key.Down && list.ItemCount > 0)
            {
                list.SelectedIndex = 0;
                list.Focus();
            }
            else if (e.Key == Key.Enter && list.ItemCount > 0)
            {
                list.SelectedIndex = 0;
                Choose();
            }
        };
        flyout.Opened += (_, _) => search.Focus();
        flyout.ShowAt(anchor);
    }

    private static Control Pad(Action<int, int, int, string> move, string what)
    {
        var pad = new Grid { ColumnDefinitions = new ColumnDefinitions("Auto,Auto,Auto,12,Auto"), RowDefinitions = new RowDefinitions("Auto,Auto,Auto") };
        void Put(string text, int column, int row, int f, int r, int u)
        {
            var button = Button(text, () => move(f, r, u, what), 40);
            Grid.SetColumn(button, column);
            Grid.SetRow(button, row);
            pad.Children.Add(button);
        }
        Put("↖", 0, 0, 1, -1, 0);
        Put("▲", 1, 0, 1, 0, 0);
        Put("↗", 2, 0, 1, 1, 0);
        Put("◀", 0, 1, 0, -1, 0);
        Put("▶", 2, 1, 0, 1, 0);
        Put("↙", 0, 2, -1, -1, 0);
        Put("▼", 1, 2, -1, 0, 0);
        Put("↘", 2, 2, -1, 1, 0);
        Put("Y+", 4, 0, 0, 0, 1);
        Put("Y−", 4, 2, 0, 0, -1);
        return pad;
    }

    private static Control AxisButton(string text, Action<Axes> run)
    {
        var main = new Button { Content = text, Margin = new Thickness(2, 2, 0, 2), Height = 32, VerticalContentAlignment = VerticalAlignment.Center };
        main.Click += (_, _) => run(Axes.All);
        var menu = new MenuFlyout();
        foreach (var (label, axes) in new[] { ("All axes", Axes.All), ("Laterally (X and Z)", Axes.Lateral), ("X only", Axes.X), ("Y only", Axes.Y), ("Z only", Axes.Z) })
        {
            var item = new MenuItem { Header = label };
            item.Click += (_, _) => run(axes);
            menu.Items.Add(item);
        }
        main.ContextFlyout = menu;
        var more = new Button { Content = "▾", Width = 24, Height = 32, Margin = new Thickness(0, 2, 2, 2), Padding = new Thickness(0), HorizontalContentAlignment = HorizontalAlignment.Center, VerticalContentAlignment = VerticalAlignment.Center, Flyout = menu };
        return Row(main, more);
    }

    private static Control PlusMinus(string label, Action minus, Action plus) => Row(Label(label, 64), Button("−", minus, 44), Button("+", plus, 44));

    private static StackPanel Section(string title, Panel parent)
    {
        var body = new StackPanel { Spacing = 8 };
        var box = new StackPanel { Spacing = 8 };
        box.Children.Add(new TextBlock { Text = title.ToUpperInvariant(), Classes = { "title" } });
        box.Children.Add(body);
        parent.Children.Add(new Border { Classes = { "card" }, Child = box });
        return body;
    }

    private static StackPanel Row(params Control[] controls)
    {
        var row = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 4 };
        row.Children.AddRange(controls);
        return row;
    }

    private static TextBlock Label(string text, double width = double.NaN) => new() { Text = text, Width = width, VerticalAlignment = VerticalAlignment.Center };

    private static Button Button(string text, Action? onClick, double width)
    {
        var button = new Button
        {
            Content = text,
            Height = 32,
            HorizontalContentAlignment = HorizontalAlignment.Center,
            VerticalContentAlignment = VerticalAlignment.Center,
            Margin = new Thickness(2),
        };
        if (width > 0)
            button.Width = width;
        if (onClick is not null)
            button.Click += (_, _) => onClick();
        return button;
    }

    private static TextBlock Hint(string text) => new() { Text = text, Classes = { "muted" }, FontSize = 12, TextWrapping = TextWrapping.Wrap, MaxWidth = 280, HorizontalAlignment = HorizontalAlignment.Left };

    private static NumericUpDown Number<T>(SettingsScope scope, Setting<T> setting)
    {
        var range = setting.Range ?? (0, 1000000, 1);
        var number = new NumericUpDown
        {
            Minimum = (decimal)range.Min,
            Maximum = (decimal)range.Max,
            Increment = (decimal)range.Step,
            FormatString = typeof(T) == typeof(int) ? "0" : "0.###",
            Width = 150,
        };
        number.Value = Math.Clamp(Convert.ToDecimal(scope.Get(setting), CultureInfo.InvariantCulture), number.Minimum, number.Maximum);
        number.ValueChanged += (_, _) =>
        {
            if (number.Value is { } v)
                scope.SetBoxed(setting, v);
        };
        return number;
    }
}
