using System;

namespace Sqor.Utils.SqliteLinq.Migrate
{
    public class SysTable
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        
        public string Name { get; set; }
        
        public override string ToString()
        {
            return Name;
        }
    }
}

