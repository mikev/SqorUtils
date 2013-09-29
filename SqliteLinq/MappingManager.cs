using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;

namespace Sqor.Utils.SqliteLinq
{
    public class MappingManager
    {
        private Dictionary<Type, Mapping> mappings;

        public MappingManager(Dictionary<Type, Mapping> mappings)
        {
            this.mappings = mappings;
        }

        public Mapping this[Type type] 
        {
            get { return mappings[type]; }
        }

        public static MappingManager Create<T>() where T : DataContext
        {
            var entityTypes = typeof(T).GetProperties()
                .Where(x => x.PropertyType.IsGenericType && x.PropertyType.GetGenericTypeDefinition() == typeof(Table<>))
                .Select(x => x.PropertyType.GetGenericArguments()[0])
                .ToDictionary(x => x, x => new Mapping(x));
            return new MappingManager(entityTypes);
        }
    }
}

