using System;
using System.Collections.Generic;
using Sqor.Utils.Dictionaries;
using System.Linq;

namespace Sqor.Utils.Json
{
    public class JsonObject : JsonValue, IDictionary<string, JsonValue>
    {
        private Dictionary<string, JsonValue> values = new Dictionary<string, JsonValue>();

        public JsonObject() : base(JsonType.Object)
        {
        }
        
        public JsonObject(IEnumerable<KeyValuePair<string, JsonValue>> values) : this()
        {
            foreach (var item in values)
            {
                this.values.Add(item.Key, item.Value);
            }
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

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
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
    }
}

