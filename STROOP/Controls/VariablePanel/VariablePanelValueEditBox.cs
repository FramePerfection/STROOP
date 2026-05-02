using STROOP.Variables.VariablePanel;
using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace STROOP.Controls.VariablePanel;

public class VariablePanelValueEditBox : TextBox, IValueEditBox
{
    private readonly EventHandler _handleLostFocus;
    private bool killed = false;

    internal VariablePanelValueEditBox()
    {
        bool updateValue = true;
        KeyDown += (_, e) =>
        {
            updateValue = true;
            if (e.KeyCode == Keys.Enter)
                Parent.Focus();
            else if (e.KeyCode == Keys.Escape)
            {
                updateValue = false;
                Parent.Focus();
            }
        };

        _handleLostFocus = (_, e) =>
        {
            if (killed)
                return;

            if (updateValue)
                Accept?.Invoke(Text);
            else
                Cancel();
        };

        LostFocus += _handleLostFocus;
        Focus();
    }

    public Action<string> Accept { get; set; }
    public Action Cancel { get; set; }

    protected override void Dispose(bool disposing)
    {
        killed = true;
        LostFocus -= _handleLostFocus;
        Parent!.Controls.Remove(this);
        base.Dispose(disposing);
    }
}
