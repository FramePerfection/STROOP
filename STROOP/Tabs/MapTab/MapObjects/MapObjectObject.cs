using STROOP.Core.Utilities;
using System;
using System.Drawing;
using STROOP.Utilities;
using STROOP.Structs.Configurations;
using STROOP.Structs;
using STROOP.Models;
using System.Windows.Forms;

namespace STROOP.Tabs.MapTab.MapObjects
{
    public class MapObjectObject : MapIconPointObject
    {
        public static void AddPositionAngleSubTrackers(string name, MapTracker tracker, ContextMenuStrip targetStrip, PositionAngleProvider positionAngleProvider)
        {
            MapTab mapTab = tracker.mapTab;

            // Cylinders
            var cylindersItem = targetStrip.Items.GetSubItem("Cylinders");
            cylindersItem.DropDownItems.AddHandlerToItem("Add Tracker for Custom Cylinders",
                tracker.MakeCreateTrackerHandler(mapTab, "CustomCylinders",
                    _ =>
                    {
                        var customCylinderObject = new MapObjectCylinderObject(
                            positionAngleProvider,
                            null,
                            "Custom Cylinders");
                        customCylinderObject.getDimensions = MapObjectCylinderObject.Dimensions.CustomSize(() => (customCylinderObject.Size, 0, 100));
                        return customCylinderObject;
                    }));


            cylindersItem.DropDownItems.AddHandlerToItem("Add Tracker for Custom Cylinders for Home of ",
                tracker.MakeCreateTrackerHandler(mapTab, "CustomHomeCylinders",
                    _ =>
                    {
                        var customCylinderObject = new MapObjectCylinderObject(
                            () => positionAngleProvider().ConvertAndRemoveNull(obj => PositionAngle.ObjHome(PositionAngle.GetObjectAddress(obj))),
                            null,
                            "Custom Cylinders");
                        customCylinderObject.getDimensions = MapObjectCylinderObject.Dimensions.CustomSize(() => (customCylinderObject.Size, 0, 100));
                        return customCylinderObject;
                    }));


            // Spheres
            var spheresItem = targetStrip.Items.GetSubItem("Spheres");
            spheresItem.DropDownItems.AddHandlerToItem("Add Tracker for Custom Spheres",
                tracker.MakeCreateTrackerHandler(mapTab, "CustomHomeSpheres", _ =>
                {
                    var customSpheresObject = new MapObjectSphereObject(
                        positionAngleProvider,
                        null,
                        "Custom Spheres");
                    customSpheresObject.getDimensions = MapObjectSphereObject.Dimensions.CustomSize(() => customSpheresObject.Size);
                    return customSpheresObject;
                }));
            spheresItem.DropDownItems.AddHandlerToItem("Add Tracker for Custom Spheres for Home",
                tracker.MakeCreateTrackerHandler(mapTab, "CustomSpheres", _ =>
                {
                    var customSpheresObject = new MapObjectSphereObject(
                        () => positionAngleProvider().ConvertAndRemoveNull(obj => PositionAngle.ObjHome(PositionAngle.GetObjectAddress(obj))),
                        null,
                        "Custom Spheres");
                    customSpheresObject.getDimensions = MapObjectSphereObject.Dimensions.CustomSize(() => customSpheresObject.Size);
                    return customSpheresObject;
                }));

            // Angles
            var anglesItem = targetStrip.Items.GetSubItem("Angles");
            anglesItem.DropDownItems.AddHandlerToItem("Add Tracker for Angle Range",
                tracker.MakeCreateTrackerHandler(mapTab, "AngleRange", _ => new MapAngleRangeObject(positionAngleProvider)));

            anglesItem.DropDownItems.AddHandlerToItem("Add Tracker for Facing Divider",
                tracker.MakeCreateTrackerHandler(mapTab, "FacingDivider", _ => new MapFacingDividerObject(positionAngleProvider)));

            anglesItem.DropDownItems.AddHandlerToItem("Add Tracker for Sector",
                tracker.MakeCreateTrackerHandler(mapTab, "Sector", _ => new MapSectorObject(positionAngleProvider)));

            var marioAnglesItem = targetStrip.Items.GetSubItem("Mario Angles");

            marioAnglesItem.DropDownItems.AddHandlerToItem("Add Tracker for Mario Facing Angle",
                tracker.MakeCreateTrackerHandler(mapTab, "MarioFacingAngle", _ => new MapArrowObject(
                    positionAngleProvider,
                    MapArrowObject.ArrowSource.MarioFacingYaw,
                    MapArrowObject.ArrowSource.MarioHSpeed,
                    $"Mario Facing Angle")));

            marioAnglesItem.DropDownItems.AddHandlerToItem("Add Tracker for Mario Intended Angle",
                tracker.MakeCreateTrackerHandler(mapTab, "MarioIntendedAngle", _ => new MapArrowObject(
                    positionAngleProvider,
                    MapArrowObject.ArrowSource.MarioIndendedYaw,
                    MapArrowObject.ArrowSource.MarioHSpeed,
                    $"Mario Intended Angle")));

            marioAnglesItem.DropDownItems.AddHandlerToItem("Add Tracker for Mario Moving Angle",
                tracker.MakeCreateTrackerHandler(mapTab, "MarioMovingAngle", _ => new MapArrowObject(
                    positionAngleProvider,
                    MapArrowObject.ArrowSource.MarioMovingYaw,
                    MapArrowObject.ArrowSource.MarioHSpeed,
                    $"Mario Moving Angle")));

            marioAnglesItem.DropDownItems.AddHandlerToItem("Add Tracker for Mario Sliding Angle",
                tracker.MakeCreateTrackerHandler(mapTab, "MarioSlidingAngle", _ => new MapArrowObject(
                    positionAngleProvider,
                    MapArrowObject.ArrowSource.MarioSlidingYaw,
                    MapArrowObject.ArrowSource.MarioSlidingSpeed,
                    $"Mario Sliding Angle")));

            marioAnglesItem.DropDownItems.AddHandlerToItem("Add Tracker for Mario Twirling Angle",
                tracker.MakeCreateTrackerHandler(mapTab, "MarioTwirlingAngle", _ => new MapArrowObject(
                    positionAngleProvider,
                    MapArrowObject.ArrowSource.MarioTwirlingYaw,
                    MapArrowObject.ArrowSource.MarioHSpeed,
                    $"Mario Twirling Angle")));

            // Current
            var currentItem = targetStrip.Items.GetSubItem("Current");

            currentItem.DropDownItems.AddHandlerToItem("Add Tracker for Current Unit",
                tracker.MakeCreateTrackerHandler(mapTab, "CurrentUnit", _ => new MapCurrentUnitsObject(positionAngleProvider)));

            currentItem.DropDownItems.AddHandlerToItem("Add Tracker for Nearby Floor Units",
                tracker.MakeCreateTrackerHandler(mapTab, "NearbyFloorUnits", _ => new MapNearbyFloorUnits(positionAngleProvider)));

            currentItem.DropDownItems.AddHandlerToItem("Add Tracker for Nearby Ceiling Units",
                tracker.MakeCreateTrackerHandler(mapTab, "NearbyCeilingUnits", _ => new MapNearbyCeilingUnits(positionAngleProvider)));

            currentItem.DropDownItems.AddHandlerToItem("Add Tracker for Nearby Objects",
                tracker.MakeCreateTrackerHandler(mapTab, "NearbyObjects", _ => new MapNearbyObjectsObject(name, positionAngleProvider)));

            targetStrip.Items.AddHandlerToItem("Add Tracker for aggregated Path",
                tracker.MakeCreateTrackerHandler(mapTab, "AggregatedPath", _ => new MapPathObject(positionAngleProvider)));
        }

        public static void AddObjectSubTrackers(MapTracker tracker, string name, ContextMenuStrip targetStrip, PositionAngleProvider positionAngleProvider)
        {
            MapTab mapTab = tracker.mapTab;
            var cylindersItem = targetStrip.Items.GetSubItem("Cylinders");
            cylindersItem.DropDownItems.AddHandlerToItem("Add Tracker for Hitbox Cylinders",
                tracker.MakeCreateTrackerHandler(mapTab, "HitboxCylinders", _ => new MapObjectCylinderObject(
                    positionAngleProvider,
                    MapObjectCylinderObject.Dimensions.HitBox,
                    "Hitbox Cylinders")));

            cylindersItem.DropDownItems.AddHandlerToItem("Add Tracker for Effective Hitbox Cylinders",
                tracker.MakeCreateTrackerHandler(mapTab, "EffectiveHitboxCylinders", _ => new MapObjectCylinderObject(
                    positionAngleProvider,
                    MapObjectCylinderObject.Dimensions.EffectiveHitBox,
                    "Effective Hitbox Cylinders")));

            cylindersItem.DropDownItems.AddHandlerToItem("Add Tracker for Hurtbox Cylinders",
                tracker.MakeCreateTrackerHandler(mapTab, "HurtboxCylinders", _ => new MapObjectCylinderObject(
                    positionAngleProvider,
                    MapObjectCylinderObject.Dimensions.HurtBox,
                    "Hurtbox Cylinders")));

            cylindersItem.DropDownItems.AddHandlerToItem("Add Tracker for Effective Hurtbox Cylinders",
                tracker.MakeCreateTrackerHandler(mapTab, "EffectiveHurtboxCylinders", _ => new MapObjectCylinderObject(
                    positionAngleProvider,
                    MapObjectCylinderObject.Dimensions.EffectiveHitBox,
                    "Effective Hurtbox Cylinders")));

            var spheresItem = targetStrip.Items.GetSubItem("Spheres");

            spheresItem.DropDownItems.AddHandlerToItem("Add Tracker for Tangibility Sphere",
                tracker.MakeCreateTrackerHandler(mapTab, "TangibilitySphere", _ => new MapObjectSphereObject(
                    positionAngleProvider,
                    MapObjectSphereObject.Dimensions.Tangibility,
                    "Tangibility Spheres")));

            spheresItem.DropDownItems.AddHandlerToItem("Add Tracker for Draw Distance Sphere",
                tracker.MakeCreateTrackerHandler(mapTab, "DrawDistanceSphere", _ => new MapObjectSphereObject(
                    positionAngleProvider,
                    MapObjectSphereObject.Dimensions.DrawDistance,
                    "Draw Distance Spheres")));

            var anglesItem = targetStrip.Items.GetSubItem("Angles");

            anglesItem.DropDownItems.AddHandlerToItem("Add Tracker for Object Facing Angle",
                tracker.MakeCreateTrackerHandler(mapTab, "ObjectFacingAngle", _ => new MapArrowObject(
                    positionAngleProvider,
                    MapArrowObject.ArrowSource.ObjectFacingYaw,
                    MapArrowObject.ArrowSource.ObjectHSpeed,
                    $"Object Facing Angle {name}")));

            anglesItem.DropDownItems.AddHandlerToItem("Add Tracker for Object Graphics Angle",
                tracker.MakeCreateTrackerHandler(mapTab, "ObjectGraphicsAngle", _ => new MapArrowObject(
                    positionAngleProvider,
                    MapArrowObject.ArrowSource.ObjectGraphicsYaw,
                    MapArrowObject.ArrowSource.ObjectHSpeed,
                    $"Object Graphics Angle {name}")));

            anglesItem.DropDownItems.AddHandlerToItem("Add Tracker for Object Moving Angle",
                tracker.MakeCreateTrackerHandler(mapTab, "ObjectMovingAngle", _ => new MapArrowObject(
                    positionAngleProvider,
                    MapArrowObject.ArrowSource.ObjectMovingYaw,
                    MapArrowObject.ArrowSource.ObjectHSpeed,
                    $"Object Moving Angle {name}")));

            anglesItem.DropDownItems.AddHandlerToItem("Add Tracker for Object Angle to Mario",
                tracker.MakeCreateTrackerHandler(mapTab, "AngleToMario", _ => new MapArrowObject(
                    positionAngleProvider,
                    MapArrowObject.ArrowSource.ObjectAngleToMario,
                    MapArrowObject.ArrowSource.ObjectDistanceToMario,
                    $"Object Angle to Mario for {name}")));

            anglesItem.DropDownItems.AddHandlerToItem("Add Tracker for Home Line",
                tracker.MakeCreateTrackerHandler(mapTab, "HomeLine", _ => new MapHomeLineObject(positionAngleProvider)));

            var collisionItem = targetStrip.Items.GetSubItem("Collision");

            collisionItem.DropDownItems.AddHandlerToItem("Add Tracker for Object Wall Triangles",
                tracker.MakeCreateTrackerHandler(mapTab, "ObjectWallsPredicition", _ => new MapObjectWallPrediction(positionAngleProvider)));

            collisionItem.DropDownItems.AddHandlerToItem("Add Tracker for Object Floor Triangles",
                tracker.MakeCreateTrackerHandler(mapTab, "ObjectFloorsPrediction", _ => new MapObjectFloorPrediction(positionAngleProvider)));

            collisionItem.DropDownItems.AddHandlerToItem("Add Tracker for Object Ceiling Triangles",
                tracker.MakeCreateTrackerHandler(mapTab, "ObjectCeilingsPrediction", _ => new MapObjectCeilingPrediction(positionAngleProvider)));
        }

        protected delegate ObjectDataModel[] ObjectProvider();

        private readonly ObjectProvider objectProvider;

        MapObjectObject() : base(null)
        {
            positionAngleProvider = () => objectProvider().ConvertAndRemoveNull(_ => PositionAngle.Obj(_.Address));
        }

        public MapObjectObject(uint objAddress)
            : this()
        {
            objectProvider = () => new[] { new ObjectDataModel(objAddress) };
        }

        protected MapObjectObject(ObjectProvider objectProvider)
            : this()
        {
            this.objectProvider = objectProvider;
        }

        public override Lazy<Image> GetInternalImage()
        {
            Lazy<Image> result = null;
            foreach (var _obj in objectProvider())
            {
                _obj.Update();
                Lazy<Image> objResult;
                if (_obj.BehaviorAssociation == null)
                    objResult = Config.ObjectAssociations.DefaultImage;
                else
                    objResult = _iconType == MapTrackerIconType.ObjectSlotImage ? _obj.BehaviorAssociation.Image : _obj.BehaviorAssociation.MapImage;

                if (result == null)
                    result = objResult;
                else if (result != objResult)
                    result = Config.ObjectAssociations.DefaultImage;
            }

            return result ?? Config.ObjectAssociations.EmptyImage;
        }

        public override string GetName() => PositionAngle.NameOfMultiple(positionAngleProvider());

        public override void Update()
        {
            base.Update();
            InternalRotates = false;
            foreach (var _obj in objectProvider())
            {
                _obj.Update();
                InternalRotates |= Config.ObjectAssociations.GetObjectMapRotates(_obj.BehaviorCriteria);
            }
        }

        public override void InitSubTrackerContextMenuStrip(MapTab mapTab, ContextMenuStrip targetStrip)
        {
            base.InitSubTrackerContextMenuStrip(mapTab, targetStrip);
            MapObjectObject.AddObjectSubTrackers(tracker,
                GetName(),
                targetStrip,
                () => Array.ConvertAll(objectProvider(), _ => PositionAngle.Obj(_.Address))
            );
        }
    }
}
