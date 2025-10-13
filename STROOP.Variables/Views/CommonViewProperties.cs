using STROOP.Core.Utilities;

namespace STROOP.Variables.Views;

public class CommonViewProperties
{
    static CommonViewProperties() => StringSymbolAttribute.InitializeDeclaredStrings(typeof(CommonViewProperties));

    [StringSymbol]
    public static readonly string
        useHex,
        invertBool,
        specialType,
        roundingLimit,
        display,
        color;
}
