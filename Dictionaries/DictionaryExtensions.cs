using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;

namespace Sqor.Utils.Dictionaries
{
    public static class DictionaryExtensions
    {
        public static U GetOrThrow<T, U>(this IDictionary<T, U> dictionary, T key)
        {
            U result;
            if (dictionary.TryGetValue(key, out result))
                return result;
            else
                throw new InvalidOperationException("Key not found in dictionary: " + key);
        }

        public static U Get<T, U>(this IDictionary<T, U> dictionary, T key, U returnIfNotFound = default(U))
        {
            if (dictionary == null)
                return returnIfNotFound;
            if (key == null)
                return returnIfNotFound;

            U result;
            if (dictionary.TryGetValue(key, out result))
                return result;
            else
                return returnIfNotFound;
        }

        public static U GetOrCreate<T, U>(this IDictionary<T, U> dictionary, T key)
            where U : new()
        {
            return dictionary.GetOrCreate(key, () => new U());
        }

        public static U GetOrCreate<T, U>(this IDictionary<T, U> dictionary, T key, Func<U> creator)
        {
            U result;
            if (!dictionary.TryGetValue(key, out result))
            {
                result = creator();
                dictionary[key] = result;
            }
            return result;
        }        

        public static IDictionary<string, object> ToDictionary<T>(this T obj)
        {
            if (obj == null)
                return new Dictionary<string, object>();
            if (obj is IDictionary<string, object>)
                return (IDictionary<string, object>)obj;
            return obj.GetType().GetProperties().ToDictionary(o => o.Name, o => o.GetValue(obj, null));
        }

		/// <summary>
		/// Copies all of the entries from the first dictionary into the second, overwriting any
		/// existing ones.
		/// </summary>
		/// <param name="source">The source dictionary</param>
		/// <param name="target">The target dictionary</param>
		public static Dictionary<TKey, TValue> Merge<TKey, TValue>(this IDictionary<TKey, TValue> source, IDictionary<TKey, TValue> target)
		{
            var result = new Dictionary<TKey, TValue>(source);
			foreach (var entry in target)
			{
				result[entry.Key] = entry.Value;
			}
            return result;
		}

        public static TValue GetValueByType<TValue>(Dictionary<Type, TValue> dictionary, Type type)
        {
            if (type.IsArray)
            {
                TValue result;
                dictionary.TryGetValue(type, out result);
                return result;
            }

            Stack<Type> types = new Stack<Type>();
            if (type.IsEnum)
                types.Push(typeof(Enum));
            types.Push(type);
            while (types.Count > 0)
            {
                Type current = types.Pop();

                TValue result;
                if (dictionary.TryGetValue(current, out result))
                    return result;

                if (current.IsGenericType && !current.IsGenericTypeDefinition)
                    types.Push(current.GetGenericTypeDefinition());
                if (current.BaseType != null)
                    types.Push(current.BaseType);
                foreach (Type intf in current.GetInterfaces())
                    types.Push(intf);
            }

            return default(TValue);
        }

        public static void AddRange<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, IDictionary<TKey, TValue> itemsToAdd)
        {
            foreach (var item in itemsToAdd)
                dictionary[item.Key] = item.Value;
        }

        public static void AddRange(this IDictionary<string, object> dictionary, NameValueCollection itemsToAdd)
        {
            foreach (var item in itemsToAdd.AllKeys)
                dictionary[item] = itemsToAdd[item];
        }
    }
}
