using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace STROOP.Utilities
{
    public static class MouseUtility
    {
        const int SM_SWAPBUTTON = 23;

        [DllImport("user32.dll")]
        static extern short GetAsyncKeyState(Keys vKey);

        [DllImport("user32.dll")]
        static extern int GetSystemMetrics(int nIndex);

        public static bool IsMouseDown(int button)
        {
            bool buttonsSwapped = GetSystemMetrics(SM_SWAPBUTTON) != 0;
            if (buttonsSwapped)
                if (button == 0) button = 1;
                else if (button == 1) button = 0;

            Keys key = button == 2 ? Keys.MButton : (Keys)(button + 1);
            return (GetAsyncKeyState(key) & 0x8000) != 0;
        }
    }
}
