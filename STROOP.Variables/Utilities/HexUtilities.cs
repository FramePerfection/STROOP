using STROOP.Variables.Utilities;

namespace STROOP.Utilities;

public static class HexUtilities
{
    public static string FormatValue(object number, int? numDigits = null, bool usePrefix = true)
    {
        object numberFormatted = number;

        // Make sure it's a number
        if (!TypeUtilities.IsNumber(numberFormatted))
        {
            numberFormatted = double.TryParse((string)numberFormatted, out double result) ? result : null;
            if (numberFormatted == null) return number.ToString();
        }

        // Convert floats/doubles into ints/uints
        if (numberFormatted is float || numberFormatted is double)
        {
            if (numberFormatted is float floatValue) numberFormatted = Math.Round(floatValue);
            else if (numberFormatted is double doubleValue) numberFormatted = Math.Round(doubleValue);
            else
            {
                int? intValueNullable = int.TryParse((string)numberFormatted, out int intValue) ? intValue : null;
                if (intValueNullable.HasValue)
                    numberFormatted = intValueNullable.Value;
                else
                {
                    uint? uintValueNullable = uint.TryParse((string)numberFormatted, out uint uintValue) ? uintValue : null;
                    if (uintValueNullable.HasValue) numberFormatted = uintValueNullable.Value;
                }
            }
        }

        if (!TypeUtilities.IsIntegerNumber(numberFormatted)) return number.ToString();

        string numDigitsString = numDigits.HasValue ? numDigits.Value.ToString() : "";
        string hexString = string.Format("{0:X" + numDigitsString + "}", numberFormatted);
        string prefix = usePrefix ? "0x" : "";
        if (numDigits.HasValue)
        {
            hexString = StringUtilities.ExactLength(hexString, numDigits.Value, true, '0');
        }

        return prefix + hexString;
    }

    public static string FormatMemory(object number, int? numDigits = null, bool usePrefix = true)
    {
        if (number is bool boolValue)
            number = boolValue ? 1 : 0;
        if (!TypeUtilities.IsNumber(number)) throw new ArgumentOutOfRangeException();

        byte[] bytes = TypeUtilities.GetBytes(number);
        List<byte> byteList = new List<byte>(bytes);
        byteList.Reverse();
        List<string> stringList = byteList.ConvertAll(b => string.Format("{0:X2}", b));
        string byteString = string.Join("", stringList);
        if (numDigits.HasValue) byteString = StringUtilities.ExactLength(byteString, numDigits.Value, true, '0');
        string prefix = usePrefix ? "0x" : "";
        return prefix + byteString;
    }
}
