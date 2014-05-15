using System;
using System.Reflection;
using System.Linq.Expressions;
using System.Collections.Generic;
using System.Linq;
using Sqor.Utils.Enumerables;
using Sqor.Utils.Strings;

namespace Sqor.Utils.Types
{
    public static class Class<T>
    {
        public static MethodInfo GetMethodInfo(Expression<Action<T>> accessor)
        {
            MethodCallExpression call = (MethodCallExpression)accessor.Body;
            return call.Method;
        }

        public static MethodInfo GetMethodInfo<TResult>(Expression<Func<T, TResult>> accessor)
        {
            MethodCallExpression call = (MethodCallExpression)accessor.Body;
            return call.Method;
        }

        public static MemberInfo GetMemberInfo<TResult>(Expression<Func<T, TResult>> accessor)
        {
            MemberExpression call = (MemberExpression)accessor.Body;
            return call.Member;
        }

        public static PropertyInfo GetPropertyInfo<TResult>(Expression<Func<T, TResult>> accessor)
        {
            var expression = accessor.Body;
            if (expression is UnaryExpression)
                expression = ((UnaryExpression)expression).Operand;
            MemberExpression call = (MemberExpression)expression;
            return (PropertyInfo)call.Member;
        }
    }

    public static class Class
    {
        public static string GetPropertyName(this LambdaExpression expression)
        {
            var current = expression.Body;
            var unary = current as UnaryExpression;
            if (unary != null)
                current = unary.Operand;
            MemberExpression member = (MemberExpression)current;
            return member
                .SelectRecursive(o => o.Expression is MemberExpression ? (MemberExpression)o.Expression : null)
                .Select(o => o.Member.Name)
                .Reverse()
                .Concatenate(".");
        }

        public static PropertyInfo GetPropertyInfo(this LambdaExpression expression)
        {
            var current = expression.Body;
            var unary = current as UnaryExpression;
            if (unary != null)
                current = unary.Operand;
            var call = (MemberExpression)current;
            return (PropertyInfo)call.Member;
        }

        public static Type GetExpressionType(this LambdaExpression expression)
        {
            var current = expression.Body;
            return current.Type;
        }

        public static IEnumerable<MemberInfo> GetPropertyPath(this LambdaExpression expression)
        {
            MemberExpression member = expression.Body as MemberExpression;
            if (member == null) 
                return Enumerable.Empty<MemberInfo>();
            return member
                .SelectRecursive(o => o.Expression is MemberExpression ? (MemberExpression)o.Expression : null)
                .Select(o => o.Member).Reverse();
        }
    }
}

