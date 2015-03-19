using System.Collections.Generic;

namespace Sqor.Utils.Chars
{
    public static class CharExtensions
    {
        public static IEnumerable<char> To(this char from, char to)
        {
            for (var c = from; c <= to; c++)
            {
                yield return c;
            }
        }
    }
}