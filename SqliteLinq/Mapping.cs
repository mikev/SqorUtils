using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Sqor.Utils.Enumerables;
using Sqor.Utils.Dictionaries;

namespace Sqor.Utils.SqliteLinq
{
    public class Mapping
    {
        public string TableName { get; private set; }

        private Type entityType;
        private List<string> columnNames;
        private List<PropertyInfo> properties;
        private Dictionary<string, string> columnNamesByPropertyName;
        private Dictionary<string, PropertyInfo> propertiesByColumnName;
        private PropertyInfo autoIncrementProperty;
        private List<string> insertColumnNames;
        private List<PropertyInfo> primaryKeyProperties;
        private List<PropertyInfo> insertProperties;
        private List<string> updateColumnNames;
        private List<PropertyInfo> updateProperties;

        public Mapping(Type entityType)
        {
            this.entityType = entityType;

            var tableNameAttribute = (TableAttribute)Attribute.GetCustomAttribute(entityType, typeof(TableAttribute), true);
            TableName = tableNameAttribute != null ? tableNameAttribute.TableName : entityType.Name;
            
            var properties = entityType
                .GetProperties()
                .Select(x => new { Property = x, ColumnAttribute = (ColumnAttribute)Attribute.GetCustomAttribute(x, typeof(ColumnAttribute), true) })
                .Select(x => new { Property = x.Property, Column = x.ColumnAttribute != null ? x.ColumnAttribute.ColumnName : x.Property.Name });
            columnNamesByPropertyName = properties.ToDictionary(x => x.Property.Name, x => x.Column);
            propertiesByColumnName = properties.ToDictionary(x => x.Column, x => x.Property);
            columnNames = properties.Select(x => x.Column).ToList();
            this.properties = properties.Select(x => x.Property).ToList();
            autoIncrementProperty = this.properties.SingleOrDefault(x => Attribute.IsDefined(x, typeof(AutoIncrementAttribute), true));
            primaryKeyProperties = this.properties.Where(x => Attribute.IsDefined(x, typeof(PrimaryKeyAttribute), true)).ToList();
            insertColumnNames = properties.Where(x => autoIncrementProperty == null || x.Property.Name != autoIncrementProperty.Name).Select(x => x.Column).ToList();
            updateColumnNames = properties.Where(x => !primaryKeyProperties.Contains(x.Property)).Select(x => x.Column).ToList();
            insertProperties = this.properties.Where(x => autoIncrementProperty == null || x.Name != autoIncrementProperty.Name).ToList();
            updateProperties = this.properties.Where(x => !primaryKeyProperties.Contains(x)).ToList();
        }
        
        private Type EntityType
        {
            get { return this.entityType; }
        }
        
        public int Count
        {
            get { return columnNames.Count; }
        }

        public IEnumerable<string> ColumnNames 
        {
            get { return columnNames; }
        }
        
        public IEnumerable<string> InsertColumnNames
        {
            get { return insertColumnNames; }
        }
        
        public IEnumerable<PropertyInfo> Properties 
        {
            get { return properties; }
        }
        
        public IEnumerable<PropertyInfo> PrimaryKeyProperties
        {
            get { return primaryKeyProperties; }
        }
        
        public IEnumerable<PropertyInfo> InsertProperties
        {
            get { return insertProperties; }
        }
        
        public IEnumerable<PropertyInfo> UpdateProperties
        {
            get { return updateProperties; }
        }
        
        public IEnumerable<string> UpdateColumnNames
        {
            get { return updateColumnNames; }
        }

        public string GetColumnName(string propertyName)
        {
            return columnNamesByPropertyName[propertyName];
        }

        public string GetPropertyName(string columnName)
        {
            return propertiesByColumnName[columnName].Name;
        }

        public PropertyInfo GetProperty(string columnName)
        {
            var result = propertiesByColumnName.Get(columnName);
            if (result == null)
                throw new InvalidOperationException("Column " + columnName + " not found in " + TableName);
            return result;
        }
        
        public bool IsAutoIncrement(PropertyInfo property)
        {
            return autoIncrementProperty.Name == property.Name;
        }
        
        public PropertyInfo AutoIncrementProperty 
        {
            get { return autoIncrementProperty; }
        }
    }
}

