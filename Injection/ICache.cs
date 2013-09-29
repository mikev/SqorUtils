using System;

namespace Sqor.Utils.Injection
{
    public interface ICache
    {
        object Get(Type type); 
        void Set(Type type, object value);
    }
}