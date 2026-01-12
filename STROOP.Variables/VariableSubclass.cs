using STROOP.Core.Utilities;

namespace STROOP.Variables;

public static class VariableSubclass
{
    static VariableSubclass() => StringSymbolAttribute.InitializeDeclaredStrings(typeof(VariableSubclass));

    [StringSymbol]
    public static string
        Number,
        String,
        Angle,
        Object,
        Triangle,
        Address,
        Boolean;
}

public static class VariableSubclassExtensions
{
    public static string DefaultIfNull<T>(this string? subclass)
        => subclass ?? typeof(T) switch
        {
            _ when typeof(T) == typeof(sbyte) || typeof(T) == typeof(short) || typeof(T) == typeof(int) || typeof(T) == typeof(long) => VariableSubclass.Number,
            _ when typeof(T) == typeof(byte) || typeof(T) == typeof(ushort) || typeof(T) == typeof(uint) || typeof(T) == typeof(ulong) => VariableSubclass.Number,
            _ when typeof(T) == typeof(float) || typeof(T) == typeof(double) => VariableSubclass.Number,
            _ when typeof(T) == typeof(bool) => VariableSubclass.Boolean,
            _ when typeof(T) == typeof(string) => VariableSubclass.String,
            _ when typeof(T).IsGenericType && typeof(T).GetGenericTypeDefinition() == typeof(Nullable<>) => VariableSubclass.Number,
            _ => throw new NotImplementedException("Figure out what to do with these types..."),
        };
}
