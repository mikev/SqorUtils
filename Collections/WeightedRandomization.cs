using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sqor.Utils.Collections
{

    public interface IWeighted
    {
        int Weight { get; set; }
    }

    public static class WeightedRandomization
    {
        public static T[] ChooseWeighted<T>(this ICollection<T> c, int n) where T : IWeighted
        {
            if (!c.Any())
            {
                throw new ArgumentException("Collection is null.");
            }
            if (c.Any(x => x.Weight < 0))
            {
                throw new ArgumentException("Weights must be greater than 0.");
            }
            var list = c.ToList();

            Random rand = new Random((int)(DateTime.UtcNow.Ticks % int.MaxValue));

            var rv = new List<T>();
            for (int z = 0; z < n; z++)
            {
                int totalweight = list.Sum(x => x.Weight);
                int selectedValue = rand.Next(totalweight) + 1; // [1, totalweight]
                int cumulativeSum = 0;

                // loop through adding weights until we reach the selected number.
                // e.g. If items have these weights [A:400, B:5, C:100] 
                // A random number (R) will be chosen from [1, 505]
                // 1 <= R <= 400 => A
                // 401 <= R <= 405 => B
                // 406 <= R <= 505 => C
                foreach (var obj in list)
                {
                    cumulativeSum += obj.Weight;

                    if (cumulativeSum < selectedValue) continue;

                    rv.Add(obj);
                    list.Remove(obj);
                    break;
                }
                if (rv.Count() < z + 1)
                {
                    // iterated through all items and did not reach selected weight.
                    // this really shouldn't happen.
                    rv.Add(list.Last());
                    list.Remove(list.Last());
                }
            }
            return rv.ToArray();
        }
    }
}
