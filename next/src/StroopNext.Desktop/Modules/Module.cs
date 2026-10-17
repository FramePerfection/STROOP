using Avalonia.Controls;
using StroopNext.Memory;
using StroopNext.Session;
using StroopNext.Settings;
using StroopNext.Variables;

namespace StroopNext.Desktop.Modules;

public interface IStroopModule
{
    string Id { get; }
    string Title { get; }
    ModuleView CreateView(ModuleContext context);
}

public abstract class ModuleView : UserControl
{
    protected ModuleView(ModuleContext context) => Context = context;

    protected ModuleContext Context { get; }

    public abstract void OnFrame(GameFrame frame);

    public virtual void OnDisplayOptionsChanged() { }
}

public sealed class ModuleContext(GameSession session, Selection selection, SettingsScope settings, Func<DisplayOptions> display, Func<GameFrame?> currentFrame, Action<string, bool> status)
{
    public GameSession Session { get; } = session;
    public Selection Selection { get; } = selection;

    public SettingsScope Settings { get; } = settings;

    public DisplayOptions Display => display();

    public GameFrame? CurrentFrame => currentFrame();

    public void ReportStatus(string message) => status(message, false);
    public void ReportError(string message) => status(message, true);

    public bool Apply(RamPatch patch, string description)
    {
        if (patch.IsEmpty)
            return false;
        if (Session.Source is null)
        {
            ReportError("Nothing is attached: connect to an emulator or open a savestate first.");
            return false;
        }
        bool ok = Session.Apply(patch);
        if (ok)
            ReportStatus(description);
        else
            ReportError($"Write failed: {description}");
        return ok;
    }
}
