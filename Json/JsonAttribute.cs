using System;
using System.Reflection;

namespace Sqor.Utils.Json
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Parameter | AttributeTargets.Field)]
    public class JsonAttribute : Attribute
    {
        public bool Serialize { get; private set; }
        public string JsonKey { get; private set; }
        public bool CatchAll { get; private set; }
        public bool IsDenormalized { get; set; }
        public string Connector { get; set; }

        public JsonAttribute(string jsonKey)
        {
            JsonKey = jsonKey;
            Serialize = true;
        }

        public JsonAttribute(string jsonKey, bool catchAll)
        {
            JsonKey = jsonKey;
            Serialize = true;
            CatchAll = catchAll;
        }

        public JsonAttribute(bool serialize)
        {
            Serialize = serialize;
        }

        public static bool IsSerialized(PropertyInfo property)
        {
            var attribute = (JsonAttribute)GetCustomAttribute(property, typeof(JsonAttribute));
            return attribute == null || attribute.Serialize;            
        }
        
        public static string GetKey(PropertyInfo property)
        {
            var attribute = (JsonAttribute)GetCustomAttribute(property, typeof(JsonAttribute));
            return attribute == null || attribute.JsonKey == null ? property.Name : attribute.JsonKey;
        }
    }
}