using STROOP.Core.Utilities;

namespace STROOP.Variables;

public class CommonVariableProperties
{
    static CommonVariableProperties() => StringSymbolAttribute.InitializeDeclaredStrings(typeof(CommonVariableProperties));

    [StringSymbol]
    public static readonly string
        useHex,
        invertBool,
        specialType,
        roundingLimit,
        display,
        color;
}
