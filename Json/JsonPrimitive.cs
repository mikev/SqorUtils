using System;

namespace Sqor.Utils.Json
{
    public class JsonPrimitive : JsonValue, IConvertible
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

        public TypeCode GetTypeCode()
        {
            if (Value is bool)
                return TypeCode.Boolean;
            else if (Value is int)
                return TypeCode.Int32;
            else if (Value is long)
                return TypeCode.Int64;
            else if (Value is float)
                return TypeCode.Single;
            else if (Value is double)
                return TypeCode.Double;
            else if (Value is string)
                return TypeCode.String;
            else
                return TypeCode.Empty;
        }

        public bool ToBoolean(IFormatProvider provider)
        {
            return Convert.ToBoolean(Value, provider);
        }

        public char ToChar(IFormatProvider provider)
        {
            return Convert.ToChar(Value, provider);
        }

        public sbyte ToSByte(IFormatProvider provider)
        {
            return Convert.ToSByte(Value, provider);
        }

        public byte ToByte(IFormatProvider provider)
        {
            return Convert.ToByte(Value, provider);
        }

        public short ToInt16(IFormatProvider provider)
        {
            return Convert.ToInt16(Value, provider);
        }

        public ushort ToUInt16(IFormatProvider provider)
        {
            return Convert.ToUInt16(Value, provider);
        }

        public int ToInt32(IFormatProvider provider)
        {
            return Convert.ToInt32(Value, provider);
        }

        public uint ToUInt32(IFormatProvider provider)
        {
            return Convert.ToUInt32(Value, provider);
        }

        public long ToInt64(IFormatProvider provider)
        {
            return Convert.ToInt64(Value, provider);
        }

        public ulong ToUInt64(IFormatProvider provider)
        {
            return Convert.ToUInt64(Value, provider);
        }

        public float ToSingle(IFormatProvider provider)
        {
            return Convert.ToSingle(Value, provider);
        }

        public double ToDouble(IFormatProvider provider)
        {
            return Convert.ToDouble(Value, provider);
        }

        public decimal ToDecimal(IFormatProvider provider)
        {
            return Convert.ToDecimal(Value, provider);
        }

        public DateTime ToDateTime(IFormatProvider provider)
        {
            return Convert.ToDateTime(Value, provider);
        }

        public string ToString(IFormatProvider provider)
        {
            return Convert.ToString(Value, provider);
        }

        public object ToType(Type conversionType, IFormatProvider provider)
        {
            return Convert.ChangeType(Value, conversionType, provider);
        }
    }
}

