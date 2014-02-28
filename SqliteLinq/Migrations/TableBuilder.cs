using System;
using Sqor.Utils.SqliteLinq.Migrate.Operations;

namespace Sqor.Utils.SqliteLinq.Migrate
{
    public class TableBuilder<TColumns>
    {
        private CreateTableOperation operation;

        public TableBuilder(CreateTableOperation operation)
        {
            this.operation = operation;
        }
        
        public CreateTableOperation Operation
        {
            get { return operation; }
        }
    }
}

