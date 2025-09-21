using System;
using System.Drawing;
using System.IO;

namespace STROOP.Tabs.MapTab
{
    public class MapLayout : IComparable
    {
        public string ImagePath;
        public BackgroundImage Background;
        public Lazy<BackgroundImage> MapImage;

        public string Id;
        public byte Level;
        public byte Area;
        public ushort? LoadingPoint;
        public ushort? MissionLayout;
        public RectangleF Coordinates;
        public float Y;
        public string Name;
        public string SubName;

        public MapLayout()
        {
            MapImage = new Lazy<BackgroundImage>(() =>
            {
                var path = Path.Combine(MapTab.MapAssociations.MapImageFolderPath, ImagePath);
                return new BackgroundImage(ToString(), path);
            });
        }

        public override bool Equals(object obj)
        {
            if (!(obj is MapLayout)) return false;
            MapLayout other = (MapLayout)obj;
            return Id == other.Id;
        }

        public override int GetHashCode()
        {
            return ImagePath.GetHashCode() * 127 + Level.GetHashCode() * 31 + Area.GetHashCode() * 17 + Y.GetHashCode()
                   + 257 * MissionLayout.GetHashCode() + 67 * LoadingPoint.GetHashCode();
        }

        public override string ToString()
        {
            string subNameString = SubName != null ? ": " + SubName : "";
            string yString = Y != float.MinValue ? String.Format(" (y ≥ {0})", Y) : "";
            return Name + subNameString + yString;
        }

        public int CompareTo(object obj)
        {
            if (!(obj is MapLayout)) return -1;
            MapLayout other = (MapLayout)obj;
            return Id.CompareTo(other.Id);
        }
    }
}
