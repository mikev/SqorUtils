using System;

namespace Sqor.Utils.Json
{
    public class JsonPrimitive : JsonValue
    {
        public object Value { get; set; }

        public JsonPrimitive() : base(JsonNodeType.Object)
        {
            Value = null;
        }

        public JsonPrimitive(string value) : base(JsonNodeType.String)
        {
            Value = value;
        }
        
        public JsonPrimitive(long value) : base(JsonNodeType.Number)
        {
            Value = value;
        }
        
        public JsonPrimitive(double value) : base(JsonNodeType.Number)
        {
            Value = value;
        }
        
        public JsonPrimitive(bool value) : base(JsonNodeType.Boolean)
        {
            Value = value;
        }
        
        public JsonPrimitive(decimal value) : base(JsonNodeType.Number)
        {
            Value = value;
        }
        
        public override string ToString()
        {
            return Value != null ? Value.ToString() : "null";
        }
    }
}

