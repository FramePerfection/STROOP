using System.Collections.Generic;
using System.Text;

namespace STROOP.Tabs.BruteforceTab
{
    public class JsonNodeObject : JsonNode
    {
        public Dictionary<string, JsonNode> values = new Dictionary<string, JsonNode>();
        public override object valueObject => values;

        public bool TryGetValue<T>(string key, out T result) where T : JsonNode
        {
            result = null;
            if (values.TryGetValue(key, out var genericNode) && (genericNode is T concreteResult))
            {
                result = concreteResult;
                return true;
            }

            return false;
        }
    }

    public class JsonNodeString : JsonNode
    {
        public string value;
        public override object valueObject => value;
    }

    public class JsonNodeNumber : JsonNode
    {
        public double? valueDouble;
        public long? valueLong;
        public override object valueObject => valueLong ?? valueDouble;
    }

    public class JsonNodeArray : JsonNode
    {
        public JsonNode[] values;
        public override object valueObject => values;
    }

    public class JsonNodeBoolean : JsonNode
    {
        public bool value;
        public override object valueObject => value;
    }

    public class JsonNodeNull : JsonNode
    {
        public override object valueObject => null;
    }

    public abstract class JsonNode
    {
        public abstract object valueObject { get; }

        public string sourceString;

        protected static bool IsDigit(char c)
        {
            switch (c)
            {
                case '0':
                case '1':
                case '2':
                case '3':
                case '4':
                case '5':
                case '6':
                case '7':
                case '8':
                case '9':
                    return true;
                default:
                    return false;
            }
        }

        static bool IsWhitespace(char c)
        {
            switch (c)
            {
                case ' ':
                case '\t':
                case '\n':
                case '\r':
                    return true;
                default:
                    return false;
            }
        }

        public static JsonNodeObject ParseJsonObject(string input)
        {
            var cursor = 1;
            return ParseJsonObject(input, ref cursor);
        }

        static JsonNode ParseJsonValue(string input, ref int cursor)
        {
            int start = cursor;
            JsonNode result = null;
            while (cursor < input.Length)
            {
                char c = input[cursor++];
                if (IsWhitespace(c))
                {
                    /*Do nothing*/
                }
                else if (c == '-' || IsDigit(c))
                {
                    for (; cursor < input.Length; ++cursor)
                    {
                        c = input[cursor];
                        if (!IsDigit(c) && c != '.' && c != 'E' && c != 'e' && c != '+' && c != '-')
                        {
                            var numberString = input.Substring(start, cursor - start);
                            if (long.TryParse(numberString, out var valueLong))
                                result = new JsonNodeNumber { valueLong = valueLong, valueDouble = valueLong };
                            else if (double.TryParse(numberString, out var valueDouble))
                                result = new JsonNodeNumber { valueDouble = valueDouble };
                            else
                                throw new System.Exception($"Invalid number format at cursor position {start}");
                            goto ValidNumber;
                        }
                    }

                    throw new System.Exception("Expected '}' at end of input");
                ValidNumber:;
                }
                else if (c == '"')
                {
                    result = new JsonNodeString { value = ParseJsonString(input, ref cursor) };
                }
                else if (c == '{')
                {
                    result = ParseJsonObject(input, ref cursor);
                }
                else if (c == '[')
                {
                    result = ParseJsonArray(input, ref cursor);
                }
                else
                    throw new System.Exception($"Unexpected token '{c}' at cursor position {cursor}");

                // skip over trailing whitespace
                if (result != null)
                    for (; cursor < input.Length; cursor++)
                    {
                        c = input[cursor];
                        if (!IsWhitespace(c))
                        {
                            result.sourceString = input.Substring(start, cursor - start);
                            return result;
                        }
                    }
            }

            throw new System.Exception($"Unexpected end of input while parsing value:{System.Environment.NewLine}{input.Substring(start)}");
        }

        static JsonNodeObject ParseJsonObject(string input, ref int cursor)
        {
            var c = input[cursor - 1];
            if (c != '{')
                throw new System.Exception($"Unexpected token {c} at cursor position {cursor}");

            var obj = new JsonNodeObject();
            string identifier = null;

            while (cursor < input.Length)
            {
                c = input[cursor++];
                if (IsWhitespace(c))
                {
                    /* Do nothing */
                }
                else if (c == '"')
                {
                    if (identifier != null)
                        throw new System.Exception($"Invalid token '\"'' at cursor position {cursor}");
                    identifier = ParseJsonString(input, ref cursor);
                }
                else if (c == ':')
                {
                    while (IsWhitespace(input[++cursor]))
                        if (cursor == input.Length)
                            throw new System.Exception("Unexpected end of input");

                    obj.values[identifier] = ParseJsonValue(input, ref cursor);
                    identifier = null;
                    c = input[cursor++];
                    switch (c)
                    {
                        case '}':
                            return obj;
                        case ',':
                            continue;
                        default:
                            throw new System.Exception($"Unexpected token '{c}' at cursor position {cursor}");
                    }
                }
                else if (c == '}')
                    return obj;
                else
                    throw new System.Exception($"Unexpected token {c} at cursor position {cursor}");
            }

            throw new System.Exception("Expected '}' at end of input");
        }

        static JsonNodeArray ParseJsonArray(string input, ref int cursor)
        {
            int start = cursor;

            var c = input[cursor - 1];
            if (c != '[')
                throw new System.Exception($"Unexpected token {c} at cursor position {cursor}");

            List<JsonNode> nodes = new List<JsonNode>();
            while (cursor < input.Length)
            {
                c = input[cursor++];
                if (c == ']')
                    return new JsonNodeArray { values = nodes.ToArray() };
                else if (IsWhitespace(c))
                {
                    /* do nothing */
                }
                else if (c == ']')
                    return new JsonNodeArray();
                else
                {
                    cursor--;
                    nodes.Add(ParseJsonValue(input, ref cursor));
                    c = input[cursor++];
                    if (c == ']')
                        return new JsonNodeArray { values = nodes.ToArray() };
                    else if (c == ',')
                        continue;
                }
            }

            throw new System.Exception($"Unexpected end of array starting at {start}");
        }

        static string ParseJsonString(string input, ref int cursor)
        {
            int start = cursor;

            var c = input[cursor - 1];
            if (c != '"')
                throw new System.Exception($"Unexpected token {c} at cursor position {cursor}");

            var builder = new StringBuilder();
            while (cursor < input.Length)
            {
                c = input[cursor++];
                switch (c)
                {
                    case '"':
                        return builder.ToString();
                    case '\\':
                        c = input[cursor++];
                        switch (c)
                        {
                            case '"': builder.Append('"'); break;
                            case '\\': builder.Append('\\'); break;
                            case '/': builder.Append('/'); break;
                            case 'b': builder.Append('\b'); break;
                            case 'f': builder.Append('\f'); break;
                            case 'n': builder.Append('\n'); break;
                            case 'r': builder.Append('\r'); break;
                            case 't': builder.Append('\t'); break;
                            case 'u':
                                var digitsString = input.Substring(cursor, 4);
                                if (int.TryParse(digitsString, out var digits))
                                    builder.Append(char.ConvertFromUtf32(digits));
                                else
                                    throw new System.Exception($"Invalid unicode value {digitsString} at cursor position {cursor}");
                                cursor += 4;
                                break;
                        }

                        break;
                    default:
                        builder.Append(c);
                        break;
                }
            }

            throw new System.Exception($"Unexpected end of string starting at {start}");
        }
    }
}
