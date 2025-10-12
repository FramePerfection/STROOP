using System.Windows.Forms;
using System;

namespace STROOP.Utilities
{
    public static class FormsUtilities
    {
        public static ToolStripMenuItem AddHandlerToItem(this ToolStripItemCollection strip, string key, Action handler)
        {
            ToolStripMenuItem item;
            foreach (ToolStripItem fsjkl in strip)
                if (fsjkl is ToolStripMenuItem dadsa && dadsa.Text == key)
                {
                    item = dadsa;
                    goto skipNew;
                }

            item = new ToolStripMenuItem(key);
            strip.Add(item);
        skipNew:
            item.Click += (_, __) => handler();
            return item;
        }

        public static ToolStripMenuItem GetSubItem(this ToolStripItemCollection strip, string key)
        {
            ToolStripMenuItem item;
            foreach (ToolStripItem fsjkl in strip)
                if (fsjkl.Text == key && fsjkl is ToolStripMenuItem result)
                    return result;

            item = new ToolStripMenuItem(key);
            strip.Add(item);
            return item;
        }

        public static void PreventClosingMenuStrip(this ToolStripMenuItem item)
        {
            var parentStrip = item.GetCurrentParent() as ContextMenuStrip;
            if (parentStrip != null)
            {
                ToolStripDropDownClosingEventHandler closeOnce = null;
                closeOnce = (___, args) =>
                {
                    parentStrip.Closing -= closeOnce;
                    if (args.CloseReason == ToolStripDropDownCloseReason.ItemClicked)
                        args.Cancel = true;
                };
                parentStrip.Closing += closeOnce;
            }
        }

        public static T GetParent<T>(this Control ctrl) where T : Control
        {
            if (ctrl.Parent is T parentT)
                return parentT;
            else if (ctrl.Parent == null)
                return null;
            return ctrl.Parent.GetParent<T>();
        }
    }
}
