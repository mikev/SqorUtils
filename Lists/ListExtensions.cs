using System;
using System.Collections.Generic;
using System.Linq;

namespace Sqor.Utils.Lists
{
    public static class ListExtensions
    {
        public static void Enqueue<T>(this IList<T> list, T item)
        {
            list.Add(item);
        }
        
        public static T Dequeue<T>(this IList<T> list)
        {
            var item = list.First();
            list.RemoveAt(0);
            return item;
        }
        
        public static void JumpQueue<T>(this IList<T> list, T item)
        {
            list.Insert(0, item);
        }
    }
}

