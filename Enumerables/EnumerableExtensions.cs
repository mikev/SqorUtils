using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Sqor.Utils.Enumerables
{
    public static class EnumerableExtensions
    {
        public static IEnumerable<T> SelectRecursive<T>(this T obj, Func<T, T> next) where T : class
        {
            T current = obj;
            while (current != null)
            {
                yield return current;
                current = next(current);
            }
        }

        public static IEnumerable<T> SelectRecursive<T>(this T obj, Func<T, IEnumerable<T>> next)
        {
            Stack<T> stack = new Stack<T>();
            stack.Push(obj);
            
            while (stack.Any())
            {
                var current = stack.Pop();
                yield return current;
                
                foreach (var child in next(current).Reverse())
                {
                    stack.Push(child);
                }
            }
        }

        public static ObservableCollection<T> ToObservableCollection<T>(this IEnumerable<T> sequence)
        {
            return new ObservableCollection<T>(sequence);
        }

        public static HashSet<T> ToHashSet<T>(this IEnumerable<T> sequence)
        {
            return new HashSet<T>(sequence);
        }

        public static void Delimit<T>(this IEnumerable<T> list, Action<T> action, Action delimiterAction)
        {
            bool first = true;
            foreach (T o in list)
            {
                if (!first)
                    delimiterAction();
                action(o);
                first = false;
            }
        }
        
        /// <summary>
        /// Provides a far better error message if there are the incorrect number of elements in the sequence.
        /// </summary>
        public static T SingleOrError<T>(this IEnumerable<T> source, Func<T, bool> predicate)
        {
            try
            {
                return source.Single(predicate);
            }
            catch (Exception e)
            {
                throw new InvalidOperationException("Calling single on a sequence that contains: " + source.Count() + " elements.", e);
            }
        }
        
        public static IEnumerable<T> Distinct<T>(this IEnumerable<T> source, Func<T, T, bool> comparison, Func<T, int> hashCode)
        {
            return source.Distinct(new ComparisonComparer<T>(comparison, hashCode));
        }
        
        public static T RandomElement<T>(this IEnumerable<T> source, Random random = null)
        {
            var array = source.ToArray();
            random = random ?? new Random();
            return array[random.Next(array.Length)];
        }

        /// <summary>
        /// Compares `source` with `mergeWith`.  Items that are contained in `mergeWith` that are not contained in `source` 
        /// are placed in the `Added` property on the return value.  Items that are contained in `source` but not contained
        /// in `mergeWith` are placed in the `Removed` property on the return value.
        /// </summary>
        public static MergeResult<T, T> Merge<T>(this IEnumerable<T> source, IEnumerable<T> mergeWith)
        {
            return source.Merge(mergeWith, x => x, x => x);
        }

        public static MergeResult<TLeft, TRight> Merge<TLeft, TRight, TKey>(this IEnumerable<TLeft> source, IEnumerable<TRight> mergeWith, Func<TLeft, TKey> leftKeySelector, Func<TRight, TKey> rightKeySelector)
        {
            var sourceSet = new HashSet<TLeft>(source);
            var mergeWithSet = new HashSet<TRight>(mergeWith);

            var removed = sourceSet.Where(item => !mergeWithSet.Select(rightKeySelector).Contains(leftKeySelector(item))).ToList();
            var added = mergeWithSet.Where(item => !sourceSet.Select(leftKeySelector).Contains(rightKeySelector(item))).ToList();

            return new MergeResult<TLeft, TRight>(added, removed);
        }

        public struct MergeResult<TLeft, TRight>
        {
            private readonly List<TRight> added;
            private readonly List<TLeft> removed;

            public MergeResult(List<TRight> added, List<TLeft> removed) : this()
            {
                this.added = added;
                this.removed = removed;
            }

            public List<TRight> Added
            {
                get { return added; }
            }

            public List<TLeft> Removed
            {
                get { return removed; }
            }
        }

        class ComparisonComparer<T> : IEqualityComparer<T>
        {
            private Func<T, T, bool> comparison;
            private Func<T, int> hashCode;

            public ComparisonComparer(Func<T, T, bool> comparison, Func<T, int> hashCode)
            {
                this.comparison = comparison;
                this.hashCode = hashCode;
            }

            public bool Equals(T x, T y)
            {
                return comparison(x, y);
            }

            public int GetHashCode(T obj)
            {
                return hashCode(obj);
            }
        }
        
        public class Position<T>
        {
            public T Item { get; set; }
            public int Index { get; set; }
            public bool IsFirst { get; set; }
            public bool IsLast { get; set; }
        }

        public static IEnumerable<Position<T>> SelectPosition<T>(this IEnumerable<T> source)
        {
            IEnumerator<T> enumerator = source.GetEnumerator();

            if (enumerator.MoveNext())
            {
                Func<T, int, bool, bool, Position<T>> makePosition = (item, index, isFirst, isLast) => new Position<T>
                {
                    Item = item, Index = index, IsFirst = isFirst, IsLast = isLast
                };

                T current = enumerator.Current;
                bool hasNext = enumerator.MoveNext();

                int i = 0;

                yield return makePosition(current, i++, true, !hasNext);

                while (hasNext)
                {
                    current = enumerator.Current;
                    hasNext = enumerator.MoveNext();
                    yield return makePosition(current, i++, false, !hasNext);
                }                
            }
        }        

        /// <summary>
        /// Partitions <i>source</i> into chunks containing up to <i>size</i> elements.  This is useful when you want to break up your results 
        /// into groups for display purposes, but you don't need to base it on any values of the content, but rather by an arbitrary fixed 
        /// number per group.  (The last group will contain fewer if count / size is not an integer.)
        /// </summary>
        public static IEnumerable<IGrouping<int, TElement>> Partition<TElement>(this IEnumerable<TElement> source, int size, Func<int, IEnumerable<TElement>, IEnumerable<TElement>> resultSelector = null, bool fillEmptyWithDefault = false)
        {
            int partitionIndex = 0;
            List<TElement> group = new List<TElement>();

            if (resultSelector == null)
                resultSelector = (x, y) => y;

            foreach (var element in source)
            {
                group.Add(element);

                if (group.Count == size)
                {
                    yield return new PartitionGrouping<TElement>(partitionIndex, resultSelector(partitionIndex, group));
                    partitionIndex++;
                    group = new List<TElement>();
                }
            }

            if (group.Count > 0)
            {
                if (fillEmptyWithDefault)
                    for (int i = group.Count; i < size; i++)
                        group.Add(default(TElement));
                yield return new PartitionGrouping<TElement>(partitionIndex, resultSelector(partitionIndex, group));
            }
        }

        public static IEnumerable<T> EmptyIfNull<T>(this IEnumerable<T> sequence)
        {
            if (sequence == null)
                return Enumerable.Empty<T>();
            else
                return sequence;
        }

        private struct PartitionGrouping<TElement> : IGrouping<int, TElement>
        {
            private readonly int key;
            private readonly IEnumerable<TElement> source;

            public PartitionGrouping(int key, IEnumerable<TElement> source)
            {
                this.key = key;
                this.source = source;
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }

            public IEnumerator<TElement> GetEnumerator()
            {
                return source.GetEnumerator();
            }

            public int Key
            {
                get { return key; }
            }
        }
    }
}
