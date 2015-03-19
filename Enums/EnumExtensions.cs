using System;
using System.Collections.Generic;
using System.Linq;

namespace Sqor.Utils.Portable.Enums
{
    public static class EnumExtensions
    {
        public static T[] ToFlagsArray<T>(this T enumValue) where T : struct
        {
            var list = new List<Enum>();
            var allValues = Enum.GetValues(typeof(T)).Cast<Enum>().ToArray();
            var enumValueAsEnum = (Enum)(object)enumValue;
            foreach (var value in allValues)
            {
                var intValue = (int)Convert.ChangeType(value, typeof(int));
                if (enumValueAsEnum.HasFlag(value) && intValue != 0 && (intValue & (intValue - 1)) == 0)
                {
                    list.Add(value);
                }
            }
            return list.Cast<T>().ToArray();
        }
    }
}