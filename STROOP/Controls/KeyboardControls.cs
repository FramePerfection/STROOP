using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace STROOP.Controls;

public class KeyboardControls
{
    private readonly Control _parent;
    HashSet<Keys> pressedKeys = new();

    public KeyboardControls(Control parent)
    {
        _parent = parent;
        parent.KeyDown += OnKeyDown;
        parent.KeyUp += OnKeyUp;
        parent.Disposed += Unbind;
    }
    
    public bool IsDown(Keys key) => pressedKeys.Contains(key);

    private void Unbind(object sender, EventArgs e)
    {
        _parent.KeyDown -= OnKeyDown;
        _parent.KeyUp -= OnKeyUp;
    }

    private void OnKeyDown(object sender, KeyEventArgs e)
        => pressedKeys.Add(e.KeyCode);

    private void OnKeyUp(object sender, KeyEventArgs e)
        => pressedKeys.Remove(e.KeyCode);
}