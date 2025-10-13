using STROOP.Core.Utilities;
using STROOP.Utilities;
using STROOP.Variables;
using STROOP.Variables.Utilities;
using STROOP.Variables.Views;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.Xml.Linq;

namespace STROOP.Controls.VariablePanel
{
    public static class WatchVariableWrapperFactory
    {
        private class WatchVariableWrapperFallback : WatchVariableWrapper
        {
            public WatchVariableWrapperFallback(IVariableView watchVar, WatchVariableControl watchVarControl)
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

        static readonly Dictionary<string, Type> wrapperTypes = new Dictionary<string, Type>();

        static readonly Regex WatchVariableTypeNameRegex = new Regex("(?<=(^WatchVariable))[a-zA-Z0-9]+(?=(Wrapper))", RegexOptions.Compiled);

        static WatchVariableWrapperFactory()
        {
            GeneralUtilities.ExecuteInitializers<InitializeBaseAddressAttribute>();
            foreach (var t in typeof(WatchVariableWrapper<>).Assembly.GetTypes())
            {
                var match = WatchVariableTypeNameRegex.Match(t.Name);
                if (match.Success)
                    if (!t.IsAbstract && t.IsPublic && TypeUtilities.MatchesGenericType(typeof(WatchVariableWrapper<>), t))
                        wrapperTypes[match.Value] = t;
            }
        }

        public static bool TryCreateWrapper(IVariableView view, WatchVariableControl control, out WatchVariableWrapper result)
        {
            result = null;
            var interfaceType = view.GetType().GetInterfaces().FirstOrDefault(x => x.Name == $"{nameof(IVariableView)}`1");
            if (interfaceType == null)
            {
                result = new WatchVariableWrapperFallback(view, control);
                return false;
            }

            bool isNullable = view.ClrType.IsGenericType && view.ClrType.GetGenericTypeDefinition() == typeof(Nullable<>);
            var genericArgument = isNullable ? view.ClrType.GetGenericArguments()[0] : view.ClrType;
            var wrapperType = wrapperTypes[view.Subclass];
            if (wrapperType.IsGenericTypeDefinition)
                wrapperType = wrapperType.MakeGenericType(genericArgument);
            if (isNullable)
                wrapperType = typeof(WatchVariableNullableWrapper<,>).MakeGenericType(
                    wrapperType,
                    interfaceType.GenericTypeArguments[0].GenericTypeArguments[0]
                );
            var constructor = wrapperType.GetConstructor(new Type[] { interfaceType, typeof(WatchVariableControl) });
            if (constructor == null)
            {
                result = new WatchVariableWrapperFallback(view, control);
                return false;
            }

            result = (WatchVariableWrapper)constructor.Invoke(new object[] { view, control });
            return true;
        }

        public static IVariableView ParseXml(XElement element)
        {
            switch (element.Name.LocalName)
            {
                case "Data":
                    var specialType = element.Attribute(XName.Get("specialType"))?.Value;
                    if (specialType != null)
                    {
                        if (WatchVariableSpecialUtilities.dictionary.TryGetValue(specialType, out var value))
                            return value;
                        return null;
                    }

                    return FromXml(element).view;
            }

            return null;
        }

        public static (MemoryDescriptor descriptor, NamedVariableCollection.XmlMemoryView view) FromXml(XElement element)
        {
            string typeName = (element.Attribute(XName.Get("type"))?.Value);
            string baseAddressType = element.Attribute(XName.Get("base")).Value;
            uint? offsetUS = TryParseHex("offsetUS");
            uint? offsetJP = TryParseHex("offsetJP");
            uint? offsetSH = TryParseHex("offsetSH");
            uint? offsetEU = TryParseHex("offsetEU");
            uint? offsetDefault = TryParseHex("offset");
            uint? mask = TryParseHex("mask");
            int? shift = (int?)TryParseHex("shift");
            bool handleMapping = element.Attribute(XName.Get("handleMapping")) != null
                                 && (bool.TryParse(element.Attribute(XName.Get("handleMapping")).Value, out var v) ? v : false);

            var memoryDescriptor = new MemoryDescriptor(TypeUtilities.StringToType[typeName], baseAddressType, offsetUS, offsetJP, offsetSH, offsetEU, offsetDefault, mask, shift, handleMapping);
            var view = (NamedVariableCollection.XmlMemoryView)
                typeof(NamedVariableCollection.XmlMemoryView<>)
                    .MakeGenericType(TypeUtilities.StringToType[typeName])
                    .GetConstructor(new Type[] { typeof(MemoryDescriptor), typeof(XElement) })
                    .Invoke(new object[] { memoryDescriptor, element });
            return (memoryDescriptor, view);

            uint? TryParseHex(string elementName)
                => uint.TryParse(
                    element.Attribute(XName.Get(elementName))?.Value.Substring(2) ?? "",
                    NumberStyles.HexNumber,
                    null,
                    out var result)
                    ? result
                    : null;
        }
    }
}
