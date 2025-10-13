using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using STROOP.Structs.Configurations;
using STROOP.Utilities;
using STROOP.Variables;
using STROOP.Variables.Views;

namespace STROOP.Controls.VariablePanel
{
    public class WatchVariableSelectionWrapper<TBaseWrapper, TBackingValue> : WatchVariableWrapper<TBackingValue>
        where TBaseWrapper : WatchVariableWrapper<TBackingValue>
    {
        static StringFormat rightAlignFormat = new StringFormat() { Alignment = StringAlignment.Far };

        public bool DisplaySingleOption = false;

        public List<(string name, Func<TBackingValue> func)> options = new List<(string, Func<TBackingValue>)>();

        TBaseWrapper baseWrapper;
        bool isSingleOption => DisplaySingleOption && options.Count == 1;

        (string name, Func<TBackingValue> getter) selectedOption;

        public WatchVariableSelectionWrapper(IVariableView<TBackingValue> var, WatchVariableControl control) : base(var, control)
        {
            var interfaceType = view.GetType().GetInterfaces().First(x => x.Name == $"{nameof(IVariableView)}`1");
            baseWrapper = (TBaseWrapper)
                typeof(TBaseWrapper)
                    .GetConstructor(new Type[] { interfaceType, typeof(WatchVariableControl) })
                    .Invoke(new object[] { view, control });
            _view.ValueSet += () => selectedOption = (null, null);
        }

        public override string GetClass() => $"Selection for {baseWrapper.GetClass()}";

        public override void UpdateControls() => baseWrapper.UpdateControls();

        bool IsCursorHovering(Rectangle rect, out Rectangle drawRectangle)
        {
            int marginX = (int)SavedSettingsConfig.WatchVarPanelHorizontalMargin.value;
            int marginY = (int)SavedSettingsConfig.WatchVarPanelVerticalMargin.value;

            Rectangle screenRect;
            if (isSingleOption)
            {
                screenRect = _watchVarControl.containingPanel.RectangleToScreen(rect);
                drawRectangle = rect;
            }
            else
            {
                var sideLength = rect.Height - marginY * 2;
                drawRectangle = new Rectangle(rect.Left + marginX, rect.Top + marginY, sideLength, sideLength);
                screenRect = _watchVarControl.containingPanel.RectangleToScreen(drawRectangle);
            }

            return Cursor.Position.IsInsideRect(screenRect);
        }

        public override void SingleClick(Control parentCtrl, Rectangle bounds)
        {
            base.SingleClick(parentCtrl, bounds);
            if (IsCursorHovering(bounds, out var _))
            {
                if (isSingleOption)
                    view._setterFunction(options[0].func());
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
                baseWrapper.SingleClick(parentCtrl, bounds);
        }

        public override WatchVariablePanel.CustomDraw CustomDrawOperation => (g, rect) =>
        {
            baseWrapper.CustomDrawOperation?.Invoke(g, rect);

            int marginX = (int)SavedSettingsConfig.WatchVarPanelHorizontalMargin.value;
            int marginY = (int)SavedSettingsConfig.WatchVarPanelVerticalMargin.value;

            if (isSingleOption)
                g.FillRectangle(IsCursorHovering(rect, out var drawRect) ? Brushes.LightSlateGray : Brushes.Gray, drawRect);

            var txtPoint = new Point(rect.Right - marginX, rect.Top + marginY);
            g.DrawString(isSingleOption ? options[0].name : GetValueText(),
                _watchVarControl.containingPanel.Font,
                _watchVarControl.IsSelected ? Brushes.White : Brushes.Black,
                txtPoint,
                rightAlignFormat);

            if (!isSingleOption)
                g.DrawImage(IsCursorHovering(rect, out var drawRect) ? Properties.Resources.dropdown_box_hover : Properties.Resources.dropdown_box, drawRect);
        };

        void SetOption((string name, Func<TBackingValue> func) option)
        {
            view._setterFunction(option.func());
            selectedOption = option;
        }

        public void SelectOption(int index) => SetOption(options[index]);

        public void UpdateOption(int index)
        {
            if (selectedOption.Equals(options[index]))
            {
                view._setterFunction(selectedOption.getter());
                selectedOption = options[index];
            }
        }

        public override string DisplayValue(TBackingValue value) => baseWrapper.DisplayValue(value);

        public override bool TryParseValue(string value, out TBackingValue result) => baseWrapper.TryParseValue(value, out result);
    }
}
