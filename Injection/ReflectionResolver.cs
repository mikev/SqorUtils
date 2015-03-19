using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Sqor.Utils.Injection
{
    public class ReflectionResolver : IResolver
    {
        public event Action<object> Activated;

        private static Dictionary<Type, bool> ignoredTypes = new Dictionary<Type, bool>();
        private static Dictionary<PropertyInfo, bool> ignoredProperties = new Dictionary<PropertyInfo, bool>();
        private static object ignoredTypesLock = new object();
//        private static bool autoInjectProperties;

        public static void IgnoreType(Type type)
        {
            lock (ignoredTypesLock)
            {
                ignoredTypes[type] = true;
            }
        }

        public static void IgnoreAncestry(Type type)
        {
            lock (ignoredTypesLock)
            {
                Type current = type;
                while (current != null)
                {
                    ignoredTypes[type] = true;
                    current = current.GetTypeInfo().BaseType;
                }
            }
        }
        
        private static bool IsTypeIgnored(Type type)
        {
            if (type.GetTypeInfo().IsGenericType)
                type = type.GetGenericTypeDefinition();

            bool ignored;

            lock (ignoredTypesLock)
            {
                if (!ignoredTypes.TryGetValue(type, out ignored))
                {
                    ignored = type.GetTypeInfo().GetCustomAttributes(typeof (IgnoreAttribute), false).Any();
                    ignoredTypes[type] = ignored;
                }
            }

            return ignored;
        }

        private static bool IsPropertyIgnored(PropertyInfo property)
        {
            bool ignored;
            lock (ignoredTypesLock)
            {
                if (!ignoredProperties.TryGetValue(property, out ignored))
                {
                    ignored = property.GetCustomAttributes(typeof(IgnoreAttribute), false).Any();
                    ignoredProperties[property] = ignored;
                }
            }
            return ignored;
        }

        private Type type;
        private ConstructorInfo constructor;
        private List<Tuple<ParameterInfo, Func<Request, object>>> arguments;
        private List<Tuple<PropertyInfo, Action<object, Request>>> properties;

        public ReflectionResolver(Type type, Func<PropertyInfo, bool> propertyFilter = null)
        {
            this.type = type;
            propertyFilter = propertyFilter ?? (x => true);
            constructor = type.GetTypeInfo().DeclaredConstructors.FirstOrDefault(x => !x.IsStatic);
            if (constructor != null)
            {
                arguments = constructor
                    .GetParameters()
                        .Select(x => Tuple.Create<ParameterInfo, Func<Request, object>>(x, request =>
                        {
                            var childRequest = request.CreateChildRequest(x.ParameterType);
                            if (childRequest == null)
                                throw new InvalidOperationException("No resolvers registered for " + request.Type.FullName + " and type is not instantiatable.");
                            return childRequest.Resolve();
                        }))
                        .ToList();
            }
            properties = type
                .GetTypeInfo()
                .DeclaredProperties
                .Where(x => false)//autoInjectProperties)
                .Where(x => x.GetIndexParameters().Length == 0 && x.SetMethod != null && IsInjectionable(x.PropertyType) && !IsTypeIgnored(x.DeclaringType) && !IsPropertyIgnored(x))
                .Where(propertyFilter)
                .Select(x => Tuple.Create<PropertyInfo, Action<object, Request>>(x, (instance, request) => x.SetValue(instance, request.CreateChildRequest(x.PropertyType).Resolve(), null)))
                .ToList();
        }

        private static bool IsInjectionable(Type type)
        {
            return !type.GetTypeInfo().IsPrimitive && type != typeof(string) && !IsTypeIgnored(type);
        }

        public virtual object Instantiate(Request request)
        {
            if (constructor == null)
                throw new InvalidOperationException("No constructor found in " + type.FullName);
            if (!request.Type.GetTypeInfo().IsAssignableFrom(type.GetTypeInfo()))
                throw new InvalidOperationException("This resolver expects to return an instance of " + type.FullName + " but was asked to resolve a request expecting a type of " + request.Type.FullName);

            var arguments = new List<object>();
            foreach (var arg in this.arguments)
            {
                try
                {
                    var value = arg.Item2(request);
                    arguments.Add(value);
                }
                catch (Exception e)
                {
                    throw new InvalidOperationException(string.Format("Error resolving argument '{0}' ({1}) of constructor to {2}", arg.Item1.Name, arg.Item1.ParameterType.FullName, constructor.DeclaringType.FullName), e);
                }
            }

            return constructor.Invoke(arguments.ToArray());
        }

        public virtual void Activate(Request request, object o)
        {
            foreach (var property in properties)
            {
                try
                {
                    property.Item2(o, request);
                }
                catch (Exception e)
                {
                    throw new InvalidOperationException(string.Format("Error resolving property '{0}' ({1}) of type {2}", property.Item1.Name, property.Item1.PropertyType.FullName, property.Item1.DeclaringType.FullName), e);
                }
            }
            if (Activated != null)
                Activated(o);
        }
    }

    public class ReflectionResolver<T> : ReflectionResolver
    {
        public new event Action<T> Activated;

        public ReflectionResolver(Func<PropertyInfo, bool> propertyFilter) : base(typeof(T), propertyFilter)
        {
        }

        public override void Activate(Request request, object o)
        {
            base.Activate(request, o);

            if (Activated != null)
                Activated((T)o);
        }
    }
}
