using System;
using System.Collections.Generic;
using System.Linq;

namespace Sqor.Utils.Functional
{
    public static class FunctionalExtensions
    {
        public static T If<T>(this T o, Func<T, bool> predicate)
        {
            return predicate(o) ? o : default(T);
        }

        public static void IfAny<T>(this IEnumerable<T> enumerable, Action<IEnumerable<T>> action)
        {
            if (enumerable.Any())
                action(enumerable);
        }

        public static T IfNull<T>(this T o, Func<T> func)
        {
            bool oIsNull = EqualityComparer<T>.Default.Equals(o, default(T));
            if (oIsNull)
                return func();
            else
                return o;
        }

        public static TResult IfNotNull<TSource, TResult>(this TSource o, Func<TSource, TResult> func) 
        {
            return o.IfNotNull(func, () => default(TResult));
        }

        public static IEnumerable<TSource> NullIfEmpty<TSource>(this IEnumerable<TSource> source) 
        {
            return source.Any() ? source : null;
        }

        public static TResult IfNotNull<TSource, TResult>(this TSource o, Func<TSource, TResult> func, Func<TResult> funcElse) 
        {
            bool oIsNull = EqualityComparer<TSource>.Default.Equals(o, default(TSource));
            if (!oIsNull)
                return func(o);
            else
                return funcElse();
        }

        public static TResult IfNotNull<TSource, TResult>(this TSource o, Func<TSource, TResult> func, TResult elseValue) 
        {
            return o.IfNotNull(func, () => elseValue);
        }

        public static void IfNotNull<T>(this T o, Action<T> func) 
            where T : class
        {
            if (o != null)
                func(o);
        }

        public static TResult If<T, TResult>(this T o, Func<T, bool> condition, Func<T, TResult> ifTrue, Func<T, TResult> ifFalse)
        {
            if (condition(o))
                return ifTrue(o);
            else
                return ifFalse(o);
        }

        public static TResult If<T, TResult>(this T o, T equalTo, Func<T, TResult> ifTrue, Func<T, TResult> ifFalse)
        {
            return o.If(p => p.Equals(equalTo), ifTrue, ifFalse);
        }

        public static TResult If<T, TResult>(this T o, T equalTo, TResult ifTrue, Func<T, TResult> ifFalse)
        {
            return o.If(p => p.Equals(equalTo), p => ifTrue, ifFalse);
        }

        public static TResult If<T, TResult>(this T o, T equalTo, Func<T, TResult> ifTrue, TResult ifFalse)
        {
            return o.If(p => p.Equals(equalTo), ifTrue, p => ifFalse);
        }
    }
}