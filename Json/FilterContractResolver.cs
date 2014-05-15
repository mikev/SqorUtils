using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using Newtonsoft.Json.Serialization;
using Sqor.Utils.Types;

namespace Sqor.Utils.Json
{
    public class FilterContractResolver : DefaultContractResolver
    {
        private HashSet<PropertyInfo> excludedProperties = new HashSet<PropertyInfo>();
        private Dictionary<MemberInfo, DelegateValueProvider> mappings = new Dictionary<MemberInfo, DelegateValueProvider>();

        public OnContext<T> On<T>()
        {
            return new OnContext<T>(this);
        }

        protected override List<MemberInfo> GetSerializableMembers(Type objectType)
        {
 	        var result = base.GetSerializableMembers(objectType);
            result.RemoveAll(x => !(x is PropertyInfo) || excludedProperties.Contains((PropertyInfo)x));
            return result;
        }

        protected override JsonProperty CreateProperty(MemberInfo member, Newtonsoft.Json.MemberSerialization memberSerialization)
        {
            var result = base.CreateProperty(member, memberSerialization);
            DelegateValueProvider valueProvider;
            if (mappings.TryGetValue(member, out valueProvider))
            {
                result.PropertyType = valueProvider.ConvertedType;
            }
            return result;
        }

        protected override IValueProvider CreateMemberValueProvider(MemberInfo member)
        {
            DelegateValueProvider provider;
 	        if (mappings.TryGetValue(member, out provider))
 	        {
 	            return provider;
 	        }
            return base.CreateMemberValueProvider(member);
        }

        private void ExcludeProperty(PropertyInfo property)
        {
            excludedProperties.Add(property);
        }

        private void MapProperty<T, TValue>(PropertyInfo property, Expression<Func<T, TValue>> get, Action<T, TValue> set)
        {
            mappings[property] = new DelegateValueProvider<T, TValue>(property, get, set);
        }

        private abstract class DelegateValueProvider : IValueProvider
        {
            public abstract object GetValue(object target);
            public abstract void SetValue(object target, object value);
            public abstract PropertyInfo Property { get; }
            public abstract Type ConvertedType { get; }
        }

        private class DelegateValueProvider<T, TValue> : DelegateValueProvider
        {
            private PropertyInfo property;
            private Expression<Func<T, TValue>> get;
            private Func<T, TValue> getter;
            private Action<T, TValue> set;

            public DelegateValueProvider(PropertyInfo property, Expression<Func<T, TValue>> get, Action<T, TValue> set)
            {
                this.property = property;
                this.get = get;
                this.set = set;
                getter = get.Compile();
            }

            public override PropertyInfo Property
            {
                get { return property; }
            }

            public override Type ConvertedType
            {
                get { return get.GetExpressionType(); }
            }

            public override object GetValue(object target)
            {
                return getter((T)target);
            }

            public override void SetValue(object target, object value)
            {
                set((T)target, (TValue)value);
            }
        }

        public class OnContext<T>
        {
            private FilterContractResolver resolver;

            public OnContext(FilterContractResolver resolver)
            {
                this.resolver = resolver;
            }

            public OnContext<T> Map<TValue>(Expression<Func<T, object>> property, Expression<Func<T, TValue>> get, Action<T, TValue> set)
            {
                resolver.MapProperty(property.GetPropertyInfo(), get, set);
                return this;
            }

            public OnContext<T> Exclude(Expression<Func<T, object>> property)
            {
                resolver.ExcludeProperty(property.GetPropertyInfo());
                return this;
            }
        }
    }
}