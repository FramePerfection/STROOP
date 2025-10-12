using System.Collections.Generic;
using System.Drawing;

namespace STROOP.Tabs.MapTab.MapObjects
{
    public abstract class MapIconRectangleObject : MapIconObject
    {
        protected MapIconRectangleObject(ObjectCreateParams createParams)
            : base(createParams)
        {
        }

        protected abstract List<(PointF loc, SizeF size)> GetDimensions(MapGraphics graphics);
    }
}
