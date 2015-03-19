using System.Reflection;
using Sqor.Utils.TypeInfos;

namespace Sqor.Utils.Objects
{
    public static class ObjectExtensions
    {
        public static object Null()
        {
            return null;
        }

        public static bool SafeEquals<T>(this T o1, T o2)
        {
            bool o1Null = o1 == null;//EqualityComparer<T>.Default.Equals(o1, default(T));
            bool o2Null = o2 == null;//EqualityComparer<T>.Default.Equals(o2, default(T));
            if (o1Null && o2Null)
                return true;
            if (o1Null)
                return false;

            return o1.Equals(o2);
        }         

        public static void TrimWhitespaceOnProperties(this object o)
        {
            foreach (var property in o.GetType().GetRuntimeProperties())
            {
                if (property.PropertyType == typeof(string) && property.CanWrite)
                {
                    var s = (string)property.GetValue(o, null);
                    if (s != null)
                    {
                        s = s.Trim();
                        property.SetValue(o, s, null);
                    }
                }
            }
        }

        public static object GetValue(this object o, string path)
        {
            var current = o;
            foreach (var part in path.Split('.'))
            {
                current = current.GetType().GetTypeInfo().GetDeclaredProperty(part, false).GetValue(current);
                if (current == null)
                    break;
            }
            return current;
        }
    }
}
