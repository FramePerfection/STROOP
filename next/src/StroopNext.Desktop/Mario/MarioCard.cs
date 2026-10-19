using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using StroopNext.Desktop.Controls;
using StroopNext.Game;
using StroopNext.Session;

namespace StroopNext.Desktop.Mario;

// The Mario panel's header: Mario as he is (holding, burning, riding a shell), his action, the power meter, his
// coins / stars / lives and the object he holds, with legacy STROOP's images.
internal sealed class MarioCard : Border
{
    private const uint NumCoins = 0xA8, NumStars = 0xAA, NumLives = 0xAD;

    private readonly Image _mario = new() { Width = 64, Height = 64 };
    private readonly TextBlock _headline = new() { FontSize = 18, FontWeight = FontWeight.SemiBold, TextTrimming = TextTrimming.CharacterEllipsis };
    private readonly TextBlock _subline = new() { Classes = { "muted", "mono" }, FontSize = 12 };
    private readonly PowerMeter _power = new() { Width = 52, Height = 52 };
    private readonly TextBlock _coins = Counter(), _stars = Counter(), _lives = Counter();
    private readonly Image _held = new() { Width = 40, Height = 40 };
    private readonly TextBlock _heldName = new() { FontSize = 11, Classes = { "muted" }, TextAlignment = TextAlignment.Center, MaxWidth = 90, TextTrimming = TextTrimming.CharacterEllipsis };
    private readonly StackPanel _heldBox;
    private string? _marioFile;
    private uint _heldBehavior = uint.MaxValue;

    public MarioCard()
    {
        Classes.Add("card");
        Padding = new Thickness(10, 8);
        Margin = new Thickness(0, 10, 8, 8);

        var counters = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 14, Margin = new Thickness(0, 4, 0, 0) };
        counters.Children.AddRange([Pair("Yellow Coin.png", _coins, "Coins"), Pair("Power Star Yellow.png", _stars, "Stars"), Pair("Mario.png", _lives, "Lives")]);
        var text = new StackPanel { Spacing = 2, VerticalAlignment = VerticalAlignment.Center };
        text.Children.AddRange([_headline, _subline, counters]);

        _heldBox = new StackPanel { Spacing = 2, VerticalAlignment = VerticalAlignment.Center, IsVisible = false };
        _heldBox.Children.AddRange([new TextBlock { Text = "HOLDING", FontSize = 10, Classes = { "muted" }, HorizontalAlignment = HorizontalAlignment.Center }, _held, _heldName]);
        ToolTip.SetTip(_power, "Power meter (health wedges)");

        var grid = new Grid { ColumnDefinitions = new ColumnDefinitions("Auto,*,Auto,Auto"), ColumnSpacing = 12 };
        Grid.SetColumn(_mario, 0);
        Grid.SetColumn(text, 1);
        Grid.SetColumn(_heldBox, 2);
        Grid.SetColumn(_power, 3);
        grid.Children.AddRange([_mario, text, _heldBox, _power]);
        Child = grid;
    }

    public void Update(GameFrame frame)
    {
        var m = frame.Mario;
        string headline = m is not null ? frame.Services.Actions.NameOf(m.Action) ?? $"action 0x{m.Action:X8}"
            : frame.IsKnownGame ? "Mario not available" : "No SM64 game detected";
        Set(_headline, headline);
        Set(_subline, m is not null ? $"action 0x{m.Action:X8}  ·  timer {frame.GlobalTimer}" : $"timer {frame.GlobalTimer}");
        if (m is null)
        {
            _power.Wedges = -1;
            _heldBox.IsVisible = false;
            return;
        }
        string file = MarioImage(frame, m, headline);
        if (file != _marioFile)
        {
            _marioFile = file;
            _mario.Source = ObjectImages.Get(file, 96);
        }
        _power.Wedges = m.HealthWedges;
        var ram = frame.Ram;
        Set(_coins, ram.S16(m.Address + NumCoins).ToString());
        Set(_stars, ram.S16(m.Address + NumStars).ToString());
        Set(_lives, ram.S8(m.Address + NumLives).ToString());

        var held = m.HeldObject != 0 ? frame.Objects.Find(m.HeldObject) : null;
        _heldBox.IsVisible = held is not null;
        if (held is not null && held.SegmentedBehavior != _heldBehavior)
        {
            _heldBehavior = held.SegmentedBehavior;
            _held.Source = frame.Version is { } version ? ObjectImages.Get(frame.Services.ObjectNames.ImageOf(held.SegmentedBehavior, version), 64) : null;
            Set(_heldName, held.Name);
        }
        else if (held is null)
            _heldBehavior = uint.MaxValue;
    }

    private static string MarioImage(GameFrame frame, MarioState m, string action)
    {
        if (m.RiddenObject != 0)
            return "Mario Shell Riding.png";
        if (action.Contains("BURNING", StringComparison.OrdinalIgnoreCase) || action.Contains("LAVA", StringComparison.OrdinalIgnoreCase))
            return "Mario Burning.png";
        if (m.HeldObject != 0)
            return "Mario Holding.png";
        return "Mario.png";
    }

    private static TextBlock Counter() => new() { FontSize = 14, FontWeight = FontWeight.SemiBold, VerticalAlignment = VerticalAlignment.Center, MinWidth = 22 };

    private static StackPanel Pair(string icon, TextBlock value, string tip)
    {
        var pair = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 4 };
        pair.Children.AddRange([new Image { Source = ObjectImages.Get(icon, 40), Width = 18, Height = 18, VerticalAlignment = VerticalAlignment.Center }, value]);
        ToolTip.SetTip(pair, tip);
        return pair;
    }

    private static void Set(TextBlock block, string text)
    {
        if (block.Text != text)
            block.Text = text;
    }
}

// SM64's power meter: 8 wedges, coloured like the HUD (blue full, green, yellow, red when low).
internal sealed class PowerMeter : Control
{
    private int _wedges = -1;

    public int Wedges
    {
        get => _wedges;
        set
        {
            if (_wedges == value)
                return;
            _wedges = value;
            ToolTip.SetTip(this, value < 0 ? "Power meter" : $"Power meter: {Math.Clamp(value, 0, 8)} / 8");
            InvalidateVisual();
        }
    }

    public override void Render(DrawingContext context)
    {
        double r = Math.Min(Bounds.Width, Bounds.Height) / 2 - 2;
        var center = new Point(Bounds.Width / 2, Bounds.Height / 2);
        int filled = Math.Clamp(_wedges, 0, 8);
        var color = filled switch { >= 7 => Color.FromRgb(0x3A, 0x7B, 0xFF), >= 5 => Color.FromRgb(0x2E, 0xC4, 0x4F), >= 3 => Color.FromRgb(0xF2, 0xC2, 0x1B), _ => Color.FromRgb(0xE5, 0x39, 0x35) };
        var on = new SolidColorBrush(color);
        var off = new SolidColorBrush(Color.FromArgb(0x40, 0x80, 0x80, 0x80));
        var pen = new Pen(new SolidColorBrush(Color.FromArgb(0xC0, 0x20, 0x20, 0x20)), 1.5);
        for (int i = 0; i < 8; i++)
        {
            double a0 = -Math.PI / 2 + i * Math.PI / 4, a1 = a0 + Math.PI / 4;
            var wedge = new StreamGeometry();
            using (var g = wedge.Open())
            {
                g.BeginFigure(center, true);
                g.LineTo(new Point(center.X + r * Math.Cos(a0), center.Y + r * Math.Sin(a0)));
                g.ArcTo(new Point(center.X + r * Math.Cos(a1), center.Y + r * Math.Sin(a1)), new Size(r, r), 0, false, SweepDirection.Clockwise);
                g.EndFigure(true);
            }
            context.DrawGeometry(_wedges >= 0 && i < filled ? on : off, pen, wedge);
        }
        if (_wedges >= 0)
        {
            var text = new FormattedText(filled.ToString(), System.Globalization.CultureInfo.InvariantCulture, FlowDirection.LeftToRight,
                new Typeface(FontFamily.Default, FontStyle.Normal, FontWeight.Bold), 14, Brushes.White);
            context.DrawText(text, new Point(center.X - text.Width / 2, center.Y - text.Height / 2));
        }
    }
}
