using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using STROOP.Controls.VariablePanel;

namespace STROOP.Tabs
{
    public partial class STROOPTab : UserControl
    {
        public static void UpdateTab(TabPage page, bool active)
        {
            foreach (var pageControl in page.Controls)
                if (pageControl is STROOPTab stroopTab)
                {
                    if (stroopTab.Size != page.Size)
                        stroopTab.Size = page.Size;
                    stroopTab.UpdateOrInitialize(active);
                }
        }

        public TabPage Tab => Parent as TabPage;

        public bool IsActiveTab
        {
            get
            {
                var form = Tab.FindForm() as StroopMainForm;
                return form != null && form.tabControlMain.SelectedTab == Tab;
            }
        }

        static readonly Size InitSize = new Size(915, 463);

        bool initialized = false;

        public override Size MinimumSize
        {
            get => initialized ? base.MinimumSize : InitSize;
            set => base.MinimumSize = initialized ? value : InitSize;
        }

        public override Size MaximumSize
        {
            get => initialized ? base.MinimumSize : InitSize;
            set => base.MinimumSize = initialized ? value : InitSize;
        }

        public virtual Action<IEnumerable<ObjectSlot>> objectSlotsClicked => null;
        public virtual HashSet<uint> selection => Structs.Configurations.Config.ObjectSlotsManager.SelectedSlotsAddresses;

        public STROOPTab()
        {
            Size = InitSize;
            InitializeComponent();
        }

        public virtual string GetDisplayName()
        {
            var baseName = GetType().Name;
            if (baseName.EndsWith("Tab"))
                return baseName.Remove(baseName.Length - 3);
            else
                return baseName;
        }

        public static void PerformRecursiveAction(Control c, Action<VariablePanel> action)
        {
            if (c is VariablePanel panel)
                action(panel);
            foreach (Control child in c.Controls)
                PerformRecursiveAction(child, action);
        }

        public void UpdateOrInitialize(bool active)
        {
            if (active && !initialized)
            {
                PerformRecursiveAction(this, panel => panel.DeferredInitialize());
                InitializeTab();
                initialized = true;
            }

            Update(active);
        }

        public virtual void Update(bool active)
        {
            if (active)
                PerformRecursiveAction(this, panel => panel.UpdatePanel());
        }

        public virtual void InitializeTab()
        {
        }
    }
}
