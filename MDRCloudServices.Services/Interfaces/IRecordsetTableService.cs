using System.Threading.Tasks;
using MDRDB.Recordsets;
using NPoco;

namespace MDRCloudServices.Services.Interfaces;

/// <summary>
/// Service with functionality for recordset tables
/// </summary>
/// <remarks>
/// Separate from the recordset service in order to avoid circular
/// dependencies with the fields service.
/// </remarks>
public interface IRecordsetTableService
{
    /// <summary>Check if table exists in the specified database</summary>
    /// <param name="storageDb"></param>
    /// <param name="schema"></param>
    /// <param name="tableName"></param>
    /// <returns></returns>
    Task<bool> TableExistsAsync(IDatabase storageDb, string schema, string tableName);

    /// <summary>Check if column exists in specified database</summary>
    /// <param name="storageDb"></param>
    /// <param name="schema"></param>
    /// <param name="tableName"></param>
    /// <param name="columnName"></param>
    /// <returns></returns>
    Task<bool> ColumnExistsAsync(IDatabase storageDb, string schema, string tableName, string columnName);

    /// <summary>Get the table name (including schema) for the specified recordset</summary>
    /// <param name="rs">Recordset object</param>
    /// <param name="location">Location object for the recordset</param>
    /// <returns>Table name in storage database</returns>
    string TableNameForRecordset(Recordset rs, Location location);

    /// <summary>Get the table (including schema) name for the specified recordset</summary>
    /// <param name="rs">Recordset object</param>
    /// <returns>Table name in storage database</returns>
    Task<string> TableNameForRecordsetAsync(Recordset rs);

    /// <summary>Get the table name (excluding schema) for the specified recordset</summary>
    /// <param name="rs">Recordset object</param>
    /// <returns>Table name in storage database</returns>
    Task<string> ShortTableNameForRecordsetAsync(Recordset rs);

    /// <summary>Get the table name (excluding schema) for the specified recordset</summary>
    /// <param name="rs">Recordset object</param>
    /// <param name="location">Location object for the recordset</param>
    /// <returns>Table name in storage database</returns>
    string ShortTableNameForRecordset(Recordset rs, Location location);
}
