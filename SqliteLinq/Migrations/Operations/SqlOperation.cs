using System;
using System.Data.Common;

namespace Sqor.Utils.SqliteLinq.Migrate.Operations
{
    public class SqlOperation : MigrationOperation
    {
        public string Sql { get; set; }
    
        public override void Execute(IDatabaseDriver driver, DbConnection connection)
        {
            connection.Execute(Sql);
        }
    }
}

