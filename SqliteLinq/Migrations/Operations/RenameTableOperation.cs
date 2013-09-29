using System;
using System.Data.Common;
using System.Linq;
using Sqor.Utils.Logging;

namespace Sqor.Utils.SqliteLinq.Migrate.Operations
{
    public class RenameTableOperation : MigrationOperation
    {
        public string TableName { get; set; }
        public string NewTableName { get; set; }
    
        public override void Execute(IDatabaseDriver driver, DbConnection connection)
        {
            this.LogInfo("Renaming table " + TableName + " to " + NewTableName);
        
            var newTable = CreateTableOperation.CreateFrom(driver, TableName);
            newTable.TableName = NewTableName;
            newTable.Execute(driver, connection);
            var columns = string.Join(", ", newTable.Columns.Select(x => x.Name));
            
            // Copy all existing data into the new table
            connection.Execute(string.Format("INSERT INTO {1} SELECT {2} FROM {0}", TableName, NewTableName, columns));
                
            // Drop the original table
            connection.Execute(string.Format("DROP TABLE {0}", TableName));
        }
    }
}