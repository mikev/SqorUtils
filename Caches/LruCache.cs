using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;

namespace Sqor.Utils.Caches
{
    public class LruCache<TKey, TValue> : IDictionary<TKey, TValue>
    {
        private int capacity;
        private Dictionary<TKey, Entry<TKey, TValue>> storage = new Dictionary<TKey, Entry<TKey, TValue>>();
        private object lockObject = new object();
        private Entry<TKey, TValue> newestItem;
        private Entry<TKey, TValue> oldestItem;
        private Func<TKey, TValue> populator;

        public LruCache(int capacity, Func<TKey, TValue> populator)
        {
            if (capacity < 1)
                throw new ArgumentException("Invalid capacity");     

            this.capacity = capacity;
            this.populator = populator;
        }

        private void Sacrifice()
        {
            var storage = this.storage;
            var oldestItem = this.oldestItem;

            var key = oldestItem.Key;

            storage.Remove(key);
//            if (oldestItem.Value is IDisposable)
//                ((IDisposable)
            if (oldestItem.NewerItem != null)
            {
                oldestItem.NewerItem.OlderItem = null;
                this.oldestItem = oldestItem.NewerItem;
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            lock (lockObject)
            {
                KeyValuePair<TKey, TValue>[] keyValuePairs = storage.Select(x => new KeyValuePair<TKey, TValue>(x.Key, x.Value.Value)).ToArray();
                return ((IEnumerable<KeyValuePair<TKey, TValue>>)keyValuePairs).GetEnumerator();
            }
        }

        public void Add(KeyValuePair<TKey, TValue> item)
        {
            Add(item.Key, item.Value);
        }

        public void Clear()
        {
            lock (lockObject)
            {
                storage.Clear();
                oldestItem = null;
                newestItem = null;
            }
        }

        public bool Contains(KeyValuePair<TKey, TValue> item)
        {
            lock (lockObject)
            {
                return storage.Contains(new KeyValuePair<TKey, Entry<TKey, TValue>>(item.Key, new Entry<TKey, TValue>(item.Key, item.Value)));
            }
        }

        public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
        {
            KeyValuePair<TKey, TValue>[] keyValuePairs = this.ToArray();
            Array.Copy(keyValuePairs, 0, array, arrayIndex, keyValuePairs.Length);
        }

        public bool Remove(KeyValuePair<TKey, TValue> item)
        {
            return Remove(item.Key);
        }

        public int Count
        {
            get
            {
                lock (lockObject)
                {
                    return storage.Count;
                }
            }
        }

        public bool IsReadOnly
        {
            get { return false; }
        }

        public bool ContainsKey(TKey key)
        {
            lock (lockObject)
            {
                return storage.ContainsKey(key);                
            }
        }

        public void Add(TKey key, TValue value)
        {
            lock (lockObject)
            {
                while (storage.Count >= capacity)
                    Sacrifice();

                var entry = new Entry<TKey, TValue>(key, value);
                entry.OlderItem = newestItem;
                newestItem = entry;

                if (entry.OlderItem != null)
                    entry.OlderItem.NewerItem = newestItem;
                else
                    oldestItem = newestItem;

                storage.Add(key, entry);
            }
        }

        public bool Remove(TKey key)
        {
            lock (lockObject)
            {
                if (oldestItem != null && EqualityComparer<TKey>.Default.Equals(oldestItem.Key, key))
                {
                    Sacrifice();
                    return true;
                }
                else
                {
                    Entry<TKey, TValue> entry;
                    if (storage.TryGetValue(key, out entry))
                    {
                        RemoveEntryFromList(entry);
                        storage.Remove(entry.Key);
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
            }
        }

        private void RemoveEntryFromList(Entry<TKey, TValue> entry)
        {
            lock (lockObject)
            {
                if (entry.NewerItem != null)
                    entry.NewerItem.OlderItem = entry.OlderItem;
                if (entry.OlderItem != null)
                    entry.OlderItem.NewerItem = entry.NewerItem;
                if (Equals(oldestItem, newestItem) && oldestItem != null)
                    oldestItem = newestItem = null;
                else if (Equals(oldestItem, entry))
                    oldestItem = oldestItem.NewerItem;
                else if (Equals(newestItem, entry))
                    newestItem = newestItem.OlderItem;            
                entry.OlderItem = null;
                entry.NewerItem = null;
            }
        }

        public bool TryGetValue(TKey key, out TValue value)
        {
            lock (lockObject)
            {
                Entry<TKey, TValue> entry;
                bool found = storage.TryGetValue(key, out entry);
                if (found)
                {
                    RemoveEntryFromList(entry);
                    entry.OlderItem = newestItem;
                    if (newestItem != null)
                        newestItem.NewerItem = entry;                        
                    newestItem = entry;
                    if (oldestItem == null)
                        oldestItem = newestItem;

                    value = entry.Value;
                    return true;
                }
                else
                {
                    value = populator(key);
                    this[key] = value;
                    return true;
                }
            }
        }

        public TValue this[TKey key]
        {
            get
            {
                TValue value;
                TryGetValue(key, out value);
                return value;
            }
            set
            {
                lock (lockObject)
                {
                    Remove(key);
                    Add(key, value);
                }
            }
        }

        public ICollection<TKey> Keys
        {
            get
            {
                lock (lockObject)
                {
                    return storage.Keys;
                }
            }
        }

        public ICollection<TValue> Values
        {
            get
            {
                lock (lockObject)
                {
                    return storage.Values.Select(x => x.Value).ToList();
                }
            }
        }

        class Entry<TKey, TValue>
        {
            public Entry<TKey, TValue> OlderItem;
            public Entry<TKey, TValue> NewerItem;

            public readonly TKey Key;
            public readonly TValue Value;

            public Entry(TKey key, TValue value)
            {
                Key = key;
                Value = value;
            }

            public bool Equals(Entry<TKey, TValue> other)
            {
                if (ReferenceEquals(null, other)) return false;
                if (ReferenceEquals(this, other)) return true;
                return Equals(other.Key, Key);
            }

            public override bool Equals(object obj)
            {
                if (ReferenceEquals(null, obj)) return false;
                if (ReferenceEquals(this, obj)) return true;
                if (obj.GetType() != typeof(Entry<TKey, TValue>)) return false;
                return Equals((Entry<TKey, TValue>)obj);
            }

            public override int GetHashCode()
            {
                unchecked
                {
                    return Key.GetHashCode();
                }
            }
        }
    }}

