using OpenTK;
using System;
using System.Windows.Forms;
using OpenTK.GLControl;
using STROOP.Utilities;

namespace STROOP.Tabs.MapTab
{
    public partial class MapPopout : Form
    {
        GLControl glControl;
        MapGraphics graphics;

        public MapPopout(MapTab tab)
        {
            InitializeComponent();
            ClientSize = tab.graphics.glControl.ClientRectangle.Size;
            glControl = new GLControl();
            glControl.Bounds = ClientRectangle;
            glControl.Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top | AnchorStyles.Bottom;
            Controls.Add(glControl);
            graphics = new MapGraphics(tab, glControl, () => tab.graphics.glControl.Context);
            graphics.MapViewAngleValue = tab.graphics.MapViewAngleValue;
            graphics.MapViewScaleValue = tab.graphics.MapViewScaleValue;
            graphics.view.position = tab.graphics.view.position;
            Shown += (_, __) =>
            {
                using (new AccessScope<MapTab>(tab))
                    graphics.Load(() => tab.graphics.rendererCollection);
            };
        }

        public void Redraw() => glControl.Invalidate();

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            graphics.CleanUp();
            glControl.Dispose();
        }
    }
}
