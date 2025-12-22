using System.Windows.Forms;

namespace STROOP.Controls
{
    public class ReorderFlowLayoutPanel : FlowLayoutPanel
    {
        Control currentDragControl;

        void StartDragging(object sender, MouseEventArgs e)
        {
            currentDragControl = sender as Control;
        }

        public ReorderFlowLayoutPanel()
        {
            ControlAdded += BindControlEventHandlers;
            ControlRemoved += UnbindControlEventHandlers;
            MouseMove += MaybeUpdateDragCursor;
            FlowDirection = FlowDirection.TopDown;
            WrapContents = false;
        }

        void BindControlEventHandlers(object sender, ControlEventArgs e)
        {
            e.Control.MouseDown += StartDragging;
            e.Control.MouseMove += MaybeUpdateDragCursor;
        }

        void UnbindControlEventHandlers(object sender, ControlEventArgs e)
        {
            e.Control.MouseDown -= StartDragging;
            e.Control.MouseMove -= MaybeUpdateDragCursor;
        }

        void MaybeUpdateDragCursor(object sender, MouseEventArgs e)
        {
            if (currentDragControl == null)
                return;
            if (!e.Button.HasFlag(MouseButtons.Left))
            {
                currentDragControl = null;
                return;
            }

            var pt = PointToClient(((Control)sender).PointToScreen(e.Location));
            if (FlowDirection == FlowDirection.TopDown)
            {
                int destIndex = 0;
                foreach (Control ctrl in Controls)
                {
                    if (pt.Y < ctrl.Top + ctrl.Height / 2)
                        break;
                    if (ctrl != currentDragControl)
                        destIndex++;
                }

                Controls.SetChildIndex(currentDragControl, destIndex);
            }
        }
    }
}
