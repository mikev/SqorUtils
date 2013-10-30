using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Sqor.Utils.Json
{
    internal class EnumCache
    {
        public static EnumCache GetInstance(Type type)
        {
            return (EnumCache)typeof(EnumCache<>).MakeGenericType(type).GetProperty("Instance").GetValue(null);
        }

        private Enum nullValue;
        private IReadOnlyDictionary<string, Enum> enumsByKey;
        private IReadOnlyDictionary<Enum, string> keysByEnum;

        public EnumCache(Type type)
        {
            var enumData = type
                .GetFields()
                .Select(x => new { Field = x, Value = (Enum)x.GetValue(null), Attribute = x.GetCustomAttribute<JsonAttribute>() })
                .Select(x => new { Key = x.Attribute != null ? x.Attribute.JsonKey : x.Field.Name, x.Value, x.Attribute })
                .ToArray();

            enumsByKey = enumData.ToDictionary(x => x.Key, x => x.Value);
            keysByEnum = enumData.ToDictionary(x => x.Value, x => x.Key);
            nullValue = enumData.SingleOrDefault(x => x.Attribute.RepresentsNull).Value;
        }

        public Enum NullValue
        {
            get { return nullValue; }
        }

        public IReadOnlyDictionary<string, Enum> EnumsByKey
        {
            get { return enumsByKey; }
        }

        public IReadOnlyDictionary<Enum, string> KeysByEnum
        {
            get { return keysByEnum; }
        }
    }

    internal class EnumCache<T> : EnumCache
    {
        private static EnumCache<T> instance = new EnumCache<T>();

        public static EnumCache<T> Instance
        {
            get { return instance; }
        }

        public EnumCache() : base(typeof(T))
        {
        }
    }
}