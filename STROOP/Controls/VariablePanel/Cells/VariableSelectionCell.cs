using STROOP.Structs.Configurations;
using STROOP.Utilities;
using STROOP.Variables;
using STROOP.Variables.VariablePanel;
using STROOP.Variables.VariablePanel.Cells;
using System.Drawing;
using System.Windows.Forms;

namespace STROOP.Controls.VariablePanel.Cells;

public class VariableSelectionCell<TBaseWrapper, TBackingValue>(IVariable<TBackingValue> var, WinFormsVariableControl control)
    : VariableSelectionCell<VariablePanelUiContext, TBaseWrapper, TBackingValue>(var, control)
    where TBaseWrapper : VariableCell<VariablePanelUiContext, TBackingValue>
{
        static StringFormat rightAlignFormat = new StringFormat() { Alignment = StringAlignment.Far };
        bool IsCursorHovering(VariablePanelUiContext uiContext, out Rectangle drawRectangle)
        {
            int marginX = (int)SavedSettingsConfig.WatchVarPanelHorizontalMargin.value;
            int marginY = (int)SavedSettingsConfig.WatchVarPanelVerticalMargin.value;

            Rectangle screenRect;
            if (isSingleOption)
            {
                screenRect = uiContext.parent.RectangleToScreen(uiContext.drawRegion);
                drawRectangle = uiContext.drawRegion;
            }
            else
            {
                var sideLength = uiContext.drawRegion.Height - marginY * 2;
                drawRectangle = new Rectangle(uiContext.drawRegion.Left + marginX, uiContext.drawRegion.Top + marginY, sideLength, sideLength);
                screenRect = uiContext.parent.RectangleToScreen(drawRectangle);
            }

            return Cursor.Position.IsInsideRect(screenRect);
        }

        public override void SingleClick(VariablePanelUiContext uiContext)
        {
            base.SingleClick(uiContext);
            if (IsCursorHovering(uiContext, out _))
            {
                if (isSingleOption)
                    view.setter(options[0].func());
                else if (options.Count > 0)
                {
                    var ctx = new ContextMenuStrip();
                    foreach (var option_it in options)
                    {
                        var option_cap = option_it;
                        ctx.Items.AddHandlerToItem(option_cap.name, () => SetOption(option_cap));
                    }

                    ctx.Show(Cursor.Position);
                }
            }
            else
                baseWrapper.SingleClick(uiContext);
        }

        public override IVariableCellUi<VariablePanelUiContext>.CustomDraw CustomDrawOperation => uiContext =>
        {
            var g = uiContext.graphics;
            baseWrapper.CustomDrawOperation?.Invoke(uiContext);

            int marginX = (int)SavedSettingsConfig.WatchVarPanelHorizontalMargin.value;
            int marginY = (int)SavedSettingsConfig.WatchVarPanelVerticalMargin.value;

            if (isSingleOption)
                g.FillRectangle(IsCursorHovering(uiContext, out var drawRect) ? Brushes.LightSlateGray : Brushes.Gray, drawRect);

            var txtPoint = new Point(uiContext.drawRegion.Right - marginX, uiContext.drawRegion.Top + marginY);
            g.DrawString(isSingleOption ? options[0].name : GetValueText(),
                uiContext.parent.Font,
                control.IsSelected ? Brushes.White : Brushes.Black,
                txtPoint,
                rightAlignFormat);

            if (!isSingleOption)
                g.DrawImage(IsCursorHovering(uiContext, out var drawRect) ? Properties.Resources.dropdown_box_hover : Properties.Resources.dropdown_box, drawRect);
        };

}
