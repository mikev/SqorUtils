using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Common;
using Sqor.Utils.Logging;
using Sqor.Utils.Strings;

namespace Sqor.Utils.SqliteLinq.Migrate.Operations
{
    public class CreateTableOperation : MigrationOperation
    {
        public string TableName { get; set; }
        public List<SysColumn> Columns { get; set; }
        public bool IsMetaDataEmitted { get; set; }
        public bool IsTemporaryTable { get; set; }

        public CreateTableOperation()
        {
            Columns = new List<SysColumn>();
        }

        public override void Execute(IDatabaseDriver driver, DbConnection connection)
        {
            var columns = string.Join(", ", Columns.Select(x => FormatColumn(driver, x)));
            var sql = string.Format("CREATE {2}TABLE {0} ({1})", TableName, columns, IsTemporaryTable ? "TEMPORARY " : "");

            this.LogInfo("Creating table " + TableName);
            connection.Execute(sql);
            
            // Log table data in sys tables
            if (IsMetaDataEmitted)
            {
                this.LogInfo("Creating metadata for table " + TableName);
                
                var table = new SysTable { Name = TableName };
                using (var sysDb = driver.SysDb())
                {
                    sysDb.Connection = connection;
                    sysDb.Tables.Insert(table);
                    foreach (var column in Columns)
                    {
                        column.TableId = table.Id;
                        sysDb.Columns.Insert(column);
                    }
                }
            }
        }

        public static string FormatColumn(IDatabaseDriver driver, SysColumn column)
        {
            var builder = new StringBuilder();

            builder.Append(column.Name + " " + driver.ConvertFromSqlType(column.Type));
            if (column.MaxLength > 0)
            {
                builder.Append("(" + column.MaxLength + ")");
            }
            builder.Append(" ");
            builder.Append(driver.GetColumnOptions(column));
            return builder.ToString();
        }

        public static CreateTableOperation CreateFrom(IDatabaseDriver driver, string tableName)
        {
            using (var sysDb = driver.SysDb())
            {
                var table = sysDb.Tables.SingleOrDefault(x => x.Name == tableName);
                if (table == null)
                    throw new InvalidOperationException("Table not found: " + tableName + ". Existing tables: " + sysDb.Tables.Concatenate(", "));
            
                var result = new CreateTableOperation
                {
                    TableName = tableName,
                    Columns = sysDb.Columns.Where(x => x.TableId == table.Id).ToList()
                };
                
                return result;
            }
        }

//        public class TableInfo
//        {
//            [Column("cid")]
//            public int ColumnId { get; set; }
//
//            [Column("name")]
//            public string Name { get; set; }
//
//            [Column("type")]
//            public string Type { get; set; }
//
//            [Column("notnull")]
//            public bool NotNull { get; set; }
//
//            [Column("dflt_value")]
//            public string DefaultValue { get; set; }
//
//            [Column("pk")]
//            public int PrimaryKeyColumnId { get; set; }
//        }
    }
}

