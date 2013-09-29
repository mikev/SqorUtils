using System;
using System.Data.Common;
using Sqor.Utils.Logging;

namespace Sqor.Utils.SqliteLinq.Migrate.Operations
{
    public class DropTableOperation : MigrationOperation
    {
        public string TableName { get; set; }

        public override void Execute(IDatabaseDriver driver, DbConnection connection)
        {
            this.LogInfo("Dropping table " + TableName);
            
            connection.Execute(string.Format("DROP TABLE {0}", TableName));
        }
    }
}

