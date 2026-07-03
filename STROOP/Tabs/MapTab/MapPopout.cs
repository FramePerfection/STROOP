using OpenTK;
using System;
using System.Windows.Forms;
using OpenTK.GLControl;
using STROOP.Core;
using STROOP.Utilities;

namespace STROOP.Tabs.MapTab
{
    public partial class MapPopout : Form
    {
        // Kept so we can make the main map's (render) context current when cleaning up on close.
        readonly MapTab tab;
        GLControl glControl;
        MapGraphics graphics;

        public MapPopout(MapTab tab)
        {
            this.tab = tab;
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
            // The render surfaces live in the main map's context; delete them there.
            tab.graphics.glControl.Context.MakeCurrent();
            graphics.CleanUp();
            // Disposing our control tears down our own context (and its present-FBO).
            glControl.Dispose();
        }
    }
}
