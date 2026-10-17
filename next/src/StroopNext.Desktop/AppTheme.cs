using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Media;
using Avalonia.Media.Immutable;
using Avalonia.Styling;
using Avalonia.Themes.Fluent;

namespace StroopNext.Desktop;

internal static class AppTheme
{
    public static readonly Color Background = Color.Parse("#1B1C24");
    public static readonly Color Surface = Color.Parse("#22232D");
    public static readonly Color SurfaceRaised = Color.Parse("#2A2B37");
    public static readonly Color Line = Color.Parse("#353646");
    public static readonly Color Text = Color.Parse("#E6E7EE");
    public static readonly Color Muted = Color.Parse("#9093A6");
    public static readonly Color Accent = Color.Parse("#E5534B");
    public static readonly Color Increase = Color.Parse("#62CC8E");
    public static readonly Color Decrease = Color.Parse("#EE7F70");

    public static readonly IBrush SurfaceBrush = new ImmutableSolidColorBrush(Surface);
    public static readonly IBrush SurfaceRaisedBrush = new ImmutableSolidColorBrush(SurfaceRaised);
    public static readonly IBrush LineBrush = new ImmutableSolidColorBrush(Line);
    public static readonly IBrush TextBrush = new ImmutableSolidColorBrush(Text);
    public static readonly IBrush MutedBrush = new ImmutableSolidColorBrush(Muted);
    public static readonly IBrush AccentBrush = new ImmutableSolidColorBrush(Accent);
    public static readonly IBrush IncreaseBrush = new ImmutableSolidColorBrush(Increase);
    public static readonly IBrush DecreaseBrush = new ImmutableSolidColorBrush(Decrease);
    public static readonly IBrush HoverBrush = new ImmutableSolidColorBrush(Color.FromArgb(18, 255, 255, 255));

    public static readonly FontFamily Monospace = new("Cascadia Mono, Consolas, Menlo, DejaVu Sans Mono, monospace");

    public static void Apply(Application app)
    {
        var fluent = new FluentTheme { DensityStyle = DensityStyle.Compact };
        fluent.Palettes[ThemeVariant.Dark] = new ColorPaletteResources
        {
            Accent = Accent,
            RegionColor = Background,
            AltHigh = Background,
            AltMediumHigh = Surface,
            AltMedium = Surface,
            AltMediumLow = Surface,
            AltLow = Surface,
            BaseHigh = Text,
            BaseMediumHigh = Color.Parse("#C9CBD8"),
            BaseMedium = Muted,
            BaseMediumLow = Color.Parse("#5E6074"),
            BaseLow = Line,
            ChromeAltLow = Text,
            ChromeLow = Surface,
            ChromeMediumLow = Surface,
            ChromeMedium = SurfaceRaised,
            ChromeHigh = Line,
            ChromeGray = Muted,
            ChromeWhite = Text,
            ListLow = SurfaceRaised,
            ListMedium = Line,
            ErrorText = Decrease,
        };
        app.Styles.Add(fluent);
        app.Styles.Add(Styles());
        app.RequestedThemeVariant = ThemeVariant.Dark;
    }

    private static Styles Styles() =>
    [
        new Style(x => x.OfType<Border>().Class("card"))
        {
            Setters =
            {
                new Setter(Border.BackgroundProperty, SurfaceBrush),
                new Setter(Border.BorderBrushProperty, LineBrush),
                new Setter(Border.BorderThicknessProperty, new Thickness(1)),
                new Setter(Border.CornerRadiusProperty, new CornerRadius(8)),
                new Setter(Border.PaddingProperty, new Thickness(12, 10)),
            },
        },
        new Style(x => x.OfType<TextBlock>().Class("title"))
        {
            Setters =
            {
                new Setter(TextBlock.FontSizeProperty, 11.0),
                new Setter(TextBlock.FontWeightProperty, FontWeight.SemiBold),
                new Setter(TextBlock.ForegroundProperty, MutedBrush),
                new Setter(TextBlock.LetterSpacingProperty, 0.8),
            },
        },
        new Style(x => x.OfType<TextBlock>().Class("muted"))
        {
            Setters = { new Setter(TextBlock.ForegroundProperty, MutedBrush) },
        },
        new Style(x => x.OfType<TextBlock>().Class("mono"))
        {
            Setters = { new Setter(TextBlock.FontFamilyProperty, Monospace) },
        },
        new Style(x => x.OfType<Button>())
        {
            Setters = { new Setter(TemplatedControl.CornerRadiusProperty, new CornerRadius(6)) },
        },
        new Style(x => x.OfType<Border>().Class("row").Class(":pointerover"))
        {
            Setters = { new Setter(Border.BackgroundProperty, HoverBrush) },
        },
        new Style(x => x.OfType<Border>().Class("group"))
        {
            Setters =
            {
                new Setter(Border.BackgroundProperty, SurfaceBrush),
                new Setter(Border.BorderBrushProperty, LineBrush),
                new Setter(Border.BorderThicknessProperty, new Thickness(0, 1, 0, 0)),
                new Setter(Border.PaddingProperty, new Thickness(10, 6)),
            },
        },
        new Style(x => x.OfType<Border>().Class("group").Class(":pointerover"))
        {
            Setters = { new Setter(Border.BackgroundProperty, SurfaceRaisedBrush) },
        },
    ];
}
