using System.Linq;
using System.Windows.Forms;
using STROOP.Utilities;
using System.Text.RegularExpressions;
using STROOP.Structs.Configurations;

namespace STROOP.Tabs.MapTab.MapObjects
{
    public class MapNearbyObjectsObject : MapMultipleObjects
    {
        const float DEFAULT_RADIUS = 500;
        const float DEFAULT_WITHIN_DIST = 500;

        delegate bool NearbyFunc(PositionAngle objPA, PositionAngle refPA);

        NearbyFunc nearbyFunc;
        float _radius = DEFAULT_RADIUS;
        float _withinDist = DEFAULT_WITHIN_DIST;
        string _nameFilter = "$";

        Regex _nameFilterRegex;

        public MapNearbyObjectsObject(string srcName, PositionAngleProvider positionAngleProvider)
            : base($"nearby {srcName}", Config.ObjectAssociations.DefaultImage, Config.ObjectAssociations.DefaultImage)
        {
            predicate = FilterNearby;
            nearbyFunc = NearbyCylindrical;
            this.positionAngleProvider = positionAngleProvider;
        }

        protected override ContextMenuStrip GetContextMenuStrip(MapTracker targetTracker)
        {
            var ctx = new ContextMenuStrip();

            ToolStripMenuItem itemSetRadius = new ToolStripMenuItem("Set Nearby Radius");
            itemSetRadius.Click += (sender, e) =>
                DialogUtilities.UpdateNumberFromDialog(ref _radius, labelText: "Enter the nearby search radius:", textboxText: _radius.ToString());
            ctx.Items.Add(itemSetRadius);

            ToolStripMenuItem itemSetWithinDist = new ToolStripMenuItem("Set Within Dist");
            itemSetWithinDist.Click += (sender, e) =>
                DialogUtilities.UpdateNumberFromDialog(
                    ref _withinDist,
                    labelText: "Enter the vertical distance within which to show objects.",
                    textboxText: (float.IsNaN(_withinDist) ? DEFAULT_WITHIN_DIST : _withinDist).ToString()
                );
            ctx.Items.Add(itemSetWithinDist);

            ToolStripMenuItem itemSetNameFilter = new ToolStripMenuItem("Set Name Filter");
            itemSetNameFilter.Click += (sender, e) =>
            {
                _nameFilter = DialogUtilities.GetStringFromDialog(labelText: "Enter a name filter:", textBoxText: _nameFilter);
                UpdateNameFilterRegex();
            };
            ctx.Items.Add(itemSetNameFilter);

            return ctx;
        }

        bool FilterNearby(Models.ObjectDataModel obj)
        {
            // Models.ObjectDataModel should be accessible as a PositionAngle imo
            if (_nameFilterRegex != null && !_nameFilterRegex.IsMatch(obj.BehaviorAssociation.Name.ToLower()))
                return false;
            return positionAngleProvider().Any(_ => nearbyFunc(obj, _));
        }

        bool NearbyCylindrical(PositionAngle objPA, PositionAngle refPA)
            => (objPA.position.Xz - refPA.position.Xz).LengthSquared < _radius * _radius;


        public override (SaveSettings, LoadSettings) SettingsSaveLoad => (
            (System.Xml.XmlNode node) =>
            {
                base.SettingsSaveLoad.save(node);
                SaveValueNode(node, "NearbyRadius", _radius.ToString());
                SaveValueNode(node, "WithinDist", _withinDist.ToString());
                SaveValueNode(node, "NameFilter", _nameFilter.ToString());
            }
            ,
            (System.Xml.XmlNode node) =>
            {
                base.SettingsSaveLoad.load(node);
                if (float.TryParse(LoadValueNode(node, "NearbyRadius"), out float radius))
                    _radius = radius;
                if (float.TryParse(LoadValueNode(node, "WithinDist"), out float withinDist))
                    _withinDist = withinDist;
                _nameFilter = LoadValueNode(node, "NameFilter") ?? "$";
                UpdateNameFilterRegex();
            }
        );

        void UpdateNameFilterRegex() =>
            _nameFilterRegex = _nameFilter == "$"
                ? null
                : new Regex($"^{Regex.Escape(_nameFilter.ToLower()).Replace("\\$", ".*")}$");

        public override string GetName() => $"{(_nameFilterRegex == null ? "Objects" : _nameFilter)} near {base.GetName()}";
    }
}
