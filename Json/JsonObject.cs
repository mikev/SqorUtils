using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Sqor.Utils.Dictionaries;
using System.Linq;
using Sqor.Utils.Enumerables;
using Sqor.Utils.Generics;

namespace Sqor.Utils.Json
{
    public class JsonObject : JsonValue, IDictionary<string, JsonValue>
    {
        private Dictionary<string, JsonValue> values = new Dictionary<string, JsonValue>();

        public JsonObject() : base(JsonNodeType.Object)
        {
        }
        
        public JsonObject(IEnumerable<KeyValuePair<string, JsonValue>> values) : this()
        {
            foreach (var item in values)
            {
                this.values.Add(item.Key, item.Value);
            }
        }

        public Dictionary<string, object> AsObjectDictionary()
        {
            return ((IDictionary<string, JsonValue>)this).ToDictionary(x => x.Key, x => (object)x.Value);
        }
        
        public override JsonValue this[string property] 
        {
            get { return values.Get(property); }
            set { values[property] = value; }
        }
        
        public override JsonValue this[int index]
        {
            get { return values[values.Keys.ElementAt(index)]; }
            set { values[values.Keys.ElementAt(index)] = value; }
        }

        public IEnumerable<string> PropertyNames
        {
            get { return values.Keys; }
        }

        public void Add(string key, JsonValue value)
        {
            values[key] = value;
        }

        public bool ContainsKey(string key)
        {
            return values.ContainsKey(key);
        }

        public bool Remove(string key)
        {
            return values.Remove(key);
        }

        public bool TryGetValue(string key, out JsonValue value)
        {
            return values.TryGetValue(key, out value);
        }

        public void Add(KeyValuePair<string, JsonValue> item)
        {
            values.Add(item.Key, item.Value);
        }

        public void Clear()
        {
            values.Clear();
        }

        public bool Contains(KeyValuePair<string, JsonValue> item)
        {
            return ((ICollection<KeyValuePair<string, JsonValue>>)values).Contains(item);
        }

        public void CopyTo(KeyValuePair<string, JsonValue>[] array, int arrayIndex)
        {
            ((ICollection<KeyValuePair<string, JsonValue>>)values).CopyTo(array, arrayIndex);
        }

        public bool Remove(KeyValuePair<string, JsonValue> item)
        {
            return ((ICollection<KeyValuePair<string, JsonValue>>)values).Remove(item);
        }

        public new IEnumerator<KeyValuePair<string, JsonValue>> GetEnumerator()
        {
            return ((ICollection<KeyValuePair<string, JsonValue>>)values).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((ICollection<KeyValuePair<string, JsonValue>>)values).GetEnumerator();
        }

        public ICollection<string> Keys
        {
            get
            {
                return values.Keys;
            }
        }

        public ICollection<JsonValue> Values
        {
            get
            {
                return values.Values;
            }
        }

        public int Count
        {
            get
            {
                return values.Count;
            }
        }

        public bool IsReadOnly
        {
            get
            {
                return ((ICollection<KeyValuePair<string, JsonValue>>)values).IsReadOnly;
            }
        }

        public object To(Type type)
        {
            return To(type, "");
        }

        private object To(Type type, string keyPrefix)
        {
			var result = Activator.CreateInstance(type);
            PropertyInfo catchAll = null;
            var unusedKeys = Keys.ToHashSet();

			foreach (var property in type.GetProperties().Where(x => JsonAttribute.IsSerialized(x) && x.CanWrite)) 
			{
                try 
                {
                    var jsonAttribute = property.GetCustomAttribute<JsonAttribute>();
                    if (jsonAttribute != null && jsonAttribute.CatchAll)
                    {
                        catchAll = property;
                        continue;
                    }
                    var keyName = keyPrefix + JsonAttribute.GetKey(property);
                    unusedKeys.Remove(keyName);

                    if (jsonAttribute != null && jsonAttribute.IsDenormalized)
                    {
                        var connector = jsonAttribute.Connector ?? "";
                        var newPrefix = keyName + connector;
                        var value = To(property.PropertyType, newPrefix);
                        property.SetValue(result, value, null);
                    }
                    else
                    {
                        var jsonValue = this[keyName];
                        if (jsonValue != null)
                        {
                            var value = JsonObjectSerializer.ConvertJsonObjectToType(jsonValue, property.PropertyType);
                            property.SetValue(result, value, null);
                        }
                    }
                }
                catch (Exception e) 
                {
                    throw new InvalidOperationException("Error deserializing property " + property.Name, e);
                }
			}

            if (catchAll != null)
            {
                var dictionary = (IDictionary)Activator.CreateInstance(catchAll.PropertyType);
                foreach (var key in unusedKeys)
                {
                    dictionary[key] = Convert.ChangeType(this[key], catchAll.PropertyType.GetGenericArgument(typeof(Dictionary<,>), 1));
                }
                catchAll.SetValue(result, dictionary, null);
            }

			return result;            
        }

        public T To<T>()
        {
            return (T)To(typeof(T));
        }
        
        public static JsonObject From(object source)
        {
            if (source == null)
                throw new ArgumentNullException("source");

            var keys = new HashSet<string>();
			var values = new List<KeyValuePair<string, JsonValue>>();

			foreach (var property in source.GetType().GetProperties().Where(x => JsonAttribute.IsSerialized(x)))
			{
                try 
                {
                    var jsonAttribute = property.GetCustomAttribute<JsonAttribute>();
                    var keyName = JsonAttribute.GetKey(property);
                    if (jsonAttribute != null && jsonAttribute.CatchAll)
                    {
                        var dictionary = (IDictionary)property.GetValue(source, null);
                        foreach (string key in dictionary.Keys)
                        {
                            var value = dictionary[key];
                            if (keys.Contains(key))
                                throw new InvalidOperationException("Key already exists: " + key);
                            keys.Add(key);
                            values.Add(new KeyValuePair<string, JsonValue>(key, JsonObjectSerializer.ConvertObjectToJsonValue(value)));
                        }
                    }
                    else
                    {
                        var value = property.GetValue(source, null);
                        var jsonValue = JsonObjectSerializer.ConvertObjectToJsonValue(value);
                        if (jsonAttribute != null && jsonAttribute.IsDenormalized)
                        {
                            var connector = jsonAttribute.Connector ?? "";
                            var jsonObject = (JsonObject)jsonValue;
                            foreach (var item in jsonObject)
                            {
                                var newKey = keyName + connector + item.Key;
                                if (keys.Contains(newKey))
                                    throw new InvalidOperationException("Key already exists: " + newKey);
                                keys.Add(newKey);
                                values.Add(new KeyValuePair<string, JsonValue>(newKey, item.Value));
                            }
                        }
                        else
                        {
                            if (keys.Contains(keyName))
                                throw new InvalidOperationException("Key already exists: " + keyName);
                            keys.Add(keyName);
                            values.Add(new KeyValuePair<string, JsonValue>(keyName, jsonValue));
                        }
                    }
                }
                catch (Exception e)
                {
                    throw new InvalidOperationException("Error serializing property " + property.Name, e);
                }
			}

			var result = new JsonObject(values);
			return result;
        }
    }
}