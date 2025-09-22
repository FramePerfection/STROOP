namespace STROOP.Variables
{
    public enum WatchVariableSubclass
    {
        Number,
        String,
        Angle,
        Object,
        Triangle,
        Address,
        Boolean,
    }

    // public sealed class WatchVariableSubclass
    // {
    //     public static WatchVariableSubclass Number = new WatchVariableSubclass(
    //         VariableBehavior.Number,
    //         typeof(sbyte), typeof(short), typeof(int), typeof(long),
    //         typeof(byte), typeof(ushort), typeof(uint), typeof(ulong),
    //         typeof(float), typeof(double)
    //     );
    //
    //     public static WatchVariableSubclass String = new WatchVariableSubclass(VariableBehavior.String, typeof(string));
    //     public static WatchVariableSubclass Angle = new WatchVariableSubclass(Number, VariableBehavior.Angle, typeof(string));
    //     public static WatchVariableSubclass Address = new WatchVariableSubclass(Number, VariableBehavior.Address, typeof(uint));
    //     public static WatchVariableSubclass Object = new WatchVariableSubclass(Address, VariableBehavior.Object, typeof(uint));
    //     public static WatchVariableSubclass Triangle = new WatchVariableSubclass(Address, VariableBehavior.Triangle, typeof(uint));
    //     public static WatchVariableSubclass Boolean = new WatchVariableSubclass(VariableBehavior.Boolean, typeof(bool));
    //
    //     public readonly VariableBehavior Behavior;
    //     public readonly Type[] ClrTypes;
    //     public readonly WatchVariableSubclass? BaseClass;
    //
    //     internal WatchVariableSubclass(VariableBehavior behavior, params Type[] clrTypes)
    //     {
    //         Behavior = behavior;
    //         ClrTypes = clrTypes;
    //     }
    //
    //     internal WatchVariableSubclass(WatchVariableSubclass baseClass, VariableBehavior behavior, params Type[] clrTypes)
    //         : this(behavior, clrTypes)
    //     {
    //         BaseClass = baseClass;
    //     }
    // }
    //
    // public static class WatchVariableSubclassExtensions
    // {
    //     public static WatchVariableSubclass DefaultIfNull<T>(this WatchVariableSubclass? subclass)
    //         => subclass ?? typeof(T) switch
    //         {
    //             _ when typeof(T) == typeof(sbyte) || typeof(T) == typeof(short) || typeof(T) == typeof(int)|| typeof(T) == typeof(long) => WatchVariableSubclass.Number,
    //             _ when typeof(T) == typeof(byte) || typeof(T) == typeof(ushort) || typeof(T) == typeof(uint)|| typeof(T) == typeof(ulong) => WatchVariableSubclass.Number,
    //             _ when typeof(T) == typeof(float) || typeof(T) == typeof(double) => WatchVariableSubclass.Number,
    //             _ when typeof(T) == typeof(bool) => WatchVariableSubclass.Boolean,
    //             _ when typeof(T) == typeof(string) => WatchVariableSubclass.String,
    //             _ when typeof(T).IsGenericType && typeof(T).GetGenericTypeDefinition() == typeof(Nullable<>) => WatchVariableSubclass.Number,
    //             _ => throw new NotImplementedException("Figure out what to do with these types..."),
    //         };
    // }
}
