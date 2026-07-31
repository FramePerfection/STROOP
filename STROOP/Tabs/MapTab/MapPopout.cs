using System;
using System.Windows.Forms;
using OpenTK.GLControl;
using STROOP.Core;

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
            // Own GL context, but sharing resources with the main map's context so we can present the
            // shared color texture the main context renders into. See issue #39.
            glControl = new GLControl()
            {
                APIVersion = new Version(3, 3),
                SharedContext = tab.graphics.glControl,
            };
            glControl.Bounds = ClientRectangle;
            glControl.Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top | AnchorStyles.Bottom;
            Controls.Add(glControl);
            // Render in the main map's (shared) context; present into our own context (handled in MapGraphics).
            graphics = new MapGraphics(tab, glControl, () => tab.graphics.glControl.Context);
            graphics.MapViewAngleValue = tab.graphics.MapViewAngleValue;
            graphics.MapViewScaleValue = tab.graphics.MapViewScaleValue;
            graphics.currentView.position = tab.graphics.currentView.position;
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
