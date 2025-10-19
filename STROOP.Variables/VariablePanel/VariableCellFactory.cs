using STROOP.Core.Utilities;
using STROOP.Variables.Utilities;
using STROOP.Variables.VariablePanel.Cells;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace STROOP.Variables.VariablePanel
{
    public static class VariableCellFactory<TUiContext> where TUiContext : IUiContext
    {
        private class WatchVariableCellFallback(IVariable view, VariableCellControl<TUiContext> cellControl)
            : IVariableCell
                , IVariableCellUi<TUiContext>
        {
            public VariableCellControl<TUiContext> control { get; } = cellControl;
            public DescribedMemoryState memory => null;
            public IVariableCellUi<TUiContext>.CustomDraw CustomDrawOperation => null;

            List<string> IVariableCell.GetVarInfo() => [];

            public string GetClass() => "INVALID VARIABLE";

            public string GetValueText() => "INVALID VARIABLE";

            public bool TrySetValue(string value) => false;

            public void SingleClick(TUiContext uiContext) { }
            public void DoubleClick(TUiContext uiContext) { }
            public void Update() { }
        }

        static readonly Dictionary<string, Type> wrapperTypes = new Dictionary<string, Type>();

        static readonly Regex WatchVariableTypeNameRegex = new Regex("(?<=(^Variable))[a-zA-Z0-9]+(?=(Cell))", RegexOptions.Compiled);

        static VariableCellFactory()
        {
            GeneralUtilities.ExecuteInitializers<InitializeBaseAddressAttribute>();
            foreach (var t in GeneralUtilities.GetStroopTypes())
            {
                var match = WatchVariableTypeNameRegex.Match(t.Name);
                if (match.Success)
                    if (!t.IsAbstract && t.IsPublic && TypeUtilities.MatchesGenericType(typeof(VariableCell<,>), t))
                        wrapperTypes[match.Value] = t;
            }
        }

        internal static bool TryCreateWrapper(IVariable view, VariableCellControl<TUiContext> cellControl, out IVariableCell result)
        {
            result = null;
            var interfaceType = view.GetType().GetInterfaces().FirstOrDefault(x => x.Name == $"{nameof(IVariable)}`1");
            if (interfaceType == null)
            {
                result = new WatchVariableCellFallback(view, cellControl);
                return false;
            }

            bool isNullable = view.ClrType.IsGenericType && view.ClrType.GetGenericTypeDefinition() == typeof(Nullable<>);
            var genericArgument = isNullable ? view.ClrType.GetGenericArguments()[0] : view.ClrType;
            var wrapperType = wrapperTypes[view.Subclass];
            if (wrapperType.IsGenericTypeDefinition)
                wrapperType = wrapperType.MakeGenericType(genericArgument);
            if (isNullable)
                wrapperType = typeof(VariableNullableCell<,,>).MakeGenericType(
                    typeof(TUiContext),
                    wrapperType,
                    interfaceType.GenericTypeArguments[0].GenericTypeArguments[0]
                );
            var constructor = wrapperType.GetConstructor([ interfaceType, typeof(VariableCellControl<TUiContext>) ]);
            if (constructor == null)
            {
                result = new WatchVariableCellFallback(view, cellControl);
                return false;
            }

            result = (IVariableCell)constructor.Invoke([ view, cellControl ]);
            return true;
        }

        public static IVariable ParseXml(XElement element, VariableSpecialDictionary sepcialVariables)
        {
            switch (element.Name.LocalName)
            {
                case "Data":
                    var specialType = element.Attribute(XName.Get("specialType"))?.Value;
                    return specialType != null
                        ? sepcialVariables.TryGetValue(specialType, out var special)
                            ? special
                            : null
                        : FromXml(element).view;
            }

            return null;
        }

        public static (MemoryDescriptor descriptor, IMemoryVariable view) FromXml(XElement element)
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
            var view = (IMemoryVariable)
                typeof(XmlMemoryVariable<>)
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
