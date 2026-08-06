using System;
using System.Windows.Forms;
using OpenTK.GLControl;
using STROOP.Core;
using STROOP.Utilities;

namespace STROOP.Tabs.MapTab
{
    public partial class MapPopout : Form
    {
        GLControl glControl;
        MapGraphics graphics;
        MapTab mapTab;

        public MapPopout(MapTab mapTab)
        {
            this.mapTab = mapTab;
            InitializeComponent();
            ClientSize = mapTab.graphics.glControl.ClientRectangle.Size;
            // Own GL context, but sharing resources with the main map's context so we can present the
            // shared color texture the main context renders into. See issue #39.
            glControl = new GLControl()
            {
                APIVersion = new Version(3, 3),
                SharedContext = mapTab.graphics.glControl,
            };
            glControl.Bounds = ClientRectangle;
            glControl.Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top | AnchorStyles.Bottom;
            Controls.Add(glControl);
            // Render in the main map's (shared) context; present into our own context (handled in MapGraphics).
            graphics = new MapGraphics(mapTab, glControl, () => mapTab.graphics.glControl.Context);
            graphics.MapViewAngleValue = mapTab.graphics.MapViewAngleValue;
            graphics.MapViewScaleValue = mapTab.graphics.MapViewScaleValue;
            graphics.currentView.position = mapTab.graphics.currentView.position;
            Shown += (_, __) =>
            {
                using (new AccessScope<MapTab>(mapTab))
                    graphics.Load(() => mapTab.graphics.rendererCollection);
            };

            glControl.MouseDown += (sender, e) =>
            {
                if (e.Button == MouseButtons.Right)
                    ShowRightClickMenu();
            };
        }

        ContextMenuStrip contextMenu;
        void ShowRightClickMenu()
        {
            contextMenu?.Dispose();
            contextMenu = new ContextMenuStrip();

            mapTab.AddViewContextMenuItems(contextMenu, graphics);

            contextMenu.Show(Cursor.Position);
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
