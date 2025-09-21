using System.Collections.Generic;
using STROOP.Utilities;
using System;
using System.Linq;
using STROOP.Structs.Configurations;
using STROOP.Structs;
using STROOP.Tabs.BruteforceTab.BF_Utilities;
using GetterFuncsDic = System.Collections.Generic.Dictionary<string, System.Func<STROOP.Tabs.BruteforceTab.ValueGetters.Option>>;

namespace STROOP.Tabs.BruteforceTab
{
    static class ValueGetters
    {
        public class Option
        {
            public readonly string optionName;
            public readonly Func<string, string> moduleVariableGetter;

            public Option(string watchVariableName, Func<string, string> moduleVariableGetter)
            {
                this.optionName = watchVariableName;
                this.moduleVariableGetter = moduleVariableGetter;
            }

            public static implicit operator Option((string watchVariableName, Func<string, string> func) val) => new Option(val.watchVariableName, val.func);
        }

        public class GetterFuncs
        {
            public readonly GetterFuncsDic dic;

            public GetterFuncs(string displayName, GetterFuncsDic dic, Option defaultOption = null)
            {
                this.displayName = displayName;
                this.dic = dic;
                this.defaultOption = defaultOption;
            }

            public readonly string displayName;
            public readonly Option defaultOption;
        }

        public static Dictionary<(string moduleName, string variableName), GetterFuncs> valueGetters;

        static ValueGetters()
        {
            valueGetters = new Dictionary<(string moduleName, string variableName), GetterFuncs>(
                GeneralUtilities.GetEqualityComparer<(string moduleName, string variableName)>(
                    (x, y) => x.variableName == y.variableName && (x.moduleName == null || y.moduleName == null || x.moduleName == y.moduleName),
                    obj => obj.variableName.GetHashCode())
            )
            {
                [(null, "static_tris")] = new GetterFuncs("Static Triangles", new GetterFuncsDic
                    {
                        ["From Map Tracker"] = () => ("From Map Tracker", GetTrackedTriangles),
                        ["All Level Triangles"] = () => ("All Level Triangles", GetLevelTriangles)
                    }, ("All Level Triangles", GetLevelTriangles)
                ),

                [(null, "dynamic_tris")] = GetDynamicTriangles,
                [(null, "behavior_scripts")] = GetObjectsVars,
                [(null, "object_states")] = GetObjectsVars,
                [(null, "dynamic_object_tris")] = GetObjectsVars,

                [(null, "environment_regions")] = new GetterFuncs("Environment Boxes", new GetterFuncsDic
                {
                    ["From Area"] = () => ("From Area", GetEnvironmentRegions)
                }, ("From Area", GetEnvironmentRegions)),

                [("fp_gwk", "plane_nx")] = GetFPGwkVars,
                [("fp_gwk", "plane_nz")] = GetFPGwkVars,
                [("fp_gwk", "plane_d")] = GetFPGwkVars,
                [("fp_gwk", "gwk_angle")] = GetFPGwkVars,

                [("general_purpose", "scoring_methods")] = GetSurfaceVars,
                [("general_purpose", "perturbators")] = GetSurfaceVars,
            };
        }


        static string GetObjectState(uint slot, uint[] behaviorScriptPtrArray, uint[] chosenObjectSlots)
        {
            var obj = Config.ObjectSlotsManager.ObjectSlots[(int)slot - 1].CurrentObject;
            var parentSlot = (uint)Config.ObjectSlotsManager.ObjectSlots.FindIndex(x => x.CurrentObject.Address == obj.Parent) + 1;
            var parentObjectIndex = Array.IndexOf(chosenObjectSlots, parentSlot);
            var rawData = new uint[0x50];
            for (uint i = 0; i < rawData.Length; i++)
                rawData[i] = Config.Stream.GetUInt32(obj.Address + 0x88 + i * 4);
            // TODO: behaviorStack (may be idiotic to do in the first place)
            return $@"
        {{
            ""raw_data"": [{string.Join(", ", rawData.Select(x => $"\"0x{x.ToString("X8")}\""))}],
            ""collided_obj_interact_types"": {Config.Stream.GetUInt32(obj.Address + 0x70)},
            ""active_flags"": {Config.Stream.GetInt16(obj.Address + 0x74)},
            ""num_collided_objs"": {Config.Stream.GetInt16(obj.Address + 0x76)},
            ""behavior_stack_index"": { Config.Stream.GetUInt32(obj.Address + 0x1D0)},
            ""behavior_stack"": [{string.Join(", ", Config.Stream.ReadRam(obj.Address + 0x1D4, 8, EndiannessType.Little))}],
            ""behavior_delay_timer"": {Config.Stream.GetInt16(obj.Address + 0x1F4)},
            ""hitbox_radius"": {Config.Stream.GetSingle(obj.Address + 0x1F8)},
            ""hitbox_height"": {Config.Stream.GetSingle(obj.Address + 0x1FC)},
            ""hurtbox_radius"": {Config.Stream.GetSingle(obj.Address + 0x200)},
            ""hurtbox_height"": {Config.Stream.GetSingle(obj.Address + 0x204)},
            ""hitbox_down_offset"": {Config.Stream.GetSingle(obj.Address + 0x208)},
            ""behavior_script_index"": {Array.IndexOf(behaviorScriptPtrArray, obj.AbsoluteBehavior)},
            ""parent_object_index"": {parentObjectIndex}
        }}";
        }

        static Func<string, string> GetObjectData(uint[] slots) => (string inputName) =>
        {
            var usedBehaviorScriptPtrSet = new HashSet<uint>();
            foreach (var slot in slots)
                usedBehaviorScriptPtrSet.Add(Config.ObjectSlotsManager.ObjectSlots[(int)slot - 1].CurrentObject.AbsoluteBehavior);
            var behaviorScriptPtrArray = usedBehaviorScriptPtrSet.ToArray();

            if (inputName == "object_states")
                return $"[{string.Join(", ", slots.Select(x => GetObjectState(x, behaviorScriptPtrArray, slots)))}]";

            var usedBehaviorScripts = new uint[behaviorScriptPtrArray.Length][];
            var collisionPointerSet = new HashSet<uint>();
            for (int i = 0; i < behaviorScriptPtrArray.Length; i++)
            {
                usedBehaviorScripts[i] = BF_ObjectUtilities.GetAndUnrollBehaviorScript(behaviorScriptPtrArray[i], out var segmentedCollisionPtr);
                if (segmentedCollisionPtr.HasValue)
                    collisionPointerSet.Add(segmentedCollisionPtr.Value);
            }

            switch (inputName)
            {
                case "behavior_scripts":
                    return $"[{string.Join(", ", usedBehaviorScripts.Select(x => $"[{string.Join(", ", x.Select(y => $"\"0x{y.ToString("X8")}\""))}]"))}]";

                case "dynamic_object_tris":
                    return $"[{string.Join(", ", collisionPointerSet.Select(x => BF_ObjectUtilities.GetObjectCollisionOverride(x)))}]";
                default:
                    throw new InvalidOperationException("unreachable");
            }
        };

        static GetterFuncs GetObjectsVars = new GetterFuncs("Object data", new GetterFuncsDic
        {
            ["From slots"] = () =>
            {
                var slots = ParsingUtilities.ParseIntList(DialogUtilities.GetStringFromDialog(labelText: "Enter the object slot numbers:"));
                if (!slots.Any())
                    return ("Nothing", inputName => inputName == "behavior_scripts" ? "[]" : "{}");
                return (
                    $"Slots [{string.Join("; ", slots.Where((int? slot) => slot != null).Select(slot => slot.Value.ToString()))}]",
                    GetObjectData(slots.Where(x => x.HasValue).Select(x => (uint)x.Value).ToArray())
                );
            }
        });

        static GetterFuncs GetSurfaceVars = new GetterFuncs(null, new GetterFuncsDic { ["From Surface"] = () => ("From Surface", GetScoringFuncs) });
        static string GetScoringFuncs(string inputName) => AccessScope<BruteforceTab>.content.surface?.GetParameter(inputName) ?? "\"\"";

        static string GetEnvironmentRegions(string inputName)
        {
            var strBuilder = new System.Text.StringBuilder();
            uint waterAddress = Config.Stream.GetUInt32(MiscConfig.WaterPointerAddress);
            int numWaterLevels = waterAddress == 0 ? 0 : Config.Stream.GetInt16(waterAddress);
            strBuilder.Append($"[{numWaterLevels}");
            for (int i = 0; i < numWaterLevels; i++)
                for (int k = 0; k < 6; k++) // each environment box consists of six s16
                {
                    waterAddress += 2;
                    strBuilder.Append($", {Config.Stream.GetInt16(waterAddress)}");
                }

            strBuilder.Append("]");
            return strBuilder.ToString();
        }

        static string GetTrackedTriangles(string inputName)
        {
            MapTab.MapTab tab = AccessScope<StroopMainForm>.content.GetTab<MapTab.MapTab>();
            if (tab == null)
                return "";
            foreach (var tracker in tab.EnumerateTrackers())
                if (tracker.mapObject is MapTab.MapObjects.MapBruteforceTriangles trisObject)
                    return trisObject.GetJsonString();
            return "";
        }

        static string GetLevelTriangles(string inputName) => TriangleUtilities.ToJsonString(TriangleUtilities.GetLevelTriangleAddresses());

        static GetterFuncs GetDynamicTriangles = new GetterFuncs("Dynamic Triangles", new GetterFuncsDic
        {
            ["From Objects..."] = () =>
            {
                var slots = ParsingUtilities.ParseIntList(DialogUtilities.GetStringFromDialog(labelText: "Enter the object slot numbers:"));
                return (
                    $"Slots [{string.Join("; ", slots.Where((int? slot) => slot != null).Select(slot => slot.Value.ToString()))}]",
                    var =>
                    {
                        var triangles = new List<Models.TriangleDataModel>();
                        foreach (var slot in slots)
                            if (slot > 0 && slot <= Config.ObjectSlotsManager.ObjectSlots.Count)
                                triangles.AddRange(new MapTab.DataUtil.ObjectTrianglePrediction(() => new PositionAngle[] { Config.ObjectSlotsManager.ObjectSlots[slot.Value - 1].CurrentObject }, _ => true).GetTriangles());
                        return TriangleUtilities.ToJsonString(triangles);
                    }
                );
            }
        });

        static GetterFuncs GetFPGwkVars = new GetterFuncs("Gwk Triangle", new GetterFuncsDic
        {
            ["From Triangle Address..."] = () =>
            {
                if (ParsingUtilities.TryParseHex(DialogUtilities.GetStringFromDialog(labelText: "Enter the triangle address:"), out var triAddress))
                    return ($"Tri@0x{triAddress.ToString("X8")}", var => GetFPGwkVarFromAddress(triAddress, var));
                return ("[Invalid]", var => "0");
            }
        });

        static string GetFPGwkVarFromAddress(uint address, string inputName)
        {
            var tri = Models.TriangleDataModel.Create(address);
            switch (inputName)
            {
                case "plane_nx":
                    return ((double)tri.NormX).ToString();
                case "plane_nz":
                    return ((double)tri.NormZ).ToString();
                case "plane_d":
                    return ((double)tri.NormOffset).ToString();
                case "gwk_angle":
                    ushort marioAngle = Config.Stream.GetUInt16(MarioConfig.StructAddress + MarioConfig.FacingYawOffset);
                    ushort wallAngle = InGameTrigUtilities.InGameATan(tri.NormZ, tri.NormX);
                    return MoreMath.NormalizeAngleUshort(wallAngle - (marioAngle - wallAngle) + 32768).ToString();
                default:
                    return "?";
            }
        }
    }
}
