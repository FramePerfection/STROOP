using System.Windows.Forms;
using OpenTK;
using OpenTK.Mathematics;

namespace STROOP.Tabs.MapTab
{
    public enum DragMask
    {
        None = 0,
        X = 1 << 0,
        Y = 1 << 1,
        Z = 1 << 2,
        Angle = 1 << 3,
        All = X | Y | Z | Angle,
    }

    public interface IHoverData
    {
        void AddContextMenuItems(MapTab tab, ContextMenuStrip menu);
        void LeftClick(Vector3 position);
        void RightClick(Vector3 position);
        void DragTo(Vector3 position, bool setY);
        void SetLookAt(Vector3 lookAt);
        DragMask CanDrag();
    }
}
