using MDRCloudServices.DataLayer.Models;
using MDRCloudServices.Services.Interfaces;
using MDRDB.Recordsets;
using NPoco;

namespace MDRCloudServices.Services.Services;

public class RecordsetTableService : IRecordsetTableService
{
    private readonly IDatabase _db;

    public RecordsetTableService(IDatabase db)
    {
        _db = db;
    }

    public async Task<bool> TableExistsAsync(IDatabase storageDb, string schema, string tableName)
    {
        var count = await storageDb.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES " +
            "WHERE TABLE_SCHEMA = @0 AND TABLE_NAME = @1", schema, tableName);
        return count == 1;
    }

    public async Task<bool> ColumnExistsAsync(IDatabase storageDb, string schema, string tableName, string columnName)
    {
        var count = await storageDb.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS " +
            "WHERE TABLE_SCHEMA = @0 AND TABLE_NAME = @1 AND COLUMN_NAME = @2", schema, tableName, columnName);
        return count == 1;
    }

    public string TableNameForRecordset(Recordset rs, Location location)
    {
        if (!string.IsNullOrEmpty(rs.TableName))
        {
            return $"{location.Schema}.{rs.TableName}";
        }
        return $"{location.Schema}.{location.TableNamePrefix}{rs.Id}";
    }

    public async Task<string> TableNameForRecordsetAsync(Recordset rs)
    {
        var loc = await _db.SingleAsync<Location>(rs.Location);
        return TableNameForRecordset(rs, loc);
    }

    public async Task<string> ShortTableNameForRecordsetAsync(Recordset rs)
    {
        if (!string.IsNullOrEmpty(rs.TableName))
        {
            return rs.TableName;
        }
        else
        {
            var loc = await _db.SingleAsync<Location>(rs.Location);
            return loc.TableNamePrefix + rs.Id.ToString();
        }
    }

    public string ShortTableNameForRecordset(Recordset rs, Location location)
    {
        if (rs.TableName != null)
        {
            return rs.TableName;
        }
        else
        {
            return location.TableNamePrefix + rs.Id.ToString();
        }
    }
}
