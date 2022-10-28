using NPoco;

namespace MDRCloudServices.DataLayer.Npoco;

public class PostgresServerDatabase : Database
{
    public PostgresServerDatabase(string connectionString)
        : this(connectionString, DatabaseType.PostgreSQL)
    {
    }

    public PostgresServerDatabase(string connectionString, DatabaseType databaseType)
        : base(connectionString, databaseType, Npgsql.NpgsqlFactory.Instance)
    {
    }
}
