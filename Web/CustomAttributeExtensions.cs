#if FULL
using System;
using System.Linq;
using System.Web.Mvc;

namespace Sqor.Utils.Web
{
    public static class CustomAttributeExtensions
    {
        public static bool IsDefined<T>(this ActionDescriptor actionDescriptor)
        {
            return actionDescriptor.IsDefined(typeof(T), true);
        }

        public static T GetCustomAttribute<T>(this ActionDescriptor actionDescriptor)
        {
            return actionDescriptor.GetCustomAttributes(typeof(T), true).Cast<T>().SingleOrDefault();
        }

        public static Attribute GetCustomAttribute(this ActionDescriptor actionDescriptor, Type attributeType)
        {
            return actionDescriptor.GetCustomAttributes(attributeType, true).Cast<Attribute>().SingleOrDefault();
        }

        public static bool IsDefined<T>(this ParameterDescriptor parameterDescriptor)
        {
            return parameterDescriptor.IsDefined(typeof(T), true);
        }

        public static Attribute GetCustomAttribute(this ParameterDescriptor parameterDescriptor, Type attributeType)
        {
            return parameterDescriptor.GetCustomAttributes(attributeType, true).Cast<Attribute>().SingleOrDefault();
        }

        public static T GetCustomAttribute<T>(this ParameterDescriptor parameterDescriptor)
        {
            return parameterDescriptor.GetCustomAttributes(typeof(T), true).Cast<T>().SingleOrDefault();
        }
    }
}
#endif