using System;

namespace Sqor.Utils.Json
{
    public static class JsonExtensions
    {
        public static JsonValue FromJson(this string s)
        {
            if (string.IsNullOrEmpty(s))
                return null;
        
            return new JsonObjectSerializer().Parse(s);
        }
        
        public static T FromJson<T>(this string s)
        {
            return new JsonObjectSerializer().Parse<T>(s);
        }
        
        public static string ToJson(this object o)
        {
            return new JsonObjectSerializer().Serialize(o);
        }
    }
}

