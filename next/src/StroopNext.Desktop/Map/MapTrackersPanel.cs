using System.Globalization;
using System.Numerics;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Platform.Storage;
using StroopNext.Map;
using StroopNext.Settings;

namespace StroopNext.Desktop.Map;

internal sealed partial class MapModuleView
{
    private static readonly uint[] Palette =
    [
        0xFFFFD42A, 0xFFE5534B, 0xFFF08C2E, 0xFF62CC8E, 0xFF3A9BF0, 0xFF3DD6F5, 0xFFC04CE0, 0xFFFF7AC8, 0xFFFFFFFF, 0xFF9093A6,
    ];

    private readonly StackPanel _trackerRows = new() { Spacing = 2 };
    private readonly HashSet<Guid> _expandedTrackers = new();

    private TrackersLayer TrackerLayer => _layers.OfType<TrackersLayer>().First();
    private SettingsScope TrackerScope => LayerScope(TrackerLayer);

    private Control BuildTrackersSection()
    {
        var section = new StackPanel { Spacing = 4 };
        section.Children.Add(new TextBlock { Text = "TRACKERS", Classes = { "title" }, Margin = new Thickness(0, 6, 0, 0) });

        var add = new Button { Content = "Add ▾", Padding = new Thickness(10, 3) };
        add.Click += (_, _) => AddMenu().ShowAt(add);
        var export = new Button { Content = "Export…", Padding = new Thickness(8, 3), FontSize = 12 };
        export.Click += async (_, _) => await ExportTrackers();
        var import = new Button { Content = "Import…", Padding = new Thickness(8, 3), FontSize = 12 };
        import.Click += async (_, _) => await ImportTrackers();
        var presets = new Button { Content = "Presets ▾", Padding = new Thickness(8, 3), FontSize = 12 };
        ToolTip.SetTip(presets, "Save the current trackers under a name, and switch between setups in one click");
        presets.Click += (_, _) => PresetsMenu(presets).ShowAt(presets);
        var bar = new WrapPanel { Orientation = Orientation.Horizontal };
        foreach (var b in new Control[] { add, presets, export, import })
        {
            b.Margin = new Thickness(0, 0, 4, 4);
            bar.Children.Add(b);
        }
        section.Children.Add(bar);
        section.Children.Add(_trackerRows);
        section.Children.Add(new TextBlock
        {
            Text = "Right-click an object on the map to track it. Trackers follow their target: Mario, the HOLP, the camera, selected objects, every object of a kind, or a fixed point.",
            Classes = { "muted" },
            FontSize = 12,
            TextWrapping = TextWrapping.Wrap,
        });

        TrackerLayer.Changed += () =>
        {
            RefreshTrackerRows();
            Rebuild();
        };
        RefreshTrackerRows();
        return section;
    }

    private MenuFlyout PresetsMenu(Control anchor)
    {
        var menu = new MenuFlyout();
        var names = TrackerLayer.PresetNames(TrackerScope);
        foreach (var name in names)
        {
            var load = new MenuItem { Header = $"Load \"{name}\"" };
            load.Click += (_, _) =>
            {
                TrackerLayer.LoadPreset(TrackerScope, name);
                Context.ReportStatus($"Loaded the tracker preset \"{name}\".");
            };
            menu.Items.Add(load);
        }
        if (names.Count > 0)
            menu.Items.Add(new Separator());
        var save = new MenuItem { Header = "Save current trackers as…" };
        save.Click += (_, _) => AskPresetName(anchor);
        menu.Items.Add(save);
        if (names.Count > 0)
        {
            var delete = new MenuItem { Header = "Delete" };
            foreach (var name in names)
            {
                var item = new MenuItem { Header = name };
                item.Click += (_, _) => TrackerLayer.DeletePreset(TrackerScope, name);
                delete.Items.Add(item);
            }
            menu.Items.Add(delete);
        }
        return menu;
    }

    private void AskPresetName(Control anchor)
    {
        var name = new TextBox { PlaceholderText = "Preset name", Width = 200 };
        var ok = new Button { Content = "Save" };
        var panel = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 6 };
        panel.Children.AddRange([name, ok]);
        var flyout = new Flyout { Content = panel };
        void Save()
        {
            string text = (name.Text ?? "").Trim();
            if (text.Length == 0)
                return;
            TrackerLayer.SavePreset(TrackerScope, text);
            Context.ReportStatus($"Saved {TrackerLayer.Trackers(TrackerScope).Count} trackers as the preset \"{text}\".");
            flyout.Hide();
        }
        ok.Click += (_, _) => Save();
        name.KeyDown += (_, e) =>
        {
            if (e.Key == Avalonia.Input.Key.Enter)
                Save();
        };
        flyout.Opened += (_, _) => name.Focus();
        flyout.ShowAt(anchor);
    }

    public int ImportTrackers(string json) => TrackerLayer.Import(TrackerScope, json);

    private MenuFlyout AddMenu()
    {
        var menu = new MenuFlyout();
        void Target(string header, TrackerTarget target, uint address = 0, string? name = null, Vector3 point = default, int yaw = 0)
        {
            var parent = new MenuItem { Header = header };
            foreach (var kind in TrackerSpec.KindsFor(target))
            {
                var item = new MenuItem { Header = TrackerSpec.KindName(kind) };
                item.Click += (_, _) => TrackerLayer.Add(TrackerScope, TrackerSpec.Create(kind, target, address, name, point, yaw));
                parent.Items.Add(item);
            }
            menu.Items.Add(parent);
        }
        Target("Mario", TrackerTarget.Mario);
        Target("HOLP", TrackerTarget.Holp);
        Target("Camera", TrackerTarget.Camera);
        Target("Selected objects", TrackerTarget.SelectedObjects);
        if (Context.CurrentFrame is { } frame)
        {
            var selected = frame.Objects.All.FirstOrDefault(o => Context.Selection.IsSelected(o.Address));
            if (selected is not null)
            {
                Target($"This object: {selected.Name} (slot {selected.SlotNumber})", TrackerTarget.Object, selected.Address, selected.Name);
                Target($"Every \"{selected.Name}\"", TrackerTarget.ObjectsNamed, name: selected.Name);
            }
            if (frame.Mario is { } m)
                Target("A fixed point at Mario's position", TrackerTarget.Point, point: m.Position, yaw: m.FacingYaw);
        }
        return menu;
    }

    private void AddTrackMenuItems(ContextMenu menu, IMapHit? hit, Vector3 point)
    {
        MenuItem Sub(string header, TrackerTarget target, uint address = 0, string? name = null)
        {
            var parent = new MenuItem { Header = header };
            foreach (var kind in TrackerSpec.KindsFor(target))
            {
                var item = new MenuItem { Header = TrackerSpec.KindName(kind) };
                item.Click += (_, _) => TrackerLayer.Add(TrackerScope, TrackerSpec.Create(kind, target, address, name, point));
                parent.Items.Add(item);
            }
            return parent;
        }
        menu.Items.Add(new Separator());
        if (hit is ObjectHit { Object: var o })
        {
            menu.Items.Add(Sub($"Track {o.Name} (slot {o.SlotNumber})", TrackerTarget.Object, o.Address, o.Name));
            menu.Items.Add(Sub($"Track every \"{o.Name}\"", TrackerTarget.ObjectsNamed, name: o.Name));
        }
        else if (hit is MarioHit)
            menu.Items.Add(Sub("Track Mario", TrackerTarget.Mario));
        menu.Items.Add(Sub("Add a tracker at this point", TrackerTarget.Point,
            name: string.Create(CultureInfo.InvariantCulture, $"Point ({point.X:0}, {point.Y:0}, {point.Z:0})")));
    }

    private void RefreshTrackerRows()
    {
        _trackerRows.Children.Clear();
        var trackers = TrackerLayer.Trackers(TrackerScope);
        if (trackers.Count == 0)
            _trackerRows.Children.Add(new TextBlock { Text = "No trackers yet.", Classes = { "muted" }, FontSize = 12 });
        foreach (var t in trackers)
            _trackerRows.Children.Add(TrackerRow(t));
    }

    private Control TrackerRow(TrackerSpec t)
    {
        void Save(TrackerSpec changed) => TrackerLayer.Replace(TrackerScope, changed);

        var visible = new CheckBox { IsChecked = t.Visible, MinWidth = 0, Padding = new Thickness(0) };
        visible.IsCheckedChanged += (_, _) => Save(t with { Visible = visible.IsChecked == true });

        var swatch = new Button
        {
            Width = 18,
            Height = 18,
            Padding = new Thickness(0),
            Background = new SolidColorBrush(Color.FromUInt32(t.Color)),
            BorderBrush = Brushes.Black,
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(4),
            VerticalAlignment = VerticalAlignment.Center,
        };
        ToolTip.SetTip(swatch, "Colour");
        var colors = new WrapPanel { Width = 150 };
        foreach (uint argb in Palette)
        {
            var choice = new Button { Width = 24, Height = 24, Margin = new Thickness(2), Background = new SolidColorBrush(Color.FromUInt32(argb)), Padding = new Thickness(0) };
            choice.Click += (_, _) => Save(t with { Color = argb });
            colors.Children.Add(choice);
        }
        swatch.Flyout = new Flyout { Content = colors };

        var title = new TextBlock { Text = t.Title, VerticalAlignment = VerticalAlignment.Center, TextTrimming = TextTrimming.CharacterEllipsis, Cursor = new Avalonia.Input.Cursor(Avalonia.Input.StandardCursorType.Hand) };
        ToolTip.SetTip(title, "Click for options");
        title.PointerPressed += (_, _) =>
        {
            if (!_expandedTrackers.Remove(t.Id))
                _expandedTrackers.Add(t.Id);
            RefreshTrackerRows();
        };

        Button Small(string text, string tip, Action action)
        {
            var b = new Button { Content = text, Padding = new Thickness(5, 1), FontSize = 11, MinWidth = 0, VerticalAlignment = VerticalAlignment.Center };
            ToolTip.SetTip(b, tip);
            b.Click += (_, _) => action();
            return b;
        }

        var row = new Grid { ColumnDefinitions = new ColumnDefinitions("Auto,Auto,*,Auto,Auto,Auto"), ColumnSpacing = 4 };
        Control[] cells =
        [
            visible, swatch, title,
            Small("▲", "Draw earlier (under the next ones)", () => TrackerLayer.Move(TrackerScope, t.Id, -1)),
            Small("▼", "Draw later (over the previous ones)", () => TrackerLayer.Move(TrackerScope, t.Id, 1)),
            Small("✕", "Remove", () => TrackerLayer.Remove(TrackerScope, t.Id)),
        ];
        for (int i = 0; i < cells.Length; i++)
        {
            Grid.SetColumn(cells[i], i);
            row.Children.Add(cells[i]);
        }

        var container = new StackPanel { Spacing = 4 };
        container.Children.Add(row);
        if (_expandedTrackers.Contains(t.Id))
            container.Children.Add(TrackerOptions(t, Save));
        return new Border { Child = container, Padding = new Thickness(4, 3), CornerRadius = new CornerRadius(6), Background = AppTheme.SurfaceBrush };
    }

    private static Control TrackerOptions(TrackerSpec t, Action<TrackerSpec> save)
    {
        var panel = new StackPanel { Spacing = 4, Margin = new Thickness(26, 2, 0, 2) };
        NumericUpDown Number(double value, double min, double max, double step, Action<double> changed)
        {
            var n = new NumericUpDown { Minimum = (decimal)min, Maximum = (decimal)max, Increment = (decimal)step, Value = (decimal)value, FormatString = "0.##", Width = 130 };
            n.ValueChanged += (_, _) =>
            {
                if (n.Value is { } v)
                    changed((double)v);
            };
            return n;
        }
        StackPanel Line(string label, Control editor)
        {
            var line = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 6 };
            line.Children.AddRange([new TextBlock { Text = label, Width = 110, VerticalAlignment = VerticalAlignment.Center, FontSize = 12 }, editor]);
            return line;
        }
        if (TrackerSpec.SizeLabel(t.Kind) is { } sizeLabel)
            panel.Children.Add(Line(sizeLabel, Number(t.Size, 0, 100000, t.Kind is TrackerKind.Sector or TrackerKind.AngleRange ? 256 : 10, v => save(t with { Size = v }))));
        string? size2 = t.Kind switch { TrackerKind.Cylinder => "Height", TrackerKind.Sector or TrackerKind.AngleRange => "Length", _ => null };
        if (size2 is not null)
            panel.Children.Add(Line(size2, Number(t.Size2, 0, 100000, 10, v => save(t with { Size2 = v }))));
        panel.Children.Add(Line("Opacity", Number(t.Opacity, 0, 1, 0.1, v => save(t with { Opacity = v }))));
        var order = new ComboBox { ItemsSource = Enum.GetValues<TrackerOrder>(), SelectedItem = t.Order, Width = 130 };
        order.SelectionChanged += (_, _) =>
        {
            if (order.SelectedItem is TrackerOrder o && o != t.Order)
                save(t with { Order = o });
        };
        panel.Children.Add(Line("Order", order));
        return panel;
    }

    private async Task ExportTrackers()
    {
        if (TopLevel.GetTopLevel(this)?.StorageProvider is not { } storage)
            return;
        var file = await storage.SaveFilePickerAsync(new FilePickerSaveOptions
        {
            Title = "Export trackers",
            SuggestedFileName = "trackers.json",
            FileTypeChoices = [new FilePickerFileType("Trackers") { Patterns = ["*.json"] }],
        });
        if (file is null)
            return;
        await using var stream = await file.OpenWriteAsync();
        await using var writer = new StreamWriter(stream);
        await writer.WriteAsync(TrackerLayer.Export(TrackerScope));
        Context.ReportStatus($"Exported {TrackerLayer.Trackers(TrackerScope).Count} trackers to {file.Name}.");
    }

    private async Task ImportTrackers()
    {
        if (TopLevel.GetTopLevel(this)?.StorageProvider is not { } storage)
            return;
        var files = await storage.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = "Import trackers",
            FileTypeFilter = [new FilePickerFileType("Trackers") { Patterns = ["*.json"] }],
        });
        if (files.Count == 0)
            return;
        try
        {
            await using var stream = await files[0].OpenReadAsync();
            using var reader = new StreamReader(stream);
            int count = TrackerLayer.Import(TrackerScope, await reader.ReadToEndAsync());
            Context.ReportStatus($"Imported {count} trackers from {files[0].Name}.");
        }
        catch (Exception e) when (e is System.Text.Json.JsonException or IOException)
        {
            Context.ReportError($"Cannot import {files[0].Name}: {e.Message}");
        }
    }
}
