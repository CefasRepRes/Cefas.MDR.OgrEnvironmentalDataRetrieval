using System.Data.Common;
using Npgsql;
using NPoco;
using NPoco.SqlServer;
using Serilog;

namespace MDRCloudServices.DataLayer.Models;

public class PgDatabseWithLogging : Database
{
    private readonly IPollyPolicy? _pollyPolicy;

    public PgDatabseWithLogging(string connectionString, IPollyPolicy? policy = null) : base(connectionString, DatabaseType.PostgreSQL, NpgsqlFactory.Instance)
    {
        _pollyPolicy = policy;
    }

    protected override T ExecutionHook<T>(Func<T> action)
    {
        if (_pollyPolicy?.RetryPolicy != null)
        {
            return _pollyPolicy!.RetryPolicy.Execute(action);
        }

        return base.ExecutionHook(action);
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
