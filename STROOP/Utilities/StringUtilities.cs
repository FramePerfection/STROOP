using System;
using System.Collections.Generic;
using System.Globalization;
using STROOP.Controls.VariablePanel;
using STROOP.Structs;

namespace STROOP.Utilities
{
    public static class StringUtilities
    {
        public static string Cap(string stringValue, int length)
        {
            if (stringValue == null) return stringValue;
            if (stringValue.Length <= length) return stringValue;
            return stringValue.Substring(0, length);
        }

        public static string ExactLength(string stringValue, int length, bool leftAppend, char appendChar)
        {
            if (stringValue == null) return stringValue;
            if (stringValue.Length < length)
            {
                return leftAppend
                    ? stringValue.PadLeft(length, appendChar)
                    : stringValue.PadRight(length, appendChar);
            }

            if (stringValue.Length > length)
            {
                return leftAppend
                    ? stringValue.Substring(stringValue.Length - length)
                    : stringValue.Substring(0, length);
            }

            return stringValue;
        }

        public static string FormatIntegerWithSign(int num)
        {
            return (num > 0 ? "+" : "") + num;
        }

        public static string Capitalize(string s)
        {
            if (string.IsNullOrEmpty(s)) return s;
            return s.Substring(0, 1).ToUpper() + s.Substring(1);
        }

        public static string Concat<T>(IEnumerable<T> ts, Func<T, string> toString)
        {
            List<string> strings = new List<string>();
            foreach (var t in ts)
                strings.Add(toString(t));
            return string.Concat(strings);
        }

        static System.Text.RegularExpressions.Regex needsJsonStringEscapeRegex = new System.Text.RegularExpressions.Regex("^[-]?(([0-9]+)|(([0-9]+)\\.([0-9]+)))$");

        public static string MakeJsonValue(string input)
        {
            input = input.Trim(' ', '"');
            if (!needsJsonStringEscapeRegex.IsMatch(input))
                return $"\"{input}\"";
            return input;
        }

        public static object GetJsonValue(Type variableWrapperType, string valueString)
        {
            var str = valueString.Trim('"');
            double numberValue = 0;
            if (TypeUtilities.MatchesGenericType(typeof(WatchVariableNumberWrapper<>), variableWrapperType))
            {
                bool set = true;
                if (str.StartsWith("0x"))
                {
                    if (set = long.TryParse(str.Substring(2, str.Length - 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out var hexValue))
                        numberValue = hexValue;
                }
                else
                    set = double.TryParse(str, out numberValue);

                if (set)
                    return numberValue;
            }
            else if (typeof(WatchVariableBooleanWrapper).IsAssignableFrom(variableWrapperType))
                return valueString.ToLower() != "false" && (!int.TryParse(valueString, out var boolNumber) || boolNumber != 0);
            else
                return valueString;

            return 0;
        }
    }
}
