using System;
using System.Data.Common;
using System.Linq;

namespace Sqor.Utils.SqliteLinq.Migrate.Operations
{
    public class AddColumnOperation : MigrationOperation
    {
        public string TableName { get; set; }
        public SysColumn Column { get; set; }
        
        public override void Execute(IDatabaseDriver driver, DbConnection connection)
        {
            connection.Execute(string.Format("ALTER TABLE {0} ADD COLUMN {1}", TableName, 
                CreateTableOperation.FormatColumn(driver, Column)));
                
            // Log table data in sys tables
            using (var sysDb = driver.SysDb())
            {
                var table = sysDb.Tables.Single(x => x.Name == TableName);
                Column.TableId = table.Id;
                sysDb.Connection = connection;
                sysDb.Columns.Insert(Column);
            }
        }
    }
}

