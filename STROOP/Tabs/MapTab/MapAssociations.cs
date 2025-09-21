using System;
using System.Collections.Generic;
using System.Linq;
using System.Drawing;
using STROOP.Structs.Configurations;
using STROOP.Structs;

namespace STROOP.Tabs.MapTab
{
    public class MapAssociations
    {
        public readonly Lazy<Image> BrokenBackgroundImage = new Lazy<Image>(() => Image.FromFile("Resources/Maps/broken.png"));
        public readonly Lazy<Image> DownloadingBackgroundImage = new Lazy<Image>(() => Image.FromFile("Resources/Maps/downloading.png"));

        Dictionary<Tuple<byte, byte>, List<MapLayout>> _maps = new Dictionary<Tuple<byte, byte>, List<MapLayout>>();
        Dictionary<string, BackgroundImage> _backgroundImageDictionary = new Dictionary<string, BackgroundImage>();

        public MapLayout DefaultMap;

        public MapLayout ROMHackMap = new MapLayout()
        {
            ImagePath = "Transparent.png",
            Background = new BackgroundImage(
                "ROM Hack Background",
                "Resources/Maps/Background Images/Blue Background.png"
            ),
            Coordinates = new RectangleF(-8192, -8192, 2 * 8192, 2 * 8192)
        };

        public string MapImageFolderPath;
        public string BackgroundImageFolderPath;

        public void AddAssociation(MapLayout map)
        {
            var mapKey = new Tuple<byte, byte>(map.Level, map.Area);
            if (!_maps.ContainsKey(mapKey))
                _maps.Add(mapKey, new List<MapLayout>());
            _maps[mapKey].Add(map);
        }

        public List<MapLayout> GetLevelAreaMaps(byte level, byte area)
        {
            var mapKey = new Tuple<byte, byte>(level, area);
            if (!_maps.ContainsKey(mapKey))
                return new List<MapLayout>();

            return _maps[mapKey];
        }

        public List<MapLayout> GetLevelAreaMaps(byte level, byte area, ushort loadingPoint, ushort missionLayout)
        {
            List<MapLayout> mapList = GetLevelAreaMaps(level, area);
            mapList = mapList.FindAll(map => map.LoadingPoint == null || map.LoadingPoint == loadingPoint);
            mapList = mapList.FindAll(map => map.MissionLayout == null || map.MissionLayout == missionLayout);
            return mapList;
        }

        public MapLayout GetBestMap(byte level, byte area, ushort loadingPoint, ushort missionLayout, float y)
        {
            List<MapLayout> mapList = GetLevelAreaMaps(level, area, loadingPoint, missionLayout);
            mapList = mapList.FindAll(map => map.Y <= y);
            if (mapList.Count == 0) return MapTab.MapAssociations.ROMHackMap;
            MapLayout bestMap = mapList.First();
            foreach (MapLayout map in mapList)
            {
                if (map.Y > bestMap.Y) bestMap = map;
            }

            return bestMap;
        }

        public MapLayout GetBestMap()
        {
            bool isRomHack = Config.Stream.GetUInt32(0x80402000) != 0;
            if (isRomHack)
                return ROMHackMap;

            byte level = Config.Stream.GetByte(MiscConfig.WarpDestinationAddress + MiscConfig.LevelOffset);
            byte area = Config.Stream.GetByte(MiscConfig.WarpDestinationAddress + MiscConfig.AreaOffset);
            ushort loadingPoint = Config.Stream.GetUInt16(MiscConfig.LoadingPointAddress);
            ushort missionLayout = Config.Stream.GetUInt16(MiscConfig.MissionAddress);
            float y = Config.Stream.GetSingle(MarioConfig.StructAddress + MarioConfig.YOffset);
            return GetBestMap(level, area, loadingPoint, missionLayout, y);
        }

        public (byte level, byte area, ushort loadingPoint, ushort missionLayout) GetCurrentLocationStats()
        {
            byte level = Config.Stream.GetByte(MiscConfig.WarpDestinationAddress + MiscConfig.LevelOffset);
            byte area = Config.Stream.GetByte(MiscConfig.WarpDestinationAddress + MiscConfig.AreaOffset);
            ushort loadingPoint = Config.Stream.GetUInt16(MiscConfig.LoadingPointAddress);
            ushort missionLayout = Config.Stream.GetUInt16(MiscConfig.MissionAddress);
            return (level, area, loadingPoint, missionLayout);
        }

        public List<MapLayout> GetAllMaps()
        {
            List<MapLayout> maps = _maps.Values.SelectMany(list => list).ToList();
            maps.Sort();
            return maps;
        }

        public void AddBackgroundImage(BackgroundImage backgroundImage)
        {
            _backgroundImageDictionary.Add(backgroundImage.Name, backgroundImage);
        }

        public BackgroundImage GetBackgroundImage(string name)
        {
            if (name == null) return null;
            if (_backgroundImageDictionary.TryGetValue(name, out BackgroundImage img))
                return img;
            else
                return null;
        }

        public List<BackgroundImage> GetAllBackgroundImages()
        {
            return _backgroundImageDictionary.Values.ToList();
        }
    }
}
