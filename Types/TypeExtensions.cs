using System;
using System.Reflection;

namespace Sqor.Utils.Types
{
    public static class TypeExtensions
    {
        public static object GetDefaultValue(this Type type)
        {
            if (type == typeof(bool))
                return false;
            else if (type == typeof(byte))
                return (byte)0;
            else if (type == typeof(short))
                return (short)0;
            else if (type == typeof(int))
                return 0;
            else if (type == typeof(long))
                return 0L;
            else if (type == typeof(float))
                return (float)0;
            else if (type == typeof(double))
                return (double)0;
            else if (type.GetTypeInfo().IsEnum)
                return Enum.ToObject(type, GetDefaultValue(Enum.GetUnderlyingType(type)));
            else if (type.GetTypeInfo().IsValueType)
                return Activator.CreateInstance(type);
            else
                return null;
        }    
    }
}

