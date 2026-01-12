using System.Reflection;

namespace STROOP.Core.Utilities;

/// <summary>
/// Denotes that a static string variable's value shall be initialized with its field name when <see cref="InitializeDeclaredStrings(Type)"/> is called on its declaring type.
/// </summary>
[AttributeUsage(AttributeTargets.Field)]
public class StringSymbolAttribute : Attribute
{
    public static void InitializeDeclaredStrings(Type t)
    {
        foreach (FieldInfo? field in t.GetFields(BindingFlags.Public | BindingFlags.Static))
            if (field.FieldType == typeof(string))
                field.SetValue(null, field.Name);
    }
}
