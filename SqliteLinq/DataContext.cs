using System;
using System.Collections.Generic;
using System.Data.Common;

namespace Sqor.Utils.SqliteLinq
{
    public class DataContext : IDisposable
    {
        private DbConnection connection;
        private bool isConnectionOwned;
        private DbProviderFactory connectionFactory;
        private MappingManager mappings;

        public DataContext(DbProviderFactory connectionFactory, MappingManager mappings)
        {
            this.connectionFactory = connectionFactory;
            this.mappings = mappings;
        }

        public DbConnection Connection
        {
            get 
            {
                if (connection == null)
                {
                    isConnectionOwned = true;
                    connection = connectionFactory.CreateConnection();
                    connection.Open();
                }
                return connection;
            }
            set
            {
                isConnectionOwned = false;
                connection = value;
            }
        }

        public DbProviderFactory ConnectionFactory
        {
            get { return this.connectionFactory; }
        }

        public Table<T> Table<T>() where T : new()
        {
            return new Table<T>(this, mappings[typeof(T)]);
        }

        public void Dispose()
        {
            if (connection != null && isConnectionOwned)
                connection.Dispose();
        }
    }
}

