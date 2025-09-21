using System;
using System.Collections.Generic;
using System.Windows.Forms;
using OpenTK.Mathematics;

namespace STROOP.Controls;

public class MouseControls
{
    private readonly Control _parent;

    Vector2 _pMouseCoords, _mouseCoords;
    float? _pMouseScroll = null;
    MouseButtons _mouseDown = MouseButtons.None;

    public MouseControls(Control parent)
    {
        _parent = parent;
        parent.MouseDown += OnMouseDown;
        parent.MouseMove += OnMouseMove;
        parent.MouseUp += OnMouseUp;
        parent.Disposed += Unbind;
    }

    public bool IsDown(MouseButtons button) => _mouseDown.HasFlag(button);

    public Vector2 delta = Vector2.Zero;

    public void NextFrame()
    {
        delta = _mouseCoords - _pMouseCoords;
        _pMouseCoords = _mouseCoords;
    }

    private void Unbind(object sender, EventArgs e)
    {
        _parent.MouseDown -= OnMouseDown;
        _parent.MouseMove -= OnMouseMove;
        _parent.MouseUp -= OnMouseUp;
    }

    private void OnMouseDown(object sender, MouseEventArgs e)
    {
        _mouseDown |= e.Button;
        _pMouseCoords = new Vector2(e.X, e.Y);
        _mouseCoords = new Vector2(e.X, e.Y);
    }

    private void OnMouseMove(object sender, MouseEventArgs e)
    {
        _mouseCoords = new Vector2(e.X, e.Y);
    }

    private void OnMouseUp(object sender, MouseEventArgs e)
        => _mouseDown &= ~e.Button;
}
