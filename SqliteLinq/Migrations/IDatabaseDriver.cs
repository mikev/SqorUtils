using System;
using System.Collections.Generic;
using System.Data.Common;

namespace Sqor.Utils.SqliteLinq.Migrate
{
    public interface IDatabaseDriver
    {
        DbProviderFactory ConnectionFactory { get; }
        Func<SysDb> SysDb { get; }
        string ConvertFromSqlType(SqlType sqlType);
        string GetColumnOptions(SysColumn column);
        string FormatObject(object o);
    }
}

