using System;

namespace Sqor.Utils.Json
{
    public class JsonPrimitive : JsonValue
    {
        public object Value { get; set; }

        public JsonPrimitive() : base(JsonType.Object)
        {
            Value = null;
        }

        public JsonPrimitive(string value) : base(JsonType.String)
        {
            Value = value;
        }
        
        public JsonPrimitive(long value) : base(JsonType.Number)
        {
            Value = value;
        }
        
        public JsonPrimitive(double value) : base(JsonType.Number)
        {
            Value = value;
        }
        
        public JsonPrimitive(bool value) : base(JsonType.Boolean)
        {
            Value = value;
        }
        
        public JsonPrimitive(decimal value) : base(JsonType.Number)
        {
            Value = value;
        }
        
        public override string ToString()
        {
            return Value != null ? Value.ToString() : "null";
        }
    }
}

