using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;

namespace StroopNext.Platform;

public static partial class MupenLua
{
    private const int IdcAddInstance = 10145, IdcPath = 10150, IdcStart = 10154;
    private const int IdOk = 1, IdcFileName = 1148;
    private const uint WmCommand = 0x0111, WmSetText = 0x000C, BmClick = 0x00F5, MfByPosition = 0x400;

    public static async Task<string?> StartScriptAsync(int processId, string script, CancellationToken cancel = default)
    {
        if (!OperatingSystem.IsWindows())
            return "starting a Lua script in Mupen needs Windows";
        if (!File.Exists(script))
            return $"script not found: {script}";
        nint main = MainWindow(processId);
        if (main == 0)
            return "Mupen's window was not found";
        nint menu = GetMenu(main);
        if (menu == 0)
            return "Mupen's menu was not found";

        string name = Path.GetFileName(script);
        if (FindItem(menu, text => text.Contains(name, StringComparison.OrdinalIgnoreCase)) is int recent)
        {
            PostMessageW(main, WmCommand, recent, 0);
            return null;
        }

        if (FindItem(menu, text => text.Replace("&", "").Contains("Show Instances", StringComparison.OrdinalIgnoreCase)) is not int show)
            return "Mupen's \"Lua Script → Show Instances\" menu entry was not found";
        PostMessageW(main, WmCommand, show, 0);
        nint manager = await WaitFor(() => ProcessWindows(processId).FirstOrDefault(w => GetDlgItem(w, IdcAddInstance) != 0), cancel);
        if (manager == 0)
            return "Mupen's Lua window did not open";
        nint picker = ProcessWindows(processId).FirstOrDefault(w => FileNameBox(w) != 0);
        if (picker == 0)
        {
            PostMessageW(GetDlgItem(manager, IdcAddInstance), BmClick, 0, 0);
            picker = await WaitFor(() => ProcessWindows(processId).FirstOrDefault(w => FileNameBox(w) != 0), cancel);
        }
        if (picker == 0)
            return "Mupen's file picker did not open";
        SendMessageW(FileNameBox(picker), WmSetText, 0, script);
        PostMessageW(GetDlgItem(picker, IdOk), BmClick, 0, 0);
        nint instance = await WaitFor(() => Descendants(manager).FirstOrDefault(w => GetDlgItem(w, IdcPath) != 0 && GetDlgItem(w, IdcStart) != 0
            && WindowText(GetDlgItem(w, IdcPath)).EndsWith(name, StringComparison.OrdinalIgnoreCase)), cancel);
        if (instance == 0)
            return "the script was not added to Mupen's Lua window";
        PostMessageW(GetDlgItem(instance, IdcStart), BmClick, 0, 0);
        return null;
    }

    private static async Task<nint> WaitFor(Func<nint> find, CancellationToken cancel)
    {
        for (int i = 0; i < 50; i++)
        {
            if (find() is var w and not 0)
                return w;
            await Task.Delay(100, cancel);
        }
        return 0;
    }

    private static nint MainWindow(int processId)
    {
        try
        {
            using var process = Process.GetProcessById(processId);
            return process.MainWindowHandle;
        }
        catch (ArgumentException)
        {
            return 0;
        }
    }

    private static int? FindItem(nint menu, Func<string, bool> match)
    {
        int count = GetMenuItemCount(menu);
        var text = new StringBuilder(512);
        for (int i = 0; i < count; i++)
        {
            nint sub = GetSubMenu(menu, i);
            if (sub != 0)
            {
                if (FindItem(sub, match) is int found)
                    return found;
                continue;
            }
            text.Clear();
            if (GetMenuStringW(menu, (uint)i, text, text.Capacity, MfByPosition) > 0 && match(text.ToString()))
            {
                int id = (int)GetMenuItemID(menu, i);
                if (id > 0)
                    return id;
            }
        }
        return null;
    }

    private static nint FileNameBox(nint dialog)
    {
        nint box = GetDlgItem(dialog, IdcFileName);
        if (box == 0)
            box = Descendants(dialog).FirstOrDefault(w => GetDlgCtrlID(w) == IdcFileName);
        if (box == 0)
            return 0;
        nint edit = FindWindowExW(box, 0, "Edit", null);
        if (edit == 0 && FindWindowExW(box, 0, "ComboBox", null) is var combo and not 0)
            edit = FindWindowExW(combo, 0, "Edit", null);
        return edit != 0 ? edit : box;
    }

    private static List<nint> ProcessWindows(int processId)
    {
        var list = new List<nint>();
        EnumWindows((w, _) =>
        {
            GetWindowThreadProcessId(w, out uint pid);
            if (pid == processId && IsWindowVisible(w))
                list.Add(w);
            return true;
        }, 0);
        return list;
    }

    private static List<nint> Descendants(nint parent)
    {
        var list = new List<nint>();
        EnumChildWindows(parent, (w, _) =>
        {
            list.Add(w);
            return true;
        }, 0);
        return list;
    }

    private static string WindowText(nint window)
    {
        var text = new StringBuilder(1024);
        SendMessageW(window, 0x000D , text.Capacity, text);
        return text.ToString();
    }

    private delegate bool EnumProc(nint window, nint param);

    [DllImport("user32.dll")]
    private static extern nint GetMenu(nint window);
    [DllImport("user32.dll")]
    private static extern int GetMenuItemCount(nint menu);
    [DllImport("user32.dll")]
    private static extern nint GetSubMenu(nint menu, int position);
    [DllImport("user32.dll")]
    private static extern uint GetMenuItemID(nint menu, int position);
    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    private static extern int GetMenuStringW(nint menu, uint item, StringBuilder text, int max, uint flags);
    [DllImport("user32.dll")]
    private static extern bool PostMessageW(nint window, uint message, nint wParam, nint lParam);
    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    private static extern nint SendMessageW(nint window, uint message, nint wParam, string lParam);
    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    private static extern nint SendMessageW(nint window, uint message, nint wParam, StringBuilder lParam);
    [DllImport("user32.dll")]
    private static extern nint GetDlgItem(nint dialog, int id);
    [DllImport("user32.dll")]
    private static extern int GetDlgCtrlID(nint window);
    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    private static extern nint FindWindowExW(nint parent, nint after, string? className, string? title);
    [DllImport("user32.dll")]
    private static extern bool EnumWindows(EnumProc proc, nint param);
    [DllImport("user32.dll")]
    private static extern bool EnumChildWindows(nint parent, EnumProc proc, nint param);
    [DllImport("user32.dll")]
    private static extern uint GetWindowThreadProcessId(nint window, out uint processId);
    [DllImport("user32.dll")]
    private static extern bool IsWindowVisible(nint window);
}
