using System;
using System.Data.Common;
using System.Linq;
using Sqor.Utils.Logging;

namespace Sqor.Utils.SqliteLinq.Migrate.Operations
{
    /// <summary>
    /// SQLite does not support dropping columns short of rebuilding the tables.
    /// </summary>
    public class DropColumnOperation : MigrationOperation
    {
        public string TableName { get; set; }
        public string ColumnName { get; set; }

        public override void Execute(IDatabaseDriver driver, DbConnection connection)
        {
            using (var sysDb = driver.SysDb())
            {
                sysDb.Connection = connection;
                
                this.LogInfo("Dropping column " + ColumnName + " from " + TableName);
                
                // Create a temporary table to house the data while we drop the original
                var temporaryTable = CreateTableOperation.CreateFrom(driver, TableName);
                temporaryTable.Columns.Remove(temporaryTable.Columns.Single(x => x.Name == ColumnName));
                temporaryTable.IsTemporaryTable = true;
                temporaryTable.TableName = TableName + "__Temp";
                temporaryTable.Execute(driver, connection);
                
                var columns = string.Join(", ", temporaryTable.Columns.Select(x => x.Name));
                
                // Copy all existing data into the temporary table (sans the dropped column)
                connection.Execute(string.Format("INSERT INTO {0}__Temp SELECT {1} FROM {0}", TableName, columns));
                    
                // Drop the original table
                connection.Execute(string.Format("DROP TABLE {0}", TableName));
                
                // Create the real table again
                var realTable = new CreateTableOperation { TableName = TableName, Columns = temporaryTable.Columns };
                realTable.Execute(driver, connection);
                
                // Copy the data in the temp table back to the original table
                connection.Execute(string.Format("INSERT INTO {0} SELECT {1} FROM {0}__Temp", TableName, columns));
                
                // Delete the temp table
                connection.Execute(string.Format("DROP TABLE {0}__Temp", TableName));
            }
            
            // Log table data in sys tables
            using (var sysDb = driver.SysDb())
            {
                var table = sysDb.Tables.Single(x => x.Name == TableName);
                var column = sysDb.Columns.Single(x => x.TableId == table.Id && x.Name == ColumnName);
                sysDb.Connection = connection;
                sysDb.Columns.Delete(column);
            }
        }
    }
}

