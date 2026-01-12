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
        private class VariableCellFallback(IVariable view, VariableCellControl<TUiContext> cellControl)
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

        static readonly Regex VariableTypeNameRegex = new Regex("(?<=(^Variable))[a-zA-Z0-9]+(?=(Cell))", RegexOptions.Compiled);

        static VariableCellFactory()
        {
            GeneralUtilities.ExecuteInitializers<InitializeBaseAddressAttribute>();
            foreach (var t in GeneralUtilities.GetStroopTypes())
            {
                var match = VariableTypeNameRegex.Match(t.Name);
                if (match.Success)
                    if (!t.IsAbstract && t.IsPublic && TypeUtilities.MatchesGenericType(typeof(VariableCell<,>), t))
                        wrapperTypes[match.Value] = t;
            }
        }

        internal static bool TryCreateWrapper(
            IVariable view,
            VariableCellControl<TUiContext> cellControl,
            out IVariableCell result,
            Type wrapperType = null)
        {
            result = null;
            var interfaceType = view.GetType().GetInterfaces().FirstOrDefault(x => x.Name == $"{nameof(IVariable)}`1");
            if (interfaceType == null)
            {
                result = new VariableCellFallback(view, cellControl);
                return false;
            }

            bool isNullable = view.ClrType.IsGenericType && view.ClrType.GetGenericTypeDefinition() == typeof(Nullable<>);
            var genericArgument = isNullable ? view.ClrType.GetGenericArguments()[0] : view.ClrType;
            if (wrapperType == null)
            {
                wrapperType = wrapperTypes[view.Subclass];
                if (wrapperType.IsGenericTypeDefinition)
                    wrapperType = wrapperType.MakeGenericType(genericArgument);
                if (isNullable)
                    wrapperType = typeof(VariableNullableCell<,,>).MakeGenericType(
                        typeof(TUiContext),
                        wrapperType,
                        interfaceType.GenericTypeArguments[0].GenericTypeArguments[0]
                    );
            }

            // Try to create as a root cell
            var constructor = wrapperType.GetConstructor([ interfaceType, typeof(VariableCellControl<TUiContext>) ]);
            if (constructor != null)
            {
                result = (IVariableCell)constructor.Invoke([ view, cellControl ]);
                return true;
            }

            // Try to create as a decorator cell
            var decoratorCtor = wrapperType
                .GetConstructors()
                .Select(x => new { Parameters = x.GetParameters(), Ctor = x, })
                .SingleOrDefault(x =>
                    x.Parameters.Length == 1
                    && x.Parameters[0].ParameterType.GetInterfaces().Any(IsMatchingBaseCellType)
                );
            if (decoratorCtor != null
                && TryCreateWrapper(view, cellControl, out var baseWrapper, decoratorCtor.Parameters[0].ParameterType))
            {
                result = (IVariableCell)decoratorCtor.Ctor.Invoke([ baseWrapper ]);
                return true;
            }

            // Could not construct a matching cell, yield a fallback
            result = new VariableCellFallback(view, cellControl);
            return false;

            bool IsMatchingBaseCellType(Type t)
                => t.IsGenericType
                   && t.GetGenericTypeDefinition() == typeof(IVariableCellData<>)
                   && t.GetGenericArguments()[0] == interfaceType.GetGenericArguments()[0];
        }

        public static (string name, IVariable var) ParseXml(XElement element, VariableSpecialDictionary sepcialVariables)
        {
            switch (element.Name.LocalName)
            {
                case "Data":
                    var specialType = element.Attribute(XName.Get("specialType"))?.Value;
                    return (element.Value,
                        specialType != null
                            ? sepcialVariables.TryGetValue(specialType, out var special)
                                ? special
                                : null
                            : FromXml(element).view);
            }

            return (null, null);
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
