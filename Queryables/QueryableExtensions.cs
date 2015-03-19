using System;
using System.Linq;
using System.Linq.Expressions;

namespace Sqor.Utils.Queryables
{
    public static class QueryableExtensions
    {
        /// <summary>
        /// Same as .OrderBy / .OrderByDescending except it allows you to specify ascending/descending via a parameter
        /// rather than choosing a different method.  This can make some logic more composable.
        /// </summary>
        public static IOrderedQueryable<T> Sort<T, TValue>(this IQueryable<T> source, Expression<Func<T, TValue>> column, bool ascending = true)
        {
            if (ascending)
                return source.OrderBy(column);
            else
                return source.OrderByDescending(column);
        }

        /// <summary>
        /// Same as .OrderBy / .OrderByDescending except it allows you to specify ascending/descending via a parameter
        /// rather than choosing a different method.  This can make some logic more composable.
        /// </summary>
        public static IOrderedQueryable<T> ThenSort<T, TValue>(this IOrderedQueryable<T> source, Expression<Func<T, TValue>> column, bool ascending = true)
        {
            if (ascending)
                return source.ThenBy(column);
            else
                return source.ThenByDescending(column);
        }
    }
}