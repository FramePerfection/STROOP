using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using STROOP.Controls.VariablePanel;
using STROOP.Core;
using STROOP.Core.Utilities;
using STROOP.Models;
using STROOP.Structs.Configurations;
using STROOP.Ttc;
using STROOP.Structs;
using STROOP.Tabs;
using STROOP.Variables;
using STROOP.Variables.SM64MemoryLayout;
using STROOP.Variables.Utilities;
using STROOP.Variables.Views;

namespace STROOP.Utilities
{
    public static class WatchVariableSpecialUtilities
    {

        public static WatchVariableSpecialDictionary dictionary { get; private set; }

        static WatchVariableSpecialUtilities()
        {
            dictionary = new WatchVariableSpecialDictionary();
            AddLiteralEntriesToDictionary();
            AddGeneratedEntriesToDictionary();
            AddPanEntriesToDictionary();
            GeneralUtilities.ExecuteInitializers<InitializeSpecialAttribute>();
        }

        static IEnumerable<IEnumerable<double>> FilterNumberVariables(List<WatchVariableControl> controls)
        {
            foreach (var ctrl in controls)
            {
                if (ctrl.view is IVariableView<byte> byteView) yield return byteView._getterFunction().Select(x => (double)x);
                else if (ctrl.view is IVariableView<sbyte> sbyteView) yield return sbyteView._getterFunction().Select(x => (double)x);
                else if (ctrl.view is IVariableView<ushort> ushortView) yield return ushortView._getterFunction().Select(x => (double)x);
                else if (ctrl.view is IVariableView<short> shortView) yield return shortView._getterFunction().Select(x => (double)x);
                else if (ctrl.view is IVariableView<uint> uintView) yield return uintView._getterFunction().Select(x => (double)x);
                else if (ctrl.view is IVariableView<int> intView) yield return intView._getterFunction().Select(x => (double)x);
                else if (ctrl.view is IVariableView<ulong> ulongView) yield return ulongView._getterFunction().Select(x => (double)x);
                else if (ctrl.view is IVariableView<long> longView) yield return longView._getterFunction().Select(x => (double)x);
                else if (ctrl.view is IVariableView<float> floatView) yield return floatView._getterFunction().Select(x => (double)x);
                else if (ctrl.view is IVariableView<double> doubleView) yield return doubleView._getterFunction().Select(x => (double)x);
                else if (ctrl.view is IVariableView<decimal> decmialView) yield return decmialView._getterFunction().Select(x => (double)x);
            }
        }

        static IEnumerable<double> CrossOperationOnControls(List<WatchVariableControl> controls, Func<IEnumerable<double>, double> op)
            => CrossOperation(op, FilterNumberVariables(controls).ToArray());

        static IEnumerable<TResult> CrossOperation<TInput, TResult>(Func<IEnumerable<TInput>, TResult> op, params IEnumerable<TInput>[] variableStreams)
        {
            int index;
            var streamEnumerators = variableStreams.Select(x => x.GetEnumerator());
            do
            {
                var nextResult = new List<TInput>(variableStreams.Length);
                index = 0;
                foreach (var enumerator in streamEnumerators)
                    if (enumerator.MoveNext())
                        nextResult.Add(enumerator.Current);
                yield return op(nextResult);
            } while (index > 0);
        }

        public static GetterFunction<double> AddAggregateMathOperationEntry(List<WatchVariableControl> controls, AggregateMathOperation operation)
        {
            switch (operation)
            {
                case AggregateMathOperation.Mean:
                    return () => CrossOperationOnControls(controls, Enumerable.Average);
                case AggregateMathOperation.Median:
                    return () => CrossOperationOnControls(controls, x =>
                    {
                        var doubleValues = x.ToList();
                        doubleValues.Sort();
                        return (doubleValues.Count % 2 == 1)
                            ? doubleValues[doubleValues.Count / 2]
                            : (doubleValues[doubleValues.Count / 2 - 1] + doubleValues[doubleValues.Count / 2]) / 2;
                    });
                case AggregateMathOperation.Min:
                    return () => CrossOperationOnControls(controls, Enumerable.Min);
                case AggregateMathOperation.Max:
                    return () => CrossOperationOnControls(controls, Enumerable.Max);
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private static Func<IEnumerable<TElement>, TResult> Unroll2ElementsGetter<TElement, TResult>(Func<TElement, TElement, TResult> getter, TResult defaultValue)
            => elements =>
            {
                if (elements.Count() == 2)
                    return getter(elements.First(), elements.ElementAt(1));
                return defaultValue;
            };

        private static Func<IEnumerable<TElement>, TValue, bool> Unroll2ElementsSetter<TElement, TValue>(Func<TElement, TElement, TValue, bool> setter)
            => (elements, value) =>
            {
                if (elements.Count() == 2)
                    return setter(elements.First(), elements.ElementAt(1), value);
                return false;
            };

        private static (
            string type,
            Func<IEnumerable<TElement>, TValue> getter,
            Func<IEnumerable<TElement>, TValue, bool> setter
            ) DeclareGetterSetter<TElement, TValue>(string type, Func<TElement, TElement, TValue> getter, Func<TElement, TElement, TValue, bool> setter, TValue defaultValue)
            => (type, Unroll2ElementsGetter(getter, defaultValue), Unroll2ElementsSetter(setter));

        public static readonly (
            string type,
            Func<IEnumerable<PositionAngle>, double> getter,
            Func<IEnumerable<PositionAngle>, double, bool> setter
            )[] distFuncs =
                new (
                    string type,
                    Func<IEnumerable<PositionAngle>, double> getter,
                    Func<IEnumerable<PositionAngle>, double, bool> setter
                    )[]
                    {
                        DeclareGetterSetter<PositionAngle, double>("X", PositionAngle.GetXDistance, PositionAngle.SetXDistance, double.NaN),
                        DeclareGetterSetter<PositionAngle, double>("Y", PositionAngle.GetYDistance, PositionAngle.SetYDistance, double.NaN),
                        DeclareGetterSetter<PositionAngle, double>("Z", PositionAngle.GetZDistance, PositionAngle.SetZDistance, double.NaN),
                        DeclareGetterSetter<PositionAngle, double>("H", PositionAngle.GetHDistance, PositionAngle.SetHDistance, double.NaN),
                        DeclareGetterSetter<PositionAngle, double>("", PositionAngle.GetDistance, PositionAngle.SetDistance, double.NaN),
                        DeclareGetterSetter<PositionAngle, double>("F", PositionAngle.GetFDistance, PositionAngle.SetFDistance, double.NaN),
                        DeclareGetterSetter<PositionAngle, double>("S", PositionAngle.GetSDistance, PositionAngle.SetSDistance, double.NaN),
                    };

        public static void AddGeneratedEntriesToDictionary()
        {
            // TODO: These should be more generic
            List<Func<IEnumerable<PositionAngle>>> posAngleFuncs =
                new List<Func<IEnumerable<PositionAngle>>>()
                {
                    () => PositionAngle.Mario.Yield(),
                    () => PositionAngle.Holp.Yield(),
                    () => PositionAngle.Camera.Yield(),
                    () => WatchVariableUtilities.GetBaseAddresses(BaseAddressType.Object).Select(x => PositionAngle.Obj(x)),
                    () => WatchVariableUtilities.GetBaseAddresses(BaseAddressType.Object).Select(x => PositionAngle.ObjHome(x)),
                    () => WatchVariableUtilities.GetBaseAddresses(BaseAddressType.Triangle).Select(x => PositionAngle.Tri(x, 1)),
                    () => WatchVariableUtilities.GetBaseAddresses(BaseAddressType.Triangle).Select(x => PositionAngle.Tri(x, 2)),
                    () => WatchVariableUtilities.GetBaseAddresses(BaseAddressType.Triangle).Select(x => PositionAngle.Tri(x, 3)),
                };

            List<string> posAngleStrings =
                new List<string>()
                {
                    "Mario",
                    "Holp",
                    "Camera",
                    "Obj",
                    "ObjHome",
                    "TriV1",
                    "TriV2",
                    "TriV3",
                };

            for (int i = 0; i < posAngleFuncs.Count; i++)
            {
                Func<IEnumerable<PositionAngle>> func1 = posAngleFuncs[i];
                string string1 = posAngleStrings[i];

                for (int j = 0; j < posAngleFuncs.Count; j++)
                {
                    if (j == i) continue;
                    Func<IEnumerable<PositionAngle>> func2 = posAngleFuncs[j];
                    string string2 = posAngleStrings[j];

                    for (int k = 0; k < distFuncs.Length; k++)
                    {
                        var getter = distFuncs[k].getter;
                        var setter = distFuncs[k].setter;
                        dictionary.Add($"{distFuncs[k].type}Dist{string1}To{string2}",
                            () => CrossOperation(getter, func1(), func2()),
                            (double dist) => CrossOperation(x => setter(x, dist), func1(), func2())
                        );
                    }

                    dictionary.Add($"Angle{string1}To{string2}",
                        () => CrossOperation(Unroll2ElementsGetter<PositionAngle, double>(PositionAngle.GetAngleTo, double.NaN), func1(), func2()),
                        (double angle) => CrossOperation(x => Unroll2ElementsSetter<PositionAngle, double>(PositionAngle.SetAngleTo)(x, angle), func1(), func2())
                    );

                    dictionary.Add($"DAngle{string1}To{string2}",
                        () => CrossOperation(Unroll2ElementsGetter<PositionAngle, double>(PositionAngle.GetDAngleTo, double.NaN), func1(), func2()),
                        (double angleDiff) => CrossOperation(x => Unroll2ElementsSetter<PositionAngle, double>(PositionAngle.SetDAngleTo)(x, angleDiff), func1(), func2())
                    );

                    dictionary.Add($"AngleDiff{string1}To{string2}",
                        () => CrossOperation(Unroll2ElementsGetter<PositionAngle, double>(PositionAngle.GetAngleDifference, double.NaN), func1(), func2()),
                        (double angleDiff) => CrossOperation(x => Unroll2ElementsSetter<PositionAngle, double>(PositionAngle.SetAngleDifference)(x, angleDiff), func1(), func2())
                    );
                }
            }
        }

        public static void AddPanEntriesToDictionary()
        {
            List<(string, Func<double>, Action<double>)> entries =
                new List<(string, Func<double>, Action<double>)>()
                {
                    ("NumPans", () => SpecialConfig.NumPans, (double value) => SpecialConfig.NumPans = value),
                    ("CurrentPan", () => SpecialConfig.CurrentPan, (double value) => { }),
                    ("PanCamPos", () => SpecialConfig.PanCamPos, (double value) => SpecialConfig.PanCamPos = value),
                    ("PanCamAngle", () => SpecialConfig.PanCamAngle, (double value) => SpecialConfig.PanCamAngle = value),
                    ("PanCamRotation", () => SpecialConfig.PanCamRotation, (double value) => SpecialConfig.PanCamRotation = value),
                    ("PanFOV", () => SpecialConfig.PanFOV, (double value) => SpecialConfig.PanFOV = value),
                };

            foreach ((string key, Func<double> getter, Action<double> setter) in entries)
            {
                dictionary.Add(key,
                    getter,
                    (double doubleValue) =>
                    {
                        setter(doubleValue);
                        return true;
                    }
                );
            }
        }

        public static void AddPanEntriesToDictionary(int index)
        {
            List<(string, Func<double>, Action<double>)> entries =
                new List<(string, Func<double>, Action<double>)>()
                {
                    (String.Format("Pan{0}GlobalTimer", index), () => SpecialConfig.PanModels[index].PanGlobalTimer, (double value) => SpecialConfig.PanModels[index].PanGlobalTimer = value),
                    (String.Format("Pan{0}StartTime", index), () => SpecialConfig.PanModels[index].PanStartTime, (double value) => SpecialConfig.PanModels[index].PanStartTime = value),
                    (String.Format("Pan{0}EndTime", index), () => SpecialConfig.PanModels[index].PanEndTime, (double value) => SpecialConfig.PanModels[index].PanEndTime = value),
                    (String.Format("Pan{0}Duration", index), () => SpecialConfig.PanModels[index].PanDuration, (double value) => SpecialConfig.PanModels[index].PanDuration = value),

                    (String.Format("Pan{0}EaseStart", index), () => SpecialConfig.PanModels[index].PanEaseStart, (double value) => SpecialConfig.PanModels[index].PanEaseStart = value),
                    (String.Format("Pan{0}EaseEnd", index), () => SpecialConfig.PanModels[index].PanEaseEnd, (double value) => SpecialConfig.PanModels[index].PanEaseEnd = value),
                    (String.Format("Pan{0}EaseDegree", index), () => SpecialConfig.PanModels[index].PanEaseDegree, (double value) => SpecialConfig.PanModels[index].PanEaseDegree = value),

                    (String.Format("Pan{0}RotateCW", index), () => SpecialConfig.PanModels[index].PanRotateCW, (double value) => SpecialConfig.PanModels[index].PanRotateCW = value),

                    (String.Format("Pan{0}CamStartX", index), () => SpecialConfig.PanModels[index].PanCamStartX, (double value) => SpecialConfig.PanModels[index].PanCamStartX = value),
                    (String.Format("Pan{0}CamStartY", index), () => SpecialConfig.PanModels[index].PanCamStartY, (double value) => SpecialConfig.PanModels[index].PanCamStartY = value),
                    (String.Format("Pan{0}CamStartZ", index), () => SpecialConfig.PanModels[index].PanCamStartZ, (double value) => SpecialConfig.PanModels[index].PanCamStartZ = value),
                    (String.Format("Pan{0}CamStartYaw", index), () => SpecialConfig.PanModels[index].PanCamStartYaw, (double value) => SpecialConfig.PanModels[index].PanCamStartYaw = value),
                    (String.Format("Pan{0}CamStartPitch", index), () => SpecialConfig.PanModels[index].PanCamStartPitch, (double value) => SpecialConfig.PanModels[index].PanCamStartPitch = value),

                    (String.Format("Pan{0}CamEndX", index), () => SpecialConfig.PanModels[index].PanCamEndX, (double value) => SpecialConfig.PanModels[index].PanCamEndX = value),
                    (String.Format("Pan{0}CamEndY", index), () => SpecialConfig.PanModels[index].PanCamEndY, (double value) => SpecialConfig.PanModels[index].PanCamEndY = value),
                    (String.Format("Pan{0}CamEndZ", index), () => SpecialConfig.PanModels[index].PanCamEndZ, (double value) => SpecialConfig.PanModels[index].PanCamEndZ = value),
                    (String.Format("Pan{0}CamEndYaw", index), () => SpecialConfig.PanModels[index].PanCamEndYaw, (double value) => SpecialConfig.PanModels[index].PanCamEndYaw = value),
                    (String.Format("Pan{0}CamEndPitch", index), () => SpecialConfig.PanModels[index].PanCamEndPitch, (double value) => SpecialConfig.PanModels[index].PanCamEndPitch = value),

                    (String.Format("Pan{0}RadiusStart", index), () => SpecialConfig.PanModels[index].PanRadiusStart, (double value) => SpecialConfig.PanModels[index].PanRadiusStart = value),
                    (String.Format("Pan{0}RadiusEnd", index), () => SpecialConfig.PanModels[index].PanRadiusEnd, (double value) => SpecialConfig.PanModels[index].PanRadiusEnd = value),

                    (String.Format("Pan{0}FOVStart", index), () => SpecialConfig.PanModels[index].PanFOVStart, (double value) => SpecialConfig.PanModels[index].PanFOVStart = value),
                    (String.Format("Pan{0}FOVEnd", index), () => SpecialConfig.PanModels[index].PanFOVEnd, (double value) => SpecialConfig.PanModels[index].PanFOVEnd = value),
                };

            foreach ((string key, Func<double> getter, Action<double> setter) in entries)
            {
                dictionary.Add(key, getter,
                    (double doubleValue) =>
                    {
                        setter(doubleValue);
                        return true;
                    }
                );
            }
        }

        public static void AddLiteralEntriesToDictionary()
        {
            // Object vars

            dictionary.Add("DAngleMarioToObjMod512",
                BaseAddressType.Object,
                objAddress =>
                {
                    double dAngle = PositionAngle.GetDAngleTo(PositionAngle.Mario, PositionAngle.Obj(objAddress));
                    return MoreMath.MaybeNegativeModulus(dAngle, 512);
                },
                SpecialVariableDefaults<double>.DEFAULT_SETTER_WITH_ADDRESS);

            dictionary.Add("PitchMarioToObj",
                BaseAddressType.Object,
                (uint objAddress) =>
                {
                    PositionAngle mario = PositionAngle.Mario;
                    PositionAngle obj = PositionAngle.Obj(objAddress);
                    return MoreMath.GetPitch(mario.X, mario.Y, mario.Z, obj.X, obj.Y, obj.Z);
                },
                SpecialVariableDefaults<double>.DEFAULT_SETTER_WITH_ADDRESS);

            dictionary.Add("DPitchMarioToObj",
                BaseAddressType.Object,
                objAddress =>
                {
                    PositionAngle mario = PositionAngle.Mario;
                    PositionAngle obj = PositionAngle.Obj(objAddress);
                    double pitch = MoreMath.GetPitch(mario.X, mario.Y, mario.Z, obj.X, obj.Y, obj.Z);
                    ushort marioPitch = Config.Stream.GetUInt16(MarioConfig.StructAddress + MarioConfig.FacingPitchOffset);
                    return marioPitch - pitch;
                },
                (double diff, uint objAddress) =>
                {
                    PositionAngle mario = PositionAngle.Mario;
                    PositionAngle obj = PositionAngle.Obj(objAddress);
                    double pitch = MoreMath.GetPitch(mario.X, mario.Y, mario.Z, obj.X, obj.Y, obj.Z);
                    short newMarioPitch = MoreMath.NormalizeAngleShort(pitch + diff);
                    return Config.Stream.SetValue(newMarioPitch, MarioConfig.StructAddress + MarioConfig.FacingPitchOffset);
                }
            );

            dictionary.Add("ObjectInGameDeltaYaw",
                BaseAddressType.Object,
                objAddress => GetDeltaInGameAngle(Config.Stream.GetUInt16(objAddress + ObjectConfig.YawFacingOffset)),
                SpecialVariableDefaults<int>.DEFAULT_SETTER_WITH_ADDRESS);

            dictionary.Add("EffectiveHitboxRadius",
                BaseAddressType.Object,
                objAddress =>
                {
                    uint marioObjRef = Config.Stream.GetUInt32(MarioObjectConfig.PointerAddress);
                    float mObjHitboxRadius = Config.Stream.GetSingle(marioObjRef + ObjectConfig.HitboxRadiusOffset);
                    float objHitboxRadius = Config.Stream.GetSingle(objAddress + ObjectConfig.HitboxRadiusOffset);
                    return mObjHitboxRadius + objHitboxRadius;
                },
                SpecialVariableDefaults<float>.DEFAULT_SETTER_WITH_ADDRESS);

            dictionary.Add("EffectiveHurtboxRadius",
                BaseAddressType.Object,
                objAddress =>
                {
                    uint marioObjRef = Config.Stream.GetUInt32(MarioObjectConfig.PointerAddress);
                    float mObjHurtboxRadius = Config.Stream.GetSingle(marioObjRef + ObjectConfig.HurtboxRadiusOffset);
                    float objHurtboxRadius = Config.Stream.GetSingle(objAddress + ObjectConfig.HurtboxRadiusOffset);
                    return mObjHurtboxRadius + objHurtboxRadius;
                },
                SpecialVariableDefaults<float>.DEFAULT_SETTER_WITH_ADDRESS);

            dictionary.Add("MarioHitboxAwayFromObject",
                BaseAddressType.Object,
                objAddress =>
                {
                    uint marioObjRef = Config.Stream.GetUInt32(MarioObjectConfig.PointerAddress);
                    float mObjX = Config.Stream.GetSingle(marioObjRef + ObjectConfig.XOffset);
                    float mObjZ = Config.Stream.GetSingle(marioObjRef + ObjectConfig.ZOffset);
                    float mObjHitboxRadius = Config.Stream.GetSingle(marioObjRef + ObjectConfig.HitboxRadiusOffset);

                    float objX = Config.Stream.GetSingle(objAddress + ObjectConfig.XOffset);
                    float objZ = Config.Stream.GetSingle(objAddress + ObjectConfig.ZOffset);
                    float objHitboxRadius = Config.Stream.GetSingle(objAddress + ObjectConfig.HitboxRadiusOffset);

                    double marioHitboxAwayFromObject = MoreMath.GetDistanceBetween(mObjX, mObjZ, objX, objZ) - mObjHitboxRadius - objHitboxRadius;
                    return marioHitboxAwayFromObject;
                },
                (double hitboxDistAway, uint objAddress) =>
                {
                    uint marioObjRef = Config.Stream.GetUInt32(MarioObjectConfig.PointerAddress);
                    float mObjX = Config.Stream.GetSingle(marioObjRef + ObjectConfig.XOffset);
                    float mObjZ = Config.Stream.GetSingle(marioObjRef + ObjectConfig.ZOffset);
                    float mObjHitboxRadius = Config.Stream.GetSingle(marioObjRef + ObjectConfig.HitboxRadiusOffset);

                    float objX = Config.Stream.GetSingle(objAddress + ObjectConfig.XOffset);
                    float objZ = Config.Stream.GetSingle(objAddress + ObjectConfig.ZOffset);
                    float objHitboxRadius = Config.Stream.GetSingle(objAddress + ObjectConfig.HitboxRadiusOffset);

                    PositionAngle marioPos = PositionAngle.Mario;
                    PositionAngle objPos = PositionAngle.Obj(objAddress);
                    double distAway = hitboxDistAway + mObjHitboxRadius + objHitboxRadius;

                    (double newMarioX, double newMarioZ) =
                        MoreMath.ExtrapolateLine2D(objPos.X, objPos.Z, marioPos.X, marioPos.Z, distAway);
                    return BoolUtilities.Combine(
                        marioPos.SetValues(x: newMarioX, z: newMarioZ),
                        PositionAngle.MarioObj().SetValues(x: newMarioX, z: newMarioZ)
                    );
                }
            );

            dictionary.Add("MarioHitboxAboveObject",
                BaseAddressType.Object,
                objAddress =>
                {
                    uint marioObjRef = Config.Stream.GetUInt32(MarioObjectConfig.PointerAddress);
                    float mObjY = Config.Stream.GetSingle(marioObjRef + ObjectConfig.YOffset);
                    float mObjHitboxHeight = Config.Stream.GetSingle(marioObjRef + ObjectConfig.HitboxHeightOffset);
                    float mObjHitboxDownOffset = Config.Stream.GetSingle(marioObjRef + ObjectConfig.HitboxDownOffsetOffset);
                    float mObjHitboxBottom = mObjY - mObjHitboxDownOffset;

                    float objY = Config.Stream.GetSingle(objAddress + ObjectConfig.YOffset);
                    float objHitboxHeight = Config.Stream.GetSingle(objAddress + ObjectConfig.HitboxHeightOffset);
                    float objHitboxDownOffset = Config.Stream.GetSingle(objAddress + ObjectConfig.HitboxDownOffsetOffset);
                    float objHitboxTop = objY + objHitboxHeight - objHitboxDownOffset;

                    double marioHitboxAboveObject = mObjHitboxBottom - objHitboxTop;
                    return marioHitboxAboveObject;
                },
                (double hitboxDistAbove, uint objAddress) =>
                {
                    uint marioObjRef = Config.Stream.GetUInt32(MarioObjectConfig.PointerAddress);
                    float mObjY = Config.Stream.GetSingle(marioObjRef + ObjectConfig.YOffset);
                    float mObjHitboxDownOffset = Config.Stream.GetSingle(marioObjRef + ObjectConfig.HitboxDownOffsetOffset);

                    float objY = Config.Stream.GetSingle(objAddress + ObjectConfig.YOffset);
                    float objHitboxHeight = Config.Stream.GetSingle(objAddress + ObjectConfig.HitboxHeightOffset);
                    float objHitboxDownOffset = Config.Stream.GetSingle(objAddress + ObjectConfig.HitboxDownOffsetOffset);
                    float objHitboxTop = objY + objHitboxHeight - objHitboxDownOffset;

                    double newMarioY = objHitboxTop + mObjHitboxDownOffset + hitboxDistAbove;
                    return BoolUtilities.Combine(
                        PositionAngle.Mario.SetY(newMarioY),
                        PositionAngle.MarioObj().SetY(newMarioY)
                    );
                }
            );

            dictionary.Add("MarioHitboxBelowObject",
                BaseAddressType.Object,
                objAddress =>
                {
                    uint marioObjRef = Config.Stream.GetUInt32(MarioObjectConfig.PointerAddress);
                    float mObjY = Config.Stream.GetSingle(marioObjRef + ObjectConfig.YOffset);
                    float mObjHitboxHeight = Config.Stream.GetSingle(marioObjRef + ObjectConfig.HitboxHeightOffset);
                    float mObjHitboxDownOffset = Config.Stream.GetSingle(marioObjRef + ObjectConfig.HitboxDownOffsetOffset);
                    float mObjHitboxTop = mObjY + mObjHitboxHeight - mObjHitboxDownOffset;

                    float objY = Config.Stream.GetSingle(objAddress + ObjectConfig.YOffset);
                    float objHitboxHeight = Config.Stream.GetSingle(objAddress + ObjectConfig.HitboxHeightOffset);
                    float objHitboxDownOffset = Config.Stream.GetSingle(objAddress + ObjectConfig.HitboxDownOffsetOffset);
                    float objHitboxBottom = objY - objHitboxDownOffset;

                    double marioHitboxBelowObject = objHitboxBottom - mObjHitboxTop;
                    return marioHitboxBelowObject;
                },
                (double hitboxDistBelow, uint objAddress) =>
                {
                    uint marioObjRef = Config.Stream.GetUInt32(MarioObjectConfig.PointerAddress);
                    float mObjY = Config.Stream.GetSingle(marioObjRef + ObjectConfig.YOffset);
                    float mObjHitboxHeight = Config.Stream.GetSingle(marioObjRef + ObjectConfig.HitboxHeightOffset);
                    float mObjHitboxDownOffset = Config.Stream.GetSingle(marioObjRef + ObjectConfig.HitboxDownOffsetOffset);
                    float mObjHitboxTop = mObjY + mObjHitboxHeight - mObjHitboxDownOffset;

                    float objY = Config.Stream.GetSingle(objAddress + ObjectConfig.YOffset);
                    float objHitboxHeight = Config.Stream.GetSingle(objAddress + ObjectConfig.HitboxHeightOffset);
                    float objHitboxDownOffset = Config.Stream.GetSingle(objAddress + ObjectConfig.HitboxDownOffsetOffset);
                    float objHitboxBottom = objY - objHitboxDownOffset;

                    double newMarioY = objHitboxBottom - (mObjHitboxTop - mObjY) - hitboxDistBelow;
                    return BoolUtilities.Combine(
                        PositionAngle.Mario.SetY(newMarioY),
                        PositionAngle.MarioObj().SetY(newMarioY)
                    );
                }
            );

            dictionary.Add("MarioHitboxOverlapsObject",
                BaseAddressType.Object,
                objAddress =>
                {
                    uint marioObjRef = Config.Stream.GetUInt32(MarioObjectConfig.PointerAddress);
                    float mObjX = Config.Stream.GetSingle(marioObjRef + ObjectConfig.XOffset);
                    float mObjY = Config.Stream.GetSingle(marioObjRef + ObjectConfig.YOffset);
                    float mObjZ = Config.Stream.GetSingle(marioObjRef + ObjectConfig.ZOffset);
                    float mObjHitboxRadius = Config.Stream.GetSingle(marioObjRef + ObjectConfig.HitboxRadiusOffset);
                    float mObjHitboxHeight = Config.Stream.GetSingle(marioObjRef + ObjectConfig.HitboxHeightOffset);
                    float mObjHitboxDownOffset = Config.Stream.GetSingle(marioObjRef + ObjectConfig.HitboxDownOffsetOffset);
                    float mObjHitboxBottom = mObjY - mObjHitboxDownOffset;
                    float mObjHitboxTop = mObjY + mObjHitboxHeight - mObjHitboxDownOffset;

                    float objX = Config.Stream.GetSingle(objAddress + ObjectConfig.XOffset);
                    float objY = Config.Stream.GetSingle(objAddress + ObjectConfig.YOffset);
                    float objZ = Config.Stream.GetSingle(objAddress + ObjectConfig.ZOffset);
                    float objHitboxRadius = Config.Stream.GetSingle(objAddress + ObjectConfig.HitboxRadiusOffset);
                    float objHitboxHeight = Config.Stream.GetSingle(objAddress + ObjectConfig.HitboxHeightOffset);
                    float objHitboxDownOffset = Config.Stream.GetSingle(objAddress + ObjectConfig.HitboxDownOffsetOffset);
                    float objHitboxBottom = objY - objHitboxDownOffset;
                    float objHitboxTop = objY + objHitboxHeight - objHitboxDownOffset;

                    double marioHitboxAwayFromObject = MoreMath.GetDistanceBetween(mObjX, mObjZ, objX, objZ) - mObjHitboxRadius - objHitboxRadius;
                    double marioHitboxAboveObject = mObjHitboxBottom - objHitboxTop;
                    double marioHitboxBelowObject = objHitboxBottom - mObjHitboxTop;

                    return marioHitboxAwayFromObject < 0 && marioHitboxAboveObject <= 0 && marioHitboxBelowObject <= 0;
                },
                SpecialVariableDefaults<bool>.DEFAULT_SETTER_WITH_ADDRESS);

            dictionary.Add("MarioHurtboxAwayFromObject",
                BaseAddressType.Object,
                objAddress =>
                {
                    uint marioObjRef = Config.Stream.GetUInt32(MarioObjectConfig.PointerAddress);
                    float mObjX = Config.Stream.GetSingle(marioObjRef + ObjectConfig.XOffset);
                    float mObjZ = Config.Stream.GetSingle(marioObjRef + ObjectConfig.ZOffset);
                    float mObjHurtboxRadius = Config.Stream.GetSingle(marioObjRef + ObjectConfig.HurtboxRadiusOffset);

                    float objX = Config.Stream.GetSingle(objAddress + ObjectConfig.XOffset);
                    float objZ = Config.Stream.GetSingle(objAddress + ObjectConfig.ZOffset);
                    float objHurtboxRadius = Config.Stream.GetSingle(objAddress + ObjectConfig.HurtboxRadiusOffset);

                    double marioHurtboxAwayFromObject = MoreMath.GetDistanceBetween(mObjX, mObjZ, objX, objZ) - mObjHurtboxRadius - objHurtboxRadius;
                    return marioHurtboxAwayFromObject;
                },
                (double hurtboxDistAway, uint objAddress) =>
                {
                    uint marioObjRef = Config.Stream.GetUInt32(MarioObjectConfig.PointerAddress);
                    float mObjX = Config.Stream.GetSingle(marioObjRef + ObjectConfig.XOffset);
                    float mObjZ = Config.Stream.GetSingle(marioObjRef + ObjectConfig.ZOffset);
                    float mObjHurtboxRadius = Config.Stream.GetSingle(marioObjRef + ObjectConfig.HurtboxRadiusOffset);

                    float objX = Config.Stream.GetSingle(objAddress + ObjectConfig.XOffset);
                    float objZ = Config.Stream.GetSingle(objAddress + ObjectConfig.ZOffset);
                    float objHurtboxRadius = Config.Stream.GetSingle(objAddress + ObjectConfig.HurtboxRadiusOffset);

                    PositionAngle marioPos = PositionAngle.Mario;
                    PositionAngle objPos = PositionAngle.Obj(objAddress);
                    double distAway = hurtboxDistAway + mObjHurtboxRadius + objHurtboxRadius;

                    (double newMarioX, double newMarioZ) =
                        MoreMath.ExtrapolateLine2D(objPos.X, objPos.Z, marioPos.X, marioPos.Z, distAway);
                    return BoolUtilities.Combine(
                        marioPos.SetValues(x: newMarioX, z: newMarioZ),
                        PositionAngle.MarioObj().SetValues(x: newMarioX, z: newMarioZ)
                    );
                }
            );

            dictionary.Add("MarioHurtboxAboveObject",
                BaseAddressType.Object,
                objAddress =>
                {
                    uint marioObjRef = Config.Stream.GetUInt32(MarioObjectConfig.PointerAddress);
                    float mObjY = Config.Stream.GetSingle(marioObjRef + ObjectConfig.YOffset);
                    float mObjHitboxDownOffset = Config.Stream.GetSingle(marioObjRef + ObjectConfig.HitboxDownOffsetOffset);
                    float mObjHurtboxBottom = mObjY - mObjHitboxDownOffset;

                    float objY = Config.Stream.GetSingle(objAddress + ObjectConfig.YOffset);
                    float objHurtboxHeight = Config.Stream.GetSingle(objAddress + ObjectConfig.HurtboxHeightOffset);
                    float objHitboxDownOffset = Config.Stream.GetSingle(objAddress + ObjectConfig.HitboxDownOffsetOffset);
                    float objHurtboxTop = objY + objHurtboxHeight - objHitboxDownOffset;

                    double marioHurtboxAboveObject = mObjHurtboxBottom - objHurtboxTop;
                    return marioHurtboxAboveObject;
                },
                (double hurtboxDistAbove, uint objAddress) =>
                {
                    uint marioObjRef = Config.Stream.GetUInt32(MarioObjectConfig.PointerAddress);
                    float mObjY = Config.Stream.GetSingle(marioObjRef + ObjectConfig.YOffset);
                    float mObjHitboxDownOffset = Config.Stream.GetSingle(marioObjRef + ObjectConfig.HitboxDownOffsetOffset);

                    float objY = Config.Stream.GetSingle(objAddress + ObjectConfig.YOffset);
                    float objHurtboxHeight = Config.Stream.GetSingle(objAddress + ObjectConfig.HurtboxHeightOffset);
                    float objHitboxDownOffset = Config.Stream.GetSingle(objAddress + ObjectConfig.HitboxDownOffsetOffset);
                    float objHurtboxTop = objY + objHurtboxHeight - objHitboxDownOffset;

                    double newMarioY = objHurtboxTop + mObjHitboxDownOffset + hurtboxDistAbove;
                    return BoolUtilities.Combine(
                        PositionAngle.Mario.SetY(newMarioY),
                        PositionAngle.MarioObj().SetY(newMarioY)
                    );
                }
            );

            dictionary.Add("MarioHurtboxBelowObject",
                BaseAddressType.Object,
                objAddress =>
                {
                    uint marioObjRef = Config.Stream.GetUInt32(MarioObjectConfig.PointerAddress);
                    float mObjY = Config.Stream.GetSingle(marioObjRef + ObjectConfig.YOffset);
                    float mObjHitboxHeight = Config.Stream.GetSingle(marioObjRef + ObjectConfig.HitboxHeightOffset);
                    float mObjHitboxDownOffset = Config.Stream.GetSingle(marioObjRef + ObjectConfig.HitboxDownOffsetOffset);
                    float mObjHurtboxTop = mObjY + mObjHitboxHeight - mObjHitboxDownOffset;

                    float objY = Config.Stream.GetSingle(objAddress + ObjectConfig.YOffset);
                    float objHurtboxHeight = Config.Stream.GetSingle(objAddress + ObjectConfig.HurtboxHeightOffset);
                    float objHitboxDownOffset = Config.Stream.GetSingle(objAddress + ObjectConfig.HitboxDownOffsetOffset);
                    float objHurtboxBottom = objY - objHitboxDownOffset;

                    double marioHurtboxBelowObject = objHurtboxBottom - mObjHurtboxTop;
                    return marioHurtboxBelowObject;
                },
                (double hurtboxDistBelow, uint objAddress) =>
                {
                    uint marioObjRef = Config.Stream.GetUInt32(MarioObjectConfig.PointerAddress);
                    float mObjY = Config.Stream.GetSingle(marioObjRef + ObjectConfig.YOffset);
                    float mObjHitboxHeight = Config.Stream.GetSingle(marioObjRef + ObjectConfig.HitboxHeightOffset);
                    float mObjHitboxDownOffset = Config.Stream.GetSingle(marioObjRef + ObjectConfig.HitboxDownOffsetOffset);
                    float mObjHurtboxTop = mObjY + mObjHitboxHeight - mObjHitboxDownOffset;

                    float objY = Config.Stream.GetSingle(objAddress + ObjectConfig.YOffset);
                    float objHurtboxHeight = Config.Stream.GetSingle(objAddress + ObjectConfig.HurtboxHeightOffset);
                    float objHitboxDownOffset = Config.Stream.GetSingle(objAddress + ObjectConfig.HitboxDownOffsetOffset);
                    float objHurtboxBottom = objY - objHitboxDownOffset;

                    double newMarioY = objHurtboxBottom - (mObjHurtboxTop - mObjY) - hurtboxDistBelow;
                    return BoolUtilities.Combine(
                        PositionAngle.Mario.SetY(newMarioY),
                        PositionAngle.MarioObj().SetY(newMarioY)
                    );
                }
            );

            dictionary.Add("MarioHurtboxOverlapsObject",
                BaseAddressType.Object,
                objAddress =>
                {
                    uint marioObjRef = Config.Stream.GetUInt32(MarioObjectConfig.PointerAddress);
                    float mObjX = Config.Stream.GetSingle(marioObjRef + ObjectConfig.XOffset);
                    float mObjY = Config.Stream.GetSingle(marioObjRef + ObjectConfig.YOffset);
                    float mObjZ = Config.Stream.GetSingle(marioObjRef + ObjectConfig.ZOffset);
                    float mObjHurtboxRadius = Config.Stream.GetSingle(marioObjRef + ObjectConfig.HurtboxRadiusOffset);
                    float mObjHitboxHeight = Config.Stream.GetSingle(marioObjRef + ObjectConfig.HitboxHeightOffset);
                    float mObjHitboxDownOffset = Config.Stream.GetSingle(marioObjRef + ObjectConfig.HitboxDownOffsetOffset);
                    float mObjHurtboxBottom = mObjY - mObjHitboxDownOffset;
                    float mObjHurtboxTop = mObjY + mObjHitboxHeight - mObjHitboxDownOffset;

                    float objX = Config.Stream.GetSingle(objAddress + ObjectConfig.XOffset);
                    float objY = Config.Stream.GetSingle(objAddress + ObjectConfig.YOffset);
                    float objZ = Config.Stream.GetSingle(objAddress + ObjectConfig.ZOffset);
                    float objHurtboxRadius = Config.Stream.GetSingle(objAddress + ObjectConfig.HurtboxRadiusOffset);
                    float objHurtboxHeight = Config.Stream.GetSingle(objAddress + ObjectConfig.HurtboxHeightOffset);
                    float objHitboxDownOffset = Config.Stream.GetSingle(objAddress + ObjectConfig.HitboxDownOffsetOffset);
                    float objHurtboxBottom = objY - objHitboxDownOffset;
                    float objHurtboxTop = objY + objHurtboxHeight - objHitboxDownOffset;

                    double marioHurtboxAwayFromObject = MoreMath.GetDistanceBetween(mObjX, mObjZ, objX, objZ) - mObjHurtboxRadius - objHurtboxRadius;
                    double marioHurtboxAboveObject = mObjHurtboxBottom - objHurtboxTop;
                    double marioHurtboxBelowObject = objHurtboxBottom - mObjHurtboxTop;

                    return marioHurtboxAwayFromObject < 0 && marioHurtboxAboveObject <= 0 && marioHurtboxBelowObject <= 0;
                },
                SpecialVariableDefaults<bool>.DEFAULT_SETTER_WITH_ADDRESS);

            dictionary.Add("MarioPunchAngleAway",
                BaseAddressType.Object,
                objAddress =>
                {
                    PositionAngle marioPos = PositionAngle.Mario;
                    PositionAngle objPos = PositionAngle.Obj(objAddress);
                    ushort angleToObj = InGameTrigUtilities.InGameAngleTo(
                        marioPos.X, marioPos.Z, objPos.X, objPos.Z);
                    double angleDiff = marioPos.Angle - angleToObj;
                    int angleDiffShort = MoreMath.NormalizeAngleShort(angleDiff);
                    int angleDiffAbs = Math.Abs(angleDiffShort);
                    int angleAway = angleDiffAbs - 0x2AAA;
                    return angleAway;
                },
                (double angleAway, uint objAddress) =>
                {
                    PositionAngle marioPos = PositionAngle.Mario;
                    PositionAngle objPos = PositionAngle.Obj(objAddress);
                    ushort angleToObj = InGameTrigUtilities.InGameAngleTo(
                        marioPos.X, marioPos.Z, objPos.X, objPos.Z);
                    double oldAngleDiff = marioPos.Angle - angleToObj;
                    int oldAngleDiffShort = MoreMath.NormalizeAngleShort(oldAngleDiff);
                    int signMultiplier = oldAngleDiffShort >= 0 ? 1 : -1;

                    double angleDiffAbs = angleAway + 0x2AAA;
                    double angleDiff = angleDiffAbs * signMultiplier;
                    double marioAngleDouble = angleToObj + angleDiff;
                    ushort marioAngleUShort = MoreMath.NormalizeAngleUshort(marioAngleDouble);

                    return Config.Stream.SetValue(marioAngleUShort, MarioConfig.StructAddress + MarioConfig.FacingYawOffset);
                }
            );

            dictionary.Add("ObjectRngCallsPerFrame",
                BaseAddressType.Object,
                objAddress =>
                {
                    uint numberOfRngObjs = Config.Stream.GetUInt32(MiscConfig.HackedAreaAddress);
                    int numOfCalls = 0;
                    for (int i = 0; i < Math.Min(numberOfRngObjs, ObjectSlotsConfig.MaxSlots); i++)
                    {
                        uint rngStructAdd = (uint)(MiscConfig.HackedAreaAddress + 0x30 + 0x08 * i);
                        uint address = Config.Stream.GetUInt32(rngStructAdd + 0x04);
                        if (address != objAddress) continue;
                        ushort preRng = Config.Stream.GetUInt16(rngStructAdd + 0x00);
                        ushort postRng = Config.Stream.GetUInt16(rngStructAdd + 0x02);
                        numOfCalls = RngIndexer.GetRngIndexDiff(preRng, postRng);
                        break;
                    }

                    return numOfCalls;
                },
                SpecialVariableDefaults<int>.DEFAULT_SETTER_WITH_ADDRESS);

            dictionary.Add("ObjectProcessGroup",
                BaseAddressType.ProcessGroup,
                processGroupUint => processGroupUint == uint.MaxValue ? (sbyte)(-1) : (sbyte)processGroupUint,
                SpecialVariableDefaults<sbyte>.DEFAULT_SETTER_WITH_ADDRESS);

            dictionary.Add("ObjectProcessGroupDescription",
                BaseAddressType.ProcessGroup,
                processGroupUint => ProcessGroupUtilities.GetProcessGroupDescription(processGroupUint),
                SpecialVariableDefaults<string>.DEFAULT_SETTER_WITH_ADDRESS);

            dictionary.Add("ObjectRngIndex",
                BaseAddressType.Object,
                objAddress =>
                {
                    ushort coinRngValue = Config.Stream.GetUInt16(objAddress + ObjectConfig.YawMovingOffset);
                    int coinRngIndex = RngIndexer.GetRngIndex(coinRngValue);
                    return coinRngIndex;
                },
                (int rngIndex, uint objAddress) =>
                {
                    ushort coinRngValue = RngIndexer.GetRngValue(rngIndex);
                    return Config.Stream.SetValue(coinRngValue, objAddress + ObjectConfig.YawMovingOffset);
                }
            );

            dictionary.Add("ObjectRngIndexDiff",
                BaseAddressType.Object,
                objAddress =>
                {
                    ushort coinRngValue = Config.Stream.GetUInt16(objAddress + ObjectConfig.YawMovingOffset);
                    int coinRngIndex = RngIndexer.GetRngIndex(coinRngValue);
                    int rngIndexDiff = coinRngIndex - SpecialConfig.GoalRngIndex;
                    return rngIndexDiff;
                },
                (int rngIndexDiff, uint objAddress) =>
                {
                    int coinRngIndex = SpecialConfig.GoalRngIndex + rngIndexDiff;
                    ushort coinRngValue = RngIndexer.GetRngValue(coinRngIndex);
                    return Config.Stream.SetValue(coinRngValue, objAddress + ObjectConfig.YawMovingOffset);
                }
            );

            // Object specific vars - Pendulum

            dictionary.Add("PendulumCountdown",
                BaseAddressType.Object,
                objAddress =>
                {
                    int pendulumCountdown = GetPendulumCountdown(objAddress);
                    return pendulumCountdown;
                },
                SpecialVariableDefaults<int>.DEFAULT_SETTER_WITH_ADDRESS);

            dictionary.Add("PendulumAmplitude",
                BaseAddressType.Object,
                objAddress =>
                {
                    float pendulumAmplitude = GetPendulumAmplitude(objAddress);
                    return pendulumAmplitude;
                },
                (double amplitude, uint objAddress) =>
                {
                    float accelerationDirection = amplitude > 0 ? -1 : 1;

                    bool success = true;
                    success &= Config.Stream.SetValue(accelerationDirection, objAddress + ObjectConfig.PendulumAccelerationDirectionOffset);
                    success &= Config.Stream.SetValue(0f, objAddress + ObjectConfig.PendulumAngularVelocityOffset);
                    success &= Config.Stream.SetValue((float)amplitude, objAddress + ObjectConfig.PendulumAngleOffset);
                    return success;
                }
            );

            dictionary.Add("PendulumSwingIndex",
                BaseAddressType.Object,
                objAddress =>
                {
                    float pendulumAmplitudeFloat = GetPendulumAmplitude(objAddress);
                    int? pendulumAmplitudeIntNullable = ParsingUtilities.ParseIntNullable(pendulumAmplitudeFloat);
                    if (!pendulumAmplitudeIntNullable.HasValue) return Double.NaN.ToString();
                    int pendulumAmplitudeInt = pendulumAmplitudeIntNullable.Value;
                    return TableConfig.PendulumSwings.GetPendulumSwingIndexExtended(pendulumAmplitudeInt);
                },
                (string indexString, uint objAddress) =>
                {
                    bool validIndex = int.TryParse(indexString, out var index);
                    if (!validIndex)
                        return false;
                    float amplitude = TableConfig.PendulumSwings.GetPendulumAmplitude(index);
                    float accelerationDirection = amplitude > 0 ? -1 : 1;

                    bool success = true;
                    success &= Config.Stream.SetValue(accelerationDirection, objAddress + ObjectConfig.PendulumAccelerationDirectionOffset);
                    success &= Config.Stream.SetValue(0f, objAddress + ObjectConfig.PendulumAngularVelocityOffset);
                    success &= Config.Stream.SetValue(amplitude, objAddress + ObjectConfig.PendulumAngleOffset);
                    return success;
                }
            );

            // Object specific vars - Cog

            dictionary.Add("CogCountdown",
                BaseAddressType.Object,
                objAddress => GetCogNumFramesInRotation(objAddress),
                SpecialVariableDefaults<int>.DEFAULT_SETTER_WITH_ADDRESS);

            dictionary.Add("CogEndingYaw",
                BaseAddressType.Object,
                objAddress => GetCogEndingYaw(objAddress),
                SpecialVariableDefaults<ushort>.DEFAULT_SETTER_WITH_ADDRESS);

            dictionary.Add("CogRotationIndex",
                BaseAddressType.Object,
                objAddress =>
                {
                    ushort yawFacing = Config.Stream.GetUInt16(objAddress + ObjectConfig.YawFacingOffset);
                    double rotationIndex = CogUtilities.GetRotationIndex(yawFacing) ?? Double.NaN;
                    return rotationIndex;
                },
                SpecialVariableDefaults<double>.DEFAULT_SETTER_WITH_ADDRESS);

            // Object specific vars - Waypoint

            dictionary.Add("ObjectDotProductToWaypoint",
                BaseAddressType.Object,
                objAddress =>
                {
                    (double dotProduct, double distToWaypointPlane, double distToWaypoint) =
                        GetWaypointSpecialVars(objAddress);
                    return dotProduct;
                },
                SpecialVariableDefaults<double>.DEFAULT_SETTER_WITH_ADDRESS);

            dictionary.Add("ObjectDistanceToWaypointPlane",
                BaseAddressType.Object,
                objAddress =>
                {
                    (double dotProduct, double distToWaypointPlane, double distToWaypoint) =
                        GetWaypointSpecialVars(objAddress);
                    return distToWaypointPlane;
                },
                SpecialVariableDefaults<double>.DEFAULT_SETTER_WITH_ADDRESS);

            dictionary.Add("ObjectDistanceToWaypoint",
                BaseAddressType.Object,
                objAddress =>
                {
                    (double dotProduct, double distToWaypointPlane, double distToWaypoint) =
                        GetWaypointSpecialVars(objAddress);
                    return distToWaypoint;
                },
                SpecialVariableDefaults<double>.DEFAULT_SETTER_WITH_ADDRESS);

            // Object specific vars - Racing Penguin

            dictionary.Add("RacingPenguinEffortTarget",
                BaseAddressType.Object,
                objAddress =>
                {
                    (double effortTarget, double effortChange, double minHSpeed, double hSpeedTarget) =
                        GetRacingPenguinSpecialVars(objAddress);
                    return effortTarget;
                },
                SpecialVariableDefaults<double>.DEFAULT_SETTER_WITH_ADDRESS);

            dictionary.Add("RacingPenguinEffortChange",
                BaseAddressType.Object,
                objAddress =>
                {
                    (double effortTarget, double effortChange, double minHSpeed, double hSpeedTarget) =
                        GetRacingPenguinSpecialVars(objAddress);
                    return effortChange;
                },
                SpecialVariableDefaults<double>.DEFAULT_SETTER_WITH_ADDRESS);

            dictionary.Add("RacingPenguinMinHSpeed",
                BaseAddressType.Object,
                objAddress =>
                {
                    (double effortTarget, double effortChange, double minHSpeed, double hSpeedTarget) =
                        GetRacingPenguinSpecialVars(objAddress);
                    return minHSpeed;
                },
                SpecialVariableDefaults<double>.DEFAULT_SETTER_WITH_ADDRESS);

            dictionary.Add("RacingPenguinHSpeedTarget",
                BaseAddressType.Object,
                objAddress =>
                {
                    (double effortTarget, double effortChange, double minHSpeed, double hSpeedTarget) =
                        GetRacingPenguinSpecialVars(objAddress);
                    return hSpeedTarget;
                },
                SpecialVariableDefaults<double>.DEFAULT_SETTER_WITH_ADDRESS);

            dictionary.Add("RacingPenguinDiffHSpeedTarget",
                BaseAddressType.Object,
                objAddress =>
                {
                    (double effortTarget, double effortChange, double minHSpeed, double hSpeedTarget) =
                        GetRacingPenguinSpecialVars(objAddress);
                    float hSpeed = Config.Stream.GetSingle(objAddress + ObjectConfig.HSpeedOffset);
                    double hSpeedDiff = hSpeed - hSpeedTarget;
                    return hSpeedDiff;
                },
                SpecialVariableDefaults<double>.DEFAULT_SETTER_WITH_ADDRESS);

            dictionary.Add("RacingPenguinProgress",
                BaseAddressType.Object,
                objAddress => TableConfig.RacingPenguinWaypoints.GetProgress(objAddress),
                SpecialVariableDefaults<double>.DEFAULT_SETTER_WITH_ADDRESS);

            // Object specific vars - Koopa the Quick

            dictionary.Add("KoopaTheQuickHSpeedTarget",
                BaseAddressType.Object,
                objAddress =>
                {
                    (double hSpeedTarget, double hSpeedChange) = GetKoopaTheQuickSpecialVars(objAddress);
                    return hSpeedTarget;
                },
                SpecialVariableDefaults<double>.DEFAULT_SETTER_WITH_ADDRESS);

            dictionary.Add("KoopaTheQuickHSpeedChange",
                BaseAddressType.Object,
                objAddress =>
                {
                    (double hSpeedTarget, double hSpeedChange) = GetKoopaTheQuickSpecialVars(objAddress);
                    return hSpeedChange;
                },
                SpecialVariableDefaults<double>.DEFAULT_SETTER_WITH_ADDRESS);

            dictionary.Add("KoopaTheQuick1Progress",
                BaseAddressType.Object,
                objAddress => TableConfig.KoopaTheQuick1Waypoints.GetProgress(objAddress),
                SpecialVariableDefaults<double>.DEFAULT_SETTER_WITH_ADDRESS);

            dictionary.Add("KoopaTheQuick2Progress",
                BaseAddressType.Object,
                objAddress => TableConfig.KoopaTheQuick2Waypoints.GetProgress(objAddress),
                SpecialVariableDefaults<double>.DEFAULT_SETTER_WITH_ADDRESS);

            dictionary.Add("KoopaTheQuick1ProgressOld",
                BaseAddressType.Object,
                objAddress => PlushUtilities.GetProgress(Config.Stream.GetUInt32(MiscConfig.GlobalTimerAddress)),
                SpecialVariableDefaults<double>.DEFAULT_SETTER_WITH_ADDRESS);

            dictionary.Add("KoopaTheQuick1ProgressDiff",
                BaseAddressType.Object,
                objAddress =>
                {
                    uint globalTimer = Config.Stream.GetUInt32(MiscConfig.GlobalTimerAddress);
                    double progressOld = PlushUtilities.GetProgress(globalTimer);
                    double progressNew = TableConfig.KoopaTheQuick1Waypoints.GetProgress(objAddress);
                    return progressNew - progressOld;
                },
                SpecialVariableDefaults<double>.DEFAULT_SETTER_WITH_ADDRESS);

            // Object specific vars - Fly Guy

            dictionary.Add("FlyGuyZone",
                BaseAddressType.Object,
                objAddress =>
                {
                    float marioY = Config.Stream.GetSingle(MarioConfig.StructAddress + MarioConfig.YOffset);
                    float objY = Config.Stream.GetSingle(objAddress + ObjectConfig.YOffset);
                    double heightDiff = marioY - objY;
                    if (heightDiff < -400) return "Low";
                    if (heightDiff > -200) return "High";
                    return "Medium";
                },
                SpecialVariableDefaults<string>.DEFAULT_SETTER_WITH_ADDRESS);

            dictionary.Add("FlyGuyRelativeHeight",
                BaseAddressType.Object,
                objAddress =>
                {
                    int oscillationTimer = Config.Stream.GetInt32(objAddress + ObjectConfig.FlyGuyOscillationTimerOffset);
                    return TableConfig.FlyGuyData.GetRelativeHeight(oscillationTimer);
                },
                SpecialVariableDefaults<double>.DEFAULT_SETTER_WITH_ADDRESS);

            dictionary.Add("FlyGuyMinHeight",
                BaseAddressType.Object,
                objAddress =>
                {
                    float objY = Config.Stream.GetSingle(objAddress + ObjectConfig.YOffset);
                    int oscillationTimer = Config.Stream.GetInt32(objAddress + ObjectConfig.FlyGuyOscillationTimerOffset);
                    double minHeight = TableConfig.FlyGuyData.GetMinHeight(oscillationTimer, objY);
                    return minHeight;
                },
                (double newMinHeight, uint objAddress) =>
                {
                    int oscillationTimer = Config.Stream.GetInt32(objAddress + ObjectConfig.FlyGuyOscillationTimerOffset);
                    float oldHeight = Config.Stream.GetSingle(objAddress + ObjectConfig.YOffset);
                    double oldMinHeight = TableConfig.FlyGuyData.GetMinHeight(oscillationTimer, oldHeight);
                    double heightDiff = newMinHeight - oldMinHeight;
                    double newHeight = oldHeight + heightDiff;
                    return Config.Stream.SetValue((float)newHeight, objAddress + ObjectConfig.YOffset);
                }
            );

            dictionary.Add("FlyGuyMaxHeight",
                BaseAddressType.Object,
                objAddress =>
                {
                    float objY = Config.Stream.GetSingle(objAddress + ObjectConfig.YOffset);
                    int oscillationTimer = Config.Stream.GetInt32(objAddress + ObjectConfig.FlyGuyOscillationTimerOffset);
                    double maxHeight = TableConfig.FlyGuyData.GetMaxHeight(oscillationTimer, objY);
                    return maxHeight;
                },
                (double newMaxHeight, uint objAddress) =>
                {
                    int oscillationTimer = Config.Stream.GetInt32(objAddress + ObjectConfig.FlyGuyOscillationTimerOffset);
                    float oldHeight = Config.Stream.GetSingle(objAddress + ObjectConfig.YOffset);
                    double oldMaxHeight = TableConfig.FlyGuyData.GetMaxHeight(oscillationTimer, oldHeight);
                    double heightDiff = newMaxHeight - oldMaxHeight;
                    double newHeight = oldHeight + heightDiff;
                    return Config.Stream.SetValue((float)newHeight, objAddress + ObjectConfig.YOffset);
                }
            );

            dictionary.Add("FlyGuyActivationDistanceDiff",
                BaseAddressType.Object,
                objAddress =>
                {
                    PositionAngle marioPos = PositionAngle.Mario;
                    PositionAngle objPos = PositionAngle.Obj(objAddress);
                    double dist = MoreMath.GetDistanceBetween(
                        marioPos.X, marioPos.Y, marioPos.Z, objPos.X, objPos.Y, objPos.Z);
                    double distDiff = dist - 4000;
                    return distDiff;
                },
                (double distDiff, uint objAddress) =>
                {
                    PositionAngle marioPos = PositionAngle.Mario;
                    PositionAngle objPos = PositionAngle.Obj(objAddress);
                    double distAway = distDiff + 4000;
                    (double newMarioX, double newMarioY, double newMarioZ) =
                        MoreMath.ExtrapolateLine3D(
                            objPos.X, objPos.Y, objPos.Z, marioPos.X, marioPos.Y, marioPos.Z, distAway);
                    return marioPos.SetValues(x: newMarioX, y: newMarioY, z: newMarioZ);
                }
            );

            // Object specific vars - Bob-omb

            dictionary.Add("BobombBloatSize",
                BaseAddressType.Object,
                objAddress =>
                {
                    float hitboxRadius = Config.Stream.GetSingle(objAddress + ObjectConfig.HitboxRadiusOffset);
                    float bloatSize = (hitboxRadius - 65) / 13;
                    return bloatSize;
                },
                (float bloatSize, uint objAddress) =>
                {
                    float hitboxRadius = bloatSize * 13 + 65;
                    float hitboxHeight = bloatSize * 22.6f + 113;
                    float scale = bloatSize / 5 + 1;

                    bool success = true;
                    success &= Config.Stream.SetValue(hitboxRadius, objAddress + ObjectConfig.HitboxRadiusOffset);
                    success &= Config.Stream.SetValue(hitboxHeight, objAddress + ObjectConfig.HitboxHeightOffset);
                    success &= Config.Stream.SetValue(scale, objAddress + ObjectConfig.ScaleWidthOffset);
                    success &= Config.Stream.SetValue(scale, objAddress + ObjectConfig.ScaleHeightOffset);
                    success &= Config.Stream.SetValue(scale, objAddress + ObjectConfig.ScaleDepthOffset);
                    return success;
                }
            );

            dictionary.Add("BobombRadius",
                BaseAddressType.Object,
                objAddress =>
                {
                    float hitboxRadius = Config.Stream.GetSingle(objAddress + ObjectConfig.HitboxRadiusOffset);
                    float radius = hitboxRadius + 32;
                    return radius;
                },
                (float radius, uint objAddress) =>
                {
                    float bloatSize = (radius - 97) / 13;
                    float hitboxRadius = bloatSize * 13 + 65;
                    float hitboxHeight = bloatSize * 22.6f + 113;
                    float scale = bloatSize / 5 + 1;

                    bool success = true;
                    success &= Config.Stream.SetValue(hitboxRadius, objAddress + ObjectConfig.HitboxRadiusOffset);
                    success &= Config.Stream.SetValue(hitboxHeight, objAddress + ObjectConfig.HitboxHeightOffset);
                    success &= Config.Stream.SetValue(scale, objAddress + ObjectConfig.ScaleWidthOffset);
                    success &= Config.Stream.SetValue(scale, objAddress + ObjectConfig.ScaleHeightOffset);
                    success &= Config.Stream.SetValue(scale, objAddress + ObjectConfig.ScaleDepthOffset);
                    return success;
                }
            );

            dictionary.Add("BobombSpaceBetween",
                BaseAddressType.Object,
                objAddress =>
                {
                    PositionAngle marioPos = PositionAngle.Mario;
                    PositionAngle objPos = PositionAngle.Obj(objAddress);
                    double hDist = MoreMath.GetDistanceBetween(
                        marioPos.X, marioPos.Z, objPos.X, objPos.Z);
                    float hitboxRadius = Config.Stream.GetSingle(objAddress + ObjectConfig.HitboxRadiusOffset);
                    float radius = hitboxRadius + 32;
                    double spaceBetween = hDist - radius;
                    return spaceBetween;
                },
                (double spaceBetween, uint objAddress) =>
                {
                    float hitboxRadius = Config.Stream.GetSingle(objAddress + ObjectConfig.HitboxRadiusOffset);
                    float radius = hitboxRadius + 32;
                    double distAway = spaceBetween + radius;

                    PositionAngle marioPos = PositionAngle.Mario;
                    PositionAngle objPos = PositionAngle.Obj(objAddress);
                    (double newMarioX, double newMarioZ) =
                        MoreMath.ExtrapolateLine2D(
                            objPos.X, objPos.Z, marioPos.X, marioPos.Z, distAway);
                    return marioPos.SetValues(x: newMarioX, z: newMarioZ);
                }
            );

            dictionary.Add("BobombHomeRadiusDiff",
                BaseAddressType.Object,
                objAddress => GetRadiusDiff(PositionAngle.Mario, PositionAngle.ObjHome(objAddress), 400),
                (double dist, uint objAddress) => SetRadiusDiff(PositionAngle.Mario, PositionAngle.ObjHome(objAddress), 400, dist)
            );

            // Object specific vars - Chuckya

            dictionary.Add("ChuckyaAngleMod1024",
                BaseAddressType.Object,
                objAddress => Config.Stream.GetUInt16(objAddress + ObjectConfig.YawMovingOffset) % 1024,
                SpecialVariableDefaults<int>.DEFAULT_SETTER_WITH_ADDRESS);

            // Object specific vars - Scuttlebug

            dictionary.Add("ScuttlebugDeltaAngleToTarget",
                BaseAddressType.Object,
                objAddress =>
                {
                    ushort facingAngle = Config.Stream.GetUInt16(objAddress + ObjectConfig.YawFacingOffset);
                    ushort targetAngle = Config.Stream.GetUInt16(objAddress + ObjectConfig.ScuttlebugTargetAngleOffset);
                    int angleDiff = facingAngle - targetAngle;
                    return MoreMath.NormalizeAngleDoubleSigned(angleDiff);
                },
                (double angleDiff, uint objAddress) =>
                {
                    ushort targetAngle = Config.Stream.GetUInt16(objAddress + ObjectConfig.ScuttlebugTargetAngleOffset);
                    double newObjAngleDouble = targetAngle + angleDiff;
                    ushort newObjAngleUShort = MoreMath.NormalizeAngleUshort(newObjAngleDouble);
                    return PositionAngle.Obj(objAddress).SetAngle(newObjAngleUShort);
                }
            );

            // Object specific vars - Goomba Triplet Spawner

            dictionary.Add("GoombaTripletLoadingRadiusDiff",
                BaseAddressType.Object,
                objAddress => GetRadiusDiff(PositionAngle.Mario, PositionAngle.Obj(objAddress), 3000),
                (double dist, uint objAddress) => SetRadiusDiff(PositionAngle.Mario, PositionAngle.Obj(objAddress), 3000, dist)
            );

            dictionary.Add("GoombaTripletUnloadingRadiusDiff",
                BaseAddressType.Object,
                objAddress => GetRadiusDiff(PositionAngle.Mario, PositionAngle.Obj(objAddress), 4000),
                (double dist, uint objAddress) => SetRadiusDiff(PositionAngle.Mario, PositionAngle.Obj(objAddress), 4000, dist)
            );

            // Object specific vars - BitFS Platform

            dictionary.Add("BitfsPlatformGroupMinHeight",
                BaseAddressType.Object,
                objAddress =>
                {
                    int timer = Config.Stream.GetInt32(objAddress + ObjectConfig.BitfsPlatformGroupTimerOffset);
                    float height = Config.Stream.GetSingle(objAddress + ObjectConfig.YOffset);
                    return BitfsPlatformGroupTable.GetMinHeight(timer, height);
                },
                (double newMinHeight, uint objAddress) =>
                {
                    int timer = Config.Stream.GetInt32(objAddress + ObjectConfig.BitfsPlatformGroupTimerOffset);
                    float height = Config.Stream.GetSingle(objAddress + ObjectConfig.YOffset);
                    double oldMinHeight = BitfsPlatformGroupTable.GetMinHeight(timer, height);
                    double heightDiff = newMinHeight - oldMinHeight;
                    float oldHeight = Config.Stream.GetSingle(objAddress + ObjectConfig.YOffset);
                    double newHeight = oldHeight + heightDiff;
                    return Config.Stream.SetValue((float)newHeight, objAddress + ObjectConfig.YOffset);
                }
            );

            dictionary.Add("BitfsPlatformGroupMaxHeight",
                BaseAddressType.Object,
                objAddress =>
                {
                    int timer = Config.Stream.GetInt32(objAddress + ObjectConfig.BitfsPlatformGroupTimerOffset);
                    float height = Config.Stream.GetSingle(objAddress + ObjectConfig.YOffset);
                    return BitfsPlatformGroupTable.GetMaxHeight(timer, height);
                },
                (double newMaxHeight, uint objAddress) =>
                {
                    int timer = Config.Stream.GetInt32(objAddress + ObjectConfig.BitfsPlatformGroupTimerOffset);
                    float height = Config.Stream.GetSingle(objAddress + ObjectConfig.YOffset);
                    double oldMaxHeight = BitfsPlatformGroupTable.GetMaxHeight(timer, height);
                    double heightDiff = newMaxHeight - oldMaxHeight;
                    float oldHeight = Config.Stream.GetSingle(objAddress + ObjectConfig.YOffset);
                    double newHeight = oldHeight + heightDiff;
                    return Config.Stream.SetValue((float)newHeight, objAddress + ObjectConfig.YOffset);
                }
            );

            dictionary.Add("BitfsPlatformGroupRelativeHeight",
                BaseAddressType.Object,
                objAddress =>
                {
                    int timer = Config.Stream.GetInt32(objAddress + ObjectConfig.BitfsPlatformGroupTimerOffset);
                    return BitfsPlatformGroupTable.GetRelativeHeightFromMin(timer);
                },
                SpecialVariableDefaults<float>.DEFAULT_SETTER_WITH_ADDRESS);

            dictionary.Add("BitfsPlatformGroupDisplacedHeight",
                BaseAddressType.Object,
                objAddress =>
                {
                    int timer = Config.Stream.GetInt32(objAddress + ObjectConfig.BitfsPlatformGroupTimerOffset);
                    float height = Config.Stream.GetSingle(objAddress + ObjectConfig.YOffset);
                    float homeHeight = Config.Stream.GetSingle(objAddress + ObjectConfig.HomeYOffset);
                    return BitfsPlatformGroupTable.GetDisplacedHeight(timer, height, homeHeight);
                },
                (double displacedHeight, uint objAddress) =>
                {
                    float homeHeight = Config.Stream.GetSingle(objAddress + ObjectConfig.HomeYOffset);
                    double newMaxHeight = homeHeight + displacedHeight;
                    int timer = Config.Stream.GetInt32(objAddress + ObjectConfig.BitfsPlatformGroupTimerOffset);
                    float relativeHeightFromMax = BitfsPlatformGroupTable.GetRelativeHeightFromMax(timer);
                    double newHeight = newMaxHeight + relativeHeightFromMax;
                    return Config.Stream.SetValue((float)newHeight, objAddress + ObjectConfig.YOffset);
                }
            );

            // Object specific vars - Hoot

            dictionary.Add("HootReleaseTimer",
                BaseAddressType.Object,
                objAddress =>
                {
                    uint globalTimer = Config.Stream.GetUInt32(MiscConfig.GlobalTimerAddress);
                    uint lastReleaseTime = Config.Stream.GetUInt32(objAddress + ObjectConfig.HootLastReleaseTimeOffset);
                    int diff = (int)(globalTimer - lastReleaseTime);
                    return diff;
                },
                (int newDiff, uint objAddress) =>
                {
                    uint globalTimer = Config.Stream.GetUInt32(MiscConfig.GlobalTimerAddress);
                    uint newLastReleaseTime = (uint)(globalTimer - newDiff);
                    return Config.Stream.SetValue(newLastReleaseTime, objAddress + ObjectConfig.HootLastReleaseTimeOffset);
                }
            );

            // Object specific vars - Power Star

            dictionary.Add("PowerStarMissionName",
                BaseAddressType.Object,
                objAddress =>
                {
                    int courseIndex = Config.Stream.GetInt16(MiscConfig.LevelIndexAddress);
                    int missionIndex = Config.Stream.GetByte(objAddress + ObjectConfig.PowerStarMissionIndexOffset);
                    return TableConfig.Missions.GetInGameMissionName(courseIndex, missionIndex);
                },
                SpecialVariableDefaults<string>.DEFAULT_SETTER_WITH_ADDRESS);

            // Object specific vars - Coordinates

            dictionary.Add("MinXCoordinate",
                BaseAddressType.Object,
                objAddress =>
                {
                    List<TriangleDataModel> tris = TriangleUtilities.GetObjectTrianglesForObject(objAddress);
                    if (tris.Count == 0) return null;
                    return tris.Min(tri => tri.GetMinX());
                },
                (float? newMinX, uint objAddress) =>
                {
                    List<TriangleDataModel> tris = TriangleUtilities.GetObjectTrianglesForObject(objAddress);
                    if (!newMinX.HasValue || tris.Count == 0)
                        return false;
                    int minX = tris.Min(tri => tri.GetMinX());
                    float diff = newMinX.Value - minX;
                    float objX = Config.Stream.GetSingle(objAddress + ObjectConfig.XOffset);
                    float newObjX = objX + diff;
                    return Config.Stream.SetValue(newObjX, objAddress + ObjectConfig.XOffset);
                }
            );

            dictionary.Add("MaxXCoordinate",
                BaseAddressType.Object,
                objAddress =>
                {
                    List<TriangleDataModel> tris = TriangleUtilities.GetObjectTrianglesForObject(objAddress);
                    if (tris.Count == 0) return null;
                    return tris.Max(tri => tri.GetMaxX());
                },
                (float? newMaxX, uint objAddress) =>
                {
                    List<TriangleDataModel> tris = TriangleUtilities.GetObjectTrianglesForObject(objAddress);
                    if (!newMaxX.HasValue || tris.Count == 0)
                        return false;
                    int maxX = tris.Max(tri => tri.GetMaxX());
                    float diff = newMaxX.Value - maxX;
                    float objX = Config.Stream.GetSingle(objAddress + ObjectConfig.XOffset);
                    float newObjX = objX + diff;
                    return Config.Stream.SetValue(newObjX, objAddress + ObjectConfig.XOffset);
                }
            );

            dictionary.Add("MinYCoordinate",
                BaseAddressType.Object,
                objAddress =>
                {
                    List<TriangleDataModel> tris = TriangleUtilities.GetObjectTrianglesForObject(objAddress);
                    if (tris.Count == 0) return null;
                    return tris.Min(tri => tri.GetMinY());
                },
                (short? newMinY, uint objAddress) =>
                {
                    List<TriangleDataModel> tris = TriangleUtilities.GetObjectTrianglesForObject(objAddress);
                    if (!newMinY.HasValue || tris.Count == 0)
                        return false;
                    int minY = tris.Min(tri => tri.GetMinY());
                    float diff = newMinY.Value - minY;
                    float objY = Config.Stream.GetSingle(objAddress + ObjectConfig.YOffset);
                    float newObjY = objY + diff;
                    return Config.Stream.SetValue(newObjY, objAddress + ObjectConfig.YOffset);
                }
            );

            dictionary.Add("MaxYCoordinate",
                BaseAddressType.Object,
                objAddress =>
                {
                    List<TriangleDataModel> tris = TriangleUtilities.GetObjectTrianglesForObject(objAddress);
                    if (tris.Count == 0)
                        return null;
                    return tris.Max(tri => tri.GetMaxY());
                },
                (short? newMaxY, uint objAddress) =>
                {
                    List<TriangleDataModel> tris = TriangleUtilities.GetObjectTrianglesForObject(objAddress);
                    if (!newMaxY.HasValue || tris.Count == 0)
                        return false;
                    int maxY = tris.Max(tri => tri.GetMaxY());
                    float diff = newMaxY.Value - maxY;
                    float objY = Config.Stream.GetSingle(objAddress + ObjectConfig.YOffset);
                    float newObjY = objY + diff;
                    return Config.Stream.SetValue(newObjY, objAddress + ObjectConfig.YOffset);
                }
            );

            dictionary.Add("MinZCoordinate",
                BaseAddressType.Object,
                objAddress =>
                {
                    List<TriangleDataModel> tris = TriangleUtilities.GetObjectTrianglesForObject(objAddress);
                    if (tris.Count == 0)
                        return null;
                    return tris.Min(tri => tri.GetMinZ());
                },
                (short? newMinZ, uint objAddress) =>
                {
                    List<TriangleDataModel> tris = TriangleUtilities.GetObjectTrianglesForObject(objAddress);
                    if (!newMinZ.HasValue || tris.Count == 0)
                        return false;
                    int minZ = tris.Min(tri => tri.GetMinZ());
                    float diff = newMinZ.Value - minZ;
                    float objZ = Config.Stream.GetSingle(objAddress + ObjectConfig.ZOffset);
                    float newObjZ = objZ + diff;
                    return Config.Stream.SetValue(newObjZ, objAddress + ObjectConfig.ZOffset);
                }
            );

            dictionary.Add("MaxZCoordinate",
                BaseAddressType.Object,
                objAddress =>
                {
                    List<TriangleDataModel> tris = TriangleUtilities.GetObjectTrianglesForObject(objAddress);
                    if (tris.Count == 0)
                        return null;
                    return tris.Max(tri => tri.GetMaxZ());
                },
                (short? newMaxZ, uint objAddress) =>
                {
                    List<TriangleDataModel> tris = TriangleUtilities.GetObjectTrianglesForObject(objAddress);
                    if (!newMaxZ.HasValue || tris.Count == 0)
                        return false;
                    int maxZ = tris.Max(tri => tri.GetMaxZ());
                    float diff = newMaxZ.Value - maxZ;
                    float objZ = Config.Stream.GetSingle(objAddress + ObjectConfig.ZOffset);
                    float newObjZ = objZ + diff;
                    return Config.Stream.SetValue(newObjZ, objAddress + ObjectConfig.ZOffset);
                }
            );

            dictionary.Add("RangeXCoordinate",
                BaseAddressType.Object,
                objAddress =>
                {
                    List<TriangleDataModel> tris = TriangleUtilities.GetObjectTrianglesForObject(objAddress);
                    if (tris.Count == 0)
                        return null;
                    return (short)(tris.Max(tri => tri.GetMaxX()) - tris.Min(tri => tri.GetMinX()));
                },
                (short? newXRange, uint objAddress) =>
                {
                    List<TriangleDataModel> tris = TriangleUtilities.GetObjectTrianglesForObject(objAddress);
                    if (!newXRange.HasValue || tris.Count == 0)
                        return false;
                    float xRange = tris.Max(tri => tri.GetMaxX()) - tris.Min(tri => tri.GetMinX());
                    float ratio = newXRange.Value / xRange;
                    float scaleX = Config.Stream.GetSingle(objAddress + ObjectConfig.ScaleWidthOffset);
                    float newScaleX = scaleX * ratio;
                    return Config.Stream.SetValue(newScaleX, objAddress + ObjectConfig.ScaleWidthOffset);
                }
            );

            dictionary.Add("RangeYCoordinate",
                BaseAddressType.Object,
                objAddress =>
                {
                    List<TriangleDataModel> tris = TriangleUtilities.GetObjectTrianglesForObject(objAddress);
                    if (tris.Count == 0)
                        return null;
                    return (short)(tris.Max(tri => tri.GetMaxY()) - tris.Min(tri => tri.GetMinY()));
                },
                (short? newYRange, uint objAddress) =>
                {
                    List<TriangleDataModel> tris = TriangleUtilities.GetObjectTrianglesForObject(objAddress);
                    if (!newYRange.HasValue || tris.Count == 0)
                        return false;
                    float yRange = tris.Max(tri => tri.GetMaxY()) - tris.Min(tri => tri.GetMinY());
                    float ratio = newYRange.Value / yRange;
                    float scaleY = Config.Stream.GetSingle(objAddress + ObjectConfig.ScaleHeightOffset);
                    float newScaleY = scaleY * ratio;
                    return Config.Stream.SetValue(newScaleY, objAddress + ObjectConfig.ScaleHeightOffset);
                }
            );

            dictionary.Add("RangeZCoordinate",
                BaseAddressType.Object,
                objAddress =>
                {
                    List<TriangleDataModel> tris = TriangleUtilities.GetObjectTrianglesForObject(objAddress);
                    if (tris.Count == 0)
                        return null;
                    return (short)(tris.Max(tri => tri.GetMaxZ()) - tris.Min(tri => tri.GetMinZ()));
                },
                (short? newZRange, uint objAddress) =>
                {
                    List<TriangleDataModel> tris = TriangleUtilities.GetObjectTrianglesForObject(objAddress);
                    if (!newZRange.HasValue || tris.Count == 0)
                        return false;
                    float zRange = tris.Max(tri => tri.GetMaxZ()) - tris.Min(tri => tri.GetMinZ());
                    float ratio = newZRange.Value / zRange;
                    float scaleZ = Config.Stream.GetSingle(objAddress + ObjectConfig.ScaleDepthOffset);
                    float newScaleZ = scaleZ * ratio;
                    return Config.Stream.SetValue(newScaleZ, objAddress + ObjectConfig.ScaleDepthOffset);
                }
            );

            dictionary.Add("MidpointXCoordinate",
                BaseAddressType.Object,
                objAddress =>
                {
                    List<TriangleDataModel> tris = TriangleUtilities.GetObjectTrianglesForObject(objAddress);
                    if (tris.Count == 0) return float.NaN;
                    return (tris.Max(tri => tri.GetMaxX()) + tris.Min(tri => tri.GetMinX())) / 2.0f;
                },
                (float newMidpointX, uint objAddress) =>
                {
                    List<TriangleDataModel> tris = TriangleUtilities.GetObjectTrianglesForObject(objAddress);
                    if (tris.Count == 0) return false;
                    float midpointX = (tris.Max(tri => tri.GetMaxX()) + tris.Min(tri => tri.GetMinX())) / 2.0f;
                    float diff = newMidpointX - midpointX;
                    float objX = Config.Stream.GetSingle(objAddress + ObjectConfig.XOffset);
                    float newObjX = objX + diff;
                    return Config.Stream.SetValue(newObjX, objAddress + ObjectConfig.XOffset);
                }
            );

            dictionary.Add("MidpointYCoordinate",
                BaseAddressType.Object,
                objAddress =>
                {
                    List<TriangleDataModel> tris = TriangleUtilities.GetObjectTrianglesForObject(objAddress);
                    if (tris.Count == 0) return float.NaN;
                    return (tris.Max(tri => tri.GetMaxY()) + tris.Min(tri => tri.GetMinY())) / 2.0f;
                },
                (float newMidpointY, uint objAddress) =>
                {
                    List<TriangleDataModel> tris = TriangleUtilities.GetObjectTrianglesForObject(objAddress);
                    if (tris.Count == 0) return false;
                    float midpointY = (tris.Max(tri => tri.GetMaxY()) + tris.Min(tri => tri.GetMinY())) / 2.0f;
                    float diff = newMidpointY - midpointY;
                    float objY = Config.Stream.GetSingle(objAddress + ObjectConfig.YOffset);
                    float newObjY = objY + diff;
                    return Config.Stream.SetValue(newObjY, objAddress + ObjectConfig.YOffset);
                }
            );

            dictionary.Add("MidpointZCoordinate",
                BaseAddressType.Object,
                objAddress =>
                {
                    List<TriangleDataModel> tris = TriangleUtilities.GetObjectTrianglesForObject(objAddress);
                    if (tris.Count == 0) return float.NaN;
                    return (tris.Max(tri => tri.GetMaxZ()) + tris.Min(tri => tri.GetMinZ())) / 2.0f;
                },
                (float newMidpointZ, uint objAddress) =>
                {
                    List<TriangleDataModel> tris = TriangleUtilities.GetObjectTrianglesForObject(objAddress);
                    if (tris.Count == 0) return false;
                    float midpointZ = (tris.Max(tri => tri.GetMaxZ()) + tris.Min(tri => tri.GetMinZ())) / 2f;
                    float diff = newMidpointZ - midpointZ;
                    float objZ = Config.Stream.GetSingle(objAddress + ObjectConfig.ZOffset);
                    float newObjZ = objZ + diff;
                    return Config.Stream.SetValue(newObjZ, objAddress + ObjectConfig.ZOffset);
                }
            );

            dictionary.Add("FarthestCoordinateDistance",
                BaseAddressType.Object,
                objAddress =>
                {
                    float objX = Config.Stream.GetSingle(objAddress + ObjectConfig.XOffset);
                    float objY = Config.Stream.GetSingle(objAddress + ObjectConfig.YOffset);
                    float objZ = Config.Stream.GetSingle(objAddress + ObjectConfig.ZOffset);

                    List<TriangleDataModel> tris = TriangleUtilities.GetObjectTrianglesForObject(objAddress);
                    if (tris.Count == 0) return double.NaN;

                    List<(int, int, int)> coordinates = new List<(int, int, int)>();
                    tris.ForEach(tri =>
                    {
                        coordinates.Add((tri.X1, tri.Y1, tri.Z1));
                        coordinates.Add((tri.X2, tri.Y2, tri.Z2));
                        coordinates.Add((tri.X3, tri.Y3, tri.Z3));
                    });
                    return coordinates.Max(coord => MoreMath.GetDistanceBetween(objX, objY, objZ, coord.Item1, coord.Item2, coord.Item3));
                },
                SpecialVariableDefaults<double>.DEFAULT_SETTER_WITH_ADDRESS);

            // Object specific vars - Rolling Log

            dictionary.Add("RollingLogDistLimit",
                BaseAddressType.Object,
                objAddress =>
                {
                    float distLimitSquared = Config.Stream.GetSingle(objAddress + ObjectConfig.RollingLogDistLimitSquaredOffset);
                    double distLimit = Math.Sqrt(distLimitSquared);
                    return distLimit;
                }
                ,
                (double newDistLimit, uint objAddress) =>
                {
                    double newDistLimitSquared = newDistLimit * newDistLimit;
                    return Config.Stream.SetValue((float)newDistLimitSquared, objAddress + ObjectConfig.RollingLogDistLimitSquaredOffset);
                }
            );

            dictionary.Add("RollingLogDist",
                BaseAddressType.Object,
                objAddress =>
                {
                    float x = Config.Stream.GetSingle(objAddress + ObjectConfig.XOffset);
                    float z = Config.Stream.GetSingle(objAddress + ObjectConfig.ZOffset);
                    float xCenter = Config.Stream.GetSingle(objAddress + ObjectConfig.RollingLogXCenterOffset);
                    float zCenter = Config.Stream.GetSingle(objAddress + ObjectConfig.RollingLogZCenterOffset);
                    return MoreMath.GetDistanceBetween(xCenter, zCenter, x, z);
                },
                SpecialVariableDefaults<double>.DEFAULT_SETTER_WITH_ADDRESS);

            // Object specific vars - Object Spawner

            dictionary.Add("ObjectSpawnerRadiusDiff",
                BaseAddressType.Object,
                objAddress =>
                {
                    float radius = Config.Stream.GetSingle(objAddress + ObjectConfig.ObjectSpawnerRadiusOffset);
                    return GetRadiusDiff(PositionAngle.Mario, PositionAngle.Obj(objAddress), radius);
                },
                (double dist, uint objAddress) =>
                {
                    float radius = Config.Stream.GetSingle(objAddress + ObjectConfig.ObjectSpawnerRadiusOffset);
                    return SetRadiusDiff(PositionAngle.Mario, PositionAngle.Obj(objAddress), radius, dist);
                }
            );

            // Object specific vars - WDW Rotating Platform

            dictionary.Add("WdwRotatingPlatformCurrentIndex",
                BaseAddressType.Object,
                objAddress =>
                {
                    ushort angle = Config.Stream.GetUInt16(objAddress + ObjectConfig.YawFacingOffset);
                    return TableConfig.WdwRotatingPlatformTable.GetIndex(angle);
                },
                (int? index, uint objAddress) =>
                {
                    if (!index.HasValue)
                        return false;
                    ushort angle = TableConfig.WdwRotatingPlatformTable.GetAngle(index.Value);
                    return Config.Stream.SetValue(angle, objAddress + ObjectConfig.YawFacingOffset);
                }
            );

            dictionary.Add("WdwRotatingPlatformGoalIndex",
                () => TableConfig.WdwRotatingPlatformTable.GetIndex(TableConfig.WdwRotatingPlatformTable.GoalAngle),
                (int? index) =>
                {
                    if (!index.HasValue)
                        return false;
                    TableConfig.WdwRotatingPlatformTable.GoalAngle = TableConfig.WdwRotatingPlatformTable.GetAngle(index.Value);
                    return true;
                }
            );

            dictionary.Add("WdwRotatingPlatformGoalAngle",
                () => TableConfig.WdwRotatingPlatformTable.GoalAngle,
                (ushort goalAngle) =>
                {
                    TableConfig.WdwRotatingPlatformTable.GoalAngle = goalAngle;
                    return true;
                }
            );

            dictionary.Add("WdwRotatingPlatformFramesUntilGoal",
                BaseAddressType.Object,
                objAddress =>
                {
                    ushort angle = Config.Stream.GetUInt16(objAddress + ObjectConfig.YawFacingOffset);
                    return TableConfig.WdwRotatingPlatformTable.GetFramesToGoalAngle(angle);
                },
                (int? numFrames, uint objAddress) =>
                {
                    if (!numFrames.HasValue)
                        return false;
                    ushort? newAngle = TableConfig.WdwRotatingPlatformTable.GetAngleNumFramesBeforeGoal(numFrames.Value);
                    if (!newAngle.HasValue) return false;
                    return Config.Stream.SetValue(newAngle.Value, objAddress + ObjectConfig.YawFacingOffset);
                }
            );

            // Object specific vars - Elevator Axle

            dictionary.Add("ElevatorAxleCurrentIndex",
                BaseAddressType.Object,
                objAddress =>
                {
                    ushort angle = Config.Stream.GetUInt16(objAddress + ObjectConfig.RollFacingOffset);
                    return TableConfig.ElevatorAxleTable.GetIndex(angle);
                },
                (int? index, uint objAddress) =>
                {
                    if (!index.HasValue)
                        return false;
                    ushort angle = TableConfig.ElevatorAxleTable.GetAngle(index.Value);
                    return Config.Stream.SetValue(angle, objAddress + ObjectConfig.RollFacingOffset);
                }
            );

            dictionary.Add("ElevatorAxleGoalIndex",
                () => TableConfig.ElevatorAxleTable.GetIndex(TableConfig.ElevatorAxleTable.GoalAngle),
                (int? index) =>
                {
                    if (!index.HasValue)
                        return false;
                    TableConfig.ElevatorAxleTable.GoalAngle = TableConfig.ElevatorAxleTable.GetAngle(index.Value);
                    return true;
                }
            );

            dictionary.Add("ElevatorAxleGoalAngle",
                () => TableConfig.ElevatorAxleTable.GoalAngle,
                (ushort goalAngle) =>
                {
                    TableConfig.ElevatorAxleTable.GoalAngle = goalAngle;
                    return true;
                }
            );

            dictionary.Add("ElevatorAxleFramesUntilGoal",
                BaseAddressType.Object,
                objAddress =>
                {
                    ushort angle = Config.Stream.GetUInt16(objAddress + ObjectConfig.RollFacingOffset);
                    return TableConfig.ElevatorAxleTable.GetFramesToGoalAngle(angle);
                },
                (int? numFrames, uint objAddress) =>
                {
                    if (!numFrames.HasValue)
                        return false;
                    ushort? newAngle = TableConfig.ElevatorAxleTable.GetAngleNumFramesBeforeGoal(numFrames.Value);
                    if (!newAngle.HasValue) return false;
                    return Config.Stream.SetValue(newAngle.Value, objAddress + ObjectConfig.RollFacingOffset);
                }
            );

            // Object specific vars - Swooper

            dictionary.Add("SwooperEffectiveTargetYaw",
                BaseAddressType.Object,
                objAddress =>
                {
                    uint globalTimer = Config.Stream.GetUInt32(MiscConfig.GlobalTimerAddress);
                    int targetAngle = Config.Stream.GetInt32(objAddress + ObjectConfig.SwooperTargetYawOffset);
                    return targetAngle + (short)(3000 * InGameTrigUtilities.InGameCosine(4000 * (int)globalTimer));
                },
                SpecialVariableDefaults<int>.DEFAULT_SETTER_WITH_ADDRESS);

            // Mario vars

            dictionary.Add("RotationDisplacementX",
                () => GetRotationDisplacement().ToTuple().Item1,
                SpecialVariableDefaults<float>.DEFAULT_SETTER);

            dictionary.Add("RotationDisplacementY",
                () => GetRotationDisplacement().ToTuple().Item2,
                SpecialVariableDefaults<float>.DEFAULT_SETTER);

            dictionary.Add("RotationDisplacementZ",
                () => GetRotationDisplacement().ToTuple().Item3,
                SpecialVariableDefaults<float>.DEFAULT_SETTER);

            dictionary.Add("SpeedMultiplier",
                () =>
                {
                    /*
                    intended dyaw = intended yaw - slide yaw (idk what this is called in stroop)
                    if cos(intended dyaw) < 0 and fspeed >= 0:
                      K = 0.5 + 0.5 * fspeed / 100
                    else:
                      K = 1

                    multiplier = (intended mag / 32) * cos(intended dyaw) * K * 0.02 + A

                    slide: A = 0.98
                    slippery: A = 0.96
                    default: A = 0.92
                    not slippery: A = 0.92
                    */

                    ushort intendedYaw = Config.Stream.GetUInt16(MarioConfig.StructAddress + MarioConfig.IntendedYawOffset);
                    ushort movingYaw = Config.Stream.GetUInt16(MarioConfig.StructAddress + MarioConfig.MovingYawOffset);
                    float scaledMagnitude = Config.Stream.GetSingle(MarioConfig.StructAddress + MarioConfig.ScaledMagnitudeOffset);
                    float hSpeed = Config.Stream.GetSingle(MarioConfig.StructAddress + MarioConfig.HSpeedOffset);
                    uint floorAddress = Config.Stream.GetUInt32(MarioConfig.StructAddress + MarioConfig.FloorTriangleOffset);
                    TriangleDataModel floorStruct = TriangleDataModel.Create(floorAddress);
                    double A = floorStruct.FrictionMultiplier;

                    int intendedDYaw = intendedYaw - movingYaw;
                    double K = InGameTrigUtilities.InGameCosine(intendedDYaw) < 0 && hSpeed >= 0 ? 0.5 + 0.5 * hSpeed / 100 : 1;
                    return (scaledMagnitude / 32) * InGameTrigUtilities.InGameCosine(intendedDYaw) * K * 0.02 + A;
                },
                SpecialVariableDefaults<double>.DEFAULT_SETTER);

            dictionary.Add("DeFactoSpeed",
                () => GetMarioDeFactoSpeed(),
                (double newDefactoSpeed) =>
                {
                    double newHSpeed = newDefactoSpeed / GetDeFactoMultiplier();
                    return Config.Stream.SetValue((float)newHSpeed, MarioConfig.StructAddress + MarioConfig.HSpeedOffset);
                }
            );

            dictionary.Add("SlidingSpeed",
                () => GetMarioSlidingSpeed(),
                (double newHSlidingSpeed) =>
                {
                    float xSlidingSpeed = Config.Stream.GetSingle(MarioConfig.StructAddress + MarioConfig.SlidingSpeedXOffset);
                    float zSlidingSpeed = Config.Stream.GetSingle(MarioConfig.StructAddress + MarioConfig.SlidingSpeedZOffset);
                    if (xSlidingSpeed == 0 && zSlidingSpeed == 0) xSlidingSpeed = 1;
                    double hSlidingSpeed = MoreMath.GetHypotenuse(xSlidingSpeed, zSlidingSpeed);

                    double multiplier = newHSlidingSpeed / hSlidingSpeed;
                    double newXSlidingSpeed = xSlidingSpeed * multiplier;
                    double newZSlidingSpeed = zSlidingSpeed * multiplier;

                    bool success = true;
                    success &= Config.Stream.SetValue((float)newXSlidingSpeed, MarioConfig.StructAddress + MarioConfig.SlidingSpeedXOffset);
                    success &= Config.Stream.SetValue((float)newZSlidingSpeed, MarioConfig.StructAddress + MarioConfig.SlidingSpeedZOffset);
                    return success;
                }
            );

            dictionary.Add("SlidingAngle",
                () => GetMarioSlidingAngle(),
                (double newHSlidingAngle) =>
                {
                    float xSlidingSpeed = Config.Stream.GetSingle(MarioConfig.StructAddress + MarioConfig.SlidingSpeedXOffset);
                    float zSlidingSpeed = Config.Stream.GetSingle(MarioConfig.StructAddress + MarioConfig.SlidingSpeedZOffset);
                    double hSlidingSpeed = MoreMath.GetHypotenuse(xSlidingSpeed, zSlidingSpeed);

                    (double newXSlidingSpeed, double newZSlidingSpeed) =
                        MoreMath.GetComponentsFromVector(hSlidingSpeed, newHSlidingAngle);

                    bool success = true;
                    success &= Config.Stream.SetValue((float)newXSlidingSpeed, MarioConfig.StructAddress + MarioConfig.SlidingSpeedXOffset);
                    success &= Config.Stream.SetValue((float)newZSlidingSpeed, MarioConfig.StructAddress + MarioConfig.SlidingSpeedZOffset);
                    return success;
                }
            );

            dictionary.Add("TwirlYawMod2048",
                () => Config.Stream.GetUInt16(MarioConfig.StructAddress + MarioConfig.TwirlYawOffset) % 2048,
                SpecialVariableDefaults<int>.DEFAULT_SETTER);

            dictionary.Add("FlyingEnergy",
                () => FlyingUtilities.GetEnergy(),
                SpecialVariableDefaults<double>.DEFAULT_SETTER);

            dictionary.Add("TrajectoryRemainingHeight",
                () =>
                {
                    float vSpeed = Config.Stream.GetSingle(MarioConfig.StructAddress + MarioConfig.YSpeedOffset);
                    return ComputeHeightChangeFromInitialVerticalSpeed(vSpeed);
                },
                (double newRemainingHeight) =>
                {
                    double initialVSpeed = ComputeInitialVerticalSpeedFromHeightChange(newRemainingHeight);
                    return Config.Stream.SetValue((float)initialVSpeed, MarioConfig.StructAddress + MarioConfig.YSpeedOffset);
                }
            );

            dictionary.Add("TrajectoryPeakHeight",
                () =>
                {
                    float vSpeed = Config.Stream.GetSingle(MarioConfig.StructAddress + MarioConfig.YSpeedOffset);
                    double remainingHeight = ComputeHeightChangeFromInitialVerticalSpeed(vSpeed);
                    float marioY = Config.Stream.GetSingle(MarioConfig.StructAddress + MarioConfig.YOffset);
                    double peakHeight = marioY + remainingHeight;
                    return peakHeight;
                },
                (double newPeakHeight) =>
                {
                    float marioY = Config.Stream.GetSingle(MarioConfig.StructAddress + MarioConfig.YOffset);
                    double newRemainingHeight = newPeakHeight - marioY;
                    double initialVSpeed = ComputeInitialVerticalSpeedFromHeightChange(newRemainingHeight);
                    return Config.Stream.SetValue((float)initialVSpeed, MarioConfig.StructAddress + MarioConfig.YSpeedOffset);
                }
            );

            dictionary.Add("DoubleJumpVerticalSpeed",
                () =>
                {
                    float hSpeed = Config.Stream.GetSingle(MarioConfig.StructAddress + MarioConfig.HSpeedOffset);
                    double vSpeed = ConvertDoubleJumpHSpeedToVSpeed(hSpeed);
                    return vSpeed;
                },
                (double newVSpeed) =>
                {
                    double newHSpeed = ConvertDoubleJumpVSpeedToHSpeed(newVSpeed);
                    return Config.Stream.SetValue((float)newHSpeed, MarioConfig.StructAddress + MarioConfig.HSpeedOffset);
                }
            );

            dictionary.Add("DoubleJumpHeight",
                () =>
                {
                    float hSpeed = Config.Stream.GetSingle(MarioConfig.StructAddress + MarioConfig.HSpeedOffset);
                    double vSpeed = ConvertDoubleJumpHSpeedToVSpeed(hSpeed);
                    double doubleJumpHeight = ComputeHeightChangeFromInitialVerticalSpeed(vSpeed);
                    return doubleJumpHeight;
                },
                (double newHeight) =>
                {
                    double initialVSpeed = ComputeInitialVerticalSpeedFromHeightChange(newHeight);
                    double newHSpeed = ConvertDoubleJumpVSpeedToHSpeed(initialVSpeed);
                    return Config.Stream.SetValue((float)newHSpeed, MarioConfig.StructAddress + MarioConfig.HSpeedOffset);
                }
            );

            dictionary.Add("DoubleJumpPeakHeight",
                () =>
                {
                    float hSpeed = Config.Stream.GetSingle(MarioConfig.StructAddress + MarioConfig.HSpeedOffset);
                    double vSpeed = ConvertDoubleJumpHSpeedToVSpeed(hSpeed);
                    double doubleJumpHeight = ComputeHeightChangeFromInitialVerticalSpeed(vSpeed);
                    float marioY = Config.Stream.GetSingle(MarioConfig.StructAddress + MarioConfig.YOffset);
                    double doubleJumpPeakHeight = marioY + doubleJumpHeight;
                    return doubleJumpPeakHeight;
                },
                (double newPeakHeight) =>
                {
                    float marioY = Config.Stream.GetSingle(MarioConfig.StructAddress + MarioConfig.YOffset);
                    double newHeight = newPeakHeight - marioY;
                    double initialVSpeed = ComputeInitialVerticalSpeedFromHeightChange(newHeight);
                    double newHSpeed = ConvertDoubleJumpVSpeedToHSpeed(initialVSpeed);
                    return Config.Stream.SetValue((float)newHSpeed, MarioConfig.StructAddress + MarioConfig.HSpeedOffset);
                }
            );

            dictionary.Add("MovementX",
                () =>
                {
                    float endX = Config.Stream.GetSingle(MiscConfig.HackedAreaAddress + 0x10);
                    float startX = Config.Stream.GetSingle(MiscConfig.HackedAreaAddress + 0x1C);
                    return endX - startX;
                },
                SpecialVariableDefaults<float>.DEFAULT_SETTER);

            dictionary.Add("MovementY",
                () =>
                {
                    float endY = Config.Stream.GetSingle(MiscConfig.HackedAreaAddress + 0x14);
                    float startY = Config.Stream.GetSingle(MiscConfig.HackedAreaAddress + 0x20);
                    return endY - startY;
                },
                SpecialVariableDefaults<float>.DEFAULT_SETTER);

            dictionary.Add("MovementZ",
                () =>
                {
                    float endZ = Config.Stream.GetSingle(MiscConfig.HackedAreaAddress + 0x18);
                    float startZ = Config.Stream.GetSingle(MiscConfig.HackedAreaAddress + 0x24);
                    return endZ - startZ;
                },
                SpecialVariableDefaults<float>.DEFAULT_SETTER);

            dictionary.Add("MovementForwards",
                () =>
                {
                    float endX = Config.Stream.GetSingle(MiscConfig.HackedAreaAddress + 0x10);
                    float startX = Config.Stream.GetSingle(MiscConfig.HackedAreaAddress + 0x1C);
                    float movementX = endX - startX;
                    float endZ = Config.Stream.GetSingle(MiscConfig.HackedAreaAddress + 0x18);
                    float startZ = Config.Stream.GetSingle(MiscConfig.HackedAreaAddress + 0x24);
                    float movementZ = endZ - startZ;
                    double movementHorizontal = MoreMath.GetHypotenuse(movementX, movementZ);
                    double movementAngle = MoreMath.AngleTo_AngleUnits(movementX, movementZ);
                    ushort marioAngle = Config.Stream.GetUInt16(MarioConfig.StructAddress + MarioConfig.FacingYawOffset);
                    (double movementSideways, double movementForwards) =
                        MoreMath.GetComponentsFromVectorRelatively(movementHorizontal, movementAngle, marioAngle);
                    return movementForwards;
                },
                SpecialVariableDefaults<double>.DEFAULT_SETTER);

            dictionary.Add("MovementSideways",
                () =>
                {
                    float endX = Config.Stream.GetSingle(MiscConfig.HackedAreaAddress + 0x10);
                    float startX = Config.Stream.GetSingle(MiscConfig.HackedAreaAddress + 0x1C);
                    float movementX = endX - startX;
                    float endZ = Config.Stream.GetSingle(MiscConfig.HackedAreaAddress + 0x18);
                    float startZ = Config.Stream.GetSingle(MiscConfig.HackedAreaAddress + 0x24);
                    float movementZ = endZ - startZ;
                    double movementHorizontal = MoreMath.GetHypotenuse(movementX, movementZ);
                    double movementAngle = MoreMath.AngleTo_AngleUnits(movementX, movementZ);
                    ushort marioAngle = Config.Stream.GetUInt16(MarioConfig.StructAddress + MarioConfig.FacingYawOffset);
                    (double movementSideways, double movementForwards) =
                        MoreMath.GetComponentsFromVectorRelatively(movementHorizontal, movementAngle, marioAngle);
                    return movementSideways;
                },
                SpecialVariableDefaults<double>.DEFAULT_SETTER);

            dictionary.Add("MovementHorizontal",
                () =>
                {
                    float endX = Config.Stream.GetSingle(MiscConfig.HackedAreaAddress + 0x10);
                    float startX = Config.Stream.GetSingle(MiscConfig.HackedAreaAddress + 0x1C);
                    float movementX = endX - startX;
                    float endZ = Config.Stream.GetSingle(MiscConfig.HackedAreaAddress + 0x18);
                    float startZ = Config.Stream.GetSingle(MiscConfig.HackedAreaAddress + 0x24);
                    float movementZ = endZ - startZ;
                    return MoreMath.GetHypotenuse(movementX, movementZ);
                },
                SpecialVariableDefaults<double>.DEFAULT_SETTER);

            dictionary.Add("MovementTotal",
                () =>
                {
                    float endX = Config.Stream.GetSingle(MiscConfig.HackedAreaAddress + 0x10);
                    float startX = Config.Stream.GetSingle(MiscConfig.HackedAreaAddress + 0x1C);
                    float movementX = endX - startX;
                    float endY = Config.Stream.GetSingle(MiscConfig.HackedAreaAddress + 0x14);
                    float startY = Config.Stream.GetSingle(MiscConfig.HackedAreaAddress + 0x20);
                    float movementY = endY - startY;
                    float endZ = Config.Stream.GetSingle(MiscConfig.HackedAreaAddress + 0x18);
                    float startZ = Config.Stream.GetSingle(MiscConfig.HackedAreaAddress + 0x24);
                    float movementZ = endZ - startZ;
                    return MoreMath.GetHypotenuse(movementX, movementY, movementZ);
                },
                SpecialVariableDefaults<double>.DEFAULT_SETTER);

            dictionary.Add("MovementAngle",
                () =>
                {
                    float endX = Config.Stream.GetSingle(MiscConfig.HackedAreaAddress + 0x10);
                    float startX = Config.Stream.GetSingle(MiscConfig.HackedAreaAddress + 0x1C);
                    float movementX = endX - startX;
                    float endZ = Config.Stream.GetSingle(MiscConfig.HackedAreaAddress + 0x18);
                    float startZ = Config.Stream.GetSingle(MiscConfig.HackedAreaAddress + 0x24);
                    float movementZ = endZ - startZ;
                    return MoreMath.AngleTo_AngleUnits(movementX, movementZ);
                },
                SpecialVariableDefaults<double>.DEFAULT_SETTER);

            dictionary.Add("QFrameCountEstimate",
                () =>
                {
                    float endX = Config.Stream.GetSingle(MiscConfig.HackedAreaAddress + 0x10);
                    float startX = Config.Stream.GetSingle(MiscConfig.HackedAreaAddress + 0x1C);
                    float movementX = endX - startX;
                    float endY = Config.Stream.GetSingle(MiscConfig.HackedAreaAddress + 0x14);
                    float startY = Config.Stream.GetSingle(MiscConfig.HackedAreaAddress + 0x20);
                    float movementY = endY - startY;
                    float endZ = Config.Stream.GetSingle(MiscConfig.HackedAreaAddress + 0x18);
                    float startZ = Config.Stream.GetSingle(MiscConfig.HackedAreaAddress + 0x24);
                    float movementZ = endZ - startZ;
                    float oldHSpeed = Config.Stream.GetSingle(MiscConfig.HackedAreaAddress + 0x28);
                    int qframes = (int)Math.Abs(Math.Round(Math.Sqrt(movementX * movementX + movementZ * movementZ) / (oldHSpeed / 4)));
                    if (qframes > 4)
                        return null;
                    return qframes;
                }
                ,
                SpecialVariableDefaults<int?>.DEFAULT_SETTER);

            dictionary.Add("DeltaYawIntendedFacing",
                () => GetDeltaYawIntendedFacing(),
                SpecialVariableDefaults<short>.DEFAULT_SETTER);

            dictionary.Add("DeltaYawIntendedBackwards",
                () => GetDeltaYawIntendedBackwards(),
                SpecialVariableDefaults<short>.DEFAULT_SETTER);

            dictionary.Add("MarioInGameDeltaYaw",
                () => GetDeltaInGameAngle(Config.Stream.GetUInt16(MarioConfig.StructAddress + MarioConfig.FacingYawOffset)),
                SpecialVariableDefaults<int>.DEFAULT_SETTER);

            dictionary.Add("FallHeight",
                () =>
                {
                    float peakHeight = Config.Stream.GetSingle(MarioConfig.StructAddress + MarioConfig.PeakHeightOffset);
                    float floorY = Config.Stream.GetSingle(MarioConfig.StructAddress + MarioConfig.FloorYOffset);
                    float fallHeight = peakHeight - floorY;
                    return fallHeight;
                },
                (double fallHeight) =>
                {
                    float floorY = Config.Stream.GetSingle(MarioConfig.StructAddress + MarioConfig.FloorYOffset);
                    double newPeakHeight = floorY + fallHeight;
                    return Config.Stream.SetValue((float)newPeakHeight, MarioConfig.StructAddress + MarioConfig.PeakHeightOffset);
                }
            );

            dictionary.Add("WalkingDistance",
                () =>
                {
                    float hSpeed = Config.Stream.GetSingle(MarioConfig.StructAddress + MarioConfig.HSpeedOffset);
                    float remainder = hSpeed % 1;
                    int numFrames = (int)Math.Abs(Math.Truncate(hSpeed)) + 1;
                    float sum = (hSpeed + remainder) * numFrames / 2;
                    return sum - hSpeed;
                },
                SpecialVariableDefaults<float>.DEFAULT_SETTER);

            dictionary.Add("ScheduleOffset",
                () => PositionAngle.ScheduleOffset,
                (int value) =>
                {
                    PositionAngle.ScheduleOffset = value;
                    return true;
                }
            );

            // HUD vars

            dictionary.Add("HudTimeText",
                () =>
                {
                    ushort time = Config.Stream.GetUInt16(MarioConfig.StructAddress + HudConfig.TimeOffset);
                    int totalDeciSeconds = time / 3;
                    int deciSecondComponent = totalDeciSeconds % 10;
                    int secondComponent = (totalDeciSeconds / 10) % 60;
                    int minuteComponent = (totalDeciSeconds / 600);
                    return minuteComponent + "'" + secondComponent.ToString("D2") + "\"" + deciSecondComponent;
                }
                ,
                (string timerString) =>
                {
                    if (timerString.Length == 0) timerString = "0" + timerString;
                    if (timerString.Length == 1) timerString = "\"" + timerString;
                    if (timerString.Length == 2) timerString = "0" + timerString;
                    if (timerString.Length == 3) timerString = "0" + timerString;
                    if (timerString.Length == 4) timerString = "'" + timerString;
                    if (timerString.Length == 5) timerString = "0" + timerString;

                    string minuteComponentString = timerString.Substring(0, timerString.Length - 5);
                    string leftMarker = timerString.Substring(timerString.Length - 5, 1);
                    string secondComponentString = timerString.Substring(timerString.Length - 4, 2);
                    string rightMarker = timerString.Substring(timerString.Length - 2, 1);
                    string deciSecondComponentString = timerString.Substring(timerString.Length - 1, 1);

                    if (leftMarker != "\"" && leftMarker != "'" && leftMarker != ".") return false;
                    if (rightMarker != "\"" && rightMarker != "'" && rightMarker != ".") return false;

                    int? minuteComponentNullable = ParsingUtilities.ParseIntNullable(minuteComponentString);
                    int? secondComponentNullable = ParsingUtilities.ParseIntNullable(secondComponentString);
                    int? deciSecondComponentNullable = ParsingUtilities.ParseIntNullable(deciSecondComponentString);

                    if (!minuteComponentNullable.HasValue ||
                        !secondComponentNullable.HasValue ||
                        !deciSecondComponentNullable.HasValue) return false;

                    int totalDeciSeconds =
                        deciSecondComponentNullable.Value +
                        secondComponentNullable.Value * 10 +
                        minuteComponentNullable.Value * 600;

                    int time = totalDeciSeconds * 3;
                    ushort timeUShort = ParsingUtilities.ParseUShortRoundingCapping(time);
                    return Config.Stream.SetValue(timeUShort, MarioConfig.StructAddress + HudConfig.TimeOffset);
                }
            );

            // Triangle vars

            dictionary.Add("Classification",
                BaseAddressType.Triangle,
                triAddress => TriangleDataModel.Create(triAddress).Classification.ToString(),
                SpecialVariableDefaults<string>.DEFAULT_SETTER_WITH_ADDRESS);

            dictionary.Add("TriangleTypeDescription",
                BaseAddressType.Triangle,
                triAddress => TriangleDataModel.Create(triAddress).Description,
                SpecialVariableDefaults<string>.DEFAULT_SETTER_WITH_ADDRESS);

            dictionary.Add("TriangleSlipperiness",
                BaseAddressType.Triangle,
                triAddress => TriangleDataModel.Create(triAddress).Slipperiness,
                SpecialVariableDefaults<short>.DEFAULT_SETTER_WITH_ADDRESS);

            dictionary.Add("TriangleSlipperinessDescription",
                BaseAddressType.Triangle,
                triAddress => TriangleDataModel.Create(triAddress).SlipperinessDescription,
                SpecialVariableDefaults<string>.DEFAULT_SETTER_WITH_ADDRESS);

            dictionary.Add("TriangleFrictionMultiplier",
                BaseAddressType.Triangle,
                triAddress => TriangleDataModel.Create(triAddress).FrictionMultiplier,
                SpecialVariableDefaults<double>.DEFAULT_SETTER_WITH_ADDRESS);

            dictionary.Add("TriangleExertion",
                BaseAddressType.Triangle,
                triAddress => TriangleDataModel.Create(triAddress).Exertion,
                SpecialVariableDefaults<bool>.DEFAULT_SETTER_WITH_ADDRESS);

            dictionary.Add("TriangleHorizontalNormal",
                BaseAddressType.Triangle,
                triAddress =>
                {
                    float normalX = Config.Stream.GetSingle(triAddress + TriangleOffsetsConfig.NormX);
                    float normalZ = Config.Stream.GetSingle(triAddress + TriangleOffsetsConfig.NormZ);
                    return (float)Math.Sqrt(normalX * normalX + normalZ * normalZ);
                },
                SpecialVariableDefaults<float>.DEFAULT_SETTER_WITH_ADDRESS);

            dictionary.Add("ClosestVertexIndex",
                BaseAddressType.Triangle,
                triAddress => GetClosestTriangleVertexIndex(triAddress),
                SpecialVariableDefaults<int>.DEFAULT_SETTER_WITH_ADDRESS);

            dictionary.Add("ClosestVertexX",
                BaseAddressType.Triangle,
                triAddress => (short)GetClosestTriangleVertexPosition(triAddress).X,
                SpecialVariableDefaults<short>.DEFAULT_SETTER_WITH_ADDRESS);

            dictionary.Add("ClosestVertexY",
                BaseAddressType.Triangle,
                triAddress => (short)GetClosestTriangleVertexPosition(triAddress).Y,
                SpecialVariableDefaults<short>.DEFAULT_SETTER_WITH_ADDRESS);

            dictionary.Add("ClosestVertexZ",
                BaseAddressType.Triangle,
                triAddress => (short)GetClosestTriangleVertexPosition(triAddress).Z,
                SpecialVariableDefaults<short>.DEFAULT_SETTER_WITH_ADDRESS);

            dictionary.Add("Steepness",
                BaseAddressType.Triangle,
                triAddress => MoreMath.RadiansToAngleUnits(Math.Acos(TriangleDataModel.Create(triAddress).NormY)),
                SpecialVariableDefaults<double>.DEFAULT_SETTER_WITH_ADDRESS);

            dictionary.Add("UpHillAngle",
                BaseAddressType.Triangle,
                triAddress => GetTriangleUphillAngle(triAddress),
                SpecialVariableDefaults<double>.DEFAULT_SETTER_WITH_ADDRESS);

            dictionary.Add("DownHillAngle",
                BaseAddressType.Triangle,
                triAddress => MoreMath.ReverseAngle(GetTriangleUphillAngle(triAddress)),
                SpecialVariableDefaults<double>.DEFAULT_SETTER_WITH_ADDRESS);

            dictionary.Add("LeftHillAngle",
                BaseAddressType.Triangle,
                triAddress => MoreMath.RotateAngleCCW(GetTriangleUphillAngle(triAddress), 0x4000),
                SpecialVariableDefaults<double>.DEFAULT_SETTER_WITH_ADDRESS);

            dictionary.Add("RightHillAngle",
                BaseAddressType.Triangle,
                triAddress => MoreMath.RotateAngleCW(GetTriangleUphillAngle(triAddress), 0x4000),
                SpecialVariableDefaults<double>.DEFAULT_SETTER_WITH_ADDRESS);

            dictionary.Add("UpHillDeltaAngle",
                BaseAddressType.Triangle,
                triAddress =>
                {
                    ushort marioAngle = Config.Stream.GetUInt16(MarioConfig.StructAddress + MarioConfig.FacingYawOffset);
                    double uphillAngle = GetTriangleUphillAngle(triAddress);
                    double angleDiff = marioAngle - uphillAngle;
                    return MoreMath.NormalizeAngleDoubleSigned(angleDiff);
                },
                (double angleDiff, uint triAddress) =>
                {
                    double uphillAngle = GetTriangleUphillAngle(triAddress);
                    double newMarioAngleDouble = uphillAngle + angleDiff;
                    ushort newMarioAngleUShort = MoreMath.NormalizeAngleUshort(newMarioAngleDouble);
                    return Config.Stream.SetValue(
                        newMarioAngleUShort, MarioConfig.StructAddress + MarioConfig.FacingYawOffset);
                }
            );

            dictionary.Add("DownHillDeltaAngle",
                BaseAddressType.Triangle,
                triAddress =>
                {
                    ushort marioAngle = Config.Stream.GetUInt16(MarioConfig.StructAddress + MarioConfig.FacingYawOffset);
                    double uphillAngle = GetTriangleUphillAngle(triAddress);
                    double downhillAngle = MoreMath.ReverseAngle(uphillAngle);
                    double angleDiff = marioAngle - downhillAngle;
                    return MoreMath.NormalizeAngleDoubleSigned(angleDiff);
                },
                (double angleDiff, uint triAddress) =>
                {
                    double uphillAngle = GetTriangleUphillAngle(triAddress);
                    double downhillAngle = MoreMath.ReverseAngle(uphillAngle);
                    double newMarioAngleDouble = downhillAngle + angleDiff;
                    ushort newMarioAngleUShort = MoreMath.NormalizeAngleUshort(newMarioAngleDouble);
                    return Config.Stream.SetValue(newMarioAngleUShort, MarioConfig.StructAddress + MarioConfig.FacingYawOffset);
                }
            );

            dictionary.Add("LeftHillDeltaAngle",
                BaseAddressType.Triangle,
                triAddress =>
                {
                    ushort marioAngle = Config.Stream.GetUInt16(MarioConfig.StructAddress + MarioConfig.FacingYawOffset);
                    double uphillAngle = GetTriangleUphillAngle(triAddress);
                    double lefthillAngle = MoreMath.RotateAngleCCW(uphillAngle, 16384);
                    double angleDiff = marioAngle - lefthillAngle;
                    return MoreMath.NormalizeAngleDoubleSigned(angleDiff);
                }
                ,
                (double angleDiff, uint triAddress) =>
                {
                    double uphillAngle = GetTriangleUphillAngle(triAddress);
                    double lefthillAngle = MoreMath.RotateAngleCCW(uphillAngle, 16384);
                    double newMarioAngleDouble = lefthillAngle + angleDiff;
                    ushort newMarioAngleUShort = MoreMath.NormalizeAngleUshort(newMarioAngleDouble);
                    return Config.Stream.SetValue(newMarioAngleUShort, MarioConfig.StructAddress + MarioConfig.FacingYawOffset);
                }
            );

            dictionary.Add("RightHillDeltaAngle",
                BaseAddressType.Triangle,
                triAddress =>
                {
                    ushort marioAngle = Config.Stream.GetUInt16(MarioConfig.StructAddress + MarioConfig.FacingYawOffset);
                    double uphillAngle = GetTriangleUphillAngle(triAddress);
                    double righthillAngle = MoreMath.RotateAngleCW(uphillAngle, 16384);
                    double angleDiff = marioAngle - righthillAngle;
                    return MoreMath.NormalizeAngleDoubleSigned(angleDiff);
                }
                ,
                (double angleDiff, uint triAddress) =>
                {
                    double uphillAngle = GetTriangleUphillAngle(triAddress);
                    double righthillAngle = MoreMath.RotateAngleCW(uphillAngle, 16384);
                    double newMarioAngleDouble = righthillAngle + angleDiff;
                    ushort newMarioAngleUShort = MoreMath.NormalizeAngleUshort(newMarioAngleDouble);
                    return Config.Stream.SetValue(newMarioAngleUShort, MarioConfig.StructAddress + MarioConfig.FacingYawOffset);
                }
            );

            dictionary.Add("HillStatus",
                BaseAddressType.Triangle,
                triAddress =>
                {
                    ushort marioAngle = Config.Stream.GetUInt16(MarioConfig.StructAddress + MarioConfig.FacingYawOffset);
                    double uphillAngle = GetTriangleUphillAngle(triAddress);
                    if (Double.IsNaN(uphillAngle)) return "No Hill";
                    double angleDiff = marioAngle - uphillAngle;
                    angleDiff = MoreMath.NormalizeAngleDoubleSigned(angleDiff);
                    bool uphill = angleDiff >= -16384 && angleDiff <= 16384;
                    return uphill ? "Uphill" : "Downhill";
                },
                SpecialVariableDefaults<string>.DEFAULT_SETTER_WITH_ADDRESS);

            dictionary.Add("WallKickAngleAway",
                BaseAddressType.Triangle,
                triAddress =>
                {
                    PositionAngle marioPos = PositionAngle.Mario;
                    double uphillAngle = GetTriangleUphillAngle(triAddress);
                    double angleDiff = marioPos.Angle - uphillAngle;
                    int angleDiffShort = MoreMath.NormalizeAngleShort(angleDiff);
                    int angleDiffAbs = Math.Abs(angleDiffShort);
                    return angleDiffAbs - 8192;
                },
                (double angleAway, uint triAddress) =>
                {
                    PositionAngle marioPos = PositionAngle.Mario;
                    double uphillAngle = GetTriangleUphillAngle(triAddress);
                    double oldAngleDiff = marioPos.Angle - uphillAngle;
                    int oldAngleDiffShort = MoreMath.NormalizeAngleShort(oldAngleDiff);
                    int signMultiplier = oldAngleDiffShort >= 0 ? 1 : -1;

                    double angleDiffAbs = angleAway + 8192;
                    double angleDiff = angleDiffAbs * signMultiplier;
                    double marioAngleDouble = uphillAngle + angleDiff;
                    ushort marioAngleUShort = MoreMath.NormalizeAngleUshort(marioAngleDouble);

                    return Config.Stream.SetValue(marioAngleUShort, MarioConfig.StructAddress + MarioConfig.FacingYawOffset);
                }
            );

            dictionary.Add("WallKickPostAngle",
                BaseAddressType.Triangle,
                triAddress =>
                {
                    ushort marioAngle = Config.Stream.GetUInt16(MarioConfig.StructAddress + MarioConfig.FacingYawOffset);
                    float normX = Config.Stream.GetSingle(triAddress + TriangleOffsetsConfig.NormX);
                    float normZ = Config.Stream.GetSingle(triAddress + TriangleOffsetsConfig.NormZ);
                    ushort wallAngle = InGameTrigUtilities.InGameATan(normZ, normX);
                    return MoreMath.NormalizeAngleUshort(wallAngle - (marioAngle - wallAngle) + 32768);
                },
                SpecialVariableDefaults<ushort>.DEFAULT_SETTER_WITH_ADDRESS);

            dictionary.Add("WallHugAngleAway",
                BaseAddressType.Triangle,
                triAddress =>
                {
                    PositionAngle marioPos = PositionAngle.Mario;
                    double uphillAngle = GetTriangleUphillAngle(triAddress);
                    double angleDiff = marioPos.Angle - uphillAngle;
                    int angleDiffShort = MoreMath.NormalizeAngleShort(angleDiff);
                    int angleDiffAbs = Math.Abs(angleDiffShort);
                    int angleAway = angleDiffAbs - (0x8000 - 0x5555);
                    return angleAway;
                },
                (double angleAway, uint triAddress) =>
                {
                    PositionAngle marioPos = PositionAngle.Mario;
                    double uphillAngle = GetTriangleUphillAngle(triAddress);
                    double oldAngleDiff = marioPos.Angle - uphillAngle;
                    int oldAngleDiffShort = MoreMath.NormalizeAngleShort(oldAngleDiff);
                    int signMultiplier = oldAngleDiffShort >= 0 ? 1 : -1;

                    double angleDiffAbs = angleAway + 0x5555;
                    double angleDiff = angleDiffAbs * signMultiplier;
                    double marioAngleDouble = uphillAngle + angleDiff;
                    ushort marioAngleUShort = MoreMath.NormalizeAngleUshort(marioAngleDouble);

                    return Config.Stream.SetValue(marioAngleUShort, MarioConfig.StructAddress + MarioConfig.FacingYawOffset);
                }
            );

            dictionary.Add("DistanceAboveFloor",
                () =>
                {
                    float marioY = Config.Stream.GetSingle(MarioConfig.StructAddress + MarioConfig.YOffset);
                    float floorY = Config.Stream.GetSingle(MarioConfig.StructAddress + MarioConfig.FloorYOffset);
                    float distAboveFloor = marioY - floorY;
                    return distAboveFloor;
                },
                (double distAbove) =>
                {
                    float floorY = Config.Stream.GetSingle(MarioConfig.StructAddress + MarioConfig.FloorYOffset);
                    double newMarioY = floorY + distAbove;
                    return Config.Stream.SetValue((float)newMarioY, MarioConfig.StructAddress + MarioConfig.YOffset);
                }
            );

            dictionary.Add("DistanceBelowCeiling",
                () =>
                {
                    float marioY = Config.Stream.GetSingle(MarioConfig.StructAddress + MarioConfig.YOffset);
                    float ceilingY = Config.Stream.GetSingle(MarioConfig.StructAddress + MarioConfig.CeilingYOffset);
                    float distBelowCeiling = ceilingY - marioY;
                    return distBelowCeiling;
                },
                (double distBelow) =>
                {
                    float ceilingY = Config.Stream.GetSingle(MarioConfig.StructAddress + MarioConfig.CeilingYOffset);
                    double newMarioY = ceilingY - distBelow;
                    return Config.Stream.SetValue((float)newMarioY, MarioConfig.StructAddress + MarioConfig.YOffset);
                }
            );

            dictionary.Add("NormalDistAway",
                BaseAddressType.Triangle,
                triAddress =>
                {
                    PositionAngle marioPos = PositionAngle.Mario;
                    TriangleDataModel triStruct = TriangleDataModel.Create(triAddress);
                    double normalDistAway =
                        marioPos.X * triStruct.NormX +
                        marioPos.Y * triStruct.NormY +
                        marioPos.Z * triStruct.NormZ +
                        triStruct.NormOffset;
                    return normalDistAway;
                },
                (double distAway, uint triAddress) =>
                {
                    PositionAngle marioPos = PositionAngle.Mario;
                    TriangleDataModel triStruct = TriangleDataModel.Create(triAddress);

                    double missingDist = distAway -
                                         marioPos.X * triStruct.NormX -
                                         marioPos.Y * triStruct.NormY -
                                         marioPos.Z * triStruct.NormZ -
                                         triStruct.NormOffset;

                    double xDiff = missingDist * triStruct.NormX;
                    double yDiff = missingDist * triStruct.NormY;
                    double zDiff = missingDist * triStruct.NormZ;

                    double newMarioX = marioPos.X + xDiff;
                    double newMarioY = marioPos.Y + yDiff;
                    double newMarioZ = marioPos.Z + zDiff;

                    return marioPos.SetValues(x: newMarioX, y: newMarioY, z: newMarioZ);
                }
            );

            dictionary.Add("VerticalDistAway",
                BaseAddressType.Triangle,
                triAddress =>
                {
                    PositionAngle marioPos = PositionAngle.Mario;
                    TriangleDataModel triStruct = TriangleDataModel.Create(triAddress);
                    double verticalDistAway =
                        marioPos.Y + (marioPos.X * triStruct.NormX + marioPos.Z * triStruct.NormZ + triStruct.NormOffset) / triStruct.NormY;
                    return verticalDistAway;
                },
                (double distAbove, uint triAddress) =>
                {
                    PositionAngle marioPos = PositionAngle.Mario;
                    TriangleDataModel triStruct = TriangleDataModel.Create(triAddress);
                    double newMarioY = distAbove - (marioPos.X * triStruct.NormX + marioPos.Z * triStruct.NormZ + triStruct.NormOffset) / triStruct.NormY;
                    return Config.Stream.SetValue((float)newMarioY, MarioConfig.StructAddress + MarioConfig.YOffset);
                }
            );

            dictionary.Add("HeightOnTriangle",
                BaseAddressType.Triangle,
                triAddress =>
                {
                    PositionAngle marioPos = PositionAngle.Mario;
                    TriangleDataModel triStruct = TriangleDataModel.Create(triAddress);
                    return triStruct.GetHeightOnTriangle(marioPos.X, marioPos.Z);
                },
                SpecialVariableDefaults<double>.DEFAULT_SETTER_WITH_ADDRESS);

            dictionary.Add("MaxHSpeedUphill",
                BaseAddressType.Triangle,
                triAddress => GetMaxHorizontalSpeedOnTriangle(triAddress, true, false),
                SpecialVariableDefaults<double>.DEFAULT_SETTER_WITH_ADDRESS);

            dictionary.Add("MaxHSpeedUphillAtAngle",
                BaseAddressType.Triangle,
                triAddress => GetMaxHorizontalSpeedOnTriangle(triAddress, true, true),
                SpecialVariableDefaults<double>.DEFAULT_SETTER_WITH_ADDRESS);

            dictionary.Add("MaxHSpeedDownhill",
                BaseAddressType.Triangle,
                triAddress => GetMaxHorizontalSpeedOnTriangle(triAddress, false, false),
                SpecialVariableDefaults<double>.DEFAULT_SETTER_WITH_ADDRESS);

            dictionary.Add("MaxHSpeedDownhillAtAngle",
                BaseAddressType.Triangle,
                triAddress => GetMaxHorizontalSpeedOnTriangle(triAddress, false, true),
                SpecialVariableDefaults<double>.DEFAULT_SETTER_WITH_ADDRESS);

            dictionary.Add("TriangleCells",
                BaseAddressType.Triangle,
                triAddress =>
                {
                    TriangleDataModel tri = TriangleDataModel.Create(triAddress);
                    short minCellX = lower_cell_index(tri.GetMinX());
                    short maxCellX = upper_cell_index(tri.GetMaxX());
                    short minCellZ = lower_cell_index(tri.GetMinZ());
                    short maxCellZ = upper_cell_index(tri.GetMaxZ());
                    return string.Format("X:{0}-{1},Z:{2}-{3}",
                        minCellX, maxCellX, minCellZ, maxCellZ);
                },
                SpecialVariableDefaults<string>.DEFAULT_SETTER_WITH_ADDRESS);

            dictionary.Add("MarioCell",
                () =>
                {
                    (int cellX, int cellZ) = GetMarioCell();
                    return string.Format("X:{0},Z:{1}", cellX, cellZ);
                },
                SpecialVariableDefaults<string>.DEFAULT_SETTER);

            dictionary.Add("ObjectTriCount",
                () =>
                {
                    int totalTriangleCount = Config.Stream.GetInt32(TriangleConfig.TotalTriangleCountAddress);
                    int levelTriangleCount = Config.Stream.GetInt32(TriangleConfig.LevelTriangleCountAddress);
                    int objectTriangleCount = totalTriangleCount - levelTriangleCount;
                    return objectTriangleCount;
                },
                SpecialVariableDefaults<int>.DEFAULT_SETTER);

            dictionary.Add("CurrentTriangleIndex",
                BaseAddressType.Triangle,
                triAddress =>
                {
                    uint triangleListStartAddress = Config.Stream.GetUInt32(TriangleConfig.TriangleListPointerAddress);
                    uint structSize = TriangleConfig.TriangleStructSize;
                    int addressDiff = triAddress >= triangleListStartAddress
                        ? (int)(triAddress - triangleListStartAddress)
                        : (int)(-1 * (triangleListStartAddress - triAddress));
                    int indexGuess = (int)(addressDiff / structSize);
                    if (triangleListStartAddress + indexGuess * structSize == triAddress)
                        return indexGuess;
                    return null;
                },
                (int? index, uint triAddress) =>
                {
                    if (!index.HasValue)
                        return false;
                    uint triangleListStartAddress = Config.Stream.GetUInt32(TriangleConfig.TriangleListPointerAddress);
                    uint structSize = TriangleConfig.TriangleStructSize;
                    uint newTriAddress = (uint)(triangleListStartAddress + index * structSize);

                    AccessScope<StroopMainForm>.content.GetTab<Tabs.TrianglesTab>().SetCustomTriangleAddresses(newTriAddress);
                    return true;
                }
            );

            dictionary.Add("CurrentTriangleObjectIndex",
                BaseAddressType.Triangle,
                triAddress =>
                {
                    uint objAddress = Config.Stream.GetUInt32(triAddress + TriangleOffsetsConfig.AssociatedObject);
                    if (objAddress == 0)
                        return null;
                    List<TriangleDataModel> objTris = TriangleUtilities.GetObjectTrianglesForObject(objAddress);
                    for (int i = 0; i < objTris.Count; i++)
                        if (objTris[i].Address == triAddress)
                            return i;
                    return null;
                },
                SpecialVariableDefaults<int?>.DEFAULT_SETTER_WITH_ADDRESS
            );

            dictionary.Add("CurrentTriangleAddress",
                () => AccessScope<StroopMainForm>.content.GetTab<Tabs.TrianglesTab>().TriangleAddresses,
                (uint address) =>
                {
                    AccessScope<StroopMainForm>.content.GetTab<Tabs.TrianglesTab>().SetCustomTriangleAddresses(address);
                    return true.Yield();
                }
            );

            dictionary.Add("CurrentCellsTriangleAddress",
                () => AccessScope<StroopMainForm>.content.GetTab<Tabs.CellsTab>().TriangleAddress,
                (uint address) =>
                {
                    AccessScope<StroopMainForm>.content.GetTab<Tabs.CellsTab>().TriangleAddress = address;
                    return true;
                }
            );

            dictionary.Add("ObjectNodeCount",
                () =>
                {
                    int totalNodeCount = Config.Stream.GetInt32(TriangleConfig.TotalNodeCountAddress);
                    int levelNodeCount = Config.Stream.GetInt32(TriangleConfig.LevelNodeCountAddress);
                    return totalNodeCount - levelNodeCount;
                }
                ,
                SpecialVariableDefaults<int>.DEFAULT_SETTER);

            dictionary.Add("TriMinX",
                BaseAddressType.Triangle,
                triAddress => TriangleDataModel.Create(triAddress).GetMinX(),
                SpecialVariableDefaults<short>.DEFAULT_SETTER_WITH_ADDRESS);

            dictionary.Add("TriMaxX",
                BaseAddressType.Triangle,
                triAddress => TriangleDataModel.Create(triAddress).GetMaxX(),
                SpecialVariableDefaults<short>.DEFAULT_SETTER_WITH_ADDRESS);

            dictionary.Add("TriMinY",
                BaseAddressType.Triangle,
                triAddress => TriangleDataModel.Create(triAddress).GetMinY(),
                SpecialVariableDefaults<short>.DEFAULT_SETTER_WITH_ADDRESS);

            dictionary.Add("TriMaxY",
                BaseAddressType.Triangle,
                triAddress => TriangleDataModel.Create(triAddress).GetMaxY(),
                SpecialVariableDefaults<short>.DEFAULT_SETTER_WITH_ADDRESS);

            dictionary.Add("TriMinZ",
                BaseAddressType.Triangle,
                triAddress => TriangleDataModel.Create(triAddress).GetMinZ(),
                SpecialVariableDefaults<short>.DEFAULT_SETTER_WITH_ADDRESS);

            dictionary.Add("TriMaxZ",
                BaseAddressType.Triangle,
                triAddress => TriangleDataModel.Create(triAddress).GetMaxZ(),
                SpecialVariableDefaults<short>.DEFAULT_SETTER_WITH_ADDRESS);

            dictionary.Add("TriRangeX",
                BaseAddressType.Triangle,
                triAddress => TriangleDataModel.Create(triAddress).GetRangeX(),
                SpecialVariableDefaults<int>.DEFAULT_SETTER_WITH_ADDRESS);

            dictionary.Add("TriRangeY",
                BaseAddressType.Triangle,
                triAddress => TriangleDataModel.Create(triAddress).GetRangeY(),
                SpecialVariableDefaults<int>.DEFAULT_SETTER_WITH_ADDRESS);

            dictionary.Add("TriRangeZ",
                BaseAddressType.Triangle,
                triAddress => TriangleDataModel.Create(triAddress).GetRangeZ(),
                SpecialVariableDefaults<int>.DEFAULT_SETTER_WITH_ADDRESS);

            dictionary.Add("TriMidpointX",
                BaseAddressType.Triangle,
                triAddress => TriangleDataModel.Create(triAddress).GetMidpointX(),
                SpecialVariableDefaults<double>.DEFAULT_SETTER_WITH_ADDRESS);

            dictionary.Add("TriMidpointY",
                BaseAddressType.Triangle,
                triAddress => TriangleDataModel.Create(triAddress).GetMidpointY(),
                SpecialVariableDefaults<double>.DEFAULT_SETTER_WITH_ADDRESS);

            dictionary.Add("TriMidpointZ",
                BaseAddressType.Triangle,
                triAddress => TriangleDataModel.Create(triAddress).GetMidpointZ(),
                SpecialVariableDefaults<double>.DEFAULT_SETTER_WITH_ADDRESS);

            dictionary.Add("DistanceToLine12",
                BaseAddressType.Triangle,
                triAddress =>
                {
                    PositionAngle marioPos = PositionAngle.Mario;
                    TriangleDataModel triStruct = TriangleDataModel.Create(triAddress);
                    double signedDistToLine12 = MoreMath.GetSignedDistanceFromPointToLine(
                        marioPos.X, marioPos.Z,
                        triStruct.X1, triStruct.Z1,
                        triStruct.X2, triStruct.Z2,
                        triStruct.X3, triStruct.Z3, 1, 2,
                        TriangleDataModel.Create(triAddress).Classification);
                    return signedDistToLine12;
                },
                (double dist, uint triAddress) =>
                {
                    PositionAngle marioPos = PositionAngle.Mario;
                    TriangleDataModel triStruct = TriangleDataModel.Create(triAddress);
                    double signedDistToLine12 = MoreMath.GetSignedDistanceFromPointToLine(
                        marioPos.X, marioPos.Z,
                        triStruct.X1, triStruct.Z1,
                        triStruct.X2, triStruct.Z2,
                        triStruct.X3, triStruct.Z3, 1, 2,
                        TriangleDataModel.Create(triAddress).Classification);

                    double missingDist = dist - signedDistToLine12;
                    double lineAngle = MoreMath.AngleTo_AngleUnits(triStruct.X1, triStruct.Z1, triStruct.X2, triStruct.Z2);
                    bool floorTri = MoreMath.IsPointLeftOfLine(triStruct.X3, triStruct.Z3, triStruct.X1, triStruct.Z1, triStruct.X2, triStruct.Z2);
                    double inwardAngle = floorTri ? MoreMath.RotateAngleCCW(lineAngle, 16384) : MoreMath.RotateAngleCW(lineAngle, 16384);

                    (double xDiff, double zDiff) = MoreMath.GetComponentsFromVector(missingDist, inwardAngle);
                    double newMarioX = marioPos.X + xDiff;
                    double newMarioZ = marioPos.Z + zDiff;
                    return marioPos.SetValues(x: newMarioX, z: newMarioZ);
                }
            );

            dictionary.Add("DistanceToLine23",
                BaseAddressType.Triangle,
                triAddress =>
                {
                    PositionAngle marioPos = PositionAngle.Mario;
                    TriangleDataModel triStruct = TriangleDataModel.Create(triAddress);
                    double signedDistToLine23 = MoreMath.GetSignedDistanceFromPointToLine(
                        marioPos.X, marioPos.Z,
                        triStruct.X1, triStruct.Z1,
                        triStruct.X2, triStruct.Z2,
                        triStruct.X3, triStruct.Z3, 2, 3,
                        TriangleDataModel.Create(triAddress).Classification);
                    return signedDistToLine23;
                },
                (double dist, uint triAddress) =>
                {
                    PositionAngle marioPos = PositionAngle.Mario;
                    TriangleDataModel triStruct = TriangleDataModel.Create(triAddress);
                    double signedDistToLine23 = MoreMath.GetSignedDistanceFromPointToLine(
                        marioPos.X, marioPos.Z,
                        triStruct.X1, triStruct.Z1,
                        triStruct.X2, triStruct.Z2,
                        triStruct.X3, triStruct.Z3, 2, 3,
                        TriangleDataModel.Create(triAddress).Classification);

                    double missingDist = dist - signedDistToLine23;
                    double lineAngle = MoreMath.AngleTo_AngleUnits(triStruct.X2, triStruct.Z2, triStruct.X3, triStruct.Z3);
                    bool floorTri = MoreMath.IsPointLeftOfLine(triStruct.X3, triStruct.Z3, triStruct.X1, triStruct.Z1, triStruct.X2, triStruct.Z2);
                    double inwardAngle = floorTri ? MoreMath.RotateAngleCCW(lineAngle, 16384) : MoreMath.RotateAngleCW(lineAngle, 16384);

                    (double xDiff, double zDiff) = MoreMath.GetComponentsFromVector(missingDist, inwardAngle);
                    double newMarioX = marioPos.X + xDiff;
                    double newMarioZ = marioPos.Z + zDiff;
                    return marioPos.SetValues(x: newMarioX, z: newMarioZ);
                }
            );

            dictionary.Add("DistanceToLine31",
                BaseAddressType.Triangle,
                triAddress =>
                {
                    PositionAngle marioPos = PositionAngle.Mario;
                    TriangleDataModel triStruct = TriangleDataModel.Create(triAddress);
                    double signedDistToLine31 = MoreMath.GetSignedDistanceFromPointToLine(
                        marioPos.X, marioPos.Z,
                        triStruct.X1, triStruct.Z1,
                        triStruct.X2, triStruct.Z2,
                        triStruct.X3, triStruct.Z3, 3, 1,
                        TriangleDataModel.Create(triAddress).Classification);
                    return signedDistToLine31;
                },
                (double dist, uint triAddress) =>
                {
                    PositionAngle marioPos = PositionAngle.Mario;
                    TriangleDataModel triStruct = TriangleDataModel.Create(triAddress);
                    double signedDistToLine31 = MoreMath.GetSignedDistanceFromPointToLine(
                        marioPos.X, marioPos.Z,
                        triStruct.X1, triStruct.Z1,
                        triStruct.X2, triStruct.Z2,
                        triStruct.X3, triStruct.Z3, 3, 1,
                        TriangleDataModel.Create(triAddress).Classification);

                    double missingDist = dist - signedDistToLine31;
                    double lineAngle = MoreMath.AngleTo_AngleUnits(triStruct.X3, triStruct.Z3, triStruct.X1, triStruct.Z1);
                    bool floorTri = MoreMath.IsPointLeftOfLine(triStruct.X3, triStruct.Z3, triStruct.X1, triStruct.Z1, triStruct.X2, triStruct.Z2);
                    double inwardAngle = floorTri ? MoreMath.RotateAngleCCW(lineAngle, 16384) : MoreMath.RotateAngleCW(lineAngle, 16384);

                    (double xDiff, double zDiff) = MoreMath.GetComponentsFromVector(missingDist, inwardAngle);
                    double newMarioX = marioPos.X + xDiff;
                    double newMarioZ = marioPos.Z + zDiff;
                    return marioPos.SetValues(x: newMarioX, z: newMarioZ);
                }
            );

            dictionary.Add("DeltaAngleLine12",
                BaseAddressType.Triangle,
                triAddress =>
                {
                    PositionAngle marioPos = PositionAngle.Mario;
                    TriangleDataModel triStruct = TriangleDataModel.Create(triAddress);
                    double angleV1ToV2 = MoreMath.AngleTo_AngleUnits(
                        triStruct.X1, triStruct.Z1, triStruct.X2, triStruct.Z2);
                    double angleDiff = marioPos.Angle - angleV1ToV2;
                    return MoreMath.NormalizeAngleDoubleSigned(angleDiff);
                },
                (double angleDiff, uint triAddress) =>
                {
                    PositionAngle marioPos = PositionAngle.Mario;
                    TriangleDataModel triStruct = TriangleDataModel.Create(triAddress);
                    ;
                    double angleV1ToV2 = MoreMath.AngleTo_AngleUnits(
                        triStruct.X1, triStruct.Z1, triStruct.X2, triStruct.Z2);
                    double newMarioAngleDouble = angleV1ToV2 + angleDiff;
                    ushort newMarioAngleUShort = MoreMath.NormalizeAngleUshort(newMarioAngleDouble);
                    return Config.Stream.SetValue(
                        newMarioAngleUShort, MarioConfig.StructAddress + MarioConfig.FacingYawOffset);
                }
            );

            dictionary.Add("DeltaAngleLine21",
                BaseAddressType.Triangle,
                triAddress =>
                {
                    PositionAngle marioPos = PositionAngle.Mario;
                    TriangleDataModel triStruct = TriangleDataModel.Create(triAddress);
                    double angleV2ToV1 = MoreMath.AngleTo_AngleUnits(
                        triStruct.X2, triStruct.Z2, triStruct.X1, triStruct.Z1);
                    double angleDiff = marioPos.Angle - angleV2ToV1;
                    return MoreMath.NormalizeAngleDoubleSigned(angleDiff);
                },
                (double angleDiff, uint triAddress) =>
                {
                    PositionAngle marioPos = PositionAngle.Mario;
                    TriangleDataModel triStruct = TriangleDataModel.Create(triAddress);
                    double angleV2ToV1 = MoreMath.AngleTo_AngleUnits(
                        triStruct.X2, triStruct.Z2, triStruct.X1, triStruct.Z1);
                    double newMarioAngleDouble = angleV2ToV1 + angleDiff;
                    ushort newMarioAngleUShort = MoreMath.NormalizeAngleUshort(newMarioAngleDouble);
                    return Config.Stream.SetValue(
                        newMarioAngleUShort, MarioConfig.StructAddress + MarioConfig.FacingYawOffset);
                }
            );

            dictionary.Add("DeltaAngleLine23",
                BaseAddressType.Triangle,
                triAddress =>
                {
                    PositionAngle marioPos = PositionAngle.Mario;
                    TriangleDataModel triStruct = TriangleDataModel.Create(triAddress);
                    double angleV2ToV3 = MoreMath.AngleTo_AngleUnits(
                        triStruct.X2, triStruct.Z2, triStruct.X3, triStruct.Z3);
                    double angleDiff = marioPos.Angle - angleV2ToV3;
                    return MoreMath.NormalizeAngleDoubleSigned(angleDiff);
                },
                (double angleDiff, uint triAddress) =>
                {
                    PositionAngle marioPos = PositionAngle.Mario;
                    TriangleDataModel triStruct = TriangleDataModel.Create(triAddress);
                    double angleV2ToV3 = MoreMath.AngleTo_AngleUnits(
                        triStruct.X2, triStruct.Z2, triStruct.X3, triStruct.Z3);
                    double newMarioAngleDouble = angleV2ToV3 + angleDiff;
                    ushort newMarioAngleUShort = MoreMath.NormalizeAngleUshort(newMarioAngleDouble);
                    return Config.Stream.SetValue(
                        newMarioAngleUShort, MarioConfig.StructAddress + MarioConfig.FacingYawOffset);
                });

            dictionary.Add("DeltaAngleLine32",
                BaseAddressType.Triangle,
                triAddress =>
                {
                    PositionAngle marioPos = PositionAngle.Mario;
                    TriangleDataModel triStruct = TriangleDataModel.Create(triAddress);
                    double angleV3ToV2 = MoreMath.AngleTo_AngleUnits(
                        triStruct.X3, triStruct.Z3, triStruct.X2, triStruct.Z2);
                    double angleDiff = marioPos.Angle - angleV3ToV2;
                    return MoreMath.NormalizeAngleDoubleSigned(angleDiff);
                },
                (double angleDiff, uint triAddress) =>
                {
                    PositionAngle marioPos = PositionAngle.Mario;
                    TriangleDataModel triStruct = TriangleDataModel.Create(triAddress);
                    double angleV3ToV2 = MoreMath.AngleTo_AngleUnits(
                        triStruct.X3, triStruct.Z3, triStruct.X2, triStruct.Z2);
                    double newMarioAngleDouble = angleV3ToV2 + angleDiff;
                    ushort newMarioAngleUShort = MoreMath.NormalizeAngleUshort(newMarioAngleDouble);
                    return Config.Stream.SetValue(
                        newMarioAngleUShort, MarioConfig.StructAddress + MarioConfig.FacingYawOffset);
                }
            );

            dictionary.Add("DeltaAngleLine31",
                BaseAddressType.Triangle,
                triAddress =>
                {
                    PositionAngle marioPos = PositionAngle.Mario;
                    TriangleDataModel triStruct = TriangleDataModel.Create(triAddress);
                    double angleV3ToV1 = MoreMath.AngleTo_AngleUnits(
                        triStruct.X3, triStruct.Z3, triStruct.X1, triStruct.Z1);
                    double angleDiff = marioPos.Angle - angleV3ToV1;
                    return MoreMath.NormalizeAngleDoubleSigned(angleDiff);
                },
                (double angleDiff, uint triAddress) =>
                {
                    PositionAngle marioPos = PositionAngle.Mario;
                    TriangleDataModel triStruct = TriangleDataModel.Create(triAddress);
                    double angleV3ToV1 = MoreMath.AngleTo_AngleUnits(
                        triStruct.X3, triStruct.Z3, triStruct.X1, triStruct.Z1);
                    double newMarioAngleDouble = angleV3ToV1 + angleDiff;
                    ushort newMarioAngleUShort = MoreMath.NormalizeAngleUshort(newMarioAngleDouble);
                    return Config.Stream.SetValue(
                        newMarioAngleUShort, MarioConfig.StructAddress + MarioConfig.FacingYawOffset);
                }
            );

            dictionary.Add("DeltaAngleLine13",
                BaseAddressType.Triangle,
                triAddress =>
                {
                    PositionAngle marioPos = PositionAngle.Mario;
                    TriangleDataModel triStruct = TriangleDataModel.Create(triAddress);
                    double angleV1ToV3 = MoreMath.AngleTo_AngleUnits(
                        triStruct.X1, triStruct.Z1, triStruct.X3, triStruct.Z3);
                    double angleDiff = marioPos.Angle - angleV1ToV3;
                    return MoreMath.NormalizeAngleDoubleSigned(angleDiff);
                },
                (double angleDiff, uint triAddress) =>
                {
                    PositionAngle marioPos = PositionAngle.Mario;
                    TriangleDataModel triStruct = TriangleDataModel.Create(triAddress);
                    double angleV1ToV3 = MoreMath.AngleTo_AngleUnits(
                        triStruct.X1, triStruct.Z1, triStruct.X3, triStruct.Z3);
                    double newMarioAngleDouble = angleV1ToV3 + angleDiff;
                    ushort newMarioAngleUShort = MoreMath.NormalizeAngleUshort(newMarioAngleDouble);
                    return Config.Stream.SetValue(
                        newMarioAngleUShort, MarioConfig.StructAddress + MarioConfig.FacingYawOffset);
                }
            );

            dictionary.Add("TriangleX1",
                BaseAddressType.Triangle,
                triAddress => TriangleOffsetsConfig.GetX1(triAddress),
                (short value, uint triAddress) => TriangleOffsetsConfig.SetX1(value, triAddress)
            );

            dictionary.Add("TriangleY1",
                BaseAddressType.Triangle,
                triAddress => TriangleOffsetsConfig.GetY1(triAddress),
                (short value, uint triAddress) => TriangleOffsetsConfig.SetY1(value, triAddress)
            );

            dictionary.Add("TriangleZ1",
                BaseAddressType.Triangle,
                triAddress => TriangleOffsetsConfig.GetZ1(triAddress),
                (short value, uint triAddress) => TriangleOffsetsConfig.SetZ1(value, triAddress)
            );

            dictionary.Add("TriangleX2",
                BaseAddressType.Triangle,
                triAddress => TriangleOffsetsConfig.GetX2(triAddress),
                (short value, uint triAddress) => TriangleOffsetsConfig.SetX2(value, triAddress)
            );

            dictionary.Add("TriangleY2",
                BaseAddressType.Triangle,
                triAddress => TriangleOffsetsConfig.GetY2(triAddress),
                (short value, uint triAddress) => TriangleOffsetsConfig.SetY2(value, triAddress)
            );

            dictionary.Add("TriangleZ2",
                BaseAddressType.Triangle,
                triAddress => TriangleOffsetsConfig.GetZ2(triAddress),
                (short value, uint triAddress) => TriangleOffsetsConfig.SetZ2(value, triAddress)
            );

            dictionary.Add("TriangleX3",
                BaseAddressType.Triangle,
                triAddress => TriangleOffsetsConfig.GetX3(triAddress),
                (short value, uint triAddress) => TriangleOffsetsConfig.SetX3(value, triAddress)
            );

            dictionary.Add("TriangleY3",
                BaseAddressType.Triangle,
                triAddress => TriangleOffsetsConfig.GetY3(triAddress),
                (short value, uint triAddress) => TriangleOffsetsConfig.SetY3(value, triAddress)
            );

            dictionary.Add("TriangleZ3",
                BaseAddressType.Triangle,
                triAddress => TriangleOffsetsConfig.GetZ3(triAddress),
                (short value, uint triAddress) => TriangleOffsetsConfig.SetZ3(value, triAddress)
            );

            // File vars

            dictionary.Add("StarsInFile",
                BaseAddressType.File,
                fileAddress => AccessScope<StroopMainForm>.content.GetTab<Tabs.FileTab>().CalculateNumStars(fileAddress),
                SpecialVariableDefaults<short>.DEFAULT_SETTER_WITH_ADDRESS);

            dictionary.Add("FileChecksumCalculated",
                BaseAddressType.File,
                fileAddress => AccessScope<StroopMainForm>.content.GetTab<Tabs.FileTab>().GetChecksum(fileAddress),
                SpecialVariableDefaults<ushort>.DEFAULT_SETTER_WITH_ADDRESS);

            // Main Save vars

            dictionary.Add("MainSaveChecksumCalculated",
                BaseAddressType.MainSave,
                mainSaveAddress => AccessScope<StroopMainForm>.content.GetTab<Tabs.MainSaveTab>().GetChecksum(mainSaveAddress),
                SpecialVariableDefaults<ushort>.DEFAULT_SETTER_WITH_ADDRESS);

            // Action vars

            dictionary.Add("ActionDescription",
                () => TableConfig.MarioActions.GetActionName(),
                SpecialVariableDefaults<string>.DEFAULT_SETTER);

            dictionary.Add("PrevActionDescription",
                () => TableConfig.MarioActions.GetPrevActionName(),
                SpecialVariableDefaults<string>.DEFAULT_SETTER);

            dictionary.Add("ActionGroupDescription",
                () => TableConfig.MarioActions.GetGroupName(),
                SpecialVariableDefaults<string>.DEFAULT_SETTER);

            dictionary.Add("AnimationDescription",
                () => TableConfig.MarioAnimations.GetAnimationName(),
                SpecialVariableDefaults<string>.DEFAULT_SETTER);

            // Water vars

            dictionary.Add("WaterAboveMedian",
                () =>
                {
                    short waterLevel = Config.Stream.GetInt16(MarioConfig.StructAddress + MarioConfig.WaterLevelOffset);
                    short waterLevelMedian = Config.Stream.GetInt16(MiscConfig.WaterLevelMedianAddress);
                    double waterAboveMedian = waterLevel - waterLevelMedian;
                    return waterAboveMedian;
                },
                SpecialVariableDefaults<double>.DEFAULT_SETTER);

            dictionary.Add("MarioAboveWater",
                () =>
                {
                    short waterLevel = Config.Stream.GetInt16(MarioConfig.StructAddress + MarioConfig.WaterLevelOffset);
                    float marioY = Config.Stream.GetSingle(MarioConfig.StructAddress + MarioConfig.YOffset);
                    float marioAboveWater = marioY - waterLevel;
                    return marioAboveWater;
                },
                (float goalMarioAboveWater) =>
                {
                    short waterLevel = Config.Stream.GetInt16(MarioConfig.StructAddress + MarioConfig.WaterLevelOffset);
                    float marioY = Config.Stream.GetSingle(MarioConfig.StructAddress + MarioConfig.YOffset);
                    double goalMarioY = waterLevel + goalMarioAboveWater;
                    return Config.Stream.SetValue((float)goalMarioY, MarioConfig.StructAddress + MarioConfig.YOffset);
                }
            );

            dictionary.Add("CurrentWater",
                () => WaterUtilities.GetCurrentWater(),
                SpecialVariableDefaults<int>.DEFAULT_SETTER);

            // Cam Hack Vars

            dictionary.Add("CamHackYaw",
                () =>
                {
                    float camX = Config.Stream.GetSingle(CamHackConfig.StructAddress + CamHackConfig.CameraXOffset);
                    float camZ = Config.Stream.GetSingle(CamHackConfig.StructAddress + CamHackConfig.CameraZOffset);
                    float focusX = Config.Stream.GetSingle(CamHackConfig.StructAddress + CamHackConfig.FocusXOffset);
                    float focusZ = Config.Stream.GetSingle(CamHackConfig.StructAddress + CamHackConfig.FocusZOffset);
                    return MoreMath.AngleTo_AngleUnits(camX, camZ, focusX, focusZ);
                },
                (double yaw) =>
                {
                    float camX = Config.Stream.GetSingle(CamHackConfig.StructAddress + CamHackConfig.CameraXOffset);
                    float camZ = Config.Stream.GetSingle(CamHackConfig.StructAddress + CamHackConfig.CameraZOffset);
                    float focusX = Config.Stream.GetSingle(CamHackConfig.StructAddress + CamHackConfig.FocusXOffset);
                    float focusZ = Config.Stream.GetSingle(CamHackConfig.StructAddress + CamHackConfig.FocusZOffset);
                    (double newFocusX, double newFocusZ) = MoreMath.RotatePointAboutPointToAngle(focusX, focusZ, camX, camZ, yaw);

                    bool success = true;
                    success &= Config.Stream.SetValue((float)newFocusX, CamHackConfig.StructAddress + CamHackConfig.FocusXOffset);
                    success &= Config.Stream.SetValue((float)newFocusZ, CamHackConfig.StructAddress + CamHackConfig.FocusZOffset);
                    return success;
                }
            );

            dictionary.Add("CamHackPitch",
                () =>
                {
                    float camX = Config.Stream.GetSingle(CamHackConfig.StructAddress + CamHackConfig.CameraXOffset);
                    float camY = Config.Stream.GetSingle(CamHackConfig.StructAddress + CamHackConfig.CameraYOffset);
                    float camZ = Config.Stream.GetSingle(CamHackConfig.StructAddress + CamHackConfig.CameraZOffset);
                    float focusX = Config.Stream.GetSingle(CamHackConfig.StructAddress + CamHackConfig.FocusXOffset);
                    float focusY = Config.Stream.GetSingle(CamHackConfig.StructAddress + CamHackConfig.FocusYOffset);
                    float focusZ = Config.Stream.GetSingle(CamHackConfig.StructAddress + CamHackConfig.FocusZOffset);
                    (double radius, double theta, double phi) = MoreMath.EulerToSpherical_AngleUnits(focusX - camX, focusY - camY, focusZ - camZ);
                    return phi;
                },
                (double pitch) =>
                {
                    float camX = Config.Stream.GetSingle(CamHackConfig.StructAddress + CamHackConfig.CameraXOffset);
                    float camY = Config.Stream.GetSingle(CamHackConfig.StructAddress + CamHackConfig.CameraYOffset);
                    float camZ = Config.Stream.GetSingle(CamHackConfig.StructAddress + CamHackConfig.CameraZOffset);
                    float focusX = Config.Stream.GetSingle(CamHackConfig.StructAddress + CamHackConfig.FocusXOffset);
                    float focusY = Config.Stream.GetSingle(CamHackConfig.StructAddress + CamHackConfig.FocusYOffset);
                    float focusZ = Config.Stream.GetSingle(CamHackConfig.StructAddress + CamHackConfig.FocusZOffset);
                    (double radius, double theta, double phi) = MoreMath.EulerToSpherical_AngleUnits(focusX - camX, focusY - camY, focusZ - camZ);
                    (double diffX, double diffY, double diffZ) = MoreMath.SphericalToEuler_AngleUnits(radius, theta, pitch);
                    (double newFocusX, double newFocusY, double newFocusZ) = (camX + diffX, camY + diffY, camZ + diffZ);

                    bool success = true;
                    success &= Config.Stream.SetValue((float)newFocusX, CamHackConfig.StructAddress + CamHackConfig.FocusXOffset);
                    success &= Config.Stream.SetValue((float)newFocusY, CamHackConfig.StructAddress + CamHackConfig.FocusYOffset);
                    success &= Config.Stream.SetValue((float)newFocusZ, CamHackConfig.StructAddress + CamHackConfig.FocusZOffset);
                    return success;
                }
            );

            // PU vars

            dictionary.Add("MarioXQpuIndex",
                () =>
                {
                    float marioX = Config.Stream.GetSingle(MarioConfig.StructAddress + MarioConfig.XOffset);
                    int puXIndex = PuUtilities.GetPuIndex(marioX);
                    double qpuXIndex = puXIndex / 4d;
                    return qpuXIndex;
                },
                (double newQpuXIndex) =>
                {
                    int newPuXIndex = (int)Math.Round(newQpuXIndex * 4);
                    float marioX = Config.Stream.GetSingle(MarioConfig.StructAddress + MarioConfig.XOffset);
                    double newMarioX = PuUtilities.GetCoordinateInPu(marioX, newPuXIndex);
                    return Config.Stream.SetValue((float)newMarioX, MarioConfig.StructAddress + MarioConfig.XOffset);
                }
            );

            dictionary.Add("MarioYQpuIndex",
                () =>
                {
                    float marioY = Config.Stream.GetSingle(MarioConfig.StructAddress + MarioConfig.YOffset);
                    int puYIndex = PuUtilities.GetPuIndex(marioY);
                    double qpuYIndex = puYIndex / 4d;
                    return qpuYIndex;
                },
                (double newQpuYIndex) =>
                {
                    int newPuYIndex = (int)Math.Round(newQpuYIndex * 4);
                    float marioY = Config.Stream.GetSingle(MarioConfig.StructAddress + MarioConfig.YOffset);
                    double newMarioY = PuUtilities.GetCoordinateInPu(marioY, newPuYIndex);
                    return Config.Stream.SetValue((float)newMarioY, MarioConfig.StructAddress + MarioConfig.YOffset);
                }
            );

            dictionary.Add("MarioZQpuIndex",
                () =>
                {
                    float marioZ = Config.Stream.GetSingle(MarioConfig.StructAddress + MarioConfig.ZOffset);
                    int puZIndex = PuUtilities.GetPuIndex(marioZ);
                    double qpuZIndex = puZIndex / 4d;
                    return qpuZIndex;
                },
                (double newQpuZIndex) =>
                {
                    int newPuZIndex = (int)Math.Round(newQpuZIndex * 4);
                    float marioZ = Config.Stream.GetSingle(MarioConfig.StructAddress + MarioConfig.ZOffset);
                    double newMarioZ = PuUtilities.GetCoordinateInPu(marioZ, newPuZIndex);
                    return Config.Stream.SetValue((float)newMarioZ, MarioConfig.StructAddress + MarioConfig.ZOffset);
                }
            );

            dictionary.Add("MarioXPuIndex",
                () =>
                {
                    float marioX = Config.Stream.GetSingle(MarioConfig.StructAddress + MarioConfig.XOffset);
                    int puXIndex = PuUtilities.GetPuIndex(marioX);
                    return puXIndex;
                },
                (int newPuXIndex) =>
                {
                    float marioX = Config.Stream.GetSingle(MarioConfig.StructAddress + MarioConfig.XOffset);
                    double newMarioX = PuUtilities.GetCoordinateInPu(marioX, newPuXIndex);
                    return Config.Stream.SetValue((float)newMarioX, MarioConfig.StructAddress + MarioConfig.XOffset);
                }
            );

            dictionary.Add("MarioYPuIndex",
                () =>
                {
                    float marioY = Config.Stream.GetSingle(MarioConfig.StructAddress + MarioConfig.YOffset);
                    int puYIndex = PuUtilities.GetPuIndex(marioY);
                    return puYIndex;
                },
                (int newPuYIndex) =>
                {
                    float marioY = Config.Stream.GetSingle(MarioConfig.StructAddress + MarioConfig.YOffset);
                    double newMarioY = PuUtilities.GetCoordinateInPu(marioY, newPuYIndex);
                    return Config.Stream.SetValue((float)newMarioY, MarioConfig.StructAddress + MarioConfig.YOffset);
                }
            );

            dictionary.Add("MarioZPuIndex",
                () =>
                {
                    float marioZ = Config.Stream.GetSingle(MarioConfig.StructAddress + MarioConfig.ZOffset);
                    int puZIndex = PuUtilities.GetPuIndex(marioZ);
                    return puZIndex;
                },
                (int newPuZIndex) =>
                {
                    float marioZ = Config.Stream.GetSingle(MarioConfig.StructAddress + MarioConfig.ZOffset);
                    double newMarioZ = PuUtilities.GetCoordinateInPu(marioZ, newPuZIndex);
                    return Config.Stream.SetValue((float)newMarioZ, MarioConfig.StructAddress + MarioConfig.ZOffset);
                }
            );

            dictionary.Add("MarioXPuRelative",
                () =>
                {
                    float marioX = Config.Stream.GetSingle(MarioConfig.StructAddress + MarioConfig.XOffset);
                    double relX = PuUtilities.GetRelativeCoordinate(marioX);
                    return relX;
                },
                (double newRelX) =>
                {
                    float marioX = Config.Stream.GetSingle(MarioConfig.StructAddress + MarioConfig.XOffset);
                    int puXIndex = PuUtilities.GetPuIndex(marioX);
                    double newMarioX = PuUtilities.GetCoordinateInPu(newRelX, puXIndex);
                    return Config.Stream.SetValue((float)newMarioX, MarioConfig.StructAddress + MarioConfig.XOffset);
                }
            );

            dictionary.Add("MarioYPuRelative",
                () =>
                {
                    float marioY = Config.Stream.GetSingle(MarioConfig.StructAddress + MarioConfig.YOffset);
                    double relY = PuUtilities.GetRelativeCoordinate(marioY);
                    return relY;
                },
                (double newRelY) =>
                {
                    float marioY = Config.Stream.GetSingle(MarioConfig.StructAddress + MarioConfig.YOffset);
                    int puYIndex = PuUtilities.GetPuIndex(marioY);
                    double newMarioY = PuUtilities.GetCoordinateInPu(newRelY, puYIndex);
                    return Config.Stream.SetValue((float)newMarioY, MarioConfig.StructAddress + MarioConfig.YOffset);
                }
            );

            dictionary.Add("MarioZPuRelative",
                () =>
                {
                    float marioZ = Config.Stream.GetSingle(MarioConfig.StructAddress + MarioConfig.ZOffset);
                    double relZ = PuUtilities.GetRelativeCoordinate(marioZ);
                    return relZ;
                },
                (double newRelZ) =>
                {
                    float marioZ = Config.Stream.GetSingle(MarioConfig.StructAddress + MarioConfig.ZOffset);
                    int puZIndex = PuUtilities.GetPuIndex(marioZ);
                    double newMarioZ = PuUtilities.GetCoordinateInPu(newRelZ, puZIndex);
                    return Config.Stream.SetValue((float)newMarioZ, MarioConfig.StructAddress + MarioConfig.ZOffset);
                });

            dictionary.Add("DeFactoMultiplier",
                () => GetDeFactoMultiplier(),
                (double newDeFactoMultiplier) =>
                {
                    float marioY = Config.Stream.GetSingle(MarioConfig.StructAddress + MarioConfig.YOffset);
                    float floorY = Config.Stream.GetSingle(MarioConfig.StructAddress + MarioConfig.FloorYOffset);
                    float distAboveFloor = marioY - floorY;
                    if (distAboveFloor != 0) return false;

                    uint floorTri = Config.Stream.GetUInt32(MarioConfig.StructAddress + MarioConfig.FloorTriangleOffset);
                    if (floorTri == 0) return false;
                    return Config.Stream.SetValue((float)newDeFactoMultiplier, floorTri + TriangleOffsetsConfig.NormY);
                }
            );

            dictionary.Add("SyncingSpeed",
                () => GetSyncingSpeed(),
                (double newSyncingSpeed) =>
                {
                    float marioY = Config.Stream.GetSingle(MarioConfig.StructAddress + MarioConfig.YOffset);
                    float floorY = Config.Stream.GetSingle(MarioConfig.StructAddress + MarioConfig.FloorYOffset);
                    float distAboveFloor = marioY - floorY;
                    if (distAboveFloor != 0) return false;

                    uint floorTri = Config.Stream.GetUInt32(MarioConfig.StructAddress + MarioConfig.FloorTriangleOffset);
                    if (floorTri == 0) return false;
                    double newYnorm = PuUtilities.QpuSpeed / newSyncingSpeed * SpecialConfig.PuHypotenuse;
                    return Config.Stream.SetValue((float)newYnorm, floorTri + TriangleOffsetsConfig.NormY);
                }
            );

            dictionary.Add("QpuSpeed",
                () => GetQpuSpeed(),
                (double newQpuSpeed) =>
                {
                    double newHSpeed = newQpuSpeed * GetSyncingSpeed();
                    return Config.Stream.SetValue((float)newHSpeed, MarioConfig.StructAddress + MarioConfig.HSpeedOffset);
                }
            );

            dictionary.Add("PuSpeed",
                () => GetQpuSpeed() * 4,
                (double newPuSpeed) =>
                {
                    double newQpuSpeed = newPuSpeed / 4;
                    double newHSpeed = newQpuSpeed * GetSyncingSpeed();
                    return Config.Stream.SetValue((float)newHSpeed, MarioConfig.StructAddress + MarioConfig.HSpeedOffset);
                }
            );

            dictionary.Add("QpuSpeedComponent",
                () => (int)Math.Round(GetQpuSpeed()),
                (int newQpuSpeedComp) =>
                {
                    double relativeSpeed = GetRelativePuSpeed();
                    double newHSpeed = newQpuSpeedComp * GetSyncingSpeed() + relativeSpeed / GetDeFactoMultiplier();
                    return Config.Stream.SetValue((float)newHSpeed, MarioConfig.StructAddress + MarioConfig.HSpeedOffset);
                }
            );

            dictionary.Add("PuSpeedComponent",
                () => (int)Math.Round(GetQpuSpeed() * 4),
                (int newPuSpeedComp) =>
                {
                    double newQpuSpeedComp = newPuSpeedComp / 4d;
                    double relativeSpeed = GetRelativePuSpeed();
                    double newHSpeed = newQpuSpeedComp * GetSyncingSpeed() + relativeSpeed / GetDeFactoMultiplier();
                    return Config.Stream.SetValue((float)newHSpeed, MarioConfig.StructAddress + MarioConfig.HSpeedOffset);
                }
            );

            dictionary.Add("RelativeSpeed",
                () => GetRelativePuSpeed(),
                (double newRelativeSpeed) =>
                {
                    double puSpeed = GetQpuSpeed() * 4;
                    double puSpeedRounded = Math.Round(puSpeed);
                    double newHSpeed = (puSpeedRounded / 4) * GetSyncingSpeed() + newRelativeSpeed / GetDeFactoMultiplier();
                    return Config.Stream.SetValue((float)newHSpeed, MarioConfig.StructAddress + MarioConfig.HSpeedOffset);
                }
            );

            dictionary.Add("Qs1RelativeXSpeed",
                () => GetQsRelativeSpeed(1 / 4d, true),
                (double newValue) => GetQsRelativeIntendedNextComponent(newValue, 1 / 4d, true, true)
            );

            dictionary.Add("Qs1RelativeZSpeed",
                () => GetQsRelativeSpeed(1 / 4d, false),
                (double newValue) => GetQsRelativeIntendedNextComponent(newValue, 1 / 4d, false, true)
            );

            dictionary.Add("Qs1RelativeIntendedNextX",
                () => GetQsRelativeIntendedNextComponent(1 / 4d, true),
                (double newValue) => GetQsRelativeIntendedNextComponent(newValue, 1 / 4d, true, false)
            );

            dictionary.Add("Qs1RelativeIntendedNextZ",
                () => GetQsRelativeIntendedNextComponent(1 / 4d, false),
                (double newValue) => GetQsRelativeIntendedNextComponent(newValue, 1 / 4d, false, false)
            );

            dictionary.Add("Qs2RelativeXSpeed",
                () => GetQsRelativeSpeed(2 / 4d, true),
                (double newValue) => GetQsRelativeIntendedNextComponent(newValue, 2 / 4d, true, true)
            );

            dictionary.Add("Qs2RelativeZSpeed",
                () => GetQsRelativeSpeed(2 / 4d, false),
                (double newValue) => GetQsRelativeIntendedNextComponent(newValue, 2 / 4d, false, true)
            );

            dictionary.Add("Qs2RelativeIntendedNextX",
                () => GetQsRelativeIntendedNextComponent(2 / 4d, true),
                (double newValue) => GetQsRelativeIntendedNextComponent(newValue, 2 / 4d, true, false)
            );

            dictionary.Add("Qs2RelativeIntendedNextZ",
                () => GetQsRelativeIntendedNextComponent(2 / 4d, false),
                (double newValue) => GetQsRelativeIntendedNextComponent(newValue, 2 / 4d, false, false)
            );

            dictionary.Add("Qs3RelativeXSpeed",
                () => GetQsRelativeSpeed(3 / 4d, true),
                (double newValue) => GetQsRelativeIntendedNextComponent(newValue, 3 / 4d, true, true)
            );

            dictionary.Add("Qs3RelativeZSpeed",
                () => GetQsRelativeSpeed(3 / 4d, false),
                (double newValue) => GetQsRelativeIntendedNextComponent(newValue, 3 / 4d, false, true)
            );

            dictionary.Add("Qs3RelativeIntendedNextX",
                () => GetQsRelativeIntendedNextComponent(3 / 4d, true),
                (double newValue) => GetQsRelativeIntendedNextComponent(newValue, 3 / 4d, true, false)
            );

            dictionary.Add("Qs3RelativeIntendedNextZ",
                () => GetQsRelativeIntendedNextComponent(3 / 4d, false),
                (double newValue) => GetQsRelativeIntendedNextComponent(newValue, 3 / 4d, false, false)
            );

            dictionary.Add("Qs4RelativeXSpeed",
                () => GetQsRelativeSpeed(4 / 4d, true),
                (double newValue) => GetQsRelativeIntendedNextComponent(newValue, 4 / 4d, true, true)
            );

            dictionary.Add("Qs4RelativeZSpeed",
                () => GetQsRelativeSpeed(4 / 4d, false),
                (double newValue) => GetQsRelativeIntendedNextComponent(newValue, 4 / 4d, false, true)
            );

            dictionary.Add("Qs4RelativeIntendedNextX",
                () => GetQsRelativeIntendedNextComponent(4 / 4d, true),
                (double newValue) => GetQsRelativeIntendedNextComponent(newValue, 4 / 4d, true, false)
            );

            dictionary.Add("Qs4RelativeIntendedNextZ",
                () => GetQsRelativeIntendedNextComponent(4 / 4d, false),
                (double newValue) => GetQsRelativeIntendedNextComponent(newValue, 4 / 4d, false, false)
            );

            dictionary.Add("PuParams",
                () => "(" + SpecialConfig.PuParam1 + "," + SpecialConfig.PuParam2 + ")",
                (string puParamsString) =>
                {
                    List<string> stringList = ParsingUtilities.ParseStringList(puParamsString);
                    List<int?> intList = stringList.ConvertAll(
                        stringVal => ParsingUtilities.ParseIntNullable(stringVal));
                    if (intList.Count == 1) intList.Insert(0, 0);
                    if (intList.Count != 2 || intList.Exists(intValue => !intValue.HasValue)) return false;
                    SpecialConfig.PuParam1 = intList[0].Value;
                    SpecialConfig.PuParam2 = intList[1].Value;
                    return true;
                }
            );

            // Misc vars

            dictionary.Add("GlobalTimerMod64",
                () => Config.Stream.GetUInt32(MiscConfig.GlobalTimerAddress),
                SpecialVariableDefaults<uint>.DEFAULT_SETTER);

            dictionary.Add("RngIndex",
                () => RngIndexer.GetRngIndex(Config.Stream.GetUInt16(MiscConfig.RngAddress)),
                (ushort rngIndex) => Config.Stream.SetValue(RngIndexer.GetRngValue(rngIndex), MiscConfig.RngAddress)
            );

            dictionary.Add("RngIndexMod4",
                () => RngIndexer.GetRngIndex() % 4,
                SpecialVariableDefaults<int>.DEFAULT_SETTER);

            dictionary.Add("LastCoinRngIndex",
                BaseAddressType.Coin,
                (uint coinAddress) => RngIndexer.GetRngIndex(Config.Stream.GetUInt16(coinAddress + ObjectConfig.YawMovingOffset)),
                (ushort rngIndex, uint coinAddress) => Config.Stream.SetValue(RngIndexer.GetRngValue(rngIndex), coinAddress + ObjectConfig.YawMovingOffset)
            );

            dictionary.Add("LastCoinRngIndexDiff",
                BaseAddressType.Coin,
                (uint coinAddress) =>
                {
                    ushort coinRngValue = Config.Stream.GetUInt16(coinAddress + ObjectConfig.YawMovingOffset);
                    int coinRngIndex = RngIndexer.GetRngIndex(coinRngValue);
                    int rngIndexDiff = coinRngIndex - SpecialConfig.GoalRngIndex;
                    return (ushort)rngIndexDiff;
                },
                (ushort rngIndexDiff, uint coinAddress) =>
                {
                    ushort coinRngIndex = (ushort)(SpecialConfig.GoalRngIndex + rngIndexDiff);
                    ushort coinRngValue = RngIndexer.GetRngValue(coinRngIndex);
                    return Config.Stream.SetValue(coinRngValue, coinAddress + ObjectConfig.YawMovingOffset);
                }
            );

            dictionary.Add("GoalRngValue",
                () => SpecialConfig.GoalRngValue,
                (ushort goalRngValue) =>
                {
                    SpecialConfig.GoalRngValue = goalRngValue;
                    return true;
                }
            );

            dictionary.Add("GoalRngIndex",
                () => SpecialConfig.GoalRngIndex,
                (ushort goalRngIndex) =>
                {
                    SpecialConfig.GoalRngIndex = goalRngIndex;
                    return true;
                }
            );

            dictionary.Add("GoalRngIndexDiff",
                () =>
                {
                    ushort rngValue = Config.Stream.GetUInt16(MiscConfig.RngAddress);
                    int rngIndex = RngIndexer.GetRngIndex(rngValue);
                    int rngIndexDiff = rngIndex - SpecialConfig.GoalRngIndex;
                    return rngIndexDiff;
                }
                ,
                (int rngIndexDiff) =>
                {
                    int rngIndex = SpecialConfig.GoalRngIndex + rngIndexDiff;
                    ushort rngValue = RngIndexer.GetRngValue(rngIndex);
                    return Config.Stream.SetValue(rngValue, MiscConfig.RngAddress);
                }
            );

            dictionary.Add("NumRngCalls",
                () => ObjectRngUtilities.GetNumRngUsages(),
                SpecialVariableDefaults<int>.DEFAULT_SETTER);

            dictionary.Add("NumberOfLoadedObjects",
                () => DataModels.ObjectProcessor.ActiveObjectCount,
                SpecialVariableDefaults<int>.DEFAULT_SETTER);

            dictionary.Add("PlayTime",
                () => GetRealTime(Config.Stream.GetUInt32(MiscConfig.GlobalTimerAddress)),
                SpecialVariableDefaults<string>.DEFAULT_SETTER);

            dictionary.Add("DemoCounterDescription",
                () => DemoCounterUtilities.GetDemoCounterDescription(),
                (string description) =>
                {
                    short? demoCounterNullable = DemoCounterUtilities.GetDemoCounter(description);
                    if (!demoCounterNullable.HasValue) return false;
                    return Config.Stream.SetValue(demoCounterNullable.Value, MiscConfig.DemoCounterAddress);
                }
            );

            dictionary.Add("TtcSpeedSettingDescription",
                () => TtcSpeedSettingUtilities.GetTtcSpeedSettingDescription(),
                (string description) =>
                {
                    short? ttcSpeedSettingNullable = TtcSpeedSettingUtilities.GetTtcSpeedSetting(description);
                    if (!ttcSpeedSettingNullable.HasValue) return false;
                    return Config.Stream.SetValue(ttcSpeedSettingNullable.Value, MiscConfig.TtcSpeedSettingAddress);
                }
            );

            dictionary.Add("TtcSaveState",
                () => new TtcSaveState().ToString(),
                (string saveStateString) =>
                {
                    TtcSaveState saveState = new TtcSaveState(saveStateString);
                    TtcUtilities.ApplySaveState(saveState);
                    return true;
                }
            );

            dictionary.Add("GfxBufferSpace",
                () =>
                {
                    uint gfxBufferStart = Config.Stream.GetUInt32(0x8033B06C);
                    uint gfxBufferEnd = Config.Stream.GetUInt32(0x8033B070);
                    return gfxBufferEnd - gfxBufferStart;
                },
                SpecialVariableDefaults<uint>.DEFAULT_SETTER);

            dictionary.Add("SegmentedToVirtualAddress",
                () => { return SpecialConfig.SegmentedToVirtualAddress; }
                ,
                (uint value) =>
                {
                    SpecialConfig.SegmentedToVirtualAddress = value;
                    return true;
                }
            );

            dictionary.Add("SegmentedToVirtualOutput",
                () => SpecialConfig.SegmentedToVirtualOutput,
                SpecialVariableDefaults<uint>.DEFAULT_SETTER);

            dictionary.Add("VirtualToSegmentedSegment",
                () => SpecialConfig.VirtualToSegmentedSegment,
                (uint value) =>
                {
                    SpecialConfig.VirtualToSegmentedSegment = value;
                    return true;
                }
            );

            dictionary.Add("VirtualToSegmentedAddress",
                () => SpecialConfig.VirtualToSegmentedAddress,
                (uint value) =>
                {
                    SpecialConfig.VirtualToSegmentedAddress = value;
                    return true;
                }
            );

            dictionary.Add("VirtualToSegmentedOutput",
                () => SpecialConfig.VirtualToSegmentedOutput,
                SpecialVariableDefaults<uint>.DEFAULT_SETTER);

            // Options vars

            dictionary.Add("GotoAboveOffset",
                () => GotoRetrieveConfig.GotoAboveOffset,
                (float value) =>
                {
                    GotoRetrieveConfig.GotoAboveOffset = value;
                    return true;
                }
            );

            dictionary.Add("GotoInfrontOffset",
                () => GotoRetrieveConfig.GotoInfrontOffset,
                (float value) =>
                {
                    GotoRetrieveConfig.GotoInfrontOffset = value;
                    return true;
                }
            );

            dictionary.Add("RetrieveAboveOffset",
                () => GotoRetrieveConfig.RetrieveAboveOffset,
                (float value) =>
                {
                    GotoRetrieveConfig.RetrieveAboveOffset = value;
                    return true;
                }
            );

            dictionary.Add("RetrieveInfrontOffset",
                () => GotoRetrieveConfig.RetrieveInfrontOffset,
                (float value) =>
                {
                    GotoRetrieveConfig.RetrieveInfrontOffset = value;
                    return true;
                }
            );

            dictionary.Add("FramesPerSecond",
                () => RefreshRateConfig.RefreshRateFreq,
                (uint value) =>
                {
                    RefreshRateConfig.RefreshRateFreq = value;
                    return true;
                }
            );

            // TODO: Add WatchVariablePositionAngleWrapper I guess?
            //dictionary.Add("PositionControllerRelativity",
            //    () => PositionControllerRelativityConfig.RelativityPA,
            //    (PositionAngle value) =>
            //    {
            //        PositionControllerRelativityConfig.RelativityPA = value;
            //        return true;
            //    }
            //);

            dictionary.Add("CustomReleaseStatus",
                () => SpecialConfig.CustomReleaseStatus,
                (uint value) =>
                {
                    SpecialConfig.CustomReleaseStatus = value;
                    return true;
                }
            );

            // Area vars

            dictionary.Add("CurrentAreaIndexMario",
                () =>
                {
                    uint currentAreaMario = Config.Stream.GetUInt32(MarioConfig.StructAddress + MarioConfig.AreaPointerOffset);
                    return AreaUtilities.GetAreaIndex(currentAreaMario);
                },
                (int? currentAreaIndexMario) =>
                {
                    if (!currentAreaIndexMario.HasValue || currentAreaIndexMario < 0 || currentAreaIndexMario >= 8)
                        return false;
                    uint currentAreaAddressMario = AreaUtilities.GetAreaAddress(currentAreaIndexMario.Value);
                    return Config.Stream.SetValue(currentAreaAddressMario, MarioConfig.StructAddress + MarioConfig.AreaPointerOffset);
                }
            );

            dictionary.Add("CurrentAreaIndex",
                () => AreaUtilities.GetAreaIndex(Config.Stream.GetUInt32(AreaConfig.CurrentAreaPointerAddress)),
                (int? currentAreaIndex) =>
                {
                    if (!currentAreaIndex.HasValue || currentAreaIndex < 0 || currentAreaIndex >= 8)
                        return false;
                    uint currentAreaAddress = AreaUtilities.GetAreaAddress(currentAreaIndex.Value);
                    return Config.Stream.SetValue(currentAreaAddress, AreaConfig.CurrentAreaPointerAddress);
                }
            );

            dictionary.Add("AreaTerrainDescription",
                () =>
                {
                    short terrainType = Config.Stream.GetInt16(AreaTab.SelectedAreaAddress + AreaConfig.TerrainTypeOffset);
                    return AreaUtilities.GetTerrainDescription(terrainType);
                },
                (string terrainDescription) =>
                {
                    var type = AreaUtilities.GetTerrainType(terrainDescription);
                    if (!type.HasValue)
                        return false;
                    return Config.Stream.SetValue(type.Value, AreaTab.SelectedAreaAddress + AreaConfig.TerrainTypeOffset);
                }
            );

            // Warp vars

            dictionary.Add("WarpNodesAddress",
                () => GetWarpNodesAddress(),
                SpecialVariableDefaults<uint>.DEFAULT_SETTER);

            dictionary.Add("NumWarpNodes",
                () => GetNumWarpNodes(),
                SpecialVariableDefaults<int>.DEFAULT_SETTER);

            dictionary.Add("HorizontalMovement",
                () =>
                {
                    float pos01X = Config.Stream.GetSingle(0x80372F00);
                    float pos01Z = Config.Stream.GetSingle(0x80372F08);
                    float pos15X = Config.Stream.GetSingle(0x80372FE0);
                    float pos15Z = Config.Stream.GetSingle(0x80372FE8);
                    return MoreMath.GetDistanceBetween(pos01X, pos01Z, pos15X, pos15Z);
                },
                SpecialVariableDefaults<double>.DEFAULT_SETTER);

            // Mupen vars

            dictionary.Add("MupenLag",
                () =>
                {
                    if (!MupenUtilities.IsUsingMupen())
                        return null;
                    int lag = MupenUtilities.GetLagCount() + SpecialConfig.MupenLagOffset;
                    return lag.ToString();
                },
                (string stringValue) =>
                {
                    if (!MupenUtilities.IsUsingMupen())
                        return false;

                    if (stringValue.ToLower() == "x")
                    {
                        SpecialConfig.MupenLagOffset = 0;
                        return true;
                    }

                    int? newLagNullable = ParsingUtilities.ParseIntNullable(stringValue);
                    if (!newLagNullable.HasValue)
                        return false;
                    int newLag = newLagNullable.Value;
                    int newLagOffset = newLag - MupenUtilities.GetLagCount();
                    SpecialConfig.MupenLagOffset = newLagOffset;
                    return true;
                }
            );
        }

        // Triangle utilitiy methods

        public static int GetClosestTriangleVertexIndex(uint triAddress)
        {
            PositionAngle marioPos = PositionAngle.Mario;
            TriangleDataModel triStruct = TriangleDataModel.Create(triAddress);
            double distToV1 = MoreMath.GetDistanceBetween(
                marioPos.X, marioPos.Y, marioPos.Z, triStruct.X1, triStruct.Y1, triStruct.Z1);
            double distToV2 = MoreMath.GetDistanceBetween(
                marioPos.X, marioPos.Y, marioPos.Z, triStruct.X2, triStruct.Y2, triStruct.Z2);
            double distToV3 = MoreMath.GetDistanceBetween(
                marioPos.X, marioPos.Y, marioPos.Z, triStruct.X3, triStruct.Y3, triStruct.Z3);

            if (distToV1 <= distToV2 && distToV1 <= distToV3) return 1;
            else return distToV2 <= distToV3 ? 2 : 3;
        }

        private static PositionAngle GetClosestTriangleVertexPosition(uint triAddress)
        {
            int closestTriangleVertexIndex = GetClosestTriangleVertexIndex(triAddress);
            TriangleDataModel triStruct = TriangleDataModel.Create(triAddress);
            if (closestTriangleVertexIndex == 1) return PositionAngle.Tri(triAddress, 1);
            if (closestTriangleVertexIndex == 2) return PositionAngle.Tri(triAddress, 2);
            if (closestTriangleVertexIndex == 3) return PositionAngle.Tri(triAddress, 3);
            throw new ArgumentOutOfRangeException();
        }

        private static double GetTriangleUphillAngleRadians(uint triAddress)
        {
            double angle = GetTriangleUphillAngle(triAddress);
            return MoreMath.AngleUnitsToRadians(angle);
        }

        public static double GetTriangleUphillAngle(uint triAddress)
        {
            TriangleDataModel triStruct = TriangleDataModel.Create(triAddress);
            return GetTriangleUphillAngle(triStruct);
        }

        public static double GetTriangleUphillAngle(TriangleDataModel triStruct)
        {
            double uphillAngle = 32768 + InGameTrigUtilities.InGameAngleTo(triStruct.NormX, triStruct.NormZ);
            if (triStruct.NormX == 0 && triStruct.NormZ == 0) uphillAngle = double.NaN;
            if (triStruct.IsCeiling()) uphillAngle += 32768;
            return MoreMath.NormalizeAngleDouble(uphillAngle);
        }

        private static double GetMaxHorizontalSpeedOnTriangle(uint triAddress, bool uphill, bool atAngle)
        {
            TriangleDataModel triStruct = TriangleDataModel.Create(triAddress);
            double vDist = uphill ? 78 : 100;
            if (atAngle)
            {
                ushort marioAngle = Config.Stream.GetUInt16(MarioConfig.StructAddress + MarioConfig.FacingYawOffset);
                double marioAngleRadians = MoreMath.AngleUnitsToRadians(marioAngle);
                double uphillAngleRadians = GetTriangleUphillAngleRadians(triAddress);
                double deltaAngle = marioAngleRadians - uphillAngleRadians;
                double multiplier = Math.Abs(Math.Cos(deltaAngle));
                vDist /= multiplier;
            }

            double steepnessRadians = Math.Acos(triStruct.NormY);
            double hDist = vDist / Math.Tan(steepnessRadians);
            double hSpeed = hDist * 4 / triStruct.NormY;
            return hSpeed;
        }

        // Mario special methods

        public static double GetMarioSlidingSpeed()
        {
            float xSlidingSpeed = Config.Stream.GetSingle(MarioConfig.StructAddress + MarioConfig.SlidingSpeedXOffset);
            float zSlidingSpeed = Config.Stream.GetSingle(MarioConfig.StructAddress + MarioConfig.SlidingSpeedZOffset);
            double hSlidingSpeed = MoreMath.GetHypotenuse(xSlidingSpeed, zSlidingSpeed);
            return hSlidingSpeed;
        }

        public static double GetMarioSlidingAngle()
        {
            float xSlidingSpeed = Config.Stream.GetSingle(MarioConfig.StructAddress + MarioConfig.SlidingSpeedXOffset);
            float zSlidingSpeed = Config.Stream.GetSingle(MarioConfig.StructAddress + MarioConfig.SlidingSpeedZOffset);
            double slidingAngle = MoreMath.AngleTo_AngleUnits(xSlidingSpeed, zSlidingSpeed);
            return slidingAngle;
        }

        // Radius distance utility methods

        private static double GetRadiusDiff(PositionAngle self, PositionAngle point, double radius)
        {
            double dist = MoreMath.GetDistanceBetween(
                self.X, self.Y, self.Z, point.X, point.Y, point.Z);
            return dist - radius;
        }

        private static bool SetRadiusDiff(PositionAngle self, PositionAngle point, double radius, double value)
        {
            double totalDist = radius + value;
            (double newSelfX, double newSelfY, double newSelfZ) =
                MoreMath.ExtrapolateLine3D(
                    point.X, point.Y, point.Z, self.X, self.Y, self.Z, totalDist);
            return self.SetValues(x: newSelfX, y: newSelfY, z: newSelfZ);
        }

        // Object specific utilitiy methods

        private static (double dotProduct, double distToWaypointPlane, double distToWaypoint)
            GetWaypointSpecialVars(uint objAddress)
        {
            float objX = Config.Stream.GetSingle(objAddress + ObjectConfig.XOffset);
            float objY = Config.Stream.GetSingle(objAddress + ObjectConfig.YOffset);
            float objZ = Config.Stream.GetSingle(objAddress + ObjectConfig.ZOffset);

            uint prevWaypointAddress = Config.Stream.GetUInt32(objAddress + ObjectConfig.WaypointOffset);
            short prevWaypointIndex = Config.Stream.GetInt16(prevWaypointAddress + WaypointConfig.IndexOffset);
            short prevWaypointX = Config.Stream.GetInt16(prevWaypointAddress + WaypointConfig.XOffset);
            short prevWaypointY = Config.Stream.GetInt16(prevWaypointAddress + WaypointConfig.YOffset);
            short prevWaypointZ = Config.Stream.GetInt16(prevWaypointAddress + WaypointConfig.ZOffset);
            uint nextWaypointAddress = prevWaypointAddress + WaypointConfig.StructSize;
            short nextWaypointIndex = Config.Stream.GetInt16(nextWaypointAddress + WaypointConfig.IndexOffset);
            short nextWaypointX = Config.Stream.GetInt16(nextWaypointAddress + WaypointConfig.XOffset);
            short nextWaypointY = Config.Stream.GetInt16(nextWaypointAddress + WaypointConfig.YOffset);
            short nextWaypointZ = Config.Stream.GetInt16(nextWaypointAddress + WaypointConfig.ZOffset);

            float objToWaypointX = nextWaypointX - objX;
            float objToWaypointY = nextWaypointY - objY;
            float objToWaypointZ = nextWaypointZ - objZ;
            float prevToNextX = nextWaypointX - prevWaypointX;
            float prevToNextY = nextWaypointY - prevWaypointY;
            float prevToNextZ = nextWaypointZ - prevWaypointZ;

            double dotProduct = MoreMath.GetDotProduct(objToWaypointX, objToWaypointY, objToWaypointZ, prevToNextX, prevToNextY, prevToNextZ);
            double prevToNextDist = MoreMath.GetDistanceBetween(prevWaypointX, prevWaypointY, prevWaypointZ, nextWaypointX, nextWaypointY, nextWaypointZ);
            double distToWaypointPlane = dotProduct / prevToNextDist;
            double distToWaypoint = MoreMath.GetDistanceBetween(objX, objY, objZ, nextWaypointX, nextWaypointY, nextWaypointZ);

            return (dotProduct, distToWaypointPlane, distToWaypoint);
        }

        private static (double effortTarget, double effortChange, double minHSpeed, double hSpeedTarget)
            GetRacingPenguinSpecialVars(uint racingPenguinAddress)
        {
            double marioY = Config.Stream.GetSingle(MarioConfig.StructAddress + MarioConfig.YOffset);
            double objectY = Config.Stream.GetSingle(racingPenguinAddress + ObjectConfig.YOffset);
            double heightDiff = marioY - objectY;

            uint prevWaypointAddress = Config.Stream.GetUInt32(racingPenguinAddress + ObjectConfig.WaypointOffset);
            short prevWaypointIndex = Config.Stream.GetInt16(prevWaypointAddress);
            double effort = Config.Stream.GetSingle(racingPenguinAddress + ObjectConfig.RacingPenguinEffortOffset);

            double effortTarget;
            double effortChange;
            double minHSpeed = 70;
            if (heightDiff > -100 || prevWaypointIndex >= 35)
            {
                if (prevWaypointIndex >= 35) minHSpeed = 60;
                effortTarget = -500;
                effortChange = 100;
            }
            else
            {
                effortTarget = 1000;
                effortChange = 30;
            }

            effort = MoreMath.MoveNumberTowards(effort, effortTarget, effortChange);

            double hSpeedTarget = (effort - heightDiff) * 0.1;
            hSpeedTarget = MoreMath.Clamp(hSpeedTarget, minHSpeed, 150);

            return (effortTarget, effortChange, minHSpeed, hSpeedTarget);
        }

        private static (double hSpeedTarget, double hSpeedChange)
            GetKoopaTheQuickSpecialVars(uint koopaTheQuickAddress)
        {
            double hSpeedMultiplier = Config.Stream.GetSingle(koopaTheQuickAddress + ObjectConfig.KoopaTheQuickHSpeedMultiplierOffset);
            short pitchToWaypointAngleUnits = Config.Stream.GetInt16(koopaTheQuickAddress + ObjectConfig.PitchToWaypointOffset);
            double pitchToWaypointRadians = MoreMath.AngleUnitsToRadians(pitchToWaypointAngleUnits);

            double hSpeedTarget = hSpeedMultiplier * (Math.Sin(pitchToWaypointRadians) + 1) * 6;
            double hSpeedChange = hSpeedMultiplier * 0.1;

            return (hSpeedTarget, hSpeedChange);
        }

        public static int GetPendulumCountdown(uint pendulumAddress)
        {
            // Get pendulum variables
            float accelerationDirection = Config.Stream.GetSingle(pendulumAddress + ObjectConfig.PendulumAccelerationDirectionOffset);
            float accelerationMagnitude = Config.Stream.GetSingle(pendulumAddress + ObjectConfig.PendulumAccelerationMagnitudeOffset);
            float angularVelocity = Config.Stream.GetSingle(pendulumAddress + ObjectConfig.PendulumAngularVelocityOffset);
            float angle = Config.Stream.GetSingle(pendulumAddress + ObjectConfig.PendulumAngleOffset);
            int waitingTimer = Config.Stream.GetInt32(pendulumAddress + ObjectConfig.PendulumWaitingTimerOffset);
            return GetPendulumCountdown(accelerationDirection, accelerationMagnitude, angularVelocity, angle, waitingTimer);
        }

        public static int GetPendulumCountdown(
            float accelerationDirection, float accelerationMagnitude, float angularVelocity, float angle, int waitingTimer)
        {
            return GetPendulumVars(accelerationDirection, accelerationMagnitude, angularVelocity, angle).ToTuple().Item2 + waitingTimer;
        }

        public static float GetPendulumAmplitude(uint pendulumAddress)
        {
            // Get pendulum variables
            float accelerationDirection = Config.Stream.GetSingle(pendulumAddress + ObjectConfig.PendulumAccelerationDirectionOffset);
            float accelerationMagnitude = Config.Stream.GetSingle(pendulumAddress + ObjectConfig.PendulumAccelerationMagnitudeOffset);
            float angularVelocity = Config.Stream.GetSingle(pendulumAddress + ObjectConfig.PendulumAngularVelocityOffset);
            float angle = Config.Stream.GetSingle(pendulumAddress + ObjectConfig.PendulumAngleOffset);
            return GetPendulumAmplitude(accelerationDirection, accelerationMagnitude, angularVelocity, angle);
        }

        public static float GetPendulumAmplitude(
            float accelerationDirection, float accelerationMagnitude, float angularVelocity, float angle)
        {
            return GetPendulumVars(accelerationDirection, accelerationMagnitude, angularVelocity, angle).ToTuple().Item1;
        }

        public static float GetPendulumAmplitude(float angle, float accelerationMagnitude)
        {
            float accelerationDirection = -1 * Math.Sign(angle);
            float angularVelocity = 0;
            return GetPendulumAmplitude(accelerationDirection, accelerationMagnitude, angularVelocity, angle);
        }

        public static (float amplitude, int countdown) GetPendulumVars(
            float accelerationDirection, float accelerationMagnitude, float angularVelocity, float angle)
        {
            // Get pendulum variables
            float acceleration = accelerationDirection * accelerationMagnitude;

            // Calculate one frame forwards to see if pendulum is speeding up or slowing down
            float nextAccelerationDirection = accelerationDirection;
            if (angle > 0) nextAccelerationDirection = -1;
            if (angle < 0) nextAccelerationDirection = 1;
            float nextAcceleration = nextAccelerationDirection * accelerationMagnitude;
            float nextAngularVelocity = angularVelocity + nextAcceleration;
            float nextAngle = angle + nextAngularVelocity;
            bool speedingUp = Math.Abs(nextAngularVelocity) > Math.Abs(angularVelocity);

            // Calculate duration of speeding up phase
            float inflectionAngle = angle;
            float inflectionAngularVelocity = nextAngularVelocity;
            float speedUpDistance = 0;
            int speedUpDuration = 0;

            if (speedingUp)
            {
                // d = t * v + t(t-1)/2 * a
                // d = tv + (t^2)a/2-ta/2
                // d = t(v-a/2) + (t^2)a/2
                // 0 = (t^2)a/2 + t(v-a/2) + -d
                // t = (-B +- sqrt(B^2 - 4AC)) / (2A)
                float tentativeSlowDownStartAngle = nextAccelerationDirection;
                float tentativeSpeedUpDistance = tentativeSlowDownStartAngle - angle;
                float A = nextAcceleration / 2;
                float B = nextAngularVelocity - nextAcceleration / 2;
                float C = -1 * tentativeSpeedUpDistance;
                double tentativeSpeedUpDuration = (-B + nextAccelerationDirection * Math.Sqrt(B * B - 4 * A * C)) / (2 * A);
                speedUpDuration = (int)Math.Ceiling(tentativeSpeedUpDuration);

                // d = t * v + t(t-1)/2 * a
                speedUpDistance = speedUpDuration * nextAngularVelocity + speedUpDuration * (speedUpDuration - 1) / 2 * nextAcceleration;
                inflectionAngle = angle + speedUpDistance;

                // v_f = v_i + t * a
                inflectionAngularVelocity = nextAngularVelocity + (speedUpDuration - 2) * nextAcceleration;
            }

            // Calculate duration of slowing down phase

            // v_f = v_i + t * a
            // 0 = v_i + t * a
            // t = v_i / a
            int slowDownDuration = (int)Math.Abs(inflectionAngularVelocity / accelerationMagnitude);

            // d = t * (v_i + v_f)/2
            // d = t * (v_i + 0)/2
            // d = t * v_i/2
            float slowDownDistance = (slowDownDuration + 1) * inflectionAngularVelocity / 2;

            // Combine the results from the speeding up phase and the slowing down phase
            int totalDuration = speedUpDuration + slowDownDuration;
            float totalDistance = speedUpDistance + slowDownDistance;
            float amplitude = angle + totalDistance;
            return (amplitude, totalDuration);
        }

        public static int GetCogNumFramesInRotation(uint cogAddress)
        {
            ushort yawFacing = Config.Stream.GetUInt16(cogAddress + ObjectConfig.YawFacingOffset);
            int currentYawVel = (int)Config.Stream.GetSingle(cogAddress + ObjectConfig.CogCurrentYawVelocity);
            int targetYawVel = (int)Config.Stream.GetSingle(cogAddress + ObjectConfig.CogTargetYawVelocity);
            return GetCogNumFramesInRotation(yawFacing, currentYawVel, targetYawVel);
        }

        public static int GetCogNumFramesInRotation(ushort yawFacing, int currentYawVel, int targetYawVel)
        {
            int diff = Math.Abs(targetYawVel - currentYawVel);
            int numFrames = diff / 50;
            if (numFrames == 0) numFrames = 1;
            return numFrames;
        }

        public static ushort GetCogEndingYaw(uint cogAddress)
        {
            ushort yawFacing = Config.Stream.GetUInt16(cogAddress + ObjectConfig.YawFacingOffset);
            int currentYawVel = (int)Config.Stream.GetSingle(cogAddress + ObjectConfig.CogCurrentYawVelocity);
            int targetYawVel = (int)Config.Stream.GetSingle(cogAddress + ObjectConfig.CogTargetYawVelocity);
            return GetCogEndingYaw(yawFacing, currentYawVel, targetYawVel);
        }

        public static ushort GetCogEndingYaw(ushort yawFacing, int currentYawVel, int targetYawVel)
        {
            int numFrames = GetCogNumFramesInRotation(yawFacing, currentYawVel, targetYawVel);
            int remainingRotation = (currentYawVel + targetYawVel) * (numFrames + 1) / 2 - currentYawVel;
            int endingYaw = yawFacing + remainingRotation;
            return MoreMath.NormalizeAngleUshort(endingYaw);
        }

        private static double GetObjectTrajectoryFramesToYDist(double frames)
        {
            bool reflected = false;
            if (frames < 7.5)
            {
                frames = MoreMath.ReflectValueAboutValue(frames, 7.5);
                reflected = true;
            }

            double yDist;
            if (frames <= 38)
            {
                yDist = -1.25 * frames * frames + 18.75 * frames;
            }
            else
            {
                yDist = -75 * (frames - 38) - 1092.5;
            }

            if (reflected) yDist = MoreMath.ReflectValueAboutValue(yDist, 70.3125);
            return yDist;
        }

        private static double GetObjectTrajectoryYDistToFrames(double yDist)
        {
            bool reflected = false;
            if (yDist > 70.3125)
            {
                yDist = MoreMath.ReflectValueAboutValue(yDist, 70.3125);
                reflected = true;
            }

            double frames;
            if (yDist >= -1092.5)
            {
                double radicand = 351.5625 - 5 * yDist;
                frames = 7.5 + 0.4 * Math.Sqrt(radicand);
            }
            else
            {
                frames = (yDist + 1092.5) / -75 + 38;
            }

            if (reflected) frames = MoreMath.ReflectValueAboutValue(frames, 7.5);
            return frames;
        }

        private static double GetBobombTrajectoryFramesToHDist(double frames)
        {
            return 32 + frames * 25;
        }

        private static double GetBobombTrajectoryHDistToFrames(double hDist)
        {
            return (hDist - 32) / 25;
        }

        private static double GetCorkBoxTrajectoryFramesToHDist(double frames)
        {
            return 32 + frames * 40;
        }

        private static double GetCorkBoxTrajectoryHDistToFrames(double hDist)
        {
            return (hDist - 32) / 40;
        }

        // PU methods

        private static float GetDeFactoMultiplier()
        {
            uint floorTri = Config.Stream.GetUInt32(MarioConfig.StructAddress + MarioConfig.FloorTriangleOffset);
            float yNorm = floorTri == 0 ? 1 : Config.Stream.GetSingle(floorTri + TriangleOffsetsConfig.NormY);

            float marioY = Config.Stream.GetSingle(MarioConfig.StructAddress + MarioConfig.YOffset);
            float floorY = Config.Stream.GetSingle(MarioConfig.StructAddress + MarioConfig.FloorYOffset);
            float distAboveFloor = marioY - floorY;

            float defactoMultiplier = distAboveFloor == 0 ? yNorm : 1;
            return defactoMultiplier;
        }

        public static float GetMarioDeFactoSpeed()
        {
            float hSpeed = Config.Stream.GetSingle(MarioConfig.StructAddress + MarioConfig.HSpeedOffset);
            float defactoSpeed = hSpeed * GetDeFactoMultiplier();
            return defactoSpeed;
        }

        public static double GetSyncingSpeed()
        {
            return PuUtilities.QpuSpeed / GetDeFactoMultiplier() * SpecialConfig.PuHypotenuse;
        }

        public static double GetQpuSpeed()
        {
            float hSpeed = Config.Stream.GetSingle(MarioConfig.StructAddress + MarioConfig.HSpeedOffset);
            return hSpeed / GetSyncingSpeed();
        }

        public static double GetRelativePuSpeed()
        {
            double puSpeed = GetQpuSpeed() * 4;
            double puSpeedRounded = Math.Round(puSpeed);
            double relativeSpeed = (puSpeed - puSpeedRounded) / 4 * GetSyncingSpeed() * GetDeFactoMultiplier();
            return relativeSpeed;
        }

        public static (double x, double z) GetIntendedNextPosition(double numFrames)
        {
            double deFactoSpeed = GetMarioDeFactoSpeed();
            ushort marioAngle = Config.Stream.GetUInt16(MarioConfig.StructAddress + MarioConfig.FacingYawOffset);
            ushort marioAngleTruncated = MoreMath.NormalizeAngleTruncated(marioAngle);
            (double xDiff, double zDiff) = MoreMath.GetComponentsFromVector(deFactoSpeed * numFrames, marioAngleTruncated);

            float currentX = Config.Stream.GetSingle(MarioConfig.StructAddress + MarioConfig.XOffset);
            float currentZ = Config.Stream.GetSingle(MarioConfig.StructAddress + MarioConfig.ZOffset);
            return (currentX + xDiff, currentZ + zDiff);
        }

        private static double GetQsRelativeSpeed(double numFrames, bool xComp)
        {
            uint compOffset = xComp ? MarioConfig.XOffset : MarioConfig.ZOffset;
            float currentComp = Config.Stream.GetSingle(MarioConfig.StructAddress + compOffset);
            double relCurrentComp = PuUtilities.GetRelativeCoordinate(currentComp);
            (double intendedX, double intendedZ) = GetIntendedNextPosition(numFrames);
            double intendedComp = xComp ? intendedX : intendedZ;
            double relIntendedComp = PuUtilities.GetRelativeCoordinate(intendedComp);
            double compDiff = relIntendedComp - relCurrentComp;
            return compDiff;
        }

        private static double GetQsRelativeIntendedNextComponent(double numFrames, bool xComp)
        {
            (double intendedX, double intendedZ) = GetIntendedNextPosition(numFrames);
            double intendedComp = xComp ? intendedX : intendedZ;
            double relIntendedComp = PuUtilities.GetRelativeCoordinate(intendedComp);
            return relIntendedComp;
        }

        private static bool GetQsRelativeIntendedNextComponent(double newValue, double numFrames, bool xComp, bool relativePosition)
        {
            float currentX = Config.Stream.GetSingle(MarioConfig.StructAddress + MarioConfig.XOffset);
            float currentZ = Config.Stream.GetSingle(MarioConfig.StructAddress + MarioConfig.ZOffset);
            float currentComp = xComp ? currentX : currentZ;
            (double intendedX, double intendedZ) = GetIntendedNextPosition(numFrames);
            double intendedComp = xComp ? intendedX : intendedZ;
            int intendedPuCompIndex = PuUtilities.GetPuIndex(intendedComp);
            double newRelativeComp = relativePosition ? currentComp + newValue : newValue;
            double newIntendedComp = PuUtilities.GetCoordinateInPu(newRelativeComp, intendedPuCompIndex);

            float hSpeed = Config.Stream.GetSingle(MarioConfig.StructAddress + MarioConfig.HSpeedOffset);
            double intendedXComp = xComp ? newIntendedComp : intendedX;
            double intendedZComp = xComp ? intendedZ : newIntendedComp;
            (double newDeFactoSpeed, double newAngle) =
                MoreMath.GetVectorFromCoordinates(
                    currentX, currentZ, intendedXComp, intendedZComp, hSpeed >= 0);
            double newHSpeed = newDeFactoSpeed / GetDeFactoMultiplier() / numFrames;
            ushort newAngleRounded = MoreMath.NormalizeAngleUshort(newAngle);

            bool success = true;
            success &= Config.Stream.SetValue((float)newHSpeed, MarioConfig.StructAddress + MarioConfig.HSpeedOffset);
            success &= Config.Stream.SetValue(newAngleRounded, MarioConfig.StructAddress + MarioConfig.FacingYawOffset);
            return success;
        }

        // Angle methods

        public static short GetDeltaYawIntendedFacing()
        {
            ushort marioYawFacing = Config.Stream.GetUInt16(MarioConfig.StructAddress + MarioConfig.FacingYawOffset);
            ushort marioYawIntended = Config.Stream.GetUInt16(MarioConfig.StructAddress + MarioConfig.IntendedYawOffset);
            ushort diff = MoreMath.NormalizeAngleTruncated(marioYawIntended - marioYawFacing);
            return MoreMath.NormalizeAngleShort(diff);
        }

        public static short GetDeltaYawIntendedBackwards()
        {
            short forwards = GetDeltaYawIntendedFacing();
            return MoreMath.NormalizeAngleShort(forwards + 32768);
        }

        // Mario trajectory methods

        public static double ConvertDoubleJumpHSpeedToVSpeed(double hSpeed)
        {
            return (hSpeed / 4) + 52;
        }

        public static double ConvertDoubleJumpVSpeedToHSpeed(double vSpeed)
        {
            return (vSpeed - 52) * 4;
        }

        public static double ComputeHeightChangeFromInitialVerticalSpeed(double initialVSpeed)
        {
            int numFrames = (int)Math.Ceiling(initialVSpeed / 4);
            double finalVSpeed = initialVSpeed - (numFrames - 1) * 4;
            double heightChange = numFrames * (initialVSpeed + finalVSpeed) / 2;
            return heightChange;
        }

        public static double ComputeInitialVerticalSpeedFromHeightChange(double heightChange)
        {
            int numFrames = (int)Math.Ceiling((-2 + Math.Sqrt(4 + 8 * heightChange)) / 4);
            double triangleConstant = 2 * numFrames * (numFrames - 1);
            double initialSpeed = (heightChange + triangleConstant) / numFrames;
            return initialSpeed;
        }

        // Rotation methods

        private static (float x, float y, float z) GetRotationDisplacement()
        {
            uint stoodOnObject = Config.Stream.GetUInt32(MarioConfig.StoodOnObjectPointerAddress);
            if (stoodOnObject == 0)
            {
                return (0, 0, 0);
            }

            float[] currentObjectPos = new float[]
            {
                Config.Stream.GetSingle(MarioConfig.StructAddress + MarioConfig.XOffset),
                Config.Stream.GetSingle(MarioConfig.StructAddress + MarioConfig.YOffset),
                Config.Stream.GetSingle(MarioConfig.StructAddress + MarioConfig.ZOffset),
            };

            float[] platformPos = new float[]
            {
                Config.Stream.GetSingle(stoodOnObject + ObjectConfig.XOffset),
                Config.Stream.GetSingle(stoodOnObject + ObjectConfig.YOffset),
                Config.Stream.GetSingle(stoodOnObject + ObjectConfig.ZOffset),
            };

            float[] currentObjectOffset = new float[]
            {
                currentObjectPos[0] - platformPos[0],
                currentObjectPos[1] - platformPos[1],
                currentObjectPos[2] - platformPos[2],
            };

            short[] platformAngularVelocity = new short[]
            {
                (short)Config.Stream.GetInt32(stoodOnObject + ObjectConfig.PitchVelocityOffset),
                (short)Config.Stream.GetInt32(stoodOnObject + ObjectConfig.YawVelocityOffset),
                (short)Config.Stream.GetInt32(stoodOnObject + ObjectConfig.RollVelocityOffset),
            };

            short[] platformFacingAngle = new short[]
            {
                Config.Stream.GetInt16(stoodOnObject + ObjectConfig.PitchFacingOffset),
                Config.Stream.GetInt16(stoodOnObject + ObjectConfig.YawFacingOffset),
                Config.Stream.GetInt16(stoodOnObject + ObjectConfig.RollFacingOffset),
            };

            short[] rotation = new short[]
            {
                (short)(platformFacingAngle[0] - platformAngularVelocity[0]),
                (short)(platformFacingAngle[1] - platformAngularVelocity[1]),
                (short)(platformFacingAngle[2] - platformAngularVelocity[2]),
            };

            float[,] displaceMatrix = new float[4, 4];
            float[] relativeOffset = new float[3];
            float[] newObjectOffset = new float[3];

            mtxf_rotate_zxy_and_translate(displaceMatrix, currentObjectOffset, rotation);
            linear_mtxf_transpose_mul_vec3f(displaceMatrix, relativeOffset, currentObjectOffset);

            rotation[0] = platformFacingAngle[0];
            rotation[1] = platformFacingAngle[1];
            rotation[2] = platformFacingAngle[2];

            mtxf_rotate_zxy_and_translate(displaceMatrix, currentObjectOffset, rotation);
            linear_mtxf_transpose_mul_vec3f(displaceMatrix, newObjectOffset, relativeOffset);

            float[] netDisplacement = new float[]
            {
                newObjectOffset[0] - currentObjectOffset[0],
                newObjectOffset[1] - currentObjectOffset[1],
                newObjectOffset[2] - currentObjectOffset[2],
            };

            return (netDisplacement[0], netDisplacement[1], netDisplacement[2]);
        }

        private static void mtxf_rotate_zxy_and_translate(float[,] dest, float[] translate, short[] rotate)
        {
            float sx = InGameTrigUtilities.InGameSine(rotate[0]);
            float cx = InGameTrigUtilities.InGameCosine(rotate[0]);

            float sy = InGameTrigUtilities.InGameSine(rotate[1]);
            float cy = InGameTrigUtilities.InGameCosine(rotate[1]);

            float sz = InGameTrigUtilities.InGameSine(rotate[2]);
            float cz = InGameTrigUtilities.InGameCosine(rotate[2]);

            dest[0, 0] = cy * cz + sx * sy * sz;
            dest[1, 0] = -cy * sz + sx * sy * cz;
            dest[2, 0] = cx * sy;
            dest[3, 0] = translate[0];

            dest[0, 1] = cx * sz;
            dest[1, 1] = cx * cz;
            dest[2, 1] = -sx;
            dest[3, 1] = translate[1];

            dest[0, 2] = -sy * cz + sx * cy * sz;
            dest[1, 2] = sy * sz + sx * cy * cz;
            dest[2, 2] = cx * cy;
            dest[3, 2] = translate[2];

            dest[0, 3] = dest[1, 3] = dest[2, 3] = 0.0f;
            dest[3, 3] = 1.0f;
        }

        private static void linear_mtxf_transpose_mul_vec3f(float[,] m, float[] dst, float[] v)
        {
            for (int i = 0; i < 3; i++)
            {
                dst[i] = m[i, 0] * v[0] + m[i, 1] * v[1] + m[i, 2] * v[2];
            }
        }

        // Triangle methods

        private static short lower_cell_index(short t)
        {
            short index;

            // Move from range [-0x2000, 0x2000) to [0, 0x4000)
            t += 0x2000;
            if (t < 0)
                t = 0;

            // [0, 16)
            index = (short)(t / 0x400);

            // Include extra cell if close to boundary
            if (t % 0x400 < 50)
                index -= 1;

            if (index < 0)
                index = 0;

            // Potentially > 15, but since the upper index is <= 15, not exploitable
            return index;
        }

        private static short upper_cell_index(short t)
        {
            short index;

            // Move from range [-0x2000, 0x2000) to [0, 0x4000)
            t += 0x2000;
            if (t < 0)
                t = 0;

            // [0, 16)
            index = (short)(t / 0x400);

            // Include extra cell if close to boundary
            if (t % 0x400 > 0x400 - 50)
                index += 1;

            if (index > 15)
                index = 15;

            // Potentially < 0, but since lower index is >= 0, not exploitable
            return index;
        }

        public static (int cellX, int cellZ) GetMarioCell()
        {
            float marioX = Config.Stream.GetSingle(MarioConfig.StructAddress + MarioConfig.XOffset);
            float marioZ = Config.Stream.GetSingle(MarioConfig.StructAddress + MarioConfig.ZOffset);
            return GetCell(marioX, marioZ);
        }

        public static (int cellX, int cellZ) GetCell(float floatX, float floatZ)
        {
            short x = (short)floatX;
            short z = (short)floatZ;
            int LEVEL_BOUNDARY_MAX = 0x2000;
            int CELL_SIZE = 0x400;
            int cellX = ((x + LEVEL_BOUNDARY_MAX) / CELL_SIZE) & 0x0F;
            int cellZ = ((z + LEVEL_BOUNDARY_MAX) / CELL_SIZE) & 0x0F;
            return (cellX, cellZ);
        }

        public static uint GetWarpNodesAddress()
        {
            uint gAreas = Config.Stream.GetUInt32(0x8032DDC8);
            short currentAreaIndex = Config.Stream.GetInt16(0x8033BACA);
            uint warpNodesAddress = Config.Stream.GetUInt32(gAreas + (uint)currentAreaIndex * AreaConfig.AreaStructSize + 0x14);
            return warpNodesAddress;
        }

        public static int GetNumWarpNodes()
        {
            uint address = GetWarpNodesAddress();
            int numWarpNodes = 0;
            while (address != 0)
            {
                numWarpNodes++;
                address = Config.Stream.GetUInt32(address + 0x8);
            }

            return numWarpNodes;
        }

        public static List<uint> GetWarpNodeAddresses()
        {
            List<uint> addresses = new List<uint>();
            uint address = GetWarpNodesAddress();
            while (address != 0)
            {
                addresses.Add(address);
                address = Config.Stream.GetUInt32(address + 0x8);
            }

            return addresses;
        }

        // In Game Angle Methods

        public static int GetDeltaInGameAngle(ushort angle)
        {
            (double x, double z) = MoreMath.GetComponentsFromVector(1, angle);
            int inGameAngle = InGameTrigUtilities.InGameAngleTo(x, z);
            return angle - inGameAngle;
        }

        // Play Time

        public static string GetRealTime(uint totalFrames)
        {
            uint frameConst = 30;
            uint secondConst = 60;
            uint minuteConst = 60;
            uint hourConst = 24;
            uint dayConst = 365;

            uint totalSeconds = totalFrames / frameConst;
            uint totalMinutes = totalSeconds / secondConst;
            uint totalHours = totalMinutes / minuteConst;
            uint totalDays = totalHours / hourConst;
            uint totalYears = totalDays / dayConst;

            uint frames = totalFrames % frameConst;
            uint seconds = totalSeconds % secondConst;
            uint minutes = totalMinutes % minuteConst;
            uint hours = totalHours % hourConst;
            uint days = totalDays % dayConst;
            uint years = totalYears;

            List<uint> values = new List<uint> { years, days, hours, minutes, seconds, frames };
            int firstNonZeroIndex = values.FindIndex(value => value != 0);
            if (firstNonZeroIndex == -1) firstNonZeroIndex = values.Count - 1;
            int numValuesToShow = values.Count - firstNonZeroIndex;

            StringBuilder builder = new StringBuilder();
            if (numValuesToShow >= 6) builder.Append(years + "y ");
            if (numValuesToShow >= 5) builder.Append(days + "d ");
            if (numValuesToShow >= 4) builder.Append(hours + "h ");
            if (numValuesToShow >= 3) builder.Append(minutes + "m ");
            if (numValuesToShow >= 2) builder.Append(seconds + "s ");
            if (numValuesToShow >= 1) builder.Append(String.Format("{0:D2}", frames) + "f");
            return builder.ToString();
        }
    }
}
