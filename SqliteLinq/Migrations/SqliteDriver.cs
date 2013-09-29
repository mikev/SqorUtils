using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data;
using Sqor.Utils.Strings;

namespace Sqor.Utils.SqliteLinq.Migrate
{
    public class SqliteDriver : IDatabaseDriver
    {
        private DbProviderFactory db;
        private Func<SysDb> sysDb;

        public SqliteDriver(DbProviderFactory db, Func<SysDb> sysDb)
        {
            this.db = db;
            this.sysDb = sysDb;
        }

        public DbProviderFactory ConnectionFactory
        {
            get { return db; }
        }

        public Func<SysDb> SysDb
        {
            get { return sysDb; }
        }
        
        public SqlType ConvertToSqlType(string sqlType, out int size)
        {
            var parts = sqlType.Split(new[] { '(' });
            if (parts.Length > 1)
            {
                var sizePart = parts[1];
                sizePart = sizePart.ChopEnd(")");
                size = int.Parse(sizePart);
                sqlType = parts[0];
            }
            else 
            {
                size = 0;
            }
            switch (sqlType)
            {
                case "integer":
                    return SqlType.Int;
                case "bigint":
                    return SqlType.Long;
                case "float":
                    return SqlType.Double;
                case "varchar":
                    return SqlType.String;
                case "datetime":
                    return SqlType.DateTime;
                case "blob":
                    return SqlType.Binary;
                default:
                    throw new InvalidOperationException("Unknown sql type: " + sqlType);
            }
        }

        public string ConvertFromSqlType(SqlType sqlType)
        {
            switch (sqlType)
            {
                case SqlType.Int: 
                case SqlType.Bool:
                    return "integer";
                case SqlType.Long:
                    return "bigint";
                case SqlType.Float:
                case SqlType.Double:
                    return "float";
                case SqlType.String:
                    return "varchar";
                case SqlType.DateTime:
                    return "datetime";
                case SqlType.Binary:
                    return "blob";
                default:
                    throw new InvalidOperationException("Unrecognized sql type: " + sqlType);
            }
        }
        
        public string FormatObject(object o)
        {
            if (o == null)
                return "NULL";
            else if (o is string || o is char)
                return "'" + o + "'";
            else if (o is DateTime)
                return "'" + ((DateTime)o).ToString("yyyy-MM-dd HH:mm:ss");
            else
                return o.ToString();
        }

        public string GetColumnOptions(SysColumn column)
        {
            List<string> options = new List<string>();
            if (column.IsPrimaryKey)
                options.Add("PRIMARY KEY");
            if (column.IsDbGenerated)
                options.Add("AUTOINCREMENT");
            if (!column.IsNullable)
                options.Add("NOT NULL");
            if (column.Collation != null)
                options.Add("COLLATE " + column.Collation);
            if (column.DefaultValue != "NULL" && column.DefaultValue != null)
                options.Add("DEFAULT " + FormatObject(column.DefaultValue));
            
            return string.Join(" ", options);
        }
    }
}

