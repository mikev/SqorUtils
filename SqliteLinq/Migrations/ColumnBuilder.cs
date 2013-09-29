using System;

namespace Sqor.Utils.SqliteLinq.Migrate
{
    public class ColumnBuilder
    {
        private IDatabaseDriver driver;
    
        public ColumnBuilder(IDatabaseDriver driver)
        {
            this.driver = driver;
        }

        public SysColumn Bool(bool isNullable = false, bool isDbGenerated = false, bool isPrimaryKey = false, string collation = null, int maxLength = 0, bool? defaultValue = null) 
        {
            return new SysColumn
            {
                IsDbGenerated = isDbGenerated,
                IsNullable = isNullable,
                IsPrimaryKey = isPrimaryKey,
                MaxLength = maxLength,
                Type = SqlType.Bool,
                DefaultValue = driver.FormatObject(defaultValue)
            };
        }

        public SysColumn Int(bool isNullable = false, bool isDbGenerated = false, bool isPrimaryKey = false, string collation = null, int maxLength = 0, int? defaultValue = null) 
        {
            return new SysColumn
            {
                IsDbGenerated = isDbGenerated,
                IsNullable = isNullable,
                IsPrimaryKey = isPrimaryKey,
                MaxLength = maxLength,
                Type = SqlType.Int,
                DefaultValue = driver.FormatObject(defaultValue)
            };
        }

        public SysColumn Float(bool isNullable = false, bool isDbGenerated = false, bool isPrimaryKey = false, string collation = null, int maxLength = 0, float? defaultValue = null) 
        {
            return new SysColumn
            {
                IsDbGenerated = isDbGenerated,
                IsNullable = isNullable,
                IsPrimaryKey = isPrimaryKey,
                MaxLength = maxLength,
                Type = SqlType.Float,
                DefaultValue = driver.FormatObject(defaultValue)
            };
        }

        public SysColumn String(bool isNullable = false, bool isDbGenerated = false, bool isPrimaryKey = false, string collation = null, int maxLength = 0, string defaultValue = null) 
        {
            return new SysColumn
            {
                IsDbGenerated = isDbGenerated,
                IsNullable = isNullable,
                IsPrimaryKey = isPrimaryKey,
                MaxLength = maxLength,
                Type = SqlType.String,
                DefaultValue = driver.FormatObject(defaultValue)
            };
        }

        public SysColumn Binary(bool isNullable = false, bool isDbGenerated = false, bool isPrimaryKey = false, string collation = null, int maxLength = 0) 
        {
            return new SysColumn
            {
                IsDbGenerated = isDbGenerated,
                IsNullable = isNullable,
                IsPrimaryKey = isPrimaryKey,
                MaxLength = maxLength,
                Type = SqlType.Binary
            };
        }

        public SysColumn DateTime(bool isNullable = false, bool isDbGenerated = false, bool isPrimaryKey = false, string collation = null, int maxLength = 0, DateTime? defaultValue = null) 
        {
            return new SysColumn
            {
                IsDbGenerated = isDbGenerated,
                IsNullable = isNullable,
                IsPrimaryKey = isPrimaryKey,
                MaxLength = maxLength,
                Type = SqlType.DateTime,
                DefaultValue = driver.FormatObject(defaultValue)
            };
        }        
    }
}
