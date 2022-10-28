using MDRCloudServices.Interfaces;
using MDRDB.Recordsets;

namespace MDRCloudServices.Services.Interfaces;

/// <summary>Service for manipulating recordsets</summary>
/// <remarks>
/// Functions related to the table name of the recordset have been extracted
/// into a separate service to avoid circular dependencies between this and
/// the fields service. We have pass through methods here so that we don't
/// need to inject both the recordset and recordset table services into
/// the same controller.
/// </remarks>
public interface IRecordsetService
{
    /// <summary>Get a single recordset object by Id</summary>
    /// <remarks>Throws exception if recordset or its location can't be found.</remarks>
    /// <param name="id">The id of the recordset</param>
    /// <returns>Recordset object or </returns>
    /// <exception cref="Exceptions.NotFoundException">Either the recordset or the location for the recordset can't be found.</exception>
    Task<Recordset> GetRecordsetAsync(int id);

    /// <summary>Gets a list of all recordsets that the current user is allowed to view</summary>
    /// <returns>List of recordsets</returns>
    Task<List<Recordset>> GetAllRecordsetsAsync();

    /// <summary>Gets the location object for the specified recordset</summary>
    /// <param name="rs">The recordset</param>
    /// <returns>Location</returns>
    Task<Location> GetLocationForRecordsetAsync(Recordset rs);

    /// <summary>Get fields for tabular recordset</summary>
    /// <param name="rs"></param>
    /// <param name="editMode">Are we in edit mode?</param>
    /// <returns>List of field objects</returns>
    Task<List<IField>> GetFieldsForRecordset(Recordset rs, bool editMode);
}
