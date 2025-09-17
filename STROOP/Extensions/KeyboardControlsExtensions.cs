using System.Windows.Forms;
using STROOP.Controls;

namespace STROOP.Extensions
{
    public static class KeyboardControlsExtensions
    {
        public static int? GetCurrentlyInputtedNumber(this KeyboardControls controls)
        {
            if (controls.IsDown(Keys.D1)) return 1;
            if (controls.IsDown(Keys.D2)) return 2;
            if (controls.IsDown(Keys.D3)) return 3;
            if (controls.IsDown(Keys.D4)) return 4;
            if (controls.IsDown(Keys.D5)) return 5;
            if (controls.IsDown(Keys.D6)) return 6;
            if (controls.IsDown(Keys.D7)) return 7;
            if (controls.IsDown(Keys.D8)) return 8;
            if (controls.IsDown(Keys.D9)) return 9;
            if (controls.IsDown(Keys.D0)) return 0;
            return null;
        }

        public static bool IsNumberHeld(this KeyboardControls controls)
        {
            return GetCurrentlyInputtedNumber(controls) != null;
        }

        public static bool IsCtrlDown(this KeyboardControls controls)
            => controls.IsDown(Keys.ControlKey);

        public static bool IsShiftDown(this KeyboardControls controls)
            => controls.IsDown(Keys.ShiftKey);

        public static bool IsAltDown(this KeyboardControls controls)
            => controls.IsDown(Keys.Menu) || controls.IsDown(Keys.Alt);

        public static bool IsDeletishKeyDown(this KeyboardControls controls)
        {
            return controls.IsDown(Keys.Delete) ||
                   controls.IsDown(Keys.Back) ||
                   controls.IsDown(Keys.Escape);
        }
    }
}
