using System;

namespace Sqor.Utils.Json
{
    public class JsonConverterAttribute : Attribute
    {
        public Type Converter { get; private set; }

        public JsonConverterAttribute(Type converter)
        {
            Converter = converter;
        }
    }
}