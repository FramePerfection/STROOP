using System;
using System.Windows.Forms;
using STROOP.Utilities;

namespace STROOP.Tabs.BruteforceTab
{
    public class SurfaceAttribute : Attribute
    {
        public readonly string moduleName;

        public SurfaceAttribute(string moduleName)
        {
            this.moduleName = moduleName;
        }
    }

    public class Surface : UserControl
    {
        protected BruteforceTab parentTab => this.GetParent<BruteforceTab>();

        protected Surface()
        {
            if (Program.IsVisualStudioHostProcess()) return;

            Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top | AnchorStyles.Bottom;
            ParentChanged += (_, __) =>
            {
                if (Parent == null)
                    return;
                Bounds = Parent.ClientRectangle;
            };
        }

        public virtual string GetParameter(string parameterName) => "";

        public virtual void InitJson()
        {
        }

        public virtual void Cleanup()
        {
        }
    }
}
