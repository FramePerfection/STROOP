using System;
using System.Collections.Generic;
using System.Globalization;
using OpenTK;
using System.Text.RegularExpressions;
using OpenTK.Mathematics;

namespace STROOP.Utilities
{
    public static class ParsingUtilities
    {

        public static bool TryParseNumber<T>(string value, out T result) where T: struct, IConvertible
        {
            {
                if (value.IndexOf("0x") != -1 && ParsingUtilities.TryParseHex(value, out uint uintV))
                    result = (T)Convert.ChangeType(uintV, typeof(T));
                else if (ulong.TryParse(value, out ulong ulongV))
                    result = (T)Convert.ChangeType(ulongV, typeof(T));
                else if (long.TryParse(value, out long longV))
                    result = (T)Convert.ChangeType(longV, typeof(T));
                else if (double.TryParse(value, out double doubleV))
                    result = (T)Convert.ChangeType(doubleV, typeof(T));
                else
                {
                    result = default(T);
                    return false;
                }
                return true;
            }
        }

        public static List<T> ParseTupleList<T>(string input, Func<string[], T> elementConverter)
        {
            List<T> ts = new List<T>();
            var tupleSplits = input.Split(';');
            foreach (var split in tupleSplits)
            {
                var startIndex = split.IndexOf('(') + 1;
                if (startIndex == 0)
                    continue;
                var trim = split.Substring(startIndex, split.IndexOf(')') - startIndex);
                ts.Add(elementConverter(Array.ConvertAll(trim.Split(','), o => o.Trim())));
            }
            return ts;
        }

        public static string CreatePointList(List<(float x, float y , float z)> points) => StringUtilities.Concat(points, p => $"({p.x}, {p.y}, {p.z});");

        public static List<(float, float, float)> ParsePointList(string input) => 
            ParseTupleList(input, vals =>
                {
                    if (vals.Length == 2)
                        return (float.Parse(vals[0]), 0, float.Parse(vals[1]));
                    else if (vals.Length == 3)
                        return (float.Parse(vals[0]), float.Parse(vals[1]), float.Parse(vals[2]));
                    return (0, 0, 0);
                });

        public static bool TryParseHex(string obj, out uint result)
        {
            string str = obj?.ToString() ?? "";
            int prefixPos = str.IndexOf("0x");
            if (prefixPos != -1)
                str = str.Substring(prefixPos + 2);
            return uint.TryParse(str, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out result);
        }

        public static uint ParseHex(string obj)
        {
            if (TryParseHex(obj, out uint result))
                return result;
            throw new Exception($"{obj} is not a Hex number.");
        }

        public static uint? ParseHexNullable(object obj)
        {
            if (obj == null) return null;
            if (TryParseHex(obj.ToString(), out uint parsed))
                return parsed;
            else
                return null;
        }

        public static int? ParseIntNullable(object obj)
        {
            string text = obj?.ToString();
            int parsed;
            if (int.TryParse(text, out parsed))
            {
                return parsed;
            }
            return null;
        }

        public static int ParseInt(object obj)
        {
            return ParseIntNullable(obj) ?? 0;
        }

        public static uint? ParseUIntNullable(object obj)
        {
            string text = obj?.ToString();
            uint parsed;
            if (uint.TryParse(text, out parsed))
            {
                return parsed;
            }
            return null;
        }

        public static uint ParseUInt(object obj)
        {
            return ParseUIntNullable(obj) ?? 0;
        }

        public static short? ParseShortNullable(object obj)
        {
            string text = obj?.ToString();
            short parsed;
            if (short.TryParse(text, out parsed))
            {
                return parsed;
            }
            return null;
        }

        public static short ParseShort(object obj)
        {
            return ParseShortNullable(obj) ?? 0;
        }

        public static ushort? ParseUShortNullable(object obj)
        {
            string text = obj?.ToString();
            ushort parsed;
            if (ushort.TryParse(text, out parsed))
            {
                return parsed;
            }
            return null;
        }

        public static ushort ParseUShort(object obj)
        {
            return ParseUShortNullable(obj) ?? 0;
        }

        public static long? ParseLongNullable(object obj)
        {
            string text = obj?.ToString();
            long parsed;
            if (long.TryParse(text, out parsed))
            {
                return parsed;
            }
            return null;
        }

        public static long ParseLong(object obj)
        {
            return ParseLongNullable(obj) ?? 0;
        }

        public static ulong? ParseULongNullable(object obj)
        {
            string text = obj?.ToString();
            ulong parsed;
            if (ulong.TryParse(text, out parsed))
            {
                return parsed;
            }
            return null;
        }

        public static ulong ParseULong(object obj)
        {
            return ParseULongNullable(obj) ?? 0;
        }

        public static byte? ParseByteNullable(object obj)
        {
            string text = obj?.ToString();
            byte parsed;
            if (byte.TryParse(text, out parsed))
            {
                return parsed;
            }
            return null;
        }

        public static byte ParseByte(object obj)
        {
            return ParseByteNullable(obj) ?? 0;
        }

        public static sbyte? ParseSByteNullable(object obj)
        {
            string text = obj?.ToString();
            sbyte parsed;
            if (sbyte.TryParse(text, out parsed))
            {
                return parsed;
            }
            return null;
        }

        public static sbyte ParseSByte(object obj)
        {
            return ParseSByteNullable(obj) ?? 0;
        }

        public static float? ParseFloatNullable(object obj)
        {
            if (obj is float floatValue) return floatValue;
            if (obj is double doubleValue) return (float)doubleValue;
            string text = obj?.ToString();
            float parsed;
            if (float.TryParse(text, out parsed))
            {
                return parsed;
            }
            return null;
        }

        public static float ParseFloat(object obj)
        {
            return ParseFloatNullable(obj) ?? 0;
        }

        public static double? ParseDoubleNullable(object obj)
        {
            if (obj is float floatValue) return floatValue;
            if (obj is double doubleValue) return doubleValue;
            string text = obj?.ToString();
            double parsed;
            if (double.TryParse(text, out parsed))
            {
                return parsed;
            }
            return null;
        }

        public static double ParseDouble(object obj)
        {
            return ParseDoubleNullable(obj) ?? 0;
        }

        public static bool? ParseBoolNullable(object obj)
        {
            string text = obj?.ToString();
            bool parsed;
            if (bool.TryParse(text, out parsed))
            {
                return parsed;
            }
            return null;
        }

        public static bool ParseBool(string obj)
        {
            return ParseBoolNullable(obj) ?? false;
        }

        public static object ParseValueNullable(object obj, Type type)
        {
            if (obj != null && obj.ToString().StartsWith("0x"))
            {
                obj = ParseHexNullable(obj);
            }
            if (type == typeof(byte)) return ParseByteNullable(obj);
            if (type == typeof(sbyte)) return ParseSByteNullable(obj);
            if (type == typeof(short)) return ParseShortNullable(obj);
            if (type == typeof(ushort)) return ParseUShortNullable(obj);
            if (type == typeof(int)) return ParseIntNullable(obj);
            if (type == typeof(uint)) return ParseUIntNullable(obj);
            if (type == typeof(float)) return ParseFloatNullable(obj);
            if (type == typeof(double)) return ParseDoubleNullable(obj);
            return null;
        }

        public static object ParseValueRoundingWrapping(object obj, Type type)
        {
            if (obj != null && obj.ToString().StartsWith("0x"))
            {
                obj = ParseHexNullable(obj);
            }
            if (type == typeof(byte)) return ParseByteRoundingWrapping(obj);
            if (type == typeof(sbyte)) return ParseSByteRoundingWrapping(obj);
            if (type == typeof(short)) return ParseShortRoundingWrapping(obj);
            if (type == typeof(ushort)) return ParseUShortRoundingWrapping(obj);
            if (type == typeof(int)) return ParseIntRoundingWrapping(obj);
            if (type == typeof(uint)) return ParseUIntRoundingWrapping(obj);
            if (type == typeof(float)) return ParseFloatNullable(obj);
            if (type == typeof(double)) return ParseDoubleNullable(obj);
            return null;
        }

        public static List<string> ParseStringList(string text, bool replaceCharacters = true, bool replaceComma = true)
        {
            if (text == null || text == "")
            {
                return new List<string>();
            }
            if (replaceCharacters)
            {
                text = text
                    .Replace('\n', ' ')
                    .Replace('\r', ' ')
                    .Replace('\t', ' ')
                    .Replace(';', ' ')
                    .Replace('(', ' ')
                    .Replace(')', ' ');
            }
            if (replaceComma)
            {
                text = text.Replace(',', ' ');
            }
            text = text.Trim();
            text = Regex.Replace(text, @"\s+", " ");
            string[] stringArray = text.Split(' ');
            return new List<string>(stringArray);
        }

        public static List<List<string>> ParseLines(string text)
        {
            if (text == null || text == "")
            {
                return new List<List<string>>();
            }
            string[] linesArray = text.Split('\n');
            List<string> linesList = new List<string>(linesArray);
            List<List<string>> output = linesList.ConvertAll(line => ParseStringList(line));
            output = output.FindAll(line => line.Count > 0);
            return output;
        }

        public static List<uint> ParseHexList(string text)
        {
            return ParseStringList(text).ConvertAll(stringValue => ParseHexNullable(stringValue).Value);
        }

        public static List<uint> ParseHexListNullable(string text)
        {
            return ParseStringList(text)
                .ConvertAll(stringValue => ParseHexNullable(stringValue))
                .FindAll(value => value != null)
                .ConvertAll(value => value.Value);
        }

        public static List<int?> ParseIntList(string text)
        {
            return ParseStringList(text).ConvertAll(stringValue => ParseIntNullable(stringValue));
        }

        public static List<double?> ParseDoubleList(string text)
        {
            return ParseStringList(text).ConvertAll(stringValue => ParseDoubleNullable(stringValue));
        }

        public static byte? ParseByteRoundingWrapping(object value)
        {
            if (value == null) return null;
            double? doubleValue = ParseDoubleNullable(value.ToString());
            if (doubleValue.HasValue) return ParseByteRoundingWrapping(doubleValue.Value);
            return null;
        }

        public static sbyte? ParseSByteRoundingWrapping(object value)
        {
            if (value == null) return null;
            double? doubleValue = ParseDoubleNullable(value.ToString());
            if (doubleValue.HasValue) return ParseSByteRoundingWrapping(doubleValue.Value);
            return null;
        }

        public static short? ParseShortRoundingWrapping(object value)
        {
            if (value == null) return null;
            double? doubleValue = ParseDoubleNullable(value.ToString());
            if (doubleValue.HasValue) return ParseShortRoundingWrapping(doubleValue.Value);
            return null;
        }

        public static ushort? ParseUShortRoundingWrapping(object value)
        {
            if (value == null) return null;
            double? doubleValue = ParseDoubleNullable(value.ToString());
            if (doubleValue.HasValue) return ParseUShortRoundingWrapping(doubleValue.Value);
            return null;
        }

        public static int? ParseIntRoundingWrapping(object value)
        {
            if (value == null) return null;
            double? doubleValue = ParseDoubleNullable(value.ToString());
            if (doubleValue.HasValue) return ParseIntRoundingWrapping(doubleValue.Value);
            return null;
        }

        public static uint? ParseUIntRoundingWrapping(object value)
        {
            if (value == null) return null;
            double? doubleValue = ParseDoubleNullable(value.ToString());
            if (doubleValue.HasValue) return ParseUIntRoundingWrapping(doubleValue.Value);
            return null;
        }

        public static byte ParseByteRoundingWrapping(double value)
        {
            return (byte)MoreMath.GetIntegerInRangeWrapped(value, 1.0 + byte.MaxValue - byte.MinValue, false);
        }

        public static sbyte ParseSByteRoundingWrapping(double value)
        {
            return (sbyte)MoreMath.GetIntegerInRangeWrapped(value, 1.0 + sbyte.MaxValue - sbyte.MinValue, true);
        }

        public static short ParseShortRoundingWrapping(double value)
        {
            return (short)MoreMath.GetIntegerInRangeWrapped(value, 1.0 + short.MaxValue - short.MinValue, true);
        }

        public static ushort ParseUShortRoundingWrapping(double value)
        {
            return (ushort)MoreMath.GetIntegerInRangeWrapped(value, 1.0 + ushort.MaxValue - ushort.MinValue, false);
        }

        public static int ParseIntRoundingWrapping(double value)
        {
            return (int)MoreMath.GetIntegerInRangeWrapped(value, 1.0 + int.MaxValue - int.MinValue, true);
        }

        public static uint ParseUIntRoundingWrapping(double value)
        {
            return (uint)MoreMath.GetIntegerInRangeWrapped(value, 1.0 + uint.MaxValue - uint.MinValue, false);
        }



        public static byte? ParseByteRoundingCapping(object value)
        {
            double? doubleValue = ParseDoubleNullable(value.ToString());
            if (doubleValue.HasValue) return ParseByteRoundingCapping(doubleValue.Value);
            return null;
        }

        public static sbyte? ParseSByteRoundingCapping(object value)
        {
            double? doubleValue = ParseDoubleNullable(value.ToString());
            if (doubleValue.HasValue) return ParseSByteRoundingCapping(doubleValue.Value);
            return null;
        }

        public static short? ParseShortRoundingCapping(object value)
        {
            double? doubleValue = ParseDoubleNullable(value.ToString());
            if (doubleValue.HasValue) return ParseShortRoundingCapping(doubleValue.Value);
            return null;
        }

        public static ushort? ParseUShortRoundingCapping(object value)
        {
            double? doubleValue = ParseDoubleNullable(value.ToString());
            if (doubleValue.HasValue) return ParseUShortRoundingCapping(doubleValue.Value);
            return null;
        }

        public static int? ParseIntRoundingCapping(object value)
        {
            double? doubleValue = ParseDoubleNullable(value.ToString());
            if (doubleValue.HasValue) return ParseIntRoundingCapping(doubleValue.Value);
            return null;
        }

        public static uint? ParseUIntRoundingCapping(object value)
        {
            double? doubleValue = ParseDoubleNullable(value.ToString());
            if (doubleValue.HasValue) return ParseUIntRoundingCapping(doubleValue.Value);
            return null;
        }

        public static byte ParseByteRoundingCapping(double value)
        {
            return (byte)MoreMath.GetIntegerInRangeCapped(value, 1.0 + byte.MaxValue - byte.MinValue, false);
        }

        public static sbyte ParseSByteRoundingCapping(double value)
        {
            return (sbyte)MoreMath.GetIntegerInRangeCapped(value, 1.0 + sbyte.MaxValue - sbyte.MinValue, true);
        }

        public static short ParseShortRoundingCapping(double value)
        {
            return (short)MoreMath.GetIntegerInRangeCapped(value, 1.0 + short.MaxValue - short.MinValue, true);
        }

        public static ushort ParseUShortRoundingCapping(double value)
        {
            return (ushort)MoreMath.GetIntegerInRangeCapped(value, 1.0 + ushort.MaxValue - ushort.MinValue, false);
        }

        public static int ParseIntRoundingCapping(double value)
        {
            return (int)MoreMath.GetIntegerInRangeCapped(value, 1.0 + int.MaxValue - int.MinValue, true);
        }

        public static uint ParseUIntRoundingCapping(double value)
        {
            return (uint)MoreMath.GetIntegerInRangeCapped(value, 1.0 + uint.MaxValue - uint.MinValue, false);
        }

        public static bool TryParseVector3(string text, out Vector3 value)
        {
            value = default(Vector3);
            if (text == null) return false;
            string[] split = text.Split(';');
            return (split.Length == 3
                && float.TryParse(split[0].Trim(), out value.X)
                && float.TryParse(split[1].Trim(), out value.Y)
                && float.TryParse(split[2].Trim(), out value.Z));
        }

        public static bool ParseByteString(string byteString, out byte[] result)
        {
            result = null;
            byteString = byteString.Replace(" ", "");
            if (byteString.Length % 2 != 0)
                return false;

            var bytes = new byte[byteString.Length / 2];
            for (int i = 0; i < bytes.Length; i++)
                if (!byte.TryParse(
                    byteString.Substring(i * 2, 2),
                    NumberStyles.HexNumber,
                    CultureInfo.InvariantCulture,
                    out bytes[i]))
                    return false;
            result = bytes;
            return true;
        }
    }
}
