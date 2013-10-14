using System;
using System.Collections.Generic;

namespace Sqor.Utils.Generators
{
    public static class Generate
    {
        public static IEnumerable<T> Sequence<T>(int count, Func<T> generator)
        {
            for (var i = 0; i < count; i ++)
                yield return generator();
        }
    }
}