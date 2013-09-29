using System;
using System.Data.Common;

namespace Sqor.Utils.SqliteLinq.Migrate
{
    public abstract class MigrationOperation
    {
        public abstract void Execute(IDatabaseDriver driver, DbConnection connection);
    }
}