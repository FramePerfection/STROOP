using STROOP.Core.Utilities;

namespace STROOP.Variables;

public static class WatchVariableSubclass
{
    static WatchVariableSubclass() => StringSymbolAttribute.InitializeDeclaredStrings(typeof(WatchVariableSubclass));

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

public static class WatchVariableSubclassExtensions
{
    public static string DefaultIfNull<T>(this string? subclass)
        => subclass ?? typeof(T) switch
        {
            _ when typeof(T) == typeof(sbyte) || typeof(T) == typeof(short) || typeof(T) == typeof(int) || typeof(T) == typeof(long) => WatchVariableSubclass.Number,
            _ when typeof(T) == typeof(byte) || typeof(T) == typeof(ushort) || typeof(T) == typeof(uint) || typeof(T) == typeof(ulong) => WatchVariableSubclass.Number,
            _ when typeof(T) == typeof(float) || typeof(T) == typeof(double) => WatchVariableSubclass.Number,
            _ when typeof(T) == typeof(bool) => WatchVariableSubclass.Boolean,
            _ when typeof(T) == typeof(string) => WatchVariableSubclass.String,
            _ when typeof(T).IsGenericType && typeof(T).GetGenericTypeDefinition() == typeof(Nullable<>) => WatchVariableSubclass.Number,
            _ => throw new NotImplementedException("Figure out what to do with these types..."),
        };
}
