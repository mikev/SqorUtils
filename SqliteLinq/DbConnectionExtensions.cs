using System;
using System.Data.Common;
using System.Data;
using Sqor.Utils.Logging;

namespace Sqor.Utils.SqliteLinq
{
    public static class DbConnectionExtensions
    {
        private static bool isTraceEnabled = true;
    
        public static object ExecuteScalar(this DbConnection connection, string sql)
        {
            if (isTraceEnabled)
                Logger.Instance.LogInfo(sql);
                
            using (var command = connection.CreateCommand())
            {
                command.CommandText = sql;  
                command.CommandType = CommandType.Text;
                return command.ExecuteScalar();
            }
        }

        public static int Execute(this DbConnection connection, string sql)
        {
            if (isTraceEnabled)
                Logger.Instance.LogInfo(sql);
        
            using (var command = connection.CreateCommand())
            {
                command.CommandText = sql;
                command.CommandType = CommandType.Text;
                return command.ExecuteNonQuery();
            }
        }
    }
}

