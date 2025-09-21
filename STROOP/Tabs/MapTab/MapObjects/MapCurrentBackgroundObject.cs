namespace STROOP.Tabs.MapTab.MapObjects
{
    public class MapCurrentBackgroundObject : MapBackgroundObject
    {
        public MapCurrentBackgroundObject() : base()
        {
        }

        protected override BackgroundImage GetBackgroundImage() => currentMapTab.GetBackgroundImage();

        public override string GetName()
        {
            return "Current Background";
        }
    }
}
