using System;
using System.Collections.Generic;
using System.Collections;

namespace Sqor.Utils.Arrays
{
    public class ArrayReadOnlyList<T> : IReadOnlyList<T>
    {
        private T[] array;
    
        public ArrayReadOnlyList(T[] array)
        {
            this.array = array;
        }
        
        public IEnumerator<T> GetEnumerator()
        {
            return ((IEnumerable<T>)array).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return array.GetEnumerator();
        }

        public T this[int index]
        {
            get { return array[index]; }
        }

        public int Count
        {
            get { return array.Length; }
        }
    }
}

