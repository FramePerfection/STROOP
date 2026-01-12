using OpenTK.Mathematics;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace STROOP.Utilities;

public class ColorDialogUtilities
{
    public static Color? GetColorFromDialog(Color? defaultColor = null)
    {
        ColorDialog colorDialog = new ColorDialog();
        colorDialog.FullOpen = true;
        if (defaultColor.HasValue) colorDialog.Color = defaultColor.Value;
        if (colorDialog.ShowDialog() == DialogResult.OK) return colorDialog.Color;
        return null;
    }
}
