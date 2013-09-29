using System;
using System.Collections.Generic;
using System.Linq;
using Sqor.Utils.SqliteLinq.Migrate;
using Sqor.Utils.Enumerables;
using Sqor.Utils.SqliteLinq.Migrations;
using System.Reflection;

namespace Sqor.MockBackend.Database.Migrations
{
    public class MigrationManager
    {
        private SysDb db;
        private IDatabaseDriver driver;
        private Assembly assembly;

        public MigrationManager(IDatabaseDriver driver, Assembly assembly)
        {
            this.db = driver.SysDb();
            this.driver = driver;
            this.assembly = assembly;
        }

        public IEnumerable<Migration> FindPendingMigrations()
        {
            var existingMigrationNames = db.Migrations.Select(x => x.Name).ToHashSet();

            return assembly
                .GetTypes()
                .Where(x => !x.IsAbstract && typeof(Migration).IsAssignableFrom(x) && !existingMigrationNames.Contains(x.Name))
                .Select(x => (Migration)Activator.CreateInstance(x))
                .Where(x => x.IsAutomaticallyPerformed)
                .OrderBy(x => x.Timestamp);
        }

        public void ExecutePendingMigrations()
        {
            // The DbMigration table must exist in order to apply migrations
            if (!db.Migrations.TableExists())
            {
                Console.WriteLine("Migration table missing, creating.");
                new CreateMetaTables().PerformMigration(driver);
            }
 
            // Now apply any pending migrations
            foreach (var migration in FindPendingMigrations())
            {
                Console.WriteLine("Applying migrations from " + migration.Name);
                migration.PerformMigration(driver);
                var dbMigration = new SysMigration { Name = migration.GetType().Name };
                db.Migrations.Insert(dbMigration);
            }
        }
    }
}