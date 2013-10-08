using System;
using System.Collections.Generic;
using System.Linq;

namespace Sqor.Utils.Collections
{
    public static class CollectionExtensions
    {
        public static void AddRange<T>(this ICollection<T> collection, IEnumerable<T> items)
        {
            foreach (var item in items)
                collection.Add(item);
        }

        public static void RemoveRange<T>(this ICollection<T> collection, IEnumerable<T> items)
        {
            foreach (var item in items)
                collection.Remove(item);
        }

        public static void RemoveWhere<T>(this ICollection<T> collection, Func<T, bool> deleteWhenTrue)
        {
            collection.RemoveRange(collection.Where(deleteWhenTrue));
        }
    }
}