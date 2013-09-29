using System;
using Sqor.Utils.SqliteLinq.Migrate;

namespace Sqor.Utils.SqliteLinq.Migrations
{
    public class CreateMetaTables : Migration
    {
        public CreateMetaTables() : base(DateTime.MinValue)
        {
            IsAutomaticallyPerformed = false;
            IsMetaDataEmitted = false;
        }

        protected override void Migrate()
        {
            CreateTable(
                "SysTable",
                columns => new 
                {
                    Id = columns.Int(isPrimaryKey: true, isDbGenerated: true),
                    Name = columns.String()
                });
                
            CreateTable(
                "SysColumn",
                columns => new
                {
                    Id = columns.Int(isPrimaryKey: true, isDbGenerated: true),
                    TableId = columns.Int(),
                    Name = columns.String(),
                    Type = columns.String(),
                    MaxLength = columns.Int(),
                    IsNullable = columns.Bool(),
                    IsDbGenerated = columns.Bool(),
                    IsPrimaryKey = columns.Bool(),
                    Collation = columns.String(isNullable: true),
                    DefaultValue = columns.String(isNullable: true)
                });
        
            CreateTable(
                "SysMigration",
                columns => new
                {
                    Id = columns.Int(isPrimaryKey: true, isDbGenerated: true),
                    Name = columns.String()
                });
        }
    }
}

