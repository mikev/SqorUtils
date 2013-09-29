using System;
using System.IO;
using System.Collections.Generic;

namespace Sqor.Utils.Json
{
    public class JsonValue : IEnumerable<JsonValue>
    {
        public JsonType Type { get; private set; }

        public JsonValue(JsonType type)
        {
            Type = type;
        }

        public static JsonValue Parse(string s)
        {
            var serializer = new JsonSerializer();
            return serializer.Deserialize(s);
        }

        public void Save(TextWriter writer)
        {
            var serializer = new JsonSerializer();
            var s = serializer.Serialize(this);
            writer.Write(s);
        }
        
        public override string ToString()
        {
            var writer = new StringWriter();
            Save(writer);
            return writer.ToString();
        }

        public virtual JsonValue this[int index] 
        {
            get { throw new InvalidOperationException("Cannot apply indexing to a " + GetType().FullName); }
            set { throw new InvalidOperationException("Cannot apply indexing to a " + GetType().FullName); }
        }

        public virtual JsonValue this[string property] 
        {
            get { throw new InvalidOperationException("Cannot apply indexing to a " + GetType().FullName); }
            set { throw new InvalidOperationException("Cannot apply indexing to a " + GetType().FullName); }
        }

        public virtual IEnumerator<JsonValue> GetEnumerator()
        {
            throw new InvalidOperationException("Cannot iterate over a " + GetType().FullName);
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public static implicit operator string(JsonValue value)
        {
            var primitive = (JsonPrimitive)value;
            if (primitive == null || primitive.Value == null)
                return null;
            else
                return (string)Convert.ChangeType(((JsonPrimitive)value).Value, typeof(string));
        }

        public static implicit operator bool(JsonValue value)
        {
            bool result;
            if (value == null)
                return false;
            var val = ((JsonPrimitive)value).Value;
            if (val is bool)
                result = (bool)val;
            else if (val is decimal)
                result = (decimal)val != 0;
            else 
                result = false;
            return result;
        }

        public static implicit operator bool?(JsonValue value)
        {
            var primitive = (JsonPrimitive)value;
            if (primitive.Value == null)
                return null;
            else
                return (bool)Convert.ChangeType(((JsonPrimitive)value).Value, typeof(bool));
        }

        public static implicit operator int(JsonValue value)
        {
            return (int)Convert.ChangeType(((JsonPrimitive)value).Value, typeof(int));
        }

        public static implicit operator int?(JsonValue value)
        {
            var primitive = (JsonPrimitive)value;
            if (primitive == null || primitive.Value == null)
                return null;
            else
                return (int)Convert.ChangeType(((JsonPrimitive)value).Value, typeof(int));
        }

        public static implicit operator long(JsonValue value)
        {
            return (long)Convert.ChangeType(((JsonPrimitive)value).Value, typeof(long));
        }
        
        public static implicit operator long?(JsonValue value)
        {
            var primitive = (JsonPrimitive)value;
            if (primitive.Value == null)
                return null;
            else
                return (long)Convert.ChangeType(((JsonPrimitive)value).Value, typeof(long));
        }
        
        public static implicit operator short(JsonValue value)
        {
            return (short)Convert.ChangeType(((JsonPrimitive)value).Value, typeof(short));
        }
        
        public static implicit operator short?(JsonValue value)
        {
            var primitive = (JsonPrimitive)value;
            if (primitive.Value == null)
                return null;
            else
                return (short)Convert.ChangeType(((JsonPrimitive)value).Value, typeof(short));
        }
        
        public static implicit operator byte(JsonValue value)
        {
            return (byte)Convert.ChangeType(((JsonPrimitive)value).Value, typeof(byte));
        }

        public static implicit operator byte?(JsonValue value)
        {
            var primitive = (JsonPrimitive)value;
            if (primitive.Value == null)
                return null;
            else
                return (byte)Convert.ChangeType(((JsonPrimitive)value).Value, typeof(byte));
        }

        public static implicit operator float(JsonValue value)
        {
            return (float)Convert.ChangeType(((JsonPrimitive)value).Value, typeof(float));
        }
        
        public static implicit operator float?(JsonValue value)
        {
            var primitive = (JsonPrimitive)value;
            if (primitive.Value == null)
                return null;
            else
                return (float)Convert.ChangeType(((JsonPrimitive)value).Value, typeof(float));
        }
        
        public static implicit operator double(JsonValue value)
        {
            return (double)Convert.ChangeType(((JsonPrimitive)value).Value, typeof(double));
        }
        
        public static implicit operator double?(JsonValue value)
        {
            var primitive = (JsonPrimitive)value;
            if (primitive.Value == null)
                return null;
            else
                return (double)Convert.ChangeType(((JsonPrimitive)value).Value, typeof(double));
        }
        
        public static implicit operator decimal(JsonValue value)
        {
            return (decimal)Convert.ChangeType(((JsonPrimitive)value).Value, typeof(decimal));
        }
        
        public static implicit operator decimal?(JsonValue value)
        {
            var primitive = (JsonPrimitive)value;
            if (primitive.Value == null)
                return null;
            else
                return (decimal)Convert.ChangeType(((JsonPrimitive)value).Value, typeof(decimal));
        }
        
        public static implicit operator DateTime(JsonValue value)
        {
            DateTime? result;
            result = JsonObjectSerializer.ParseDate((string)((JsonPrimitive)value).Value);
            if (result == null)
                throw new InvalidOperationException("Cannot convert \"" + value + "\" into a date (seems to represent null)");
            return result.Value;
        }
        
        public static implicit operator DateTime?(JsonValue value)
        {
            var primitive = (JsonPrimitive)value;
            if (primitive == null || primitive.Value == null || (primitive.Value is string && ((string)primitive.Value == "")))
                return null;
            else
                return JsonObjectSerializer.ParseDate((string)primitive.Value);
        }
        
        public static implicit operator JsonValue(string value)
        {
            return new JsonPrimitive(value);
        }
        
        public static implicit operator JsonValue(bool value)
        {
            return new JsonPrimitive(value);
        }
        
        public static implicit operator JsonValue(int value)
        {
            return new JsonPrimitive(value);
        }
        
        public static implicit operator JsonValue(long value)
        {
            return new JsonPrimitive(value);
        }
        
        public static implicit operator JsonValue(short value)
        {
            return new JsonPrimitive(value);
        }
        
        public static implicit operator JsonValue(byte value)
        {
            return new JsonPrimitive(value);
        }

        public static implicit operator JsonValue(float value)
        {
            return new JsonPrimitive(value);
        }

        public static implicit operator JsonValue(double value)
        {
            return new JsonPrimitive(value);
        }

        public static implicit operator JsonValue(decimal value)
        {
            return new JsonPrimitive(value);
        }
        
        public static implicit operator JsonValue(DateTime value)
        {
            return new JsonPrimitive(value.ToString(JsonObjectSerializer.dateFormat));
        }
        
        public static bool operator ==(JsonValue o1, JsonValue o2)
        {
            return CheckEquals(o1, o2);
        }
        
        public static bool operator !=(JsonValue o1, JsonValue o2)
        {
            return !(o1 == o2);
        }
        
        private static bool CheckEquals(object o1, object o2)
        {
            var p1 = o1 as JsonPrimitive;
            var p2 = o2 as JsonPrimitive;
            object v1 = o1;
            object v2 = o2;
            if (!object.ReferenceEquals(p1, null))
            {
                v1 = !object.ReferenceEquals(p1, null) ? p1.Value : null;
                if (v1 == null && o1 != null && !(o1 is JsonValue))
                    v1 = o1;
            }
            if (!object.ReferenceEquals(p2, null))
            {
                v2 = !object.ReferenceEquals(p2, null) ? p2.Value : null;
                if (v2 == null && o2 != null && !(o2 is JsonValue))
                    v2 = o2;
            }
            if (object.ReferenceEquals(v1, null) && object.ReferenceEquals(v2, null))
            {
                return true;
            }
            else if (object.ReferenceEquals(v1, null) || object.ReferenceEquals(v2, null))
            {
                return false;
            }
            else
            {
                return v1.Equals(v2);
            }
        }
        
        public override bool Equals(object obj)
        {
            if (obj is JsonValue)
                return this == (JsonValue)obj;
            else
                return false;
        }
        
        public override int GetHashCode()
        {
            if (this is JsonPrimitive)
                return ((JsonPrimitive)this).Value != null ? ((JsonPrimitive)this).Value.GetHashCode() : 0;
            else
                return base.GetHashCode();
        }
    }
}

