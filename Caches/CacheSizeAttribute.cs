using System;

namespace Sqor.Utils.Caches
{
    public class CacheSizeAttribute : Attribute
    {
        public int Value { get; private set; }

        public CacheSizeAttribute(int value)
        {
            Value = value;
        }
    }
}