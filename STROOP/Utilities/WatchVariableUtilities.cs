using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using STROOP.Controls.VariablePanel;
using STROOP.Core.Utilities;
using STROOP.Core.Variables;
using STROOP.Structs;
using STROOP.Structs.Configurations;

namespace STROOP.Utilities
{
    public static class IVariableViewExtensions
    {
        public static NamedVariableCollection.IView<T> WithKeyedValue<T, TValue>(this NamedVariableCollection.IView<T> view, string key, TValue value)
        {
            view.SetValueByKey(key, value);
            return view;
        }

        public static string GetJsonName(this NamedVariableCollection.IView view)
        {
            var explicitJsonName = view.GetValueByKey("jsonName");
            if (explicitJsonName == "")
                return null;
            if (explicitJsonName != null)
                return explicitJsonName;
            return $"{view.Name}".Replace(' ', '_').ToLower();
        }

        public static IEnumerable<T> GetNumberValues<T>(this NamedVariableCollection.IView view) where T : struct, IConvertible
        {
            if (view.TryGetNumberValues<T>(out var result))
                return result;
            throw new InvalidOperationException($"'{view.GetType().FullName}' is not a vaild number type.");
        }

        public static bool TryGetNumberValues<T>(this NamedVariableCollection.IView view, out IEnumerable<T> result)
            where T : struct, IConvertible
        {
            bool Get<Q>(out IEnumerable<T> innerResult)
            {
                innerResult = null;
                if (view is NamedVariableCollection.IView<Q> qView)
                {
                    innerResult = qView._getterFunction().Select(x => (T)Convert.ChangeType(x, typeof(T)));
                    return true;
                }

                return false;
            }

            return Get<byte>(out result)
                   || Get<sbyte>(out result)
                   || Get<ushort>(out result)
                   || Get<short>(out result)
                   || Get<uint>(out result)
                   || Get<int>(out result)
                   || Get<ulong>(out result)
                   || Get<long>(out result)
                   || Get<float>(out result)
                   || Get<double>(out result)
                ;
        }

        public static IEnumerable<bool> TrySetValue<T>(this NamedVariableCollection.IView view, T value) where T : IConvertible
        {
            IEnumerable<bool> Set<Q>()
            {
                Q convertedValue = default(Q);
                try
                {
                    convertedValue = (Q)Convert.ChangeType(value, typeof(Q));
                }
                catch (Exception)
                {
                    return null;
                }

                if (view is NamedVariableCollection.IView<Q> qView)
                    return qView._setterFunction(convertedValue);
                return null;
            }

            return Set<byte>()
                   ?? Set<sbyte>()
                   ?? Set<ushort>()
                   ?? Set<short>()
                   ?? Set<uint>()
                   ?? Set<int>()
                   ?? Set<ulong>()
                   ?? Set<long>()
                   ?? Set<float>()
                   ?? Set<double>()
                   ?? Array.Empty<bool>()
                ;
        }

        public static (CombinedValuesMeaning meaning, T value) CombineValues<T>(this NamedVariableCollection.IView view) where T : struct, IConvertible
        {
            var values = view.GetNumberValues<T>().ToArray();
            if (values.Length == 0) return (CombinedValuesMeaning.NoValue, default(T));
            T firstValue = values[0];
            for (int i = 1; i < values.Length; i++)
                if (!Equals(values[i], firstValue))
                    return (CombinedValuesMeaning.MultipleValues, default(T));
            return (CombinedValuesMeaning.SameValue, firstValue);
        }
    }

    public static class WatchVariableUtilities
    {
        private class WatchVariableWrapperFallback : WatchVariableWrapper
        {
            public WatchVariableWrapperFallback(NamedVariableCollection.IView watchVar, WatchVariableControl watchVarControl)
                : base(watchVar, watchVarControl)
            {
            }

            public override void Edit(Control parent, Rectangle bounds)
            {
            }

            public override string GetClass() => "INVALID VARIABLE";

            public override string GetValueText() => "INVALID VARIABLE";

            public override bool TrySetValue(string value) => false;

            public override void Update()
            {
            }
        }

        static readonly Dictionary<string, List<(Type wrapperType, Type typeRestriction)>> wrapperTypes = new Dictionary<string, List<(Type, Type)>>();

        static readonly Regex WatchVariableTypeNameRegex = new Regex("(?<=(^WatchVariable))[a-zA-Z0-9]+(?=(Wrapper))", RegexOptions.Compiled);

        static WatchVariableUtilities()
        {
            GeneralUtilities.ExecuteInitializers<InitializeBaseAddressAttribute>();
            foreach (var t in typeof(WatchVariableWrapper<>).Assembly.GetTypes())
            {
                var match = WatchVariableTypeNameRegex.Match(t.Name);
                if (match.Success)
                    if (!t.IsAbstract && t.IsPublic && TypeUtilities.MatchesGenericType(typeof(WatchVariableWrapper<>), t))
                    {
                        foreach (var ctor in t.GetConstructors())
                        {
                            var parameters = ctor.GetParameters();
                            if (parameters[0].ParameterType.IsGenericType
                                && parameters[0].ParameterType.GetGenericTypeDefinition() == typeof(NamedVariableCollection.IView<>)
                                && parameters[1].ParameterType == typeof(WatchVariableControl))
                            {
                                if (!wrapperTypes.TryGetValue(match.Value, out var wrapperTypeList))
                                    wrapperTypes[match.Value] = wrapperTypeList = new List<(Type wrapperType, Type typeRestriction)>();
                                wrapperTypeList.Add((t, parameters[0].ParameterType.GetGenericArguments()[0]));
                                break;
                            }
                        }

                        ;
                    }
            }
        }

        public static Type GetWrapperType(Type potentiallyNullableVariableType, string wrapperTypeName = "Number")
        {
            Type GetNonNullableWrapper(Type variableType)
            {
                int GenericTypeArgumentsLength((Type a, Type b) w) => w.a.GetGenericArguments().Length;

                bool Passt(Type restrictionType) =>
                    (variableType == restrictionType
                     || variableType.IsSubclassOf(restrictionType)
                     || variableType.GetInterfaces().Any(i => i == restrictionType));

                bool CanWrap((Type wrapperType, Type typeRestriction) wrapper, out Type result)
                {
                    result = null;
                    if (wrapper.typeRestriction.IsGenericParameter
                            ? wrapper.typeRestriction.GetGenericParameterConstraints().All(r => Passt(r))
                            : Passt(wrapper.typeRestriction)
                       )
                    {
                        var innermostType = wrapper.wrapperType;
                        Stack<Type> genericTypeArguments = new Stack<Type>();
                        genericTypeArguments.Push(wrapper.wrapperType);

                        while (innermostType != null && innermostType.GetGenericArguments().Length == 2)
                        {
                            innermostType = innermostType.GetGenericArguments()[0];
                            if (innermostType == null)
                                return false;
                            var newArgs = innermostType.GetGenericArguments();
                            if (newArgs.Length > 2)
                                return false;
                            genericTypeArguments.Push(innermostType
                                .GetGenericParameterConstraints()
                                .FirstOrDefault(c => c.IsClass)
                                ?.GetGenericTypeDefinition()
                            );
                        }

                        if (wrapper.wrapperType.IsGenericType)
                        {
                            genericTypeArguments.Push(variableType);
                            while (genericTypeArguments.Count > 1)
                            {
                                var qualifiedType = genericTypeArguments.Pop();
                                var genericType = genericTypeArguments.Pop();
                                if (genericType == null)
                                    return false;
                                genericTypeArguments.Push(genericType.MakeGenericType(genericType.GetGenericArguments().Length == 2 ? new[] { qualifiedType, variableType } : new[] { qualifiedType }));
                            }
                        }

                        result = genericTypeArguments.Pop();
                    }
                    else if (Passt(wrapper.typeRestriction))
                        result = wrapper.wrapperType;

                    return result != null;
                }

                if (wrapperTypes.TryGetValue(wrapperTypeName, out var specificCandidateList))
                    foreach (var specificCandidate in specificCandidateList.OrderBy(GenericTypeArgumentsLength))
                        if (CanWrap(specificCandidate, out var specificWrapper))
                            return specificWrapper;

                foreach (var fallbackCandidate in wrapperTypes.Values.SelectMany(list => list).OrderBy(GenericTypeArgumentsLength))
                    if (CanWrap(fallbackCandidate, out var fallbackWrapper))
                        return fallbackWrapper;

                return null;
            }

            bool isNullable = potentiallyNullableVariableType.IsGenericType && potentiallyNullableVariableType.GetGenericTypeDefinition() == typeof(Nullable<>);
            var nonNullableWrapper = GetNonNullableWrapper(isNullable ? potentiallyNullableVariableType.GetGenericArguments()[0] : potentiallyNullableVariableType);
            if (nonNullableWrapper != null)
                return isNullable
                    ? typeof(WatchVariableNullableWrapper<,>).MakeGenericType(nonNullableWrapper, nonNullableWrapper.GetGenericArguments()[0])
                    : nonNullableWrapper;

            System.Diagnostics.Debugger.Break();
            throw new InvalidOperationException($"{potentiallyNullableVariableType.FullName} could not be wrapped!");
        }

        public static bool TryCreateWrapper(NamedVariableCollection.IView view, WatchVariableControl control, out WatchVariableWrapper result)
        {
            result = null;
            var interfaceType = view.GetType().GetInterfaces().FirstOrDefault(x => x.Name == $"{nameof(NamedVariableCollection.IView)}`1");
            if (interfaceType == null)
            {
                result = new WatchVariableWrapperFallback(view, control);
                return false;
            }

            var wrapperType = view.GetWrapperType();
            var constructor = wrapperType.GetConstructor(new Type[] { interfaceType, typeof(WatchVariableControl) });
            if (constructor == null)
            {
                result = new WatchVariableWrapperFallback(view, control);
                return false;
            }

            result = (WatchVariableWrapper)constructor.Invoke(new object[] { view, control });
            return true;
        }

        public static SortedDictionary<string, Func<IEnumerable<uint>>> baseAddressGetters = new SortedDictionary<string, Func<IEnumerable<uint>>>();

        //TODO: Move some of these where they belong
        [InitializeBaseAddress]
        static void InitBaseAddresses()
        {
            baseAddressGetters[BaseAddressType.None] = GetBaseAddressListZero;
            baseAddressGetters[BaseAddressType.Absolute] = GetBaseAddressListZero;
            baseAddressGetters[BaseAddressType.Relative] = GetBaseAddressListZero;

            baseAddressGetters[BaseAddressType.Mario] = () => new List<uint> { MarioConfig.StructAddress };
            baseAddressGetters[BaseAddressType.MarioObj] = () => new List<uint> { Config.Stream.GetUInt32(MarioObjectConfig.PointerAddress) };

            baseAddressGetters[BaseAddressType.Camera] = () => new List<uint> { CameraConfig.StructAddress };
            baseAddressGetters[BaseAddressType.CameraStruct] = () => new List<uint> { CameraConfig.CamStructAddress };
            baseAddressGetters[BaseAddressType.LakituStruct] = () => new List<uint> { CameraConfig.LakituStructAddress };
            baseAddressGetters[BaseAddressType.CameraModeInfo] = () => new List<uint> { CameraConfig.ModeInfoAddress };
            baseAddressGetters[BaseAddressType.CameraModeTransition] = () => new List<uint> { CameraConfig.ModeTransitionAddress };
            baseAddressGetters[BaseAddressType.CameraSettings] = () =>
            {
                uint a1 = 0x8033B910;
                uint a2 = Config.Stream.GetUInt32(a1);
                uint a3 = Config.Stream.GetUInt32(a2 + 0x10);
                uint a4 = Config.Stream.GetUInt32(a3 + 0x08);
                uint a5 = Config.Stream.GetUInt32(a4 + 0x10);
                return new List<uint> { a5 };
            };

            baseAddressGetters[BaseAddressType.File] = () => new List<uint> { FileConfig.CurrentFileAddress };
            baseAddressGetters[BaseAddressType.MainSave] = () => new List<uint> { MainSaveConfig.CurrentMainSaveAddress };

            baseAddressGetters[BaseAddressType.Object] = () => Config.ObjectSlotsManager.SelectedSlotsAddresses;
            baseAddressGetters[BaseAddressType.ProcessGroup] = () =>
                Config.ObjectSlotsManager.SelectedObjects.ConvertAll(obj => obj.CurrentProcessGroup ?? uint.MaxValue);

            baseAddressGetters["Graphics"] = () =>
                Config.ObjectSlotsManager.SelectedSlotsAddresses.ConvertAll(objAddress => Config.Stream.GetUInt32(objAddress + ObjectConfig.BehaviorGfxOffset));

            baseAddressGetters["Animation"] = () =>
                Config.ObjectSlotsManager.SelectedSlotsAddresses.ConvertAll(objAddress => Config.Stream.GetUInt32(objAddress + ObjectConfig.AnimationOffset));

            baseAddressGetters["Waypoint"] = () =>
                Config.ObjectSlotsManager.SelectedSlotsAddresses.ConvertAll(objAddress => Config.Stream.GetUInt32(objAddress + ObjectConfig.WaypointOffset));

            baseAddressGetters[BaseAddressType.Area] = () => new List<uint> { AreaConfig.SelectedAreaAddress };
        }

        public static WatchVariableSubclass GetSubclass(string stringValue)
        {
            if (stringValue == null) return WatchVariableSubclass.Number;
            return (WatchVariableSubclass)Enum.Parse(typeof(WatchVariableSubclass), stringValue);
        }

        public static Coordinate GetCoordinate(string stringValue)
        {
            return (Coordinate)Enum.Parse(typeof(Coordinate), stringValue);
        }

        public static List<string> ParseVariableGroupList(string stringValue) =>
            new List<string>(Array.ConvertAll(stringValue.Split('|', ','), _ => _.Trim()));

        public static readonly List<uint> BaseAddressListZero = new List<uint> { 0 };
        public static readonly List<uint> BaseAddressListEmpty = new List<uint> { };
        private static List<uint> GetBaseAddressListZero() => BaseAddressListZero;
        private static List<uint> GetBaseAddressListEmpty() => BaseAddressListEmpty;

        public static IEnumerable<uint> GetBaseAddresses(string baseAddressType)
        {
            if (baseAddressGetters.TryGetValue(baseAddressType, out var result))
                return result();
            return new List<uint>();
        }
    }
}
