namespace STROOP.Tabs.MapTab.MapObjects
{
    public class MapCurrentMapObject : MapMapObject
    {
        public MapCurrentMapObject() : base()
        {
        }

        public override MapLayout GetMapLayout()
        {
            return currentMapTab.GetMapLayout();
        }

        public override string GetName()
        {
            return "Current Map";
        }
    }
}
