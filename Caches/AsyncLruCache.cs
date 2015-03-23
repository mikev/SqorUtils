#if FULL
    
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nito.AsyncEx;
using Sqor.Utils.Enumerables;

namespace Sqor.Utils.Caches
{
    public class AsyncLruCache<TKey, TValue> 
        where TValue : class
    {
        private int capacity;
        private Dictionary<TKey, Entry> storage = new Dictionary<TKey, Entry>();
        private Entry newestItem;
        private Entry oldestItem;
        private Func<TKey[], Task<Tuple<TKey, TValue>[]>> populator;
        private AsyncLock locker = new AsyncLock();

        public AsyncLruCache(int capacity, Func<TKey[], Task<Tuple<TKey, TValue>[]>> populator)
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
            if (oldestItem.NewerItem != null)
            {
                oldestItem.NewerItem.OlderItem = null;
                this.oldestItem = oldestItem.NewerItem;
            }
        }

        public void Add(KeyValuePair<TKey, TValue> item)
        {
            Add(item.Key, item.Value);
        }

        public void Clear()
        {
            using (locker.Lock())
            {
                storage.Clear();
                oldestItem = null;
                newestItem = null;
            }
        }

        public void Remove(KeyValuePair<TKey, TValue> item)
        {
            Remove(item.Key);
        }

        public int Count
        {
            get
            {
                using (locker.Lock())
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
            using (locker.Lock())
            {
                return storage.ContainsKey(key);                
            }
        }

        private Tuple<Entry, IDisposable> AddLater(TKey key)
        {
            while (storage.Count >= capacity)
                Sacrifice();

            var entry = new Entry(key);
            entry.OlderItem = newestItem;
            newestItem = entry;

            if (entry.OlderItem != null)
                entry.OlderItem.NewerItem = newestItem;
            else
                oldestItem = newestItem;

            storage.Add(key, entry);

            return Tuple.Create(entry, entry.Locker.Lock());
        }

        public void Add(TKey key, TValue value)
        {
            using (locker.Lock())
            {
                while (storage.Count >= capacity)
                    Sacrifice();

                var entry = new Entry(key, value);
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
            using (locker.Lock())
            {
                return InternalRemove(key);
            }
        }

        private bool InternalRemove(TKey key)
        {
            if (oldestItem != null && EqualityComparer<TKey>.Default.Equals(oldestItem.Key, key))
            {
                Sacrifice();
                return true;
            }
            else
            {
                Entry entry;
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

        private void RemoveEntryFromList(Entry entry)
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

        public async Task<TValue[]> TryGetValues(TKey[] keys)
        {
            var result = new TValue[keys.Length];
            List<Tuple<TKey, Entry, IDisposable, int>> missingIds = null;
            List<Tuple<Entry, int>> pendingIds = null;
            bool nullValues = false;
            using (await locker.LockAsync())
            {
                for (var i = 0; i < keys.Length; i++)
                {
                    Entry entry;
                    bool found = storage.TryGetValue(keys[i], out entry);
                    if (found)
                    {
                        RemoveEntryFromList(entry);
                        entry.OlderItem = newestItem;
                        if (newestItem != null)
                            newestItem.NewerItem = entry;                        
                        newestItem = entry;
                        if (oldestItem == null)
                            oldestItem = newestItem;

                        if (entry.HasValue)
                        {
                            result[i] = entry.Value;
                            if (entry.Value == null)
                                nullValues = true;
                        }
                        else
                        {
                            if (pendingIds == null)
                                pendingIds = new List<Tuple<Entry, int>>();
                            pendingIds.Add(Tuple.Create(entry, i));
                        }
                    }
                    else
                    {
                        if (missingIds == null)
                            missingIds = new List<Tuple<TKey, Entry, IDisposable, int>>();
                        var later = SetLater(keys[i]);
                        missingIds.Add(Tuple.Create(keys[i], later.Item1, later.Item2, i));
                    }
                }
            }

            if (missingIds != null)
            {
                try
                {
                    var values = await populator(missingIds.Select(x => x.Item1).ToArray());
                    var missingIdsById = missingIds.ToDictionary(x => x.Item1);
                    var stillMissingIds = missingIds.ToHashSet();

                    for (var i = 0; i < values.Length; i++)
                    {
                        var value = values[i];
                        var id = value.Item1;
                        Tuple<TKey, Entry, IDisposable, int> missingId;
                        if (!missingIdsById.TryGetValue(id, out missingId))
                            throw new Exception("Id " + id + " not found in missing ids.");
                        missingId = missingIdsById[id];
                        if (value.Item2 == null)
                            nullValues = true;
                        var entry = missingId.Item2;
                        var entryLock = missingId.Item3;
                        var index = missingId.Item4;
                        entry.SetValue(value.Item2);
                        entryLock.Dispose();
                        result[index] = value.Item2;
                        stillMissingIds.Remove(missingId);
                    }
                    foreach (var missingId in stillMissingIds)
                    {
                        nullValues = true;
                        var entry = missingId.Item2;
                        var entryLock = missingId.Item3;
                        entry.SetValue(null);
                        entryLock.Dispose();
                    }
                }
                catch 
                {
                    foreach (var item in missingIds)
                    {
                        Remove(item.Item1);
                        item.Item3.Dispose();
                    }
                    throw;
                }
            }
            if (pendingIds != null)
            {
                foreach (var pendingId in pendingIds)
                {
                    await pendingId.Item1.WaitForValue();
                    if (pendingId.Item1.Value == null)
                        nullValues = true;
                    result[pendingId.Item2] = pendingId.Item1.Value;
                }
            }

            if (!nullValues)
                return result;
            else
                return result.Where(x => x != null).ToArray();
        }

        public async Task<TValue> TryGetValue(TKey key)
        {
            return (await TryGetValues(new[] { key })).Single();
        }

        public Task<TValue> this[TKey key]
        {
            get
            {
                return TryGetValue(key);
            }
        }

        private Tuple<Entry, IDisposable> SetLater(TKey key)
        {
            InternalRemove(key);
            return AddLater(key);
        }

        public ICollection<TKey> Keys
        {
            get
            {
                using (locker.Lock())
                {
                    return storage.Keys;
                }
            }
        }

        public ICollection<TValue> Values
        {
            get
            {
                using (locker.Lock())
                {
                    return storage.Values.Select(x => x.Value).ToList();
                }
            }
        }

        class Entry
        {
            public Entry OlderItem;
            public Entry NewerItem;

            public readonly TKey Key;
            public TValue Value;
            public bool HasValue;
            public AsyncLock Locker = new AsyncLock();

            public Entry(TKey key)
            {
                Key = key;
            }

            public Entry(TKey key, TValue value)
            {
                Key = key;
                Value = value;
                HasValue = true;
            }

            public void SetValue(TValue value)
            {
                Value = value;
                HasValue = true;
            }

            public async Task WaitForValue()
            {
                using (await Locker.LockAsync())
                {
                }
                if (!HasValue)
                    throw new Exception("Waited for value, but value not returned.");
            }

            public bool Equals(Entry other)
            {
                if (ReferenceEquals(null, other)) return false;
                if (ReferenceEquals(this, other)) return true;
                return Equals(other.Key, Key);
            }

            public override bool Equals(object obj)
            {
                if (ReferenceEquals(null, obj)) return false;
                if (ReferenceEquals(this, obj)) return true;
                if (obj.GetType() != typeof(Entry)) return false;
                return Equals((Entry)obj);
            }

            public override int GetHashCode()
            {
                unchecked
                {
                    return Key.GetHashCode();
                }
            }
        }
    }
}
#endif
