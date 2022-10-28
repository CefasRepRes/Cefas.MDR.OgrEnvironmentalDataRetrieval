using MDRCloudServices.DataLayer.Npoco;
using Microsoft.Data.SqlClient;
using Npgsql;
using NPoco;
using NPoco.SqlServer;

namespace MDRCloudServices.DataLayer.Models;

public class DBConnectionDetails
{
    public string? DataSource { get; set; }
    public string? InitialCatalog { get; set; }
    public string? UserId { get; set; }
    public string? Password { get; set; }
    public bool Multipleactiveresultsets { get; set; }
    public int ConnectionTimeout { get; set; }
    public string? ProviderName { get; set; }
    public bool? Encrypt { get; set; }

    public virtual IDatabase CreateDb()
    {
        return ProviderName switch
        {
            "Microsoft.Data.SqlClient" => BuildSqlServerDatabase(),
            "Npgsql" => BuildPostgresDatabase(),
            _ => throw new NotImplementedException("Unknown database provider")
        };
    }

    private IDatabase BuildSqlServerDatabase()
    {
        var builder = new SqlConnectionStringBuilder
        {
            Password = Password,
            UserID = UserId,
            DataSource = DataSource,
            InitialCatalog = InitialCatalog,
            MultipleActiveResultSets = Multipleactiveresultsets,
            ConnectTimeout = ConnectionTimeout,
            Encrypt = Encrypt ?? false,
        };
        return new SqlServerDatabase(builder.ConnectionString);
    }

    private IDatabase BuildPostgresDatabase()
    {
        var builder = new NpgsqlConnectionStringBuilder
        {
            Password = Password,
            Username = UserId,
            Database = InitialCatalog,
            Host = DataSource
        };
        return new PostgresServerDatabase(builder.ConnectionString);
    }
}
