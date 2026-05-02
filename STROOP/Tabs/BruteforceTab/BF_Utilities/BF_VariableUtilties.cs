using System;
using System.Collections.Generic;
using System.Reflection;
using STROOP.Controls.VariablePanel;
using STROOP.Variables;

namespace STROOP.Tabs.BruteforceTab.BF_Utilities
{
    class BF_VariableUtilties
    {
        class WatchVariableQuarterstepWrapper : WatchVariableSelectionWrapper<WatchVariableNumberWrapper<byte>, byte>
        {
            public WatchVariableQuarterstepWrapper(NamedVariableCollection.IView<byte> var, WatchVariableControl control) : base(var, control)
            {
                for (int i = 0; i < 4; i++)
                {
                    var i_cap = (byte)i;
                    options.Add(($"QS {i_cap + 1} intended", () => (byte)(i_cap * 4 + 0)));
                    options.Add(($"QS {i_cap + 1} wall1", () => (byte)(i_cap * 4 + 1)));
                    options.Add(($"QS {i_cap + 1} wall2", () => (byte)(i_cap * 4 + 2)));
                    options.Add(($"QS {i_cap + 1} final", () => (byte)(i_cap * 4 + 3)));
                }

                view._setterFunction(4 * 4 - 1);
            }
        }

        public static readonly Dictionary<string, WatchVariableSubclass> fallbackWrapperTypes = new Dictionary<string, WatchVariableSubclass>()
        {
            ["u32"] = WatchVariableSubclass.Number,
            ["s32"] = WatchVariableSubclass.Number,
            ["u16"] = WatchVariableSubclass.Number,
            ["s16"] = WatchVariableSubclass.Number,
            ["u8"] = WatchVariableSubclass.Number,
            ["s8"] = WatchVariableSubclass.Number,
            ["f32"] = WatchVariableSubclass.Number,
            ["f64"] = WatchVariableSubclass.Number,
            ["string"] = WatchVariableSubclass.String,
            ["boolean"] = WatchVariableSubclass.Boolean,
            // ["quarterstep"] = /* what* /,
        };

        public static readonly Dictionary<string, Type> backingTypes = new Dictionary<string, Type>()
        {
            ["u32"] = typeof(uint),
            ["s32"] = typeof(int),
            ["u16"] = typeof(ushort),
            ["s16"] = typeof(short),
            ["u8"] = typeof(byte),
            ["s8"] = typeof(sbyte),
            ["f32"] = typeof(float),
            ["f64"] = typeof(double),
            ["string"] = typeof(string),
            ["boolean"] = typeof(bool),
            ["quarterstep"] = typeof(byte),
        };

        private static MethodInfo DefaultFunc = typeof(BF_VariableUtilties).GetMethod(nameof(Default), BindingFlags.NonPublic | BindingFlags.Static);

        public static IBruteforceVariableView CreateNamedVariable(string bruteforcerType, string name, object defaultValue = null)
        {
            var backingType = backingTypes[bruteforcerType];
            try
            {
                if (defaultValue != null)
                    defaultValue = Convert.ChangeType(defaultValue, backingType);
            }
            catch (Exception)
            {
            }

            return (IBruteforceVariableView)typeof(BruteforceVariableView<>)
                .MakeGenericType(backingType)
                .GetConstructor(new[] { typeof(string), typeof(string), backingType })
                .Invoke(new object[] { bruteforcerType, name, defaultValue ?? DefaultFunc.MakeGenericMethod(backingType).Invoke(null, Array.Empty<object>()) });
        }

        private static T Default<T>() => default(T);
    }
}
