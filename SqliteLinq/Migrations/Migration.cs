using System;
using System.Collections.Generic;
using System.Linq;
using Sqor.Utils.SqliteLinq.Migrate.Operations;
using Sqor.Utils.SqliteLinq.Migrate;

namespace Sqor.Utils.SqliteLinq.Migrate
{
    public abstract class Migration
    {
        protected abstract void Migrate();

        public DateTime Timestamp { get; set; }
        public bool IsAutomaticallyPerformed { get; set; }
        public bool IsMetaDataEmitted { get; set; }

        private List<MigrationOperation> operations = new List<MigrationOperation>();
        private IDatabaseDriver driver; 

        public Migration(DateTime timestamp)
        {
            Timestamp = timestamp;
            IsAutomaticallyPerformed = true;
            IsMetaDataEmitted = true;
        }

        public virtual void PerformMigration(IDatabaseDriver driver) 
        {
            this.driver = driver;
            Migrate();
            using (var connection = driver.ConnectionFactory.CreateConnection())
            {
                connection.Open();
                using (var transaction = connection.BeginTransaction())
                {
                    ExecuteOperations(driver, connection, operations);
                    transaction.Commit();
                }
            }
        }

        protected virtual void ExecuteOperations(IDatabaseDriver driver, System.Data.Common.DbConnection connection, List<MigrationOperation> operations)
        {
            foreach (var operation in operations)
            {
                operation.Execute(driver, connection);
            }
        }

        public void AddOperation(MigrationOperation operation)
        {
            operations.Add(operation);
        }

        public string Name
        {
            get
            {
                return GetType().Name;
            }
        }

        public TableBuilder<TColumns> CreateTable<TColumns>(string tableName, Func<ColumnBuilder, TColumns> columnsAction)
        {
            var operation = new CreateTableOperation { TableName = tableName, IsMetaDataEmitted = IsMetaDataEmitted };

            var columnBuilder = new ColumnBuilder(driver);
            var columns = columnsAction(columnBuilder);

            foreach (var property in columns.GetType().GetProperties())
            {
                var value = (SysColumn)property.GetValue(columns, null);
                value.Name = property.Name;
                operation.Columns.Add(value);
            }

            AddOperation(operation);
            return new TableBuilder<TColumns>(operation);
        }
        
        public void DropTable(string tableName)
        {
            AddOperation(new DropTableOperation { TableName = tableName });
        }

        public void RenameTable(string tableName, string newTableName)
        {
            AddOperation(new RenameTableOperation { TableName = tableName, NewTableName = newTableName });
        }
                
        public void DropColumn(string tableName, string columnName)
        {
            AddOperation(new DropColumnOperation { TableName = tableName, ColumnName = columnName });
        }
        
        public void AddColumn(string tableName, string columnName, Func<ColumnBuilder, SysColumn> columnAction)
        {
            var columnBuilder = new ColumnBuilder(driver);
            var column = columnAction(columnBuilder);
            column.Name = columnName;
            AddOperation(new AddColumnOperation { TableName = tableName, Column = column });
        } 
        
        public void Sql(string sql)
        {
            AddOperation(new SqlOperation { Sql = sql });
        }
        
        public void Sql(string sql, params object[] args)
        {
            Sql(string.Format(sql, args));
        }
    }
}
