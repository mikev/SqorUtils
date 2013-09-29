using System;

namespace Sqor.Utils.SqliteLinq.Migrate
{
    public class SysMigration
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        public string Name { get; set; }
    }
}