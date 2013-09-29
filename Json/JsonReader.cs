using System;
using System.Text;
using System.Globalization;

namespace Sqor.Utils.Json
{
    public class JsonReader
    {
        private string source;
        private int position;

        public JsonReader(string source)
        {
            this.source = source;
        }

        public string Source
        {
            get { return source; }
        }

        private JsonToken peekedToken;
        private int peekedTokenFinalPosition;

        public JsonToken PeekToken()
        {
            if (peekedToken != null)
                return peekedToken;

            peekedTokenFinalPosition = position;
            return peekedToken = NextToken(ref peekedTokenFinalPosition);
        }

        public JsonToken NextToken()
        {
            return NextToken(ref position);
        }

        private JsonToken NextToken(ref int position)
        {
            if (peekedToken != null)
            {
                var cachedValue = peekedToken;
                peekedToken = null;
                position = peekedTokenFinalPosition;
                return cachedValue;
            }
            var result = new StringBuilder();

            while (position < source.Length && char.IsWhiteSpace(source[position]))
                position++;

            if (position >= source.Length)
            {
                return null;
            }

            switch (source[position])
            {
                case '"':
                    // Scan for closing quote, ignoring escaped quotes
                    bool isCharacterEscaped = false;
                    while (position + 1 < source.Length)
                    {
                        position++;
                        if (isCharacterEscaped)
                        {
                            switch (source[position])
                            {
                                case 'b': 
                                    result.Append('\b');
                                    break;
                                case 'f':
                                    result.Append('\f');
                                    break;
                                case 'n':
                                    result.Append('\n');
                                    break;
                                case 'r':
                                    result.Append('\r');
                                    break;
                                case 't':
                                    result.Append('\t');
                                    break;
                                case '/':
                                    result.Append('/');
                                    break;
                                case '\"':
                                    result.Append('\"');
                                    break;
                                case '\\':
                                    result.Append('\\');
                                    break;
                                case 'u':
                                    position++;
                                    var code = source.Substring(position, 4);
                                    position += 3;
                                    result.Append((char)Int32.Parse(code, NumberStyles.HexNumber));
                                    break;
                                default:
                                    throw new InvalidOperationException("Illegal escape character: " + source[position]);
                            }
                            isCharacterEscaped = false;
                        }
                        else 
                        {
                            if (source[position] == '\\')
                            {
                                isCharacterEscaped = true;
                            }
                            else if (source[position] == '"')
                            {
                                position++;
                                return new JsonToken(JsonTokenType.String, result.ToString());
                            }
                            else
                            {
                                result.Append(source[position]);
                            }
                        }
                    }
                    throw new InvalidOperationException("Invalid JSON string: " + source);
                case '{':
                    position++;
                    return new JsonToken(JsonTokenType.OpeningBrace);
                case '}':
                    position++;
                    return new JsonToken(JsonTokenType.ClosingBrace);
                case '[':
                    position++;
                    return new JsonToken(JsonTokenType.OpeningBracket);
                case ']':
                    position++;
                    return new JsonToken(JsonTokenType.ClosingBracket);
                case ':':
                    position++;
                    return new JsonToken(JsonTokenType.Colon);
                case ',':
                    position++;
                    return new JsonToken(JsonTokenType.Comma);
                default:
                    var remaining = source.Substring(position);
                    if (remaining.StartsWith("null"))
                    {
                        position += "null".Length;
                        return new JsonToken(JsonTokenType.Null, "null");
                    }
                    if (remaining.StartsWith("true"))
                    {
                        position += "true".Length;
                        return new JsonToken(JsonTokenType.Boolean, "true");
                    }
                    else if (remaining.StartsWith("false"))
                    {
                        position += "false".Length;
                        return new JsonToken(JsonTokenType.Boolean, "false");
                    }
                    else if (char.IsDigit(source[position]) || source[position] == '+' || source[position] == '-' || source[position] == '.')
                    {
                        if (source[position] == '+' || source[position] == '-')
                        {
                            result.Append(source[position]);
                            position++;
                        }
                        while (position < source.Length && (char.IsDigit(source[position]) ||  (source[position] == '.' && position + 1 < source.Length && char.IsDigit(source[position + 1]))))
                        {
                            result.Append(source[position]);
                            position++;
                        }
                        return new JsonToken(JsonTokenType.Number, result.ToString());
                    }
                    else
                    {
                        throw new InvalidOperationException("Invalid JSON: " + source);
                    }
            }
        }
    }
}

