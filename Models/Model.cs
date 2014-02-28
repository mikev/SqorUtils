using System;
using System.Linq.Expressions;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Sqor.Utils.Ios;
using System.Runtime.CompilerServices;

namespace Sqor.Utils.Models
{
    public delegate void PropertyChangedEvent<in T>(T obj, IProperty property, object oldValue, object newValue)
        where T : Model<T>;
    public delegate void PropertyChangedEvent<TModel, TValue>(Property<TModel, TValue> property, TValue oldValue, TValue newValue) 
        where TModel : Model<TModel>;

    public static class Model
    {
        public static ModelCollection<T> ToModelCollection<T>(this IEnumerable<T> sequence) 
            where T : Model<T>
        {
            return new ModelCollection<T>(sequence);
        }
    }
    
    public interface IProperty
    {
        object Value { get; set; }
        string Name { get; }
        Type Type { get; }
    }
    
    public class Property<TModel, TValue> : IProperty
        where TModel : Model<TModel>
    {
        public event PropertyChangedEvent<TModel, TValue> Changed;
    
        private TModel model;
        private TValue value;
        private PropertyInfo propertyInfo;
    
        public Property(TModel model, PropertyInfo propertyInfo)
        {
            this.model = model;
            this.propertyInfo = propertyInfo;
        }
        
        public string Name
        {
            get { return propertyInfo.Name; }
        }
        
        public Type Type 
        {
            get { return propertyInfo.PropertyType; }
        }
        
        public TValue Value
        {
            get { return value; }
            set 
            { 
                var oldValue = value;
                this.value = value;
                OnChanged(oldValue, value);
            }
        }
        
        protected virtual void OnChanged(TValue oldValue, TValue newValue)
        {
            Changed.Fire(x => x(this, oldValue, newValue));
            model.OnPropertyChanged(this, oldValue, newValue);
        }
        
        object IProperty.Value
        {
            get { return Value; }
            set { Value = (TValue)value; }
        }
    }

    public class Model<T> where T : Model<T>
    {
        public event PropertyChangedEvent<T> PropertyChanged;
        public event Action<T> Saving;
        
        private Dictionary<string, IProperty> properties;
        private ModelMetaData metaData;
        
        public Model()
        {
            metaData = ModelMetaData<T>.Instance;
            properties = metaData.Properties.ToDictionary(x => x.Name, x => CreateProperty(x));
        }
        
        private IProperty CreateProperty(PropertyInfo property)
        {
            return (IProperty)Activator.CreateInstance(typeof(Property<,>).MakeGenericType(typeof(T), property.PropertyType), this, property);
        }
        
        public void Save()
        {
            OnSaving();
        }
        
        protected virtual void OnSaving()
        {
            Saving.Fire(x => x((T)(object)this));
        }

        public Property<T, TValue> Property<TValue>(Expression<Func<T, TValue>> property)
        {
            var propertyInfo = metaData.GetProperty(property);
            return (Property<T, TValue>)properties[propertyInfo.Name];
        }
        
        protected TValue Get<TValue>([CallerMemberName]string callerMemberName = null) 
        {
            var property = properties[callerMemberName];
            return (TValue)property.Value;
        }
        
        protected void Set(object value, [CallerMemberName]string callerMemberName = null) 
        {
            var property = properties[callerMemberName];
            property.Value = value;
        }
        
        protected TValue Get<TValue>(Expression<Func<T, TValue>> property)
        {
            var prop = Property(property);
            return (TValue)Get(prop.Name);
        }
        
        protected void Set<TValue>(Expression<Func<T, TValue>> property, TValue value)
        {
            var prop = Property(property);
            Set(prop.Name, value);
        }
        
        protected object Get(string propertyName)
        {
            return properties[propertyName].Value;
        }
        
        protected void Set(string propertyName, object value)
        {
            properties[propertyName].Value = value;
        }
        
        protected internal virtual void OnPropertyChanged(IProperty property, object oldValue, object newValue)
        {
            PropertyChanged.Fire(x => x((T)(object)this, property, oldValue, newValue));
        }
    }
}

