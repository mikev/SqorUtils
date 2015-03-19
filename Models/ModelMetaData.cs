using System;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Sqor.Utils.Types;

namespace Sqor.Utils.Models
{
    public class ModelMetaData
    {
        private Type modelType;
        private List<PropertyInfo> properties;
        private Dictionary<string, PropertyInfo> propertiesByName;
        
        public ModelMetaData(Type modelType)
        {
            this.modelType = modelType;
            
            properties = modelType.GetRuntimeProperties()
                .Where(x => x.GetMethod != null && x.SetMethod != null && x.GetMethod.IsPublic)
                .ToList();
            propertiesByName = properties.ToDictionary(x => x.Name);
        }
        
        public Type ModelType
        {
            get { return modelType; }
        }
        
        public IEnumerable<PropertyInfo> Properties 
        {
            get { return properties; }
        }
        
        public PropertyInfo GetProperty<T, TValue>(Expression<Func<T, TValue>> property)
        {
            return propertiesByName[property.Name];
        }
        
        public PropertyInfo GetProperty(string name) 
        {
            return propertiesByName[name];
        }
    }
    
    public class ModelMetaData<T> : ModelMetaData where T : Model<T>
    {
        private static ModelMetaData<T> instance = new ModelMetaData<T>();
        
        public static ModelMetaData Instance
        { 
            get { return instance; }
        }
        
        public ModelMetaData() : base(typeof(T))
        {
        }
    }
}

