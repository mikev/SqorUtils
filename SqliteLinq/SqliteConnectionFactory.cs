#if MONOTOUCH

using System;
using Mono.Data.Sqlite;
using System.Data.Common;

namespace Sqor.Utils.SqliteLinq
{
    public class SqliteConnectionFactory : DbProviderFactory
    {
        public string ConnectionString { get; set; }

        public override DbCommand CreateCommand()
        {
            return new SqliteCommand();
        }
        
        public override DbCommandBuilder CreateCommandBuilder()
        {
            return new SqliteCommandBuilder();
        }
        
        public override DbConnection CreateConnection()
        {
            var connection = new SqliteConnection();
            connection.ConnectionString = ConnectionString;
            return connection;
        }
        
        public override DbConnectionStringBuilder CreateConnectionStringBuilder()
        {
            return new SqliteConnectionStringBuilder();
        }
        
        public override DbDataAdapter CreateDataAdapter()
        {
            return new SqliteDataAdapter();
        }
        
        public override DbParameter CreateParameter()
        {
            return new SqliteParameter();
        }
    }
}

#endif