using System.Collections.Generic;
using System.Data.Entity;

namespace Sqor.Utils.EntityFramework
{
    public static class DbSetExtensions
    {
        public static void RemoveRange<T>(this DbSet<T> dbSet, IEnumerable<T> entities) where T : class
        {
            foreach (var entity in entities)
            {
                dbSet.Remove(entity);
            }
        }
    }
}