using System;
using System.Reflection;

namespace Sqor.Utils.Json
{
    [AttributeUsage(AttributeTargets.Property)]
    public class KeyAttribute : Attribute
    {
        private string key;
    
        public KeyAttribute(string key)
        {
            this.key = key;
        }
        
        public static string GetKey(PropertyInfo property)
        {
            var attribute = (KeyAttribute)Attribute.GetCustomAttribute(property, typeof(KeyAttribute));
            return attribute == null ? property.Name : attribute.key;
        }
    }
}

