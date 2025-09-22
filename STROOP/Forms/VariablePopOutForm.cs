using System.Collections.Generic;
using System.Windows.Forms;
using STROOP.Controls.VariablePanel;
using STROOP.Structs;
using STROOP.Variables;

namespace STROOP.Forms
{
    public partial class VariablePopOutForm : Form, IUpdatableForm
    {
        public static int? WIDTH = null;
        public static int? HEIGHT = null;

        public WatchVariablePanel panel => _watchVariablePanel;

        private bool _borderless = false;
        private bool _isDragging = false;
        private int _dragX = 0;
        private int _dragY = 0;

        private bool _alwaysOnTop = false;

        private static int _instanceCouner = 0;

        public VariablePopOutForm()
        {
            InitializeComponent();
            FormManager.AddForm(this);
            FormClosing += (sender, e) => FormManager.RemoveForm(this);

            _instanceCouner++;
            Text = "Pop Out " + _instanceCouner;

            if (WIDTH.HasValue) Width = WIDTH.Value;
            if (HEIGHT.HasValue) Height = HEIGHT.Value;
            Resize += (sender, e) =>
            {
                WIDTH = Width;
                HEIGHT = Height;
            };
        }

        public void Initialize(List<NamedVariableCollection.IView> vars)
        {
            // initialize panel
            _watchVariablePanel.Initialize();
            _watchVariablePanel.DeferredInitialize();
            _watchVariablePanel.AddVariables(vars);

            // add borderless item to panel
            ToolStripMenuItem itemBorderless = new ToolStripMenuItem("Borderless");
            itemBorderless.Click += (sender, e) =>
            {
                _borderless = !_borderless;
                itemBorderless.Checked = _borderless;
                FormBorderStyle = _borderless ? FormBorderStyle.None : FormBorderStyle.Sizable;
            };
            itemBorderless.Checked = _borderless;
            _watchVariablePanel.customContextMenuItems.Add(itemBorderless);

            // add always on top item to panel
            ToolStripMenuItem itemAlwaysOnTop = new ToolStripMenuItem("Always On Top");
            itemAlwaysOnTop.Click += (sender, e) =>
            {
                _alwaysOnTop = !_alwaysOnTop;
                itemAlwaysOnTop.Checked = _alwaysOnTop;
                TopMost = _alwaysOnTop;
            };
            itemBorderless.Checked = _alwaysOnTop;
            _watchVariablePanel.customContextMenuItems.Add(itemAlwaysOnTop);

            // add close item to panel
            ToolStripMenuItem itemClose = new ToolStripMenuItem("Close");
            itemClose.Click += (sender, e) => Close();
            _watchVariablePanel.customContextMenuItems.Add(itemClose);

            // make panel draggable when borderless
            _watchVariablePanel.MouseDown += (sender, e) =>
            {
                if (!_borderless) return;
                _isDragging = true;
                _dragX = e.X;
                _dragY = e.Y;
            };
            _watchVariablePanel.MouseUp += (sender, e) =>
            {
                if (!_borderless) return;
                _isDragging = false;
            };
            _watchVariablePanel.MouseMove += (sender, e) =>
            {
                if (!_borderless) return;
                if (_isDragging)
                {
                    SetDesktopLocation(MousePosition.X - _dragX, MousePosition.Y - _dragY);
                }
            };
        }

        public void UpdateForm()
        {
            _watchVariablePanel.UpdatePanel();
        }

        public void ShowForm() => Show();
    }
}
