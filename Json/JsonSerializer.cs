using System;
using System.Text;
using System.Linq;
using System.Text.RegularExpressions;
using System.Globalization;

namespace Sqor.Utils.Json
{
    public class JsonSerializer
    {
        public string Serialize(JsonValue value)
        {
            var builder = new StringBuilder();
            Serialize(builder, value);
            return builder.ToString();
        }

        public void Serialize(StringBuilder builder, JsonValue value)
        {
            if (value is JsonPrimitive)
                SerializePrimitive(builder, (JsonPrimitive)value);
            else if (value is JsonObject)
                SerializeObject(builder, (JsonObject)value);
            else if (value is JsonArray)
                SerializeArray(builder, (JsonArray)value);
            else
                throw new InvalidOperationException("Unexpected JsonValue: " + value);
        }

        private void SerializePrimitive(StringBuilder builder, JsonPrimitive value)
        {
            switch (value.Type)
            {
                case JsonType.Number:
                    builder.Append(value.Value);
                    break;
                case JsonType.String:
                    builder.Append("\"" + EscapeString((string)value.Value) + "\"");
                    break;
                case JsonType.Boolean:
                    builder.Append((bool)value.Value ? "true" : "false");
                    break;
                case JsonType.Object:
                    builder.Append("null");
                    break;
                default:
                    throw new InvalidOperationException("Unexpected json value: " + value);
            }
        }

        private string EscapeString(string s)
        {
            s = s.Replace("\\", "\\\\");
            s = s.Replace("\"", "\\\"");
            s = s.Replace("\n", "\\n");
            s = s.Replace("\r", "\\r");
            s = s.Replace("\t", "\\t");
            return s;
        }

        private string UnescapeString(string s)
        {
            var rx = new Regex(@"\\[uU]([0-9A-F]{4})");
            s = rx.Replace(s, match => ((char)Int32.Parse(match.Value.Substring(2), NumberStyles.HexNumber)).ToString());
            s = s.Replace("\\\\", "\\").Replace("\\\"", "\"");
            return s;
        }   

        private void SerializeObject(StringBuilder builder, JsonObject obj)
        {
            builder.Append("{");
            foreach (var item in obj.PropertyNames.Select((x, i) => new { Item = x, Index = i }))
            {
                if (item.Index > 0)
                {
                    builder.Append(",");
                }

                builder.Append("\"");
                builder.Append(item.Item);
                builder.Append("\":");
                var value = obj[item.Item];
                Serialize(builder, value);
            }
            builder.Append("}");
        }

        private void SerializeArray(StringBuilder builder, JsonArray array)
        {
            builder.Append("[");
            foreach (var element in array.Select((x, i) => new { Item = x, Index = i }))
            {
                if (element.Index > 0)
                {
                    builder.Append(",");
                }
                Serialize(builder, element.Item);
            }
            builder.Append("]");
        }

        public JsonValue Deserialize(string s)
        {
            var reader = new JsonReader(s);
            return Deserialize(reader);
        }

        public JsonValue Deserialize(JsonReader reader)
        {
            var token = reader.NextToken();
            switch (token.TokenType)
            {
                case JsonTokenType.Null:
                    return new JsonPrimitive();
                case JsonTokenType.Boolean:
                    return new JsonPrimitive(token.Value == "true");
                case JsonTokenType.Number:
                    return new JsonPrimitive(decimal.Parse(token.Value));
                case JsonTokenType.String:
                    return new JsonPrimitive(UnescapeString(token.Value));
                case JsonTokenType.OpeningBrace:
                    return DeserializeObject(reader);
                case JsonTokenType.OpeningBracket:
                    return DeserializeArray(reader);
                default:
                    throw new InvalidOperationException("Unexpected token type " + token.TokenType + ": " + token.Value);
            }
        }

        private JsonObject DeserializeObject(JsonReader reader)
        {
            var jsonObject = new JsonObject();
            for (JsonToken token = reader.NextToken(); token.TokenType != JsonTokenType.ClosingBrace; token = reader.NextToken())
            {
                if (token.TokenType == JsonTokenType.Comma)
                {
                    token = reader.NextToken();
                }
                var propertyName = token.Value;
                var colon = reader.NextToken();  
                if (colon.TokenType != JsonTokenType.Colon)
                    throw new InvalidOperationException("Expected colon: " + colon);
                var value = Deserialize(reader);
                jsonObject[propertyName] = value;
            }
            return jsonObject;
        }

        private JsonArray DeserializeArray(JsonReader reader)
        {
            var jsonArray = new JsonArray();
            for (var token = reader.PeekToken(); token.TokenType != JsonTokenType.ClosingBracket; token = reader.PeekToken())
            {
                if (token.TokenType == JsonTokenType.Comma)
                {
                    reader.NextToken();
                }
                var value = Deserialize(reader);
                jsonArray.Add(value);
            }
            var closingBracket = reader.NextToken();
            if (closingBracket.TokenType != JsonTokenType.ClosingBracket)
                throw new InvalidOperationException("Expected closing bracket: " + closingBracket);
            return jsonArray;
        }
    }
}

