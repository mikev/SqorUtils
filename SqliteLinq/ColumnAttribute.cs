using System;

namespace Sqor.Utils.SqliteLinq
{
    public class ColumnAttribute : Attribute
    {
        public string ColumnName { get; set; }

        public ColumnAttribute(string columnName)
        {
        }
    }
}

