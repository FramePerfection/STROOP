using System.Windows.Forms;
using Windows.Win32;

namespace STROOP.Utilities;

public static class GlobalKeyboard
{
    private static bool IsDownInternal(Keys key)
        => (PInvoke.GetAsyncKeyState((int)key) & 0x8000) != 0;

    public static bool IsDown(Keys key) => IsDownInternal(key);

    public static bool IsCtrlDown() => IsDown(Keys.ControlKey);
    public static bool IsShiftDown() => IsDown(Keys.ShiftKey);
    public static bool IsAltDown() => IsDown(Keys.Menu) || IsDown(Keys.Alt); // Don't ask me why...

    public static int? GetCurrentlyInputtedNumber()
    {
        if (IsDown(Keys.D1)) return 1;
        if (IsDown(Keys.D2)) return 2;
        if (IsDown(Keys.D3)) return 3;
        if (IsDown(Keys.D4)) return 4;
        if (IsDown(Keys.D5)) return 5;
        if (IsDown(Keys.D6)) return 6;
        if (IsDown(Keys.D7)) return 7;
        if (IsDown(Keys.D8)) return 8;
        if (IsDown(Keys.D9)) return 9;
        if (IsDown(Keys.D0)) return 0;
        return null;
    }

    public static bool IsNumberDown()
    {
        return GetCurrentlyInputtedNumber() != null;
    }

    public static bool IsDeletishKeyDown()
    {
        return IsDown(Keys.Delete) ||
               IsDown(Keys.Back) ||
               IsDown(Keys.Escape);
    }
}
