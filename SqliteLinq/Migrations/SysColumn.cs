using System;

namespace Sqor.Utils.SqliteLinq.Migrate
{
    public class SysColumn
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        
        public int TableId { get; set; }
        
        public string Name { get; set; }
        public SqlType Type { get; set; }
        public int MaxLength { get; set; }
        public bool IsNullable { get; set; }
        public bool IsDbGenerated { get; set; }
        public bool IsPrimaryKey { get; set; }
        public string Collation { get; set; }
        public string DefaultValue { get; set; }
        
        public override string ToString()
        {
            return Name;
        }
    }
}

