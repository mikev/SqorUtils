using System;
using System.Data.Common;

namespace Sqor.Utils.SqliteLinq.Migrate
{
    public class SysDb : DataContext
    {
        public SysDb(DbProviderFactory connectionFactory) : base(connectionFactory, MappingManager.Create<SysDb>())
        {
        }
        
        public Table<SysTable> Tables 
        {
            get { return Table<SysTable>(); }
        }
        
        public Table<SysColumn> Columns
        {
            get { return Table<SysColumn>(); }
        }
        
        public Table<SysMigration> Migrations 
        {
            get { return Table<SysMigration>(); }
        }
    }
}

