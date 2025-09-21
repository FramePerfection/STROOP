using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace STROOP.Utilities;

public static class GlobalKeyboard
{
    static HashSet<Keys> pressedKeys = new();
    static HashSet<Form> registeredForms = new();

    public static void AddForm(Form form)
    {
        registeredForms.Add(form);
        form.KeyPreview = true;
        form.KeyDown += OnKeyDown;
        form.KeyUp += OnKeyUp;

        EventHandler unbind = null;
        unbind = (sender, e) =>
        {
            registeredForms.Remove(form);
            ((Form)sender).Disposed -= unbind;
            form.KeyDown -= OnKeyDown;
            form.KeyUp -= OnKeyUp;
        };
        form.Disposed += unbind;
    }

    public static bool IsDown(Keys key) => pressedKeys.Contains(key) && registeredForms.Any(f => Form.ActiveForm == f);

    public static bool IsCtrlDown() => pressedKeys.Contains(Keys.ControlKey);
    public static bool IsShiftDown() => pressedKeys.Contains(Keys.ShiftKey);
    public static bool IsAltDown() => pressedKeys.Contains(Keys.Menu) || pressedKeys.Contains(Keys.Alt); // Don't ask me why...

    public static int? GetCurrentlyInputtedNumber()
    {
        if (pressedKeys.Contains(Keys.D1)) return 1;
        if (pressedKeys.Contains(Keys.D2)) return 2;
        if (pressedKeys.Contains(Keys.D3)) return 3;
        if (pressedKeys.Contains(Keys.D4)) return 4;
        if (pressedKeys.Contains(Keys.D5)) return 5;
        if (pressedKeys.Contains(Keys.D6)) return 6;
        if (pressedKeys.Contains(Keys.D7)) return 7;
        if (pressedKeys.Contains(Keys.D8)) return 8;
        if (pressedKeys.Contains(Keys.D9)) return 9;
        if (pressedKeys.Contains(Keys.D0)) return 0;
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

    private static void OnKeyDown(object sender, KeyEventArgs e)
        => pressedKeys.Add(e.KeyCode);

    private static void OnKeyUp(object sender, KeyEventArgs e)
        => pressedKeys.Remove(e.KeyCode);
}