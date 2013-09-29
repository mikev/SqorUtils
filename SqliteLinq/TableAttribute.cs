using System;

namespace Sqor.Utils.SqliteLinq
{
    public class TableAttribute : Attribute
    {
        public string TableName { get; private set; }
    
        public TableAttribute(string tableName)
        {
            TableName = tableName;
        }
    }
}

