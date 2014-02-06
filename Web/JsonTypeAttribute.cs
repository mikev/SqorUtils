using System;

namespace Sqor.Utils.Web
{
    public class JsonTypeAttribute : Attribute
    {
        public Type Type { get; private set; }

        public JsonTypeAttribute(Type type)
        {
            Type = type;
        }
    }
}
