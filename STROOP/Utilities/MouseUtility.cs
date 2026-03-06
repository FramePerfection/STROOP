using System.Windows.Forms;
using Windows.Win32;
using Windows.Win32.UI.WindowsAndMessaging;

namespace STROOP.Utilities
{
    public static class MouseUtility
    {
        public static bool IsMouseDown(int button)
        {
            bool buttonsSwapped = PInvoke.GetSystemMetrics(SYSTEM_METRICS_INDEX.SM_SWAPBUTTON) != 0;
            if (buttonsSwapped)
                if (button == 0) button = 1;
                else if (button == 1) button = 0;

            Keys key = button == 2 ? Keys.MButton : (Keys)(button + 1);
            return (PInvoke.GetAsyncKeyState((int)key) & 0x8000) != 0;
        }
    }
}
