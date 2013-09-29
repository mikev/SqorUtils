using System;
using System.Collections.Generic;
using Sqor.Utils.Dictionaries;

namespace Sqor.Utils.Injection
{
    public class ConcurrentCache : ICache
    {
        private object lockObject = new object();
        private Dictionary<Type, object> storage = new Dictionary<Type, object>();

        public object Get(Type type)
        {
            lock (lockObject)
            {
                return storage.Get(type);                
            }
        }

        public void Set(Type type, object value)
        {
            lock (lockObject)
            {
                storage[type] = value;                
            }
        }         
    }
}