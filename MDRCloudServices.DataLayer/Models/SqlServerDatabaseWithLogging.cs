using System.Data.Common;
using NPoco.DatabaseTypes;
using NPoco.SqlServer;
using Serilog;

namespace MDRCloudServices.DataLayer.Models;

public class SqlServerDatabaseWithLogging : SqlServerDatabase
{
    public SqlServerDatabaseWithLogging(string connectionString, IPollyPolicy? pollyPolicy = null) : base(connectionString, pollyPolicy)
    {
    }

    public SqlServerDatabaseWithLogging(string connectionString, SqlServerDatabaseType databaseType, IPollyPolicy pollyPolicy) : base(connectionString, databaseType, pollyPolicy)
    {
    }

    protected override void OnException(Exception exception)
    {
        Log.Error(exception, LastSQL + Environment.NewLine + "Parameters: " + string.Join(", ", LastArgs ?? Array.Empty<object>()));
        base.OnException(exception);
    }

#if INTERNAL_DEBUG || DEBUG
    protected override void OnExecutingCommand(DbCommand cmd)
    {
        Log.Information(LastSQL + Environment.NewLine + "Parameters: " + string.Join(", ", LastArgs ?? Array.Empty<object>()));
        base.OnExecutingCommand(cmd);
    }
#endif
}
