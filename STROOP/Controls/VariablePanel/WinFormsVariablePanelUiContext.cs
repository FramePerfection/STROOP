using STROOP.Variables.VariablePanel;
using System.Drawing;
using System.Windows.Forms;

namespace STROOP.Controls.VariablePanel;

public class WinFormsVariablePanelUiContext(Control parent, Graphics graphics, Rectangle drawRegion) : IUiContext
{
    public readonly Control parent = parent;
    public readonly Graphics graphics = graphics;
    public Rectangle drawRegion = drawRegion;

    public IValueEditBox CreateValueBox(string lastValue)
    {
        var textEditBox = new VariablePanelValueEditBox();
        textEditBox.Text = lastValue;
        textEditBox.Bounds = drawRegion;
        parent.Controls.Add(textEditBox);
        textEditBox.Focus();
        return textEditBox;
    }
}
