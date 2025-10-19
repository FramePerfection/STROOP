using STROOP.Core;
using STROOP.Variables.Utilities;
using System;

namespace STROOP.Utilities;

public static class ProcessStreamExtensions
{
    public static bool SetValueRoundingWrapping(this ProcessStream processStream, Type type, object value, uint address, uint? mask = null, int? shift = null)
    {
        // Allow short circuiting if object is already of type
        if (type == typeof(byte) && value is byte byteValue) return processStream.SetValue(byteValue, address, false, mask, shift);
        if (type == typeof(sbyte) && value is sbyte sbyteValue) return processStream.SetValue(sbyteValue, address, false, mask, shift);
        if (type == typeof(short) && value is short shortValue) return processStream.SetValue(shortValue, address, false, mask, shift);
        if (type == typeof(ushort) && value is ushort ushortValue) return processStream.SetValue(ushortValue, address, false, mask, shift);
        if (type == typeof(int) && value is int intValue) return processStream.SetValue(intValue, address, false, mask, shift);
        if (type == typeof(uint) && value is uint uintValue) return processStream.SetValue(uintValue, address, false, mask, shift);
        if (type == typeof(float) && value is float floatValue) return processStream.SetValue(floatValue, address, false, mask, shift);
        if (type == typeof(double) && value is double doubleValue) return processStream.SetValue(doubleValue, address, false, mask, shift);

        value = ParsingUtilities.ParseDoubleNullable(value);
        if (value == null) return false;

        if (type == typeof(byte)) value = ParsingUtilities.ParseByteRoundingWrapping(value);
        if (type == typeof(sbyte)) value = ParsingUtilities.ParseSByteRoundingWrapping(value);
        if (type == typeof(short)) value = ParsingUtilities.ParseShortRoundingWrapping(value);
        if (type == typeof(ushort)) value = ParsingUtilities.ParseUShortRoundingWrapping(value);
        if (type == typeof(int)) value = ParsingUtilities.ParseIntRoundingWrapping(value);
        if (type == typeof(uint)) value = ParsingUtilities.ParseUIntRoundingWrapping(value);

        return processStream.SetValue(type, value.ToString(), address, false, mask, shift);
    }

    public static bool SetValue(this ProcessStream processStream, Type type, object value, uint address, bool absoluteAddress = false, uint? mask = null, int? shift = null)
    {
        if (value is string)
        {
            if (type == typeof(byte)) value = ParsingUtilities.ParseByteNullable(value);
            if (type == typeof(sbyte)) value = ParsingUtilities.ParseSByteNullable(value);
            if (type == typeof(short)) value = ParsingUtilities.ParseShortNullable(value);
            if (type == typeof(ushort)) value = ParsingUtilities.ParseUShortNullable(value);
            if (type == typeof(int)) value = ParsingUtilities.ParseIntNullable(value);
            if (type == typeof(uint)) value = ParsingUtilities.ParseUIntNullable(value);
            if (type == typeof(float)) value = ParsingUtilities.ParseFloatNullable(value);
            if (type == typeof(double)) value = ParsingUtilities.ParseDoubleNullable(value);
        }

        if (value == null) return false;

        if (type == typeof(byte)) return processStream.SetValue((byte)value, address, absoluteAddress, mask, shift);
        if (type == typeof(sbyte)) return processStream.SetValue((sbyte)value, address, absoluteAddress, mask, shift);
        if (type == typeof(short)) return processStream.SetValue((short)value, address, absoluteAddress, mask, shift);
        if (type == typeof(ushort)) return processStream.SetValue((ushort)value, address, absoluteAddress, mask, shift);
        if (type == typeof(int)) return processStream.SetValue((int)value, address, absoluteAddress, mask, shift);
        if (type == typeof(uint)) return processStream.SetValue((uint)value, address, absoluteAddress, mask, shift);
        if (type == typeof(float)) return processStream.SetValue((float)value, address, absoluteAddress, mask, shift);
        if (type == typeof(double)) return processStream.SetValue((double)value, address, absoluteAddress, mask, shift);

        throw new ArgumentOutOfRangeException("Cannot call ProcessStream.SetValue with type " + type);
    }
}
