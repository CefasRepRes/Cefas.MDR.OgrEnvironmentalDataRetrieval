using System;
using System.Linq;
using Microsoft.Data.SqlClient;
using NPoco.SqlServer;
using Polly;
using Serilog;

namespace MDRCloudServices.Helpers;

public static class SqlHelper
{
    public static string QuoteIdentifier(string unquotedIdentifier)
    {
        return $"\"{unquotedIdentifier}\"";
    }

    public static string GenerateColumnName(string name)
    {
        if (name == null) name = string.Empty;

        /* we will only allow alphanumeric, underscore, and space characters in field names, that way we can't get SQL injection
            we deliberately *don't* use char.iswhitespace because weird space characters would cause us to need to escape the column names */

        var colname = new string(name.Trim().Where(c => char.IsLetterOrDigit(c) || c == ' ' || c == '_').ToArray());

        // columns prefixed with numbers cause issues with querying as the number can be ignored leading to duplicate column names
        // Prefix any column starting with a number with an underscore
        if (char.IsDigit(colname[0])) colname = "_" + colname;

        // PetaPoco's paging can have problems if the word 'from' appears in the middle of a field name, so we'll swap all spaces for underscores.
        colname = colname.Replace(' ', '_');
        if (colname.Length > 128)
        {
            /* max column name length in SQL server */
            colname = colname[..127];
        }
        return colname;
    }


    public static IPollyPolicy GetNPocoPollyPolicy()
    {
        var policy = Policy
            .Handle<SqlException>(ex => IsTransientError(ex))
            .WaitAndRetry(new[]
            {
                    TimeSpan.FromSeconds(0.5),
                    TimeSpan.FromSeconds(1),
                    TimeSpan.FromSeconds(1.5),
                    TimeSpan.FromSeconds(2),
                    TimeSpan.FromSeconds(2.5)
            }, (exception, timeSpan, context) =>
            {
                // Log that we are retrying
                Log.Error(exception, $"Retrying after transient exception {exception.Message}");
            });

        var asyncPolicy = Policy
            .Handle<SqlException>(ex => IsTransientError(ex))
            .WaitAndRetryAsync(new[]
            {
                        TimeSpan.FromSeconds(0.5),
                        TimeSpan.FromSeconds(1),
                        TimeSpan.FromSeconds(1.5),
                        TimeSpan.FromSeconds(2),
                        TimeSpan.FromSeconds(2.5)
            }, (exception, timeSpan, context) =>
            {
                // Log that we are retrying
                Log.Error(exception, $"Retrying after transient exception {exception.Message}");
            });

        var pocoPolicy = new DefaultPollyPolicy
        {
            AsyncRetryPolicy = asyncPolicy,
            RetryPolicy = policy
        };

        return pocoPolicy;
    }

    // Identifies which errors in SQL are transient
    private static bool IsTransientError(SqlException ex)
    {
        foreach (SqlError err in ex.Errors)
        {
            switch (err.Number)
            {
                // SQL Error Code: 49920
                // Cannot process request. Too many operations in progress for subscription "%ld".
                // The service is busy processing multiple requests for this subscription.
                // Requests are currently blocked for resource optimization. Query sys.dm_operation_status for operation status.
                // Wait until pending requests are complete or delete one of your pending requests and retry your request later.
                case 49920:
                // SQL Error Code: 49919
                // Cannot process create or update request. Too many create or update operations in progress for subscription "%ld".
                // The service is busy processing multiple create or update requests for your subscription or server.
                // Requests are currently blocked for resource optimization. Query sys.dm_operation_status for pending operations.
                // Wait till pending create or update requests are complete or delete one of your pending requests and
                // retry your request later.
                case 49919:
                // SQL Error Code: 49918
                // Cannot process request. Not enough resources to process request.
                // The service is currently busy.Please retry the request later.
                case 49918:
                // SQL Error Code: 41839
                // Transaction exceeded the maximum number of commit dependencies.
                case 41839:
                // SQL Error Code: 41325
                // The current transaction failed to commit due to a serializable validation failure.
                case 41325:
                // SQL Error Code: 41305
                // The current transaction failed to commit due to a repeatable read validation failure.
                case 41305:
                // SQL Error Code: 41302
                // The current transaction attempted to update a record that has been updated since the transaction started.
                case 41302:
                // SQL Error Code: 41301
                // Dependency failure: a dependency was taken on another transaction that later failed to commit.
                case 41301:
                // SQL Error Code: 40613
                // Database XXXX on server YYYY is not currently available. Please retry the connection later.
                // If the problem persists, contact customer support, and provide them the session tracing ID of ZZZZZ.
                case 40613:
                // SQL Error Code: 40501
                // The service is currently busy. Retry the request after 10 seconds. Code: (reason code to be decoded).
                case 40501:
                // SQL Error Code: 40197
                // The service has encountered an error processing your request. Please try again.
                case 40197:
                // SQL Error Code: 10936
                // Resource ID : %d. The request limit for the elastic pool is %d and has been reached.
                // See 'http://go.microsoft.com/fwlink/?LinkId=267637' for assistance.
                case 10936:
                // SQL Error Code: 10929
                // Resource ID: %d. The %s minimum guarantee is %d, maximum limit is %d and the current usage for the database is %d.
                // However, the server is currently too busy to support requests greater than %d for this database.
                // For more information, see http://go.microsoft.com/fwlink/?LinkId=267637. Otherwise, please try again.
                case 10929:
                // SQL Error Code: 10928
                // Resource ID: %d. The %s limit for the database is %d and has been reached. For more information,
                // see http://go.microsoft.com/fwlink/?LinkId=267637.
                case 10928:
                // SQL Error Code: 10060
                // A network-related or instance-specific error occurred while establishing a connection to SQL Server.
                // The server was not found or was not accessible. Verify that the instance name is correct and that SQL Server
                // is configured to allow remote connections. (provider: TCP Provider, error: 0 - A connection attempt failed
                // because the connected party did not properly respond after a period of time, or established connection failed
                // because connected host has failed to respond.)
                case 10060:
                // SQL Error Code: 10054
                // A transport-level error has occurred when sending the request to the server.
                // (provider: TCP Provider, error: 0 - An existing connection was forcibly closed by the remote host.)
                case 10054:
                // SQL Error Code: 10053
                // A transport-level error has occurred when receiving results from the server.
                // An established connection was aborted by the software in your host machine.
                case 10053:
                // SQL Error Code: 1205
                // Deadlock
                case 1205:
                // SQL Error Code: 233
                // The client was unable to establish a connection because of an error during connection initialization process before login.
                // Possible causes include the following: the client tried to connect to an unsupported version of SQL Server,
                // the server was too busy to accept new connections, or there was a resource limitation (insufficient memory or maximum
                // allowed connections) on the server. (provider: TCP Provider, error: 0 - An existing connection was forcibly closed by
                // the remote host.)
                case 233:
                // SQL Error Code: 121
                // The semaphore timeout period has expired
                case 121:
                // SQL Error Code: 64
                // A connection was successfully established with the server, but then an error occurred during the login process.
                // (provider: TCP Provider, error: 0 - The specified network name is no longer available.)
                case 64:
                // DBNETLIB Error Code: 20
                // The instance of SQL Server you attempted to connect to does not support encryption.
                case 20:
                    return true;
            }
        }

        return false;
    }
}
