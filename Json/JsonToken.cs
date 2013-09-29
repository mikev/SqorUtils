using System;

namespace Sqor.Utils.Json
{
    public class JsonToken
    {
        public JsonTokenType TokenType { get; set; }
        public string Value { get; set; }

        public JsonToken(JsonTokenType tokenType, string value = null)
        {
            TokenType = tokenType;
            Value = value;
        }

        public override string ToString()
        {
            return string.Format("[JsonToken: TokenType={0}, Value={1}]", TokenType, Value);
        }
    }
}

