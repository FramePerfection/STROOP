
namespace STROOP.Variables.Utilities
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
    }
}
